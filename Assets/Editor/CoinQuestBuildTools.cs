using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CoinQuest.EditorTools
{
    public static class CoinQuestBuildTools
    {
        [MenuItem("Coin Quest/Build Windows x64")]
        public static void BuildWindows()
        {
            const string output = "Build/Windows/CoinQuest.exe";
            Directory.CreateDirectory("Build/Windows");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/SampleScene.unity" },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("Coin Quest Windows build failed: " + report.summary.result);
                return;
            }
            Debug.Log("Coin Quest Windows build created at " + Path.GetFullPath(output));
        }
    }
}
