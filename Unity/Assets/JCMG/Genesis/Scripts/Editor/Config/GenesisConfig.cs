using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JCMG.Genesis.Editor
{
	/// <summary>
	/// Contains project settings for Genesis code generation.
	/// </summary>
	internal sealed class GenesisConfig : ScriptableObject
	{
		/// <summary>
		/// Returns true if there should be verbose logging for code-generation, otherwise false.
		/// </summary>
		public bool IsVerbose => _isVerbose;

		#pragma warning disable 0649

		[SerializeField]
		private string _outputPath;

		[SerializeField]
		private bool _isVerbose;

		[SerializeField]
		private string _genesisCLIInstallationFolder;

		#pragma warning restore 0649

		private static readonly string CONFIG_SEARCH_FILTER;

		private const string ROOT_ASSET_CREATION_PATH = "Assets/{0}.asset";

		static GenesisConfig()
		{
			CONFIG_SEARCH_FILTER = "t:" + nameof(GenesisConfig);
		}

		public static GenesisConfig Instance
		{
			get
			{
				var configGuids = AssetDatabase.FindAssets(CONFIG_SEARCH_FILTER);
				if (configGuids == null || configGuids.Length == 0)
				{
					var config = CreateInstance<GenesisConfig>();

					AssetDatabase.CreateAsset(config, string.Format(ROOT_ASSET_CREATION_PATH, nameof(GenesisConfig)));

					return config;
				}
				else
				{
					var path = AssetDatabase.GUIDToAssetPath(configGuids[0]);
					return AssetDatabase.LoadAssetAtPath<GenesisConfig>(path);
				}
			}
		}

		[InitializeOnLoadMethod]
		public static bool EnsureExists()
		{
			return Instance != null;
		}

		public string GetExecutablePath()
		{
			return Path.Combine(Path.GetFullPath(_genesisCLIInstallationFolder), EditorConstants.GENESIS_EXE);
		}

		public string GetWorkingPath()
		{
			return Path.GetFullPath(_genesisCLIInstallationFolder);
		}

		public string[] GetAssemblyPaths()
		{
			var assemblyPathsList = new List<string>();
			assemblyPathsList.AddRange(GetPackageAssembliesPath());
			assemblyPathsList.AddRange(GetAssetPathsWithDLLs());
			assemblyPathsList.Add(GetPluginsPath());

			const string RUNTIME_ASSEMBLIES_PATH = @"Managed\UnityEngine";
			assemblyPathsList.Add(Path.GetFullPath(Path.Combine(EditorApplication.applicationContentsPath, RUNTIME_ASSEMBLIES_PATH)));

			const string LIBRARY_SCRIPT_ASSEMBLIES_PATH = @"Library\ScriptAssemblies";
			assemblyPathsList.Add(Path.GetFullPath(Path.Combine(GetProjectPath(), LIBRARY_SCRIPT_ASSEMBLIES_PATH)));

			return assemblyPathsList.Distinct().ToArray();
		}

		public string GetPluginsPath()
		{
			const string PLUGINS_FOLDER_NAME = "Plugins";
			return Path.Combine(Path.GetFullPath(_genesisCLIInstallationFolder), PLUGINS_FOLDER_NAME);
		}

		public string[] GetPackageAssembliesPath()
		{
			const string ROOT_PACKAGES_PATH = @"Library\PackageCache\";

			var fullLibraryPath = Path.Combine(GetProjectPath(), ROOT_PACKAGES_PATH);
			var dllPaths = FindAllDLLs(fullLibraryPath);
			return dllPaths.Select(x => new FileInfo(x)).Select(y => y.Directory.FullName).Distinct().ToArray();
		}

		public string[] GetAssetPathsWithDLLs()
		{
			var dllPaths = FindAllDLLs(Application.dataPath);
			return dllPaths.Select(x => new FileInfo(x)).Select(y => y.Directory.FullName).Distinct().ToArray();
		}

		private string[] FindAllDLLs(string rootPath)
		{
			return Directory.GetFiles(
				rootPath,
				EditorConstants.WILDCARD_ALL_DLLS,
				SearchOption.AllDirectories);
		}

		public string GetCommandLineArguments()
		{
			var outputPathStr = Path.GetFullPath(_outputPath);
			var verboseStr = CommandLineConstants.VERBOSE_PARAM;
			var dryRunStr = GenesisPreferences.ExecuteDryRun ?
				CommandLineConstants.DRY_RUN_PARAM :
				string.Empty;

			return $"{GetExecutablePath()} " +
			       $"{CommandLineConstants.OUTPUT_PATH_PARAM} {outputPathStr}" +
			       (_isVerbose ? $"{CommandLineConstants.VERBOSE_PARAM}" : string.Empty) +
			       $"{dryRunStr}";
		}

		private static string AggregateIntoArrayArgument<T>(IEnumerable<T> paths)
		{
			return paths.Any()
				? paths
					.Select(x => EditorConstants.BACKSLASH_STR + x + EditorConstants.BACKSLASH_STR)
					.Aggregate((x, y) => x + EditorConstants.SPACE_STR + y)
				: string.Empty;
		}

		private static string GetProjectPath()
		{
			const string ASSETS_FOLDER_NAME = "Assets";
			return Application.dataPath.Replace(ASSETS_FOLDER_NAME, string.Empty);
		}
	}
}
