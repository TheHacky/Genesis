using System;
using System.ComponentModel;
using System.Diagnostics;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace JCMG.Genesis.Editor
{
	/// <summary>
	/// Orchestrates running of the Genesis.CLI executable and relaying the feedback back to Unity.
	/// </summary>
	public static class GenesisCLIRunner
	{
		public static void Run()
		{
			var config = GenesisConfig.Instance;

			var process = new Process
			{
				StartInfo =
				{
					FileName = EditorConstants.DOTNET_EXE,
					WorkingDirectory = config.GetWorkingPath(),
					Arguments = config.GetCommandLineArguments(),
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardError = true,
					RedirectStandardOutput = true
				},
				EnableRaisingEvents = true
			};

			process.Exited += OnProcessOnExited;
			process.OutputDataReceived += OnProcessStandardOut;
			process.ErrorDataReceived += OnProcessStandardError;

			try
			{
				Debug.Log(EditorConstants.STARTED_CODE_GENERATION);

				if (config.IsVerbose)
				{
					Debug.LogFormat(
						EditorConstants.DOTNET_COMMAND_EXECUTION_FORMAT,
						EditorConstants.DOTNET_EXE,
						process.StartInfo.Arguments);
				}

				process.Start();
				process.BeginErrorReadLine();
				process.BeginOutputReadLine();
			}
			catch (Win32Exception ex)
			{
				Debug.LogError(ex.ToString());

				process.Dispose();
			}
		}

		private static void OnProcessOnExited(object sender, EventArgs args)
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

		private static void DelayForceAssetDatabaseUpdate()
		{
			EditorApplication.delayCall -= DelayForceAssetDatabaseUpdate;

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		}

		private static void OnProcessStandardError(object sender, DataReceivedEventArgs args)
		{
			if (args.Data != null)
			{
				Debug.LogErrorFormat(EditorConstants.CODE_GENERATION_UPDATE_ERROR, args.Data);
			}
		}

		private static void OnProcessStandardOut(object sender, DataReceivedEventArgs args)
		{
			if (args.Data != null)
			{
				Debug.LogFormat(EditorConstants.CODE_GENERATION_UPDATE, args.Data);
			}
		}
	}
}
