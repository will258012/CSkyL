namespace CSkyL.Game.Object
{
    using CSkyL.Game.ID;
    using CSkyL.Game.Utils;
    using CSkyL.Transform;
    using System.Collections.Generic;
    using System.Linq;
    using Ctransl = CSkyL.Translation.Translations;
    public class Human : Object<HumanID>
    {
        public override string Name => manager.GetCitizenName(id.Value);

        public bool IsTourist => _Is(Citizen.Flags.Tourist);
        public bool IsStudent => _Is(Citizen.Flags.Student);
        public VehicleID RiddenVehicleID => VehicleID._FromIndex(_citizen.m_vehicle);
        public BuildingID WorkBuildingID => BuildingID._FromIndex(_citizen.m_workBuilding);
        public BuildingID HomeBuildingID => BuildingID._FromIndex(_citizen.m_homeBuilding);

        protected bool _Is(Citizen.Flags flags) => (_citizen.m_flags & flags) != 0;

        internal static Human _Of(HumanID id)
            => Of(_GetPedestrianID(id)) is Pedestrian ped ? ped : new Human(id);

        protected Human(HumanID id) : base(id) => _citizen = _GetCitizen(id);

        protected readonly Citizen _citizen;

        private static Citizen _GetCitizen(HumanID hid)
            => manager.m_citizens.m_buffer[hid.Value];
        private static PedestrianID _GetPedestrianID(HumanID hid)
            => PedestrianID._FromIndex(_GetCitizen(hid).m_instance);
        private static readonly CitizenManager manager = CitizenManager.instance;
    }

    public class Pedestrian : Human, IObjectToFollow
    {
        public override string Name => manager.GetCitizenName(id.Value);

        public bool IsWaitingTransit => _Is(CitizenInstance.Flags.WaitingTransport);
        public bool IsEnteringVehicle => _Is(CitizenInstance.Flags.EnteringVehicle);
        public bool IsHangingAround => _Is(CitizenInstance.Flags.HangAround);

        public PathData GetPathData()
        {
            ref CitizenInstance citizenInstance = ref GetCitizenInstance();
            return new PathData(
                citizenInstance.m_path,
                citizenInstance.m_lastPathOffset,
                citizenInstance.m_pathPositionIndex,
                citizenInstance.GetLastFramePosition(),
                citizenInstance.GetLastFrameData().m_velocity
            );
        }

        public Position GetTargetPos(int index)
            => Position._FromVec(GetCitizenInstance().m_targetPos);
        public byte GetLastFrame() => GetCitizenInstance().m_lastFrame;
        public uint GetTargetFrame()
        {
            // see decompiler CitizenInstance.GetSmoothPosition()
            uint i = (uint) (_pid >> 12); // = (_pid << 4 ) *65536
            return SimulationManager.instance.m_referenceFrameIndex - i;
        }

        public Positioning GetPositioning()
        {
            ref var human = ref GetCitizenInstance();
            human.GetSmoothPosition(_pid, out var position, out var rotation);
            return new Positioning(Position._FromVec(position), Angle._FromQuat(rotation));
        }
        public float GetSpeed() => GetCitizenInstance().GetLastFrameData().m_velocity.magnitude;
        public string GetStatus()
        {
            var _c = _citizen;
            var status = GetCitizenInstance().Info.m_citizenAI.GetLocalizedStatus(
                                pedestrianID.Value, ref _c, out var implID);
            switch (ObjectID._FromIID(implID)) {
            case BuildingID bid: status += Building.GetName(bid); break;
            case NodeID nid:
                if (Node.GetTransitLineID(nid) is TransitID tid)
                    status += TransitLine.GetName(tid);
                break;
            }
            return status;
        }
        public GameUtil.Infos GetInfos()
        {
            GameUtil.Infos details = new GameUtil.Infos();

            string occupation;
            if (IsTourist) occupation = Ctransl.Translate("INFO_HUMAN_TOURIST");
            else {
                if (Of(WorkBuildingID) is Building workBuilding) {
                    occupation = string.Format(IsStudent ? Ctransl.Translate("INFO_HUMAN_STUDENTAT") : Ctransl.Translate("INFO_HUMAN_WORKAT"), workBuilding.Name);
                }
                else {
                    occupation = Ctransl.Translate("INFO_HUMAN_UNENPLOYED");
                }

                details[Ctransl.Translate("INFO_HUMAN_HOME")] = Of(HomeBuildingID) is Building homeBuilding ?
                                      homeBuilding.Name : Ctransl.Translate("INFO_HUMAN_HOMELESS");
            }
            details[Ctransl.Translate("INFO_HUMAN_OCCUPATION")] = occupation;

            return details;
        }
        public string GetPrefabName() => GetCitizenInstance().Info.name;

        public static IEnumerable<Pedestrian> GetIf(System.Func<Pedestrian, bool> filter)
        {
            return Enumerable.Range(1, manager.m_instances.m_buffer.Length - 1)
                    .Select(i => Of(PedestrianID._FromIndex((ushort) i)) as Pedestrian)
                    .Where(p => p is Pedestrian && filter(p));
        }

        protected ref CitizenInstance GetCitizenInstance()
            => ref _GetCitizenInstance(pedestrianID);

        private static ref CitizenInstance _GetCitizenInstance(PedestrianID pid)
            => ref manager.m_instances.m_buffer[pid.Value];
        private static HumanID _GetHumanID(PedestrianID pid)
            => HumanID._FromIndex(_GetCitizenInstance(pid).m_citizen);

        internal static Pedestrian _Of(PedestrianID id)
            => _GetHumanID(id) is HumanID hid ? new Pedestrian(id, hid) : null;
        private Pedestrian(PedestrianID pid, HumanID hid) : base(hid)
        {
            pedestrianID = pid;
        }

        private bool _Is(CitizenInstance.Flags flags)
            => (GetCitizenInstance().m_flags & flags) != 0;

        public readonly PedestrianID pedestrianID;
        private ushort _pid => pedestrianID.Value;

        private static readonly CitizenManager manager = CitizenManager.instance;
    }
}
