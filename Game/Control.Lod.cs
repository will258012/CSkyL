using ColossalFramework;
using System;
using System.Collections;

namespace CSkyL.Game
{
    public static partial class Control
    {
        public static class LodManager
        {
            public static IEnumerator ToggleLODOptimization(bool status)
            {
                try {
                    if (status) {
                        LodConfig.SaveLodConfig();
                        LodConfig.ActiveConfig = LodConfig.Optimized;
                    }
                    else {
                        LodConfig.ActiveConfig = LodConfig.Saved;
                    }
                    Log.Msg("-- Refreshing LOD");
                    RefreshLODs();
                    //UpdateShadowDistance(LodConfig.ActiveConfig.MaxShadowDistance);
                    if (!status) LodConfig.ActiveConfig = null;
                }

                catch (Exception e) {
                    Log.Err("Unrecognized Error:" + e);
                }

                yield break;
            }
            private static void RefreshLODs()
            {
                refreshLods<TreeInfo>();
                //UpdateRenderGroups(TreeManager.instance.m_treeLayer);
                refreshLods<PropInfo>();
                //UpdateRenderGroups(LayerMask.NameToLayer("Props"));
                refreshLods<BuildingInfo>();
                refreshLods<BuildingInfoSub>();
                //UpdateRenderGroups(LayerMask.NameToLayer("Buildings"));
                refreshLods<NetInfo>();
                //UpdateRenderGroups(LayerMask.NameToLayer("Road"));
                refreshLods<VehicleInfo>();
                refreshLods<CitizenInfo>();
            }

            private static void refreshLods<T>()
                where T : PrefabInfo
            {
                // Iterate through all loaded prefabs of the specified type.
                uint prefabCount = (uint) PrefabCollection<T>.LoadedCount();
                for (uint i = 0; i < prefabCount; ++i) {
                    // Refresh LODs for all valid prefabs.
                    PrefabInfo prefab = PrefabCollection<T>.GetLoaded(i);
                    if (prefab) {
                        prefab.RefreshLevelOfDetail();
                    }
                }

                // Also refresh any edit prefab.
                if (ToolsModifierControl.toolController && ToolsModifierControl.toolController.m_editPrefabInfo is T) {
                    ToolsModifierControl.toolController.m_editPrefabInfo.RefreshLevelOfDetail();
                }
            }
            /*
            private static void UpdateShadowDistance(float ShadowDistance)
            {
                var mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                if (mainCamera) {
                    var cameraController = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<CameraController>();
                    if (cameraController != null) {
                        cameraController.m_maxShadowDistance = ShadowDistance;
                    }
                }
            }
            */
            /*
            private static void UpdateRenderGroups(int layer)
            {
                // Local reference.
                RenderManager renderManager = Singleton<RenderManager>.instance;

                // Iterate through all render groups.
                foreach (RenderGroup renderGroup in renderManager.m_groups) {
                    // Null check.
                    if (renderGroup != null) {
                        // Refresh this group.
                        renderGroup.SetLayerDataDirty(layer);
                        renderGroup.UpdateMeshData();
                    }
                }
            }
            */
            internal class LodConfig
            {
                internal LodConfig(float citizenLodDistance, float treeLodDistance, float propLodDistance, float decalPropFadeDistance, float buildingLodDistance, float networkLodDistance, float vehicleLodDistance/*, float maxShadowDistance*/)
                {
                    CitizenLodDistance = citizenLodDistance;
                    TreeLodDistance = treeLodDistance;
                    PropLodDistance = propLodDistance;
                    DecalPropFadeDistance = decalPropFadeDistance;
                    BuildingLodDistance = buildingLodDistance;
                    NetworkLodDistance = networkLodDistance;
                    VehicleLodDistance = vehicleLodDistance;
                    //MaxShadowDistance = maxShadowDistance;
                }

                internal float CitizenLodDistance { get; set; }
                internal float TreeLodDistance { get; set; }
                internal float PropLodDistance { get; set; }
                internal float DecalPropFadeDistance { get; set; }
                internal float BuildingLodDistance { get; set; }
                internal float NetworkLodDistance { get; set; }
                internal float VehicleLodDistance { get; set; }
                //internal float MaxShadowDistance { get; set; }

                internal static LodConfig Saved => savedconfig;

                internal static LodConfig Optimized =>
                    new LodConfig(100f,
                        150f,
                        150f,
                        150f,
                        250f,
                        170f,
                        150f/*,
                        200f*/);

                internal static LodConfig ActiveConfig = null;
                private static LodConfig savedconfig;

                private static float GetLodDistance<T>() where T : PrefabInfo
                {
                    var prefab = PrefabCollection<T>.GetLoaded((uint) (PrefabCollection<T>.LoadedCount() - 1));
                    switch (prefab) {
                    case CitizenInfo info:
                        return info.m_lodRenderDistance;
                    case TreeInfo info:
                        return info.m_lodRenderDistance;
                    default:
                        throw new InvalidOperationException($"Unsupported PrefabInfo type: {typeof(T)}");
                    }
                }
                private static float GetLodDistance<T>(T manager, bool IsPropManager_MaxRenderDistance = false) where T : ISimulationManager
                {
                    switch (manager) {
                    case BuildingManager buildingManager:
                        return PrefabCollection<BuildingInfo>.GetPrefab((uint) buildingManager.m_infoCount - 1).m_minLodDistance;
                    case PropManager propManager when !IsPropManager_MaxRenderDistance:
                        return PrefabCollection<PropInfo>.GetPrefab((uint) propManager.m_infoCount - 1).m_lodRenderDistance;
                    case PropManager propManager when IsPropManager_MaxRenderDistance:
                        return PrefabCollection<PropInfo>.GetPrefab((uint) propManager.m_infoCount - 1).m_maxRenderDistance;
                    case NetManager netManager:
                        return PrefabCollection<NetInfo>.GetPrefab((uint) netManager.m_infoCount - 1).m_segments[PrefabCollection<NetInfo>.GetPrefab((uint) netManager.m_infoCount - 1).m_segments.Length - 1].m_lodRenderDistance;
                    case VehicleManager vehicleManager:
                        return PrefabCollection<VehicleInfo>.GetPrefab((uint) vehicleManager.m_infoCount - 1).m_lodRenderDistance;
                    default:
                        throw new InvalidOperationException($"Unsupported SimulationManager type: {typeof(T)}");
                    }
                }

                internal static void SaveLodConfig()
                {
                    //var mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                    //var cameraController = mainCamera.GetComponent<CameraController>();
                    try {
                        savedconfig = new LodConfig(
                            citizenLodDistance: GetLodDistance<CitizenInfo>(),
                            treeLodDistance: GetLodDistance<TreeInfo>(),
                            propLodDistance: GetLodDistance(Singleton<PropManager>.instance),
                            decalPropFadeDistance: GetLodDistance(Singleton<PropManager>.instance, true),
                            buildingLodDistance: GetLodDistance(Singleton<BuildingManager>.instance),
                            networkLodDistance: GetLodDistance(Singleton<NetManager>.instance),
                            vehicleLodDistance: GetLodDistance(Singleton<VehicleManager>.instance) /*,
                           maxShadowDistance: cameraController.m_maxShadowDistance*/);

                        Log.Msg($"Saved LOD Config:\n" +
                                $"  CitizenLodDistance = {savedconfig.CitizenLodDistance}\n" +
                                $"  TreeLodDistance = {savedconfig.TreeLodDistance}\n" +
                                $"  PropLodDistance = {savedconfig.PropLodDistance}\n" +
                                $"  DecalPropFadeDistance = {savedconfig.DecalPropFadeDistance}\n" +
                                $"  BuildingLodDistance = {savedconfig.BuildingLodDistance}\n" +
                                $"  NetworkLodDistance = {savedconfig.NetworkLodDistance}\n" +
                                $"  VehicleLodDistance = {savedconfig.VehicleLodDistance}\n"/* +
                                $"  MaxShadowDistance = {savedconfig.MaxShadowDistance}\n"*/);

                    }
                    catch (Exception e) {
                        Log.Err(e.ToString());
                    }

                }

            }

        }
    }

}
