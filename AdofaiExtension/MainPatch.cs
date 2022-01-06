using ADOFAI;
using HarmonyLib;
using UnityEngine;

namespace AdofaiExtension.MainPatch {
    [HarmonyPatch(typeof(scnLevelSelect), "Awake")]
    internal static class onStart {
        internal static void Postfix() {
            Main.OpenLevel();
        }
    }

    [HarmonyPatch(typeof(scnEditor), "Start")]
    internal static class onEditor {
        internal static void Postfix() {
            if (Main.LevelPath != null) {
                Main.Mod.Logger.Log("load editor level");

                // var OpenLevelCo = scnEditor.instance.GetType().GetMethod("OpenLevelCo", BindingFlags.NonPublic | BindingFlags.Instance);
                // scnEditor.instance.StartCoroutine(OpenLevelCo.Invoke(scnEditor.instance, new object[] {Main.LevelPath}) as IEnumerator);
                
                scrController.instance.LoadCustomLevel(Main.LevelPath);

                Main.LevelPath = null;
                Main.LoadedLevel = true;
                Main.AfterLoadSetting = false;
            }
        }
    }

    [HarmonyPatch(typeof(CustomLevel), "Play")]
    internal static class afterLoad {
        internal static void Postfix() {
            if (!Main.AfterLoadSetting) {
                Main.Mod.Logger.Log("fix editor");
                Main.AfterLoadSetting = true;
                // scnEditor.instance.editor.DecideInspectorTabsAtSelected();
                // scnEditor.instance.settingsPanel.selectedEventType = LevelEventType.SongSettings;
                scnEditor.instance.settingsPanel.ShowPanel(LevelEventType.SongSettings);

                // var SelectFirstFloor = scnEditor.instance.GetType()
                //     .GetMethod("SelectFirstFloor", BindingFlags.NonPublic | BindingFlags.Instance);
                // SelectFirstFloor.Invoke(scnEditor.instance, new object[] {});

                scnEditor.instance.SwitchToEditMode();
                GCS.customLevelPaths = null;
                GCS.difficulty = Difficulty.Strict;
            }
        }
    }
}