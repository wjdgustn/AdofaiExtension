using HarmonyLib;
using UnityEngine;

namespace AdofaiExtension.MainPatch {
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

                scnEditor.instance.SwitchToEditMode();
            }
        }
    }
}