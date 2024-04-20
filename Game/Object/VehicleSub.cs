namespace CSkyL.Game.Object
{
    using CSkyL.Game.ID;
    using Ctransl = CSkyL.Translation.Translations;
    public class CargoVehicle : Vehicle
    {
        public CargoVehicle(VehicleID id) : base(id) { }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            GetLoadAndCapacity(out int load, out int capacity);
            details[Ctransl.Translate("INFO_VEHICLE_LOAD")] = capacity > 0 ? ((float)load / capacity).ToString("P1")
                                         : Ctransl.Translate("INVALID");
        }
    }

    public class TransitVehicle : Vehicle
    {
        public TransitVehicle(VehicleID id, string transitType) : base(id)
        { _transitType = transitType; }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            details[Ctransl.Translate("INFO_VEHICLE_PUBLICTRANSIT_TRANSIT")] = $"{_transitType}> " + (GetTransitLineID() is TransitID id ?
                                        TransitLine.GetName(id) : Ctransl.Translate("INFO_VEHICLE_PUBLICTRANSIT_IRREGULAR"));

            GetLoadAndCapacity(out int load, out int capacity);
            details[Ctransl.Translate("INFO_VEHICLE_PUBLICTRANSIT_PASSENGER")] = $"{load,4} /{capacity,4}";
        }

        private readonly string _transitType;
    }

    public class ServiceVehicle : Vehicle
    {
        public ServiceVehicle(VehicleID id, string typeName) : base(id) { _typeName = typeName; }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            details[Ctransl.Translate("INFO_VEHICLE_SERVICE")] = _typeName;

            GetLoadAndCapacity(out int load, out int capacity);
            if (capacity > 0) details[Ctransl.Translate("INFO_VEHICLE_LOAD")] = ((float)load / capacity).ToString("P1");
        }
        private readonly string _typeName;
    }
//Fix the mechanism that replaces 'Load' with 'Work Shift' ('负载量' 替换为 '轮班') does not work
    public class Taxi : ServiceVehicle
    {
        public Taxi(VehicleID id) : base(id, Ctransl.Translate("VEHICLE_AITYPE_TAXI")) { }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            base._MoreDetails(ref details);
            int index = details.FindIndex((_info) => _info.field == Ctransl.Translate("INFO_VEHICLE_LOAD"));
            if (index >= 0)
            {

                details[index] = new Utils.Info(Ctransl.Translate("INFO_VEHICLE_WORKSHIFT"), details[index].text);
            }
        }
    }
    //Add the same mechanism to replace 'Load' with 'Work shift' for Maintenance cars
    public class Maintenance : ServiceVehicle
    {
        public Maintenance(VehicleID id) : base(id, Ctransl.Translate("VEHICLE_AITYPE_MAINTENANCE")) { }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            base._MoreDetails(ref details);
            int index = details.FindIndex((_info) => _info.field == Ctransl.Translate("INFO_VEHICLE_LOAD"));
            if (index >= 0)
            {

                details[index] = new Utils.Info(Ctransl.Translate("INFO_VEHICLE_WORKSHIFT"), details[index].text);
            }
        }
    }
    public class PersonalVehicle : Vehicle
    {
        public PersonalVehicle(VehicleID id) : base(id) { }
        public HumanID GetDriverID() => GetOwnerID() as HumanID;
    }
    public class Bicycle : PersonalVehicle
    {
        public Bicycle(VehicleID id) : base(id) { }
        public override string GetStatus()
            => GetOwnerID() is HumanID hid ?
                   (Of(hid) as Pedestrian)?.GetStatus() : null;
    }

    public class MissionVehicle : Vehicle
    {
        public MissionVehicle(VehicleID id) : base(id) { }
    }
}
