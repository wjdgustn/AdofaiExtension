using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace AdofaiExtension {
    #if DEBUG
    [EnableReloading]
    #endif

    internal static class Main {
        internal static UnityModManager.ModEntry Mod;
        private static Harmony _harmony;
        internal static bool IsEnabled { get; private set; }
        internal static bool LoadedLevel = false;
        internal static string LevelPath;

        private static readonly string[] SpecialActions = { "openScene" };

        private static void Load(UnityModManager.ModEntry modEntry) {
            Mod = modEntry;
            Mod.OnToggle = OnToggle;

            #if DEBUG
            Mod.OnUnload = Stop;
            #endif
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            IsEnabled = value;

            if (value) Start();
            else Stop(modEntry);

            return true;
        }
        
#if DEBUG
        private static FileSystemWatcher watcher;
        private static void OnDllChanged(object source, FileSystemEventArgs e) {
	        Mod.Logger.Log($"{Mod.Info.Id} mod change detected! Reloading...");
	        watcher.Changed -= OnDllChanged;
	        watcher.Dispose();
	        watcher = null;
	        Mod.GetType().GetMethod("Reload", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(Mod, null);
        }
#endif
        
        private static void Start() {
            _harmony = new Harmony(Mod.Info.Id);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            
#if DEBUG
            watcher = new FileSystemWatcher(Mod.Path);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.dll";

            watcher.Changed += OnDllChanged;

            watcher.EnableRaisingEvents = true;
#endif

            var appidPath = $"{Path.GetFullPath(".")}\\steam_appid.txt";
            if (!File.Exists(appidPath)) {
                using(StreamWriter sw = File.CreateText(appidPath)) sw.WriteLine("977950");
                Mod.Logger.Log("created steam_appid.txt");
            }

            var runnerPath = $"{Path.GetFullPath(".")}\\runner.bat";
            if (!File.Exists(runnerPath)) {
                using (StreamWriter sw = File.CreateText(runnerPath)) {
                    sw.WriteLine($"cd {Path.GetFullPath(".")}");
                    sw.WriteLine($"start /d \"{Path.GetFullPath(".")}\" /b \"\" \"{Path.GetFullPath(".")}\\{Application.productName}.exe\" %1");
                }
                Mod.Logger.Log("created runner.bat");
            }

            var result = FileAssociations.SetAssociation(".adofai", "adofai", $"{Application.productName} Level",
                $"{Path.GetFullPath(".")}\\runner.bat", $"{Path.GetFullPath(".")}\\{Application.productName}.exe");
            Mod.Logger.Log(result ? "registry add success" : "registry add failed");
        }

        private static bool Stop(UnityModManager.ModEntry modEntry) {
            _harmony.UnpatchAll(Mod.Info.Id);
            #if RELEASE
            _harmony = null;
            #endif
            
            var runnerPath = $"{Path.GetFullPath(".")}\\runner.bat";
            if(File.Exists(runnerPath)) File.Delete(runnerPath);
            
            FileAssociations.RemoveAssociation(".adofai", "adofai");

            return true;
        }

        public static void OpenLevel(string path = null) {
            var arguments = Environment.GetCommandLineArgs();
            if (LoadedLevel || arguments.Length < 2) return;

            if (!SpecialActions.Contains(arguments[1])) {
                Mod.Logger.Log("loading clicked file...");
                
                path = string.Join(" ", arguments.Skip(1));

                Mod.Logger.Log(path);

                if (!File.Exists(path)) return;

                LevelPath = path;

                GCS.sceneToLoad = GCNS.sceneEditor;
                scrController.instance.StartLoadingScene();

                Mod.Logger.Log("loaded editor scene");
            }
            if(arguments[1] == "openScene") {
                var name = string.Join(" ", arguments.Skip(2));
                Mod.Logger.Log($"Loading {name} scene...");

                SceneManager.LoadScene(name);
                LoadedLevel = true;

                Mod.Logger.Log("loaded scene");
            }
            else Mod.Logger.Log("file not clicked");
        }
    }
}