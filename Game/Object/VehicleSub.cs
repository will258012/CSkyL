namespace CSkyL.Game.Object
{
    using CSkyL.Game.ID;

    public class CargoVehicle : Vehicle
    {
        public CargoVehicle(VehicleID id) : base(id) { }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            GetLoadAndCapacity(out int load, out int capacity);
            details["已装载"] = capacity > 0 ? ((float) load / capacity).ToString("P1")
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
            if (capacity > 0) details["已装载"] = ((float) load / capacity).ToString("P1");
        }
        private readonly string _typeName;
    }
    public class Taxi : ServiceVehicle
    {
        public Taxi(VehicleID id) : base(id, "出租车") { }

        public override void _MoreDetails(ref Utils.Infos details)
        {
            base._MoreDetails(ref details);

            if (details.Find((_info) => _info.field == "已装载") is Utils.Info info)
                info = new Utils.Info("轮班", info.text);
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
