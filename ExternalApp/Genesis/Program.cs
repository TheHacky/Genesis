using System;
using System.Collections.Generic;
using CommandLine;
using Genesis.Plugin;
using Serilog;

namespace Genesis.CLI
{
	internal class Program
	{
		private static ILogger _logger;

		public static int Main(string[] args)
		{
			ConfigureLogger(isVerbose: true);

			return Parser.Default.ParseArguments<DefaultOptions>(args: args)
				.MapResult(
					parsedFunc: ExecuteCodeGeneration,
					notParsedFunc: errs => HandleErrors(errs));
		}

		private static int ExecuteCodeGeneration(DefaultOptions defaultOptions)
		{
			var result = 0;
			try
			{
				ConfigureLogger(isVerbose: defaultOptions.IsVerbose);

				// Load all plugin assemblies
				using (var assemblyLoader = new AssemblyLoader(defaultOptions.PluginPath))
				{
					assemblyLoader.LoadAll(defaultOptions.PluginPath);

					_logger.Verbose($"Loaded {assemblyLoader.Count} plugins assemblies.");

					var codeGenerationPluginCache = new TypeCache();
					codeGenerationPluginCache.AddTypeWithInterface<object, ICodeGenerationPlugin>();

					foreach (var type in codeGenerationPluginCache)
					{
						_logger.Verbose($"Plugin loaded: {type.Name}");
					}
				}
			}
			catch (Exception e)
			{
				_logger.Error(e, "An unexpected error occurred during code generation.");

				result = 1;
			}

			return result;
		}

		private static int HandleErrors(IEnumerable<Error> errors)
		{
			foreach (var error in errors)
			{
				_logger.Error(error.ToString());
			}

			return 1;
		}

		private static void ConfigureLogger(bool isVerbose)
		{
			var config = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.Enrich.WithProperty(
					LoggingConstants.VERSION_PROPERTY,
					VersionConstants.SEM_VER)
				.Enrich.WithProperty(
					LoggingConstants.BRANCH_PROPERTY,
					VersionConstants.BRANCH_NAME);

			if (isVerbose)
			{
				config = config
					.MinimumLevel.Verbose()
					.WriteTo.Console(outputTemplate: LoggingConstants.VERBOSE_LOGGING_TEMPLATE);
			}
			else
			{
				config = config
					.MinimumLevel.Information()
					.WriteTo.Console(outputTemplate: LoggingConstants.GENERAL_LOGGING_TEMPLATE);
			}

			config = config.WriteTo.File(
				LoggingConstants.LOG_FILENAME,
				rollingInterval: RollingInterval.Month,
				rollOnFileSizeLimit: true,
				fileSizeLimitBytes: 10000000);

			Log.Logger = _logger = config.CreateLogger();
		}
	}
}
