// File: Pokemon Unity/Assets/Editor/CI/BuildAndroid.cs
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace CI
{
    public static class BuildAndroid
    {
        // Actions の customParameters から -executeMethod CI.BuildAndroid.PerformBuild で呼ばれる
        public static void PerformBuild()
        {
            // 1) ビルド対象シーンを自動収集（Build Settings に入っていればそれを優先、空なら Assets 内の .unity を全部）
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                scenes = AssetDatabase.FindAssets("t:Scene")
                    .Select(g => AssetDatabase.GUIDToAssetPath(g))
                    .Where(p => p.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p)
                    .ToArray();

                if (scenes.Length == 0)
                    throw new Exception("No scenes found. Please add at least one scene.");
            }

            // 2) 出力先を Actions の buildPath と一致させる
            var buildRoot = Path.GetFullPath("build/android");
            Directory.CreateDirectory(buildRoot);
            var locationPathName = Path.Combine(buildRoot, "PokemonUnity.apk");

            // 3) Android 向けオプション（Mono/IL2CPP などは必要に応じて調整）
            var options = BuildOptions.None;

            var report = BuildPipeline.BuildPlayer(scenes, locationPathName, BuildTarget.Android, options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"Android build failed: {report.summary.result} / {report.summary.totalErrors} errors");
            }

            Console.WriteLine($"Android build succeeded: {locationPathName}");
        }
    }
}
#endif
