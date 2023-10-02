namespace CSkyL.Game.Object
{
    using CSkyL.Game.ID;

    public class CargoVehicle : Vehicle
    {
        public CargoVehicle(VehicleID id) : base(id) { }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            GetLoadAndCapacity(out int load, out int capacity);
            details["负载量"] = capacity > 0 ? ((float)load / capacity).ToString("P1")
                                         : "(无效)";
        }
    }

    public class TransitVehicle : Vehicle
    {
        public TransitVehicle(VehicleID id, string transitType) : base(id)
        { _transitType = transitType; }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            details["线路"] = $"{_transitType}> " + (GetTransitLineID() is TransitID id ?
                                                       TransitLine.GetName(id) : "(外部)");

            GetLoadAndCapacity(out int load, out int capacity);
            details["乘客数"] = $"{load,4} /{capacity,4}";
        }

        private readonly string _transitType;
    }

    public class ServiceVehicle : Vehicle
    {
        public ServiceVehicle(VehicleID id, string typeName) : base(id) { _typeName = typeName; }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            details["服务"] = _typeName;

            GetLoadAndCapacity(out int load, out int capacity);
            if (capacity > 0) details["负载量"] = ((float)load / capacity).ToString("P1");
        }
        private readonly string _typeName;
    }
//Fix the mechanism that replaces 'Load' with 'Work Shift' ('负载量' 替换为 '轮班') does not work
    public class Taxi : ServiceVehicle
    {
        public Taxi(VehicleID id) : base(id, "出租车") { }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            base._MoreDetails(ref details);
            int index = details.FindIndex((_info) => _info.field == "负载量");
            if (index >= 0)
            {

                details[index] = new Utils.Info("轮班", details[index].text);
            }
        }
    }
    //Add the same mechanism to replace 'Load' with 'Work shift' for Maintenance cars
    public class Maintenance : ServiceVehicle
    {
        public Maintenance(VehicleID id) : base(id, "养护") { }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            base._MoreDetails(ref details);
            int index = details.FindIndex((_info) => _info.field == "负载量");
            if (index >= 0)
            {

                details[index] = new Utils.Info("轮班", details[index].text);
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
