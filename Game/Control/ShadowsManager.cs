using System;
using System.Collections;

namespace CSkyL.Game.Control
{
    public class ShadowsManager
    {
        public static IEnumerator ToggleShadowsOptimization(bool status)
        {
            try {
                Log.Msg("-- Setting shadows distance");
                if (status) {
                    beforeOptimization = UnityEngine.QualitySettings.shadowDistance;
                    UnityEngine.QualitySettings.shadowDistance = Optimization;
                }
                else {
                    UnityEngine.QualitySettings.shadowDistance = beforeOptimization;
                }
            }

            catch (Exception e) {
                Log.Err("Unrecognized Error:" + e);
            }

            yield break;
        }
        private static float beforeOptimization;
        private const float Optimization = 512f;
    }

}
