using BepInEx.Bootstrap;
using GameNetcodeStuff;
using HarmonyLib;
using MoreCompany.Cosmetics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MirrorDecor
{
    [HarmonyPatch(typeof(HUDManager), "AddPlayerChatMessageClientRpc")]
    class CosmeticPatch
    {
        static void Postfix()
        {
            if (Chainloader.PluginInfos.ContainsKey("me.swipez.melonloader.morecompany"))
            {
                CosmeticApplication app = GameObject.FindObjectOfType<CosmeticApplication>();

                if (CosmeticRegistry.locallySelectedCosmetics.Count > 0 && app.spawnedCosmetics.Count <= 0)
                {
                    foreach (string id in CosmeticRegistry.locallySelectedCosmetics)
                    {
                        app.ApplyCosmetic(id, true);
                    }

                    foreach (CosmeticInstance instance in app.spawnedCosmetics)
                    {
                        instance.transform.localScale *= 0.38f;
                        SetAllChildrenLayer(instance.transform, 29);
                    }
                }
            }
        }

        private static void SetAllChildrenLayer(Transform transform, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            transform.gameObject.layer = layer;
            foreach (Transform child in transform)
            {
                SetAllChildrenLayer(child, layerName);
            }
        }

        private static void SetAllChildrenLayer(Transform transform, int layer)
        {
            transform.gameObject.layer = layer;
            foreach (Transform child in transform)
            {
                SetAllChildrenLayer(child, layer);
            }
        }
    }
}
