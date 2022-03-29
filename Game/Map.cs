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

        public static float GetMinHeightAt(Position position)
        {
            const float defaultOffset = 2f;
            return Mathf.Max(GetTerrainLevel(position), GetWaterLevel(position)) + defaultOffset;
        }

        public static SegmentID RayCastRoad(Position position)
        {
            var input = Tool._GetRaycastInput(position._AsVec);
            input.m_netService.m_service = ItemClass.Service.Road;
            input.m_netService.m_itemLayers = ItemClass.Layer.Default |
                                              ItemClass.Layer.PublicTransport;
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;

            return SegmentID._FromIndex(Tool._RayCast(input)?.m_netSegment ?? 0);
        }

        public static DistrictID RayCastDistrict(Position position)
        {
            var input = Tool._GetRaycastInput(position._AsVec);
            input.m_ignoreDistrictFlags = District.Flags.None;

            return DistrictID._FromIndex(Tool._RayCast(input)?.m_district ?? 0);

        }

        private class Tool : ToolBase
        {
            internal static RaycastOutput? _RayCast(RaycastInput rayCastInput)
            {
                const float offset = 5f;

                foreach (var delta in new Vector3[] { Vector3.zero, Vector3.forward * offset,
                                                  Vector3.left * offset, Vector3.right * offset,
                                                  Vector3.back * offset }) {
                    var input = rayCastInput;
                    rayCastInput.m_ray.origin = rayCastInput.m_ray.origin + delta;
                    if (RayCast(rayCastInput, out RaycastOutput result)) return result;
                }
                return null;
            }

            internal static RaycastInput _GetRaycastInput(Vector3 position)
            {
                const float verticalRange = 100f;
                var input = new RaycastInput(
                        new Ray(position + Vector3.up * verticalRange, Vector3.down),
                                verticalRange * 2f);
                input.m_ignoreTerrain = true;
                return input;
            }
        }
    }
}
