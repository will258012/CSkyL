namespace CSkyL.Game
{
    using ID;
    using UnityEngine;
    using Position = Transform.Position;

    public static class Map
    {
        public static float ToKilometer(this float gameDistance)
            => gameDistance * 5f / 3f;

        public static float ToMile(this float gameDistance)
            => gameDistance.ToKilometer() * .621371f;

        public static float GetTerrainLevel(Position position)
            => TerrainManager.instance.SampleDetailHeight(position._AsVec);
        public static float GetWaterLevel(Position position)
            => TerrainManager.instance.WaterLevel(position._AsVec2);

        public static void RayCast(Position position,
                            out SegmentID segmentID, out DistrictID districtID)
        {
            const float offset = 5f;
            var pos = position._AsVec;

            segmentID = null; districtID = null;

            foreach (var delta in new Vector3[] { Vector3.zero, Vector3.forward * offset,
                                                  Vector3.left * offset, Vector3.right * offset,
                                                  Vector3.back * offset }) {
                Tool.RayCast(pos + delta, out var _segid, out var _did);
                segmentID = segmentID ?? _segid; districtID = districtID ?? _did;
                if (segmentID is object && districtID is object) return;
            }
        }

        public static float GetMinHeightAt(Position position)
        {
            const float defaultOffset = 2f;
            return Mathf.Max(GetTerrainLevel(position), GetWaterLevel(position)) + defaultOffset;
        }

        private class Tool : ToolBase
        {
            public static void RayCast(Vector3 position,
                                   out SegmentID segmentID, out DistrictID districtID)
            {
                const float verticalRange = 100f;
                RaycastInput rayCastInput = new RaycastInput(
                        new Ray(position + Vector3.up * verticalRange, Vector3.down),
                                verticalRange * 2f);
                rayCastInput.m_netService.m_service = ItemClass.Service.Road;
                rayCastInput.m_netService.m_itemLayers = ItemClass.Layer.Default |
                                                         ItemClass.Layer.PublicTransport;
                rayCastInput.m_ignoreSegmentFlags = NetSegment.Flags.None;
                rayCastInput.m_ignoreDistrictFlags = District.Flags.None;
                rayCastInput.m_ignoreTerrain = true;

                RayCast(rayCastInput, out RaycastOutput result);
                segmentID = SegmentID._FromIndex(result.m_netSegment);
                districtID = DistrictID._FromIndex(result.m_district);

            }
        }
    }
}
