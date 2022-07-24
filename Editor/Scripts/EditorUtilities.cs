namespace TWIEditor.Sinespace
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using UnityEditor;

    public static class EditorUtilities
    {
        private const string LIBRARY_PACKAGE_CACHE = "Library/PackageCache/";

        public static readonly IReadOnlyCollection<(string name, string path)> LocalDependencies = GetLocalDependencies();

        public static string GetCallerFilePath([CallerFilePath] string callerFilePath = default) => callerFilePath;

        public static string GetFullPath(string path)
        {
            if (path.StartsWith("%")) return Environment.ExpandEnvironmentVariables(path).Replace('\\', '/');
            else path = path.Replace('\\', '/');

            if (path.StartsWith(LIBRARY_PACKAGE_CACHE))
            {
                path = path.Substring(LIBRARY_PACKAGE_CACHE.Length);
                string package = path.Substring(0, path.IndexOf('/'));
                if (package.Contains('@')) package = package.Substring(0, package.IndexOf('@'));
                return Path.GetFullPath("Packages/" + package + path.Substring(path.IndexOf('/'))).Replace('\\', '/');
            }

            var localDependency = LocalDependencies.FirstOrDefault(d => path.StartsWith(d.path));
            if (string.IsNullOrWhiteSpace(localDependency.name)) return Path.GetFullPath(path).Replace('\\', '/');
            else return Path.GetFullPath("Packages/" + localDependency.name + path.Substring(localDependency.path.Length)).Replace('\\', '/');
        }

        private static IReadOnlyCollection<(string name, string path)> GetLocalDependencies()
        {
            if (File.Exists("Packages/manifest.json"))
            {
                string json = File.ReadAllText("Packages/manifest.json");
                Dictionary<string, Dictionary<string, string>> manifest = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
                if (manifest.TryGetValue("dependencies", out Dictionary<string, string> dependencies))
                {
                    string prefix = "file:";
                    List<(string, string)> localDependencies = new List<(string, string)>();
                    foreach (KeyValuePair<string, string> dependency in dependencies)
                        if (dependency.Value.StartsWith(prefix)) localDependencies.Add((dependency.Key, dependency.Value.Substring(prefix.Length)));
                    return localDependencies;
                }
            }

            return new List<(string, string)>();
        }
    }
}