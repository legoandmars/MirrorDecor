using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Extras;
using LethalLib.Modules;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

namespace MirrorDecor
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("evaisa.lethallib", "0.3.1")]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "quackandcheese.mirrordecor";
        public const string ModName = "MirrorDecor";
        public const string ModVersion = "1.1.3";

        public static AssetBundle Bundle;

        public static ConfigFile config;

        public static BepInEx.Logging.ManualLogSource logger;

        public static List<CustomUnlockable> customUnlockables;


        private void Awake()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            Bundle = QuickLoadAssetBundle("mirror.assets");
            logger = Logger;
            config = Config;

            Harmony harmony = new Harmony(ModGUID);
            harmony.PatchAll();

            MirrorDecor.Config.Load();
            RegisterItems();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void RegisterItems()
        {
            customUnlockables = new List<CustomUnlockable>
            {
                CustomUnlockable.Add("Mirror", "Assets/Mirror/Unlockables/Mirror/Mirror.asset", "Assets/Mirror/Unlockables/Mirror/MirrorInfo.asset", null, MirrorDecor.Config.mirrorPrice.Value, MirrorDecor.Config.mirrorEnabled.Value)
            };

            UnlockableItem mirror = Bundle.LoadAsset<UnlockableItemDef>("Assets/Mirror/Unlockables/Mirror/Mirror.asset").unlockable;
            mirror.alwaysInStock = MirrorDecor.Config.alwaysAvailable.Value;

            foreach (CustomUnlockable customUnlockable in customUnlockables)
            {
                if (customUnlockable.enabled)
                {
                    UnlockableItem unlockable = Bundle.LoadAsset<UnlockableItemDef>(customUnlockable.unlockablePath).unlockable;
                    if (unlockable.prefabObject != null)
                    {
                        NetworkPrefabs.RegisterNetworkPrefab(unlockable.prefabObject);
                    }
                    TerminalNode terminalNode = null;
                    if (customUnlockable.infoPath != null)
                    {
                        terminalNode = Bundle.LoadAsset<TerminalNode>(customUnlockable.infoPath);
                    }
                    Unlockables.RegisterUnlockable(unlockable, StoreType.Decor, null, null, terminalNode, customUnlockable.unlockCost);
                }
            }
        }

        #region HELPERS
        public static T FindAsset<T>(string name) where T : UnityEngine.Object
        {
            return Resources.FindObjectsOfTypeAll<T>().ToList().Find(x => x.name == name);
        }

        public static AssetBundle QuickLoadAssetBundle(string assetBundleName)
        {
            string AssetBundlePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), assetBundleName);

            return AssetBundle.LoadFromFile(AssetBundlePath);
        }
        #endregion

        #region MOD SYNC
        public void sendModInfo()
        {
            foreach (var plugin in Chainloader.PluginInfos)
            {
                if (plugin.Value.Metadata.GUID.Contains("ModSync"))
                {
                    try
                    {
                        List<string> list = new List<string>
                        {
                            "quackandcheese",
                            "MirrorDecor"
                        };
                        plugin.Value.Instance.BroadcastMessage("getModInfo", list, UnityEngine.SendMessageOptions.DontRequireReceiver);
                    }
                    catch (Exception e)
                    {
                        // ignore mod if error, removing dependency
                        logger.LogInfo($"Failed to send info to ModSync, go yell at Minx");
                    }
                    break;
                }

            }
        }
        #endregion
    }
}