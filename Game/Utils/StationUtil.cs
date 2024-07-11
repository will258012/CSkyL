using ColossalFramework;
using UnityEngine;

namespace CSkyL.Game.Utils
{
    class StationUtil
    {
        public static readonly TransportInfo.TransportType[] stationTransportType =
{
            TransportInfo.TransportType.Train,
            TransportInfo.TransportType.Metro,
            TransportInfo.TransportType.Monorail,
            TransportInfo.TransportType.Tram,
            TransportInfo.TransportType.Bus,
            TransportInfo.TransportType.TouristBus,
            TransportInfo.TransportType.Helicopter,
            TransportInfo.TransportType.Ship
        };
        private static string GetStopName(ushort stopId)
        {
            InstanceID id = default;
            id.NetNode = stopId;
            string savedName = Singleton<InstanceManager>.instance.GetName(id);
            if (!savedName.IsNullOrWhiteSpace()) {
                return savedName;
            }

            NetManager nm = Singleton<NetManager>.instance;
            NetNode nn = nm.m_nodes.m_buffer[stopId];
            var pos = nn.m_position;
            //building
            ushort buildingId = FindTransportBuilding(pos, 100f);
            savedName = GetTransportBuildingName(buildingId);
            if (!savedName.IsNullOrWhiteSpace()) {
                return savedName;
            }
            //road
            savedName = $"{stopId} {GetStationRoadName(pos)}";
            if (!savedName.IsNullOrWhiteSpace()) {
                return savedName;
            }
            //district
            savedName = $"{GetStationDistrictName(pos)}";
            if (!savedName.IsNullOrWhiteSpace()) {
                return savedName;
            }
            return $"<Somewhere>[{stopId}]";
        }

        private static string GetTransportBuildingName(ushort buildingId)
        {
            InstanceID bid = default;
            bid.Building = buildingId;
            return Singleton<BuildingManager>.instance.GetBuildingName(buildingId, bid);
        }
        public static string GetStationName(ushort stopId) => GetStopName(stopId) ?? "(" + stopId + ")";
        private static string GetStationRoadName(Vector3 pos)
        {
            var segmentid = Map.RayCastRoad(new Transform.Position { x = pos.x, up = pos.y, y = pos.z });
            var name = Object.Segment.GetName(segmentid);
            return name;
        }
        private static object GetStationDistrictName(Vector3 pos)
        {
            var districtId = Map.RayCastDistrict(new Transform.Position { x = pos.x, up = pos.y, y = pos.z });
            var name = Object.District.GetName(districtId);
            return name;
        }
        private static ushort FindTransportBuilding(Vector3 pos, float maxDistance)
        {
            BuildingManager bm = Singleton<BuildingManager>.instance;

            foreach (var tType in stationTransportType) {
                ushort buildingid = bm.FindTransportBuilding(pos, maxDistance, tType);

                if (buildingid != 0) {
                    if (bm.m_buildings.m_buffer[buildingid].m_parentBuilding != 0) {
                        buildingid = Building.FindParentBuilding(buildingid);
                    }
                    return buildingid;
                }
            }
            return default;
        }
    }
}
