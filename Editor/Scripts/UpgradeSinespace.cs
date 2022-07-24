namespace TWIEditor.Sinespace
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Unity.EditorCoroutines.Editor;
    using UnityEditor;
    using UnityEngine;

    using Stopwatch = System.Diagnostics.Stopwatch;

    public static class UpgradeSinespace
    {
        private static EditorCoroutine coroutine;
        private const long YIELD_RATE = 40;
        private const string PROGRESS_TITLE = "Upgrading Sinespace... {0:N0}/{1:N0} ({2:P0})";
        private const string PROGRESS_INFO = "Upgrading '{0}', please wait...";

        [MenuItem("TWI/Sinespace/Upgrade Sinespace")]
        public static void Upgrade()
        {
            if (coroutine == null) coroutine = EditorCoroutineUtility.StartCoroutineOwnerless(StartUpgrade());
        }

        private static IEnumerator StartUpgrade()
        {
            string path = EditorUtilities.GetCallerFilePath();
            path = EditorUtilities.GetFullPath(path);
            path = Path.GetDirectoryName(path);
            path += "/../../Upgrades/";

            Stopwatch stopwatch = new Stopwatch();
            DirectoryInfo directory  = new DirectoryInfo(path);
            IEnumerable<FileInfo> files = directory.EnumerateFiles("*.*", SearchOption.AllDirectories);
            files = files.Where(f => f.Extension != ".meta");
            stopwatch.Start();

            float index = 0;
            int count = files.Count();

            foreach (var file in directory.GetFiles("*.*", SearchOption.AllDirectories))
            {
                index++;
                path = file.FullName;
                switch (file.Extension)
                {
                    case ".txt": path = Path.ChangeExtension(path, ".cs"); break;
                    case ".json":
                        break;
                    default: throw new NotSupportedException("Extension '" + file.Extension + "' not supported");

                }

                if (stopwatch.ElapsedMilliseconds > YIELD_RATE)
                {
                    title = string.Format(PROGRESS_TITLE, index, count, index / count);
                    info = string.Format(PROGRESS_INFO, Path.GetFileName(path));

                    if (EditorUtility.DisplayCancelableProgressBar(title, info, index / count) && StopUpgrade()) yield break;

                    yield return null;
                    stopwatch.Reset();
                }

                path = "Assets/" + path.Substring(directory.FullName.Length);
                path = path.Replace('\\', '/');
                file.CopyTo(path, true);
            }

            AssetDatabase.Refresh();
            coroutine = null;
        }
        private static bool StopUpgrade()
        {
            return false;
        }
    }
}