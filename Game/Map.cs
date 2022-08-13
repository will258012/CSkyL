namespace CSkyL.Game
{
    using ID;
    using UnityEngine;
    using Position = Transform.Position;

    public static class Map
    {
        const float defaultHeightOffset = 2f;

        public static float ToKilometer(this float gameDistance)
            => gameDistance * 5f / 3f;

        public static float ToMile(this float gameDistance)
            => gameDistance.ToKilometer() * .621371f;

        public static float GetTerrainLevel(Position position)
            => TerrainManager.instance.SampleDetailHeightSmooth(position._AsVec);
        public static float GetWaterLevel(Position position)
            => TerrainManager.instance.WaterLevel(position._AsVec2);
        public static float? GetClosestSegmentLevel(Position position)
        {
            var input = Tool._GetRaycastInput(position._AsVec, new Math.Range(-100f, 2f));
            input.m_netService.m_service = ItemClass.Service.Road;
            input.m_netService.m_itemLayers = ItemClass.Layer.Default |
                                              ItemClass.Layer.PublicTransport |
                                              ItemClass.Layer.MetroTunnels;
            input.m_ignoreSegmentFlags = NetSegment.Flags.Deleted |
                                         NetSegment.Flags.Collapsed | NetSegment.Flags.Flooded;

            return Tool._RayCast(input, 5f) is ToolBase.RaycastOutput result ?
                   (float?) result.m_hitPos.y + defaultHeightOffset : null;
        }

        public static float GetMinHeightAt(Position position)
            => Mathf.Max(GetTerrainLevel(position), GetWaterLevel(position)) + defaultHeightOffset;

        public static SegmentID RayCastRoad(Position position)
        {
            var input = Tool._GetRaycastInput(position._AsVec);
            input.m_netService.m_service = ItemClass.Service.Road;
            input.m_netService.m_itemLayers = ItemClass.Layer.Default |
                                              ItemClass.Layer.PublicTransport;
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;

            return Tool._RayCast(input, 5f) is ToolBase.RaycastOutput result ?
                   SegmentID._FromIndex(result.m_netSegment) : null;
        }

        public static DistrictID RayCastDistrict(Position position)
        {
            var input = Tool._GetRaycastInput(position._AsVec);
            input.m_ignoreDistrictFlags = District.Flags.None;

            return Tool._RayCast(input, 5f) is ToolBase.RaycastOutput result ?
                   DistrictID._FromIndex(result.m_district) : null;
        }

        private class Tool : ToolBase
        {
            internal static RaycastOutput? _RayCast(
                    RaycastInput rayCastInput, float offset = 0f)
            {
                if (offset.AlmostEquals(0f))
                    return RayCast(rayCastInput, out RaycastOutput result) ?
                           (RaycastOutput?) result : null;
                foreach (var delta in new Vector3[] { Vector3.zero, Vector3.forward * offset,
                                                  Vector3.left * offset, Vector3.right * offset,
                                                  Vector3.back * offset }) {
                    var input = rayCastInput;
                    input.m_ray.origin = rayCastInput.m_ray.origin + delta;
                    if (RayCast(input, out RaycastOutput result)) return result;
                }
                return null;
            }

            internal static RaycastInput _GetRaycastInput(Vector3 position)
                => _GetRaycastInput(position, new Math.Range(-100f, 100f));
            internal static RaycastInput _GetRaycastInput(Vector3 position,
                                                          Math.Range verticalRange)
            {
                var input = new RaycastInput(
                                new Ray(position + Vector3.up * verticalRange.max, Vector3.down),
                                        verticalRange.Size)
                { m_ignoreTerrain = true };
                return input;
            }
        }
    }
}
