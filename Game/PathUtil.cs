

namespace CSkyL.Game
{
    using ColossalFramework.Math;
    using CSkyL.Game.Object;
    using CSkyL.Transform;
    using System;
    using UnityEngine;

    internal static class NetUtil
    {
        private static readonly NetManager netMan = NetManager.instance;
        private static readonly NetNode[] nodeBuffer_ = netMan.m_nodes.m_buffer;
        private static readonly NetSegment[] segmentBuffer_ = netMan.m_segments.m_buffer;
        private static readonly NetLane[] laneBuffer_ = netMan.m_lanes.m_buffer;

        public static ref NetNode ToNode(this ushort id) => ref nodeBuffer_[id];
        public static ref NetSegment ToSegment(this ushort id) => ref segmentBuffer_[id];
        public static ref NetLane ToLane(this uint id) => ref laneBuffer_[id];

        public static bool IsStartNode(ushort segmentId, ushort nodeId)
            => segmentId.ToSegment().m_startNode == nodeId;
        public static bool IsStartNode(this ref NetSegment segment, ushort nodeId)
            => segment.m_startNode == nodeId;
    }

    public static class PathUtil
    {
        const int targetPosIndex = 3;
        internal const float BYTE2FLOAT_OFFSET = 1f / 255;

        internal static ref PathUnit ToPathUnit(this uint id)
            => ref PathManager.instance.m_pathUnits.m_buffer[id];
        internal static ref NetLane GetLane(this ref PathUnit.Position pathPos)
            => ref PathManager.GetLaneID(pathPos).ToLane();
        internal static NetInfo.Lane GetLaneInfo(this ref PathUnit.Position pathPos)
            => pathPos.m_segment.ToSegment().Info?.m_lanes?[pathPos.m_lane];

        internal static bool OnPedestrianLane(this ref PathUnit.Position pathPos)
            => pathPos.GetLaneInfo().m_laneType == NetInfo.LaneType.Pedestrian;


        internal static bool CalculateTransitionBezier(this PathUnit pathUnit,
                                                       byte finePathPosIndex, out Bezier3 bezier)
        {
            bezier = default;
            if ((finePathPosIndex & 1) == 0) return false; // transition is odd
            if ((finePathPosIndex >> 1) >= pathUnit.m_positionCount) return false; // bad index
            var pathPos1 = pathUnit.GetPosition(finePathPosIndex >> 1);
            if (pathPos1.m_segment == 0) return false;
            if (!pathUnit.GetNextPosition(finePathPosIndex >> 1, out var pathPos2)) return false;
            bezier = pathPos1.CalculateTransitionBezier(pathPos2);
            return true;
        }

        internal static void CalculatePositionAndDirection(
            this PathUnit.Position pathPos, byte offset, out Vector3 pos, out Vector3 dir)
        {
            pathPos.GetLane().CalculatePositionAndDirection(
                offset * BYTE2FLOAT_OFFSET, out pos, out dir);
            dir.Normalize();
            if (offset == 0) dir = -dir;
        }

        internal static Bezier3 CalculateTransitionBezier(this PathUnit.Position pathPos1,
                                                          PathUnit.Position pathPos2)
        {
            pathPos1.CalculatePositionAndDirection(pathPos1.m_offset, out var pos1, out var dir1);

            if (pathPos1.GetNodeID() == 0) {
                // use pathPos1 offset because we are transitioning from road to pavement.
                pathPos2.CalculatePositionAndDirection(
                pathPos1.m_offset, out var pos2, out var dir2);
                // straight line:
                dir1 = (pos2 - pos1).normalized;
                dir2 = -dir1;
                return CSkyL.Math.Bezier3ByDir(pos1, dir1, pos2, dir2);
            }
            else {
                byte offset2 = pathPos2.GetEndOffsetToward(pathPos1);
                pathPos2.CalculatePositionAndDirection(
                offset2, out var pos2, out var dir2);
                if (pathPos1.OnPedestrianLane() && pathPos1.m_segment == pathPos2.m_segment) {
                    // pedestrian crossing or going to vehicle : straight line
                    dir1 = (pos2 - pos1).normalized;
                    dir2 = -dir1;
                    return CSkyL.Math.Bezier3ByDir(pos1, dir1, pos2, dir2);
                }
                else {
                    return CSkyL.Math.Bezier3ByDir(pos1, dir1, pos2, dir2, true, true);
                }
            }
        }

        internal static byte GetEndOffsetToward(this PathUnit.Position pathPos,
                                                PathUnit.Position pathPos0)
        {
            ushort nodeId = pathPos0.GetNodeID();
            bool startNode = pathPos.m_segment.ToSegment().IsStartNode(nodeId);
            if (startNode) return 0;
            else return 255;
        }

        internal static ushort GetNodeID(this ref PathUnit.Position pathPos)
        {
            switch (pathPos.m_offset) {
            case 255: return pathPos.m_segment.ToSegment().m_endNode;
            case 0: return pathPos.m_segment.ToSegment().m_startNode;
            default: return 0;

            }
        }

        internal static Vector3 GetPosition(this PathUnit.Position pathPos) =>
            pathPos.GetLane().CalculatePosition(pathPos.m_offset * BYTE2FLOAT_OFFSET);

        /// <summary>
        /// gets look ahead position.
        /// </summary>
        public static Position LookAhead(this IObjectToFollow target, float seconds)
        {
            return target.TravelBy(seconds: seconds, minDistance: 4f);
        }

        /// <summary>
        /// returns a position <paramref name="seconds"/> ahead on <paramref name="target"/>'s path.
        /// if not possible then returns target position instead.
        /// </summary>
        /// <param name="target">object to follow</param>
        /// <param name="seconds">seconds to travel based on speed</param>
        /// <param name="minDistance">minimum distance to travel at low speed</param>
        internal static Position TravelBy(this IObjectToFollow target,
                                          float seconds, float minDistance)
        {
            try {
                bool fail = target is Pedestrian pedestrian &&
                            (pedestrian.IsWaitingTransit || pedestrian.IsEnteringVehicle
                             || pedestrian.IsHangingAround);
                if (!fail && TravelImpl(target, seconds, minDistance, out Vector3 lastPos)) {
                    return Position._FromVec(lastPos);
                }
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
            return target.GetTargetPos(targetPosIndex);
        }

        /// <param name="lastPos">current position if no path was found
        ///      otherwise position that is <paramref name="seconds"/>
        ///      or <paramref name="minDistance"/> ahead</param>
        /// <returns>false if no path was found</returns>
        private static bool TravelImpl(IObjectToFollow target, float seconds,
                                       float minDistance, out Vector3 lastPos)
        {
            PathData pData = target.GetPathData();
            lastPos = pData.refPos;
            if (pData.pathUnitID == 0) return false;


            float speed = pData.velocity.magnitude * 5; // meters per second
            speed = Math.Max(5, speed);
            float distance = speed * seconds;
            distance = Math.Max(distance, minDistance);
            float accDistance = 0;

            if (pData.finePathPositionIndex == 255) {
                pData.finePathPositionIndex = 0;// initial position
                if (!pData.pathUnitID.ToPathUnit().CalculatePathPositionOffset(
                    0, pData.refPos, out pData.lastOffset)) {
                    return false;
                }
            }

            var pathPos = pData.pathUnitID.ToPathUnit()
                                          .GetPosition(pData.finePathPositionIndex >> 1);
            if (pathPos.m_segment == 0) return false;

            Bezier3 bezier;
            if ((pData.finePathPositionIndex & 1) == 0) {
                bezier = pathPos.GetLane().m_bezier;
                bezier = bezier.Cut(pData.lastOffset * BYTE2FLOAT_OFFSET,
                                    pathPos.m_offset * BYTE2FLOAT_OFFSET);
            }
            else {
                pData.pathUnitID.ToPathUnit().CalculateTransitionBezier(
                                                  pData.finePathPositionIndex, out bezier);
                bezier = bezier.Cut(pData.lastOffset * BYTE2FLOAT_OFFSET, 1);
            }

            while (true) {
                if (!bezier.TryTravel(ref accDistance, distance, ref lastPos)) {
                    return true;
                }

                //if (pathPos.GetNodeID() == 0) {
                //    // change transport type
                //    return pathPos.GetPosition();
                //}

                var pathPos0 = pathPos;
                pData.finePathPositionIndex++;
                if (!RollAndGetPathPos(ref pData.pathUnitID,
                                       ref pData.finePathPositionIndex, out pathPos))
                    return true;
                if ((pData.finePathPositionIndex & 1) != 0) {
                    pData.pathUnitID.ToPathUnit().CalculateTransitionBezier(
                                                      pData.finePathPositionIndex, out bezier);
                }
                else {
                    bezier = pathPos.GetLane().m_bezier;
                    byte offset1;
                    if (pathPos.m_segment != pathPos0.m_segment) {
                        offset1 = pathPos.GetEndOffsetToward(pathPos0);
                    }
                    else {
                        offset1 = pathPos0.m_offset;
                    }
                    float t1 = offset1 * BYTE2FLOAT_OFFSET;
                    float t2 = pathPos.m_offset * BYTE2FLOAT_OFFSET;
                    bezier = bezier.Cut(t1, t2);
                }
            }
        }

        static bool TryTravel(this Bezier3 bezier, ref float accDistance,
                              float distance, ref Vector3 lastPos)
        {
            float l = bezier.ArcLength();
            if (l == 0 || l > 1000) return true; // bad bezier
            if (accDistance + l >= distance) {
                float t = bezier.ArcTravel(distance - accDistance);
                bezier = bezier.Cut(0, t);
            }
            accDistance += l;
            lastPos = bezier.d;
            return accDistance < distance;
        }

        static bool RollAndGetPathPos(ref uint pathUnitID, ref byte finePathIndex,
                                      out PathUnit.Position pathPos)
        {
            if (finePathIndex >= 24) {
                finePathIndex = 0;
                pathUnitID = pathUnitID.ToPathUnit().m_nextPathUnit;
            }
            if ((finePathIndex >> 1) >= pathUnitID.ToPathUnit().m_positionCount
                || pathUnitID == 0) {
                pathPos = default;
                return false;
            }

            pathPos = pathUnitID.ToPathUnit().GetPosition(finePathIndex >> 1);
            if (pathPos.m_segment == 0) return false;
            return true;
        }

    }
}
