namespace CSkyL.Game.Object
{
    using CSkyL.Game.ID;
    using CSkyL.Transform;
    using System.Collections.Generic;
    using System.Linq;
    using Ctransl = CSkyL.Translation.Translations;
    public abstract class Vehicle : Object<VehicleID>, IObjectToFollow
    {
        public override string Name => manager.GetVehicleName(GetHeadVehicleID()._index);

        public PathData GetPathData()
        {
            ref global::Vehicle vehicle = ref GetVehicle();
            return new PathData(
                vehicle.m_path,
                vehicle.m_lastPathOffset,
                vehicle.m_pathPositionIndex,
                vehicle.GetLastFramePosition(),
                vehicle.GetLastFrameData().m_velocity
            );
        }

        public Position GetTargetPos(int index)
            => Position._FromVec(GetVehicle().GetTargetPos(index));
        public byte GetLastFrame() => GetVehicle().m_lastFrame;
        public uint GetTargetFrame()
        {
            ref var vehicle = ref GetVehicle();
            return vehicle.GetTargetFrame(vehicle.Info, _vid);
        }

        public Positioning GetPositioning()
        {
            ref var vehicle = ref GetVehicle();
            vehicle.GetSmoothPosition(_vid, out var pos0, out var rot0);
            return new Positioning(Position._FromVec(pos0), Angle._FromQuat(rot0));
        }

        public float GetSpeed() => GetVehicle().GetSmoothVelocity(_vid).magnitude;

        public virtual string GetStatus()
        {
            var vehicle = _Of(GetHeadVehicleID());
            var status = vehicle._VAI.GetLocalizedStatus(
                                vehicle._vid, ref vehicle.GetVehicle(), out var implID);
            switch (ObjectID._FromIID(implID)) {
            case BuildingID bid: status += " " + Building.GetName(bid); break;
            case HumanID hid: status += " " + Of(hid).Name; break;
            }
            return status;
        }
        public string GetPrefabName() => GetVehicle().Info.name;
        public Utils.Infos GetInfos()
        {
            var details = new Utils.Infos();

            var vehicle = _Of(GetHeadVehicleID());
            switch (vehicle.GetOwnerID()) {
            case BuildingID buildingID:
                details[Ctransl.Translate("INFO_VEHICLE_OWNER")] = Building.GetName(buildingID); break;
            case HumanID humanID:
                details[Ctransl.Translate("INFO_VEHICLE_OWNER")] = Of(humanID).Name; break;
            }

            vehicle._MoreDetails(ref details);
            return details;
        }
        public virtual void _MoreDetails(ref Utils.Infos details) { }

        public bool IsSpawned => _Is(global::Vehicle.Flags.Spawned);
        public bool IsGoingBack => _Is(global::Vehicle.Flags.GoingBack);
        public bool IsExporting => _Is(global::Vehicle.Flags.Exporting);
        public bool IsImporting => _Is(global::Vehicle.Flags.Importing);
        public bool IsReversed => _Is(global::Vehicle.Flags.Reversed);
        public bool IsHead => GetVehicle().m_leadingVehicle == 0;
        public bool IsEnd => GetVehicle().m_trailingVehicle == 0;
        public bool IsMiddle => !(IsHead || IsEnd);
        public bool IsFront => IsReversed ? IsEnd : IsHead;

        public float GetAttachOffsetFront() => _VInfo.m_attachOffsetFront;
        public VehicleID GetFrontVehicleID()
            => VehicleID._FromIndex(IsReversed ? GetVehicle().GetLastVehicle(_vid) :
                                               GetVehicle().GetFirstVehicle(_vid));

        public static VehicleID GetHeadVehicleIDof(VehicleID id)
            => VehicleID._FromIndex(_GetVehicle(id).GetFirstVehicle(id._index));
        public VehicleID GetHeadVehicleID()
            => VehicleID._FromIndex(GetVehicle().GetFirstVehicle(_vid));
        public ObjectID GetOwnerID()
            => ObjectID._FromIID(_VAI.GetOwnerID(_vid, ref GetVehicle()));
        public TransitID GetTransitLineID() => TransitID._FromIndex(GetVehicle().m_transportLine);
        // capacity == 0: invalid
        public void GetLoadAndCapacity(out int load, out int capacity)
            => _VAI.GetBufferStatus(_vid, ref GetVehicle(), out _, out load, out capacity);

        public static IEnumerable<Vehicle> GetIf(System.Func<Vehicle, bool> filter)
        {
            return Enumerable.Range(1, manager.m_vehicleCount)
                    .Select(i => Of(VehicleID._FromIndex((ushort) i)) as Vehicle)
                    .Where(v => v is Vehicle && filter(v));
        }

        internal static Vehicle _Of(VehicleID id)
        {
            var ai = _GetVehicle(GetHeadVehicleIDof(id)).Info.m_vehicleAI;
            switch (ai) {
            case BusAI busAi_________________: return new TransitVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_BUS"));
            case TramAI tramAi_______________: return new TransitVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_TRAM"));
            case MetroTrainAI metroTrainAi___: return new TransitVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_METRO"));
            case PassengerTrainAI pTrainAi___: return new TransitVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_TRAIN"));
            case PassengerPlaneAI pPlaneAi___: return new TransitVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_FLIGHT"));
            case PassengerBlimpAI pBlimpAi___: return new TransitVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_BLIMP"));
            case CableCarAI cableCarAi_______: return new TransitVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_GONDOLA"));
            case TrolleybusAI trolleybusAi___: return new TransitVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_TROLLEYBUS"));
            case PassengerFerryAI pFerryAi___: return new TransitVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_FERRY"));
            case PassengerShipAI pShipAi_____: return new TransitVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_SHIP"));
            case PassengerHelicopterAI phAi__: return new TransitVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_HELICOPTER"));

            case CargoTruckAI cargoTruckAi:
            case CargoTrainAI cargoTrainAi:
            case CargoShipAI cargoShipAi:
            case CargoPlaneAI cargoPlaneAi___: return new CargoVehicle(id);

            case AmbulanceAI ambulanceAi:
            case AmbulanceCopterAI aCopterAi_: return new ServiceVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_MEDICAL"));
            case DisasterResponseVehicleAI dr:
            case DisasterResponseCopterAI drc: return new ServiceVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_DISASTERRESPONSE"));
            case FireCopterAI fireCopterAi:
            case FireTruckAI fireTruckAi_____: return new ServiceVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_FIREFIGHTING"));
            case PoliceCopterAI pCopterAi:
            case PoliceCarAI policeCarAi_____: return new ServiceVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_POLICE"));
            case GarbageTruckAI gTruckAi_____: return new ServiceVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_GARBAGE"));
            case HearseAI hearseAi___________: return new ServiceVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_DEATHCARE"));
            case MaintenanceTruckAI mTruckAi_:
            case ParkMaintenanceVehicleAI pm_: return new Maintenance(id);
            case PostVanAI postVanAi_________: return new ServiceVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_POSTAL"));
            case SnowTruckAI snowTruckAi_____: return new ServiceVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_SNOWPLOWING"));
            case WaterTruckAI waterTruckAi___: return new ServiceVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_WATERPUMPING"));
            case TaxiAI taxiAi_______________: return new Taxi(id);

            case PrivatePlaneAI pPlaneAi:
            case PassengerCarAI pCarAi_______: return new PersonalVehicle(id);
            case BicycleAI bicycleAi_________: return new Bicycle(id);

            case BalloonAI balloonAi:
            case FishingBoatAI fishingBoatAi_: return new MissionVehicle(id);
            //Fix that the bank service vehicle in the Financial Districts DLC cannot enter FPScamera
            case BankVanAI bankvanAi_________: return new ServiceVehicle(id, Ctransl.Translate("VEHICLE_AITYPE_BANK"));
            default:
                Log.Warn($"Vehicle(ID:{id} of type [{ai.GetType().Name}] is not recognized.");
                return null;
            }
        }

        protected ref global::Vehicle GetVehicle() => ref _GetVehicle(id);

        protected Vehicle(VehicleID id) : base(id)
        {
            _vid = id._index;
        }
        private static ref global::Vehicle _GetVehicle(VehicleID id)
            => ref manager.m_vehicles.m_buffer[id._index];
        protected bool _Is(global::Vehicle.Flags flags) => (GetVehicle().m_flags & flags) != 0;
        protected bool _IsOfService(ItemClass.Service service)
            => GetVehicle().Info.GetService() == service;

        protected VehicleInfo _VInfo => GetVehicle().Info;
        protected VehicleAI _VAI => _VInfo.m_vehicleAI;

        protected ushort _vid;


        private static readonly VehicleManager manager = VehicleManager.instance;
    }
}
