using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorDecor
{
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
    internal class PlayerPatch
    {
        static void Postfix(ref PlayerControllerB __instance)
        {
            if (__instance == GameNetworkManager.Instance.localPlayerController)
            {
                PlayerControllerB player = __instance;

                player.thisPlayerModel.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                //V1
                //player.thisPlayerModel.gameObject.layer = 29;
                //player.thisPlayerModelArms.gameObject.layer = 3;

                //V2
                //player.thisPlayerModel.gameObject.layer = 29;
                //player.thisPlayerModelArms.gameObject.layer = 30;
                //player.gameplayCamera.cullingMask = 1094391807;

                //V3
                player.thisPlayerModel.gameObject.layer = 3;
                player.thisPlayerModelArms.gameObject.layer = 29;
                player.gameplayCamera.cullingMask = 557520887;
            }
        }
    }
}
