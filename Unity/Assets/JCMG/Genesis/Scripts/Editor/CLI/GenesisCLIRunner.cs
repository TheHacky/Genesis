﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Genesis.Common;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace JCMG.Genesis.Editor
{
	/// <summary>
	/// Orchestrates running of the Genesis.CLI executable and relaying the feedback back to Unity.
	/// </summary>
	public static class GenesisCLIRunner
	{
		private static readonly StringBuilder SB;

		private static GenesisSettings _TEMP_GENESIS_SETTINGS;

		static GenesisCLIRunner()
		{
			SB = new StringBuilder(10000);
		}

		public static void RunCodeGeneration()
		{
			var allGenesisSettings = GenesisSettings.GetAllSettings();

			RunCodeGeneration(allGenesisSettings);
		}

		public static void RunCodeGeneration(IEnumerable<GenesisSettings> settings)
		{
			var process = new Process
			{
				StartInfo =
				{
					FileName = EditorConstants.DOTNET_EXE,
					WorkingDirectory = GenesisPreferences.GetWorkingPath(),
					Arguments = GetCodeGenerationCLIArguments(settings),
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardError = true,
					RedirectStandardOutput = true
				},
				EnableRaisingEvents = true
			};

			process.Exited += OnProcessExitedForCodeGeneration;
			process.OutputDataReceived += OnProcessStandardOut;
			process.ErrorDataReceived += OnProcessStandardError;

			try
			{
				Debug.Log(EditorConstants.STARTED_CODE_GENERATION);

				if (GenesisPreferences.EnableVerboseLogging)
				{
					Debug.LogFormat(
						EditorConstants.DOTNET_COMMAND_EXECUTION_FORMAT,
						EditorConstants.DOTNET_EXE,
						process.StartInfo.Arguments);
				}

				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
			}
			catch (Win32Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
		}

		public static void RunConfigurationImport(GenesisSettings genesisSettings)
		{
			_TEMP_GENESIS_SETTINGS = genesisSettings;

			var process = new Process
			{
				StartInfo =
				{
					FileName = EditorConstants.DOTNET_EXE,
					WorkingDirectory = GenesisPreferences.GetWorkingPath(),
					Arguments = GetConfigGenerationCLIArguments(),
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardError = true,
					RedirectStandardOutput = true
				},
				EnableRaisingEvents = true
			};

			process.Exited += OnProcessExitedForConfigImport;
			process.OutputDataReceived += OnProcessStandardOut;
			process.ErrorDataReceived += OnProcessStandardError;

			try
			{
				Debug.Log(EditorConstants.STARTED_CONFIG_GENERATION);

				if (GenesisPreferences.EnableVerboseLogging)
				{
					Debug.LogFormat(
						EditorConstants.DOTNET_COMMAND_EXECUTION_FORMAT,
						EditorConstants.DOTNET_EXE,
						process.StartInfo.Arguments);
				}

				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
			}
			catch (Win32Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
		}

		private static string GetCodeGenerationCLIArguments(IEnumerable<GenesisSettings> settings)
		{
			var jsonConfigs = settings
				.Select(x => x.ConvertToJson().ConvertToBase64())
				.ToArray();
			var jsonConfigsArrayArgs = FormatAsArrayArguments(jsonConfigs);

			SB.Clear();
			SB.Append(GenesisPreferences.GetExecutablePath());
			SB.Append(EditorConstants.SPACE_STR);
			SB.Append(CommandLineConstants.GENERATE_VERB_PARAM);
			SB.Append(EditorConstants.SPACE_STR);
			SB.Append(GenesisPreferences.EnableVerboseLogging ? CommandLineConstants.VERBOSE_PARAM : string.Empty);
			SB.Append(EditorConstants.SPACE_STR);
			SB.Append(GenesisPreferences.ExecuteDryRun ? CommandLineConstants.DRY_RUN_PARAM : string.Empty);
			SB.Append(EditorConstants.SPACE_STR);
			SB.Append(CommandLineConstants.CONFIG_BASE64_PARAM);
			SB.Append(EditorConstants.SPACE_STR);
			SB.Append(jsonConfigsArrayArgs);
			SB.Append(EditorConstants.SPACE_STR);
			SB.Append(CommandLineConstants.PROJECT_PATH_PARAM);
			SB.Append(EditorConstants.SPACE_STR);
			SB.Append(EditorConstants.QUOTE_STR);
			SB.Append(FileTools.GetProjectPath());
			SB.Append(EditorConstants.QUOTE_STR);

			return SB.ToString();
		}

		private static string GetConfigGenerationCLIArguments()
		{
			SB.Clear();
			SB.Append(GenesisPreferences.GetExecutablePath());
			SB.Append(EditorConstants.SPACE_STR);
			SB.Append(CommandLineConstants.CONFIG_VERB_PARAM);
			SB.Append(EditorConstants.SPACE_STR);
			SB.Append(CommandLineConstants.CONFIG_CREATE_PARAM);
			SB.Append(EditorConstants.SPACE_STR);
			SB.Append(GenesisPreferences.EnableVerboseLogging ? CommandLineConstants.VERBOSE_PARAM : string.Empty);
			SB.Append(EditorConstants.SPACE_STR);
			SB.Append(CommandLineConstants.OUTPUT_PATH_PARAM);
			SB.Append(EditorConstants.SPACE_STR);
			SB.Append(EditorConstants.TEMP_CONFIG_FILE_PATH);

			return SB.ToString();
		}

		private static string FormatAsArrayArguments(string[] args)
		{
			if (args.Length == 0)
			{
				return string.Empty;
			}

			SB.Clear();
			SB.Append(EditorConstants.QUOTE_STR);
			SB.Append(args[0]);
			SB.Append(EditorConstants.QUOTE_STR);

			for (var i = 1; i < args.Length; i++)
			{
				SB.Append(EditorConstants.COMMA_STR);
				SB.Append(EditorConstants.QUOTE_STR);
				SB.Append(args[i]);
				SB.Append(EditorConstants.QUOTE_STR);
			}

			return SB.ToString();
		}

		private static void OnProcessExitedForCodeGeneration(object sender, EventArgs args)
		{
			var code = ((Process)sender).ExitCode;
			if (code == 0)
			{
				Debug.Log(EditorConstants.CODE_GENERATION_SUCCESS);
			}
			else
			{
				Debug.LogErrorFormat(EditorConstants.CODE_GENERATION_FAILURE, code);
			}

			((Process)sender).Dispose();

			EditorApplication.delayCall += DelayForceAssetDatabaseUpdate;
		}

		private static void OnProcessExitedForConfigImport(object sender, EventArgs args)
		{
			var code = ((Process)sender).ExitCode;
			if (code == 0)
			{
				Debug.Log(EditorConstants.CONFIG_GENERATION_SUCCESS);

				// Read in temp config file and import it into GenesisSettings
				var tempConfigPath = Path.GetFullPath(Path.Combine(
					GenesisPreferences.GetWorkingPath(),
					EditorConstants.TEMP_CONFIG_FILE_PATH));

				var genesisConfig = tempConfigPath.FromFile();

				if (_TEMP_GENESIS_SETTINGS != null)
				{
					var codeGeneratorConfig = _TEMP_GENESIS_SETTINGS.CreateAndConfigure<CodeGeneratorConfig>();
					codeGeneratorConfig.Overwrite(genesisConfig);

					EditorUtility.SetDirty(_TEMP_GENESIS_SETTINGS);
				}

				// Delete the temp config file
				File.Delete(tempConfigPath);
			}
			else
			{
				Debug.LogErrorFormat(EditorConstants.CODE_GENERATION_FAILURE, code);
			}

			((Process)sender).Dispose();

			_TEMP_GENESIS_SETTINGS = null;

			EditorApplication.delayCall += DelayForceAssetDatabaseUpdate;
		}

		private static void DelayForceAssetDatabaseUpdate()
		{
			EditorApplication.delayCall -= DelayForceAssetDatabaseUpdate;

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		}

		private static void OnProcessStandardOut(object sender, DataReceivedEventArgs args)
		{
			if (args.Data != null)
			{
				Debug.LogFormat(EditorConstants.CODE_GENERATION_UPDATE, args.Data);
			}
		}

		private static void OnProcessStandardError(object sender, DataReceivedEventArgs args)
		{
			if (args.Data != null)
			{
				Debug.LogErrorFormat(EditorConstants.CODE_GENERATION_UPDATE_ERROR, args.Data);
			}
		}
	}
}
