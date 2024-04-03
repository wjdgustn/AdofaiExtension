using HarmonyLib;
using System;
using UnityEngine;

namespace AdofaiExtension.MainPatch {
	[HarmonyPatch(typeof(scnSplash), "Start")]
	internal static class onSplash {
		internal static bool Prefix() {
			var arguments = Environment.GetCommandLineArgs();
			Debug.Log(arguments);
			if (Main.LoadedLevel || arguments.Length < 2) return true;

			ADOBase.GoToLevelSelect();
			return false;
		}
	}
	
    [HarmonyPatch(typeof(scnLevelSelect), "Awake")]
    internal static class onStart {
        internal static void Postfix() {
	        scrController.instance.UpdateListenerVolume();
            Main.OpenLevel();
        }
    }

    [HarmonyPatch(typeof(scnEditor), "Start")]
    internal static class onEditor {
        internal static void Postfix() {
            if (Main.LevelPath != null) {
                Main.Mod.Logger.Log("loading editor level");

                scnEditor.instance.StartCoroutine("OpenLevelCo", Main.LevelPath);
                Main.LevelPath = null;
                Main.LoadedLevel = true;
            }
        }
    }
}