using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Genesis.Common;
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

			return Parser.Default.ParseArguments<GenerateOptions, ConfigOptions>(args: args)
				.MapResult(
					(ConfigOptions configOptions) => HandleConfigOptions(configOptions),
					(GenerateOptions generateOptions) => ExecuteCodeGeneration(generateOptions),
					notParsedFunc: errs => HandleErrors(errs));
		}

		private static int HandleConfigOptions(ConfigOptions configOptions)
		{
			var result = 0;
			try
			{
				ConfigureLogger(isVerbose: configOptions.IsVerbose);

				if (configOptions.DoCreate)
				{
					// Create config and populate it with all sub-configs
					var genesisConfig = new GenesisConfig();

					using (var assemblyLoader = new AssemblyLoader(configOptions.PluginPath))
					{
						assemblyLoader.LoadAll(configOptions.PluginPath);

						_logger.Verbose($"Loaded {assemblyLoader.Count} plugins assemblies.");

						var codeGenerationPluginCache = new TypeCache();
						codeGenerationPluginCache.AddTypeWithInterface<object, ICodeGenerationPlugin>();

						foreach (var type in codeGenerationPluginCache)
						{
							_logger.Verbose($"Plugin loaded: {type.Name}");
						}

						// Get all configs and populate with default settings
						var allConfigs = ReflectionTools.GetAllDerivedInstancesOfType<AbstractConfigurableConfig>();
						foreach (var config in allConfigs)
						{
							config.Configure(genesisConfig);
						}

						// Populate with all plugins and search paths
						var allPluginInstances = codeGenerationPluginCache
							.Select(x => Activator.CreateInstance(x))
							.Cast<ICodeGenerationPlugin>()
							.ToArray();

						var codeGeneratorConfig = new CodeGeneratorConfig();
						codeGeneratorConfig.Configure(genesisConfig);
						codeGeneratorConfig.AutoImportPlugins(allPluginInstances);

						_logger.Verbose("Populated config with all defaults.");
					}

					// Write config to file.
					var jsonContents = genesisConfig.ConvertToJson();
					File.WriteAllText(configOptions.CreatePath, jsonContents);

					_logger.Verbose("Config is written to {CreatePath}.", configOptions.CreatePath);
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "An unexpected error occurred during code generation.");

				result = 1;
			}

			return result;
		}

		private static int ExecuteCodeGeneration(GenerateOptions generateOptions)
		{
			var result = 0;
			try
			{
				ConfigureLogger(isVerbose: generateOptions.IsVerbose);

				// Load all plugin assemblies
				using (var assemblyLoader = new AssemblyLoader(generateOptions.PluginPath))
				{
					assemblyLoader.LoadAll(generateOptions.PluginPath);

					_logger.Verbose($"Loaded {assemblyLoader.Count} plugins assemblies.");

					var codeGenerationPluginCache = new TypeCache();
					codeGenerationPluginCache.AddTypeWithInterface<object, ICodeGenerationPlugin>();

					foreach (var type in codeGenerationPluginCache)
					{
						_logger.Verbose($"Plugin loaded: {type.Name}");
					}

					// Add any configs from base64, file configs
					var configs = new List<IGenesisConfig>();
					if (generateOptions.HasConfigsAsBase64())
					{
						_logger.Verbose("Using Base64 Configs...");

						foreach (var base64Config in generateOptions.ConfigsAsBase64)
						{
							configs.Add(base64Config.LoadGenesisConfigFromBase64());
						}
					}

					if (generateOptions.HasFileConfigs())
					{
						_logger.Verbose("Using File Configs...");

						foreach (var configFilePath in generateOptions.ConfigFilePaths)
						{
							if (File.Exists(configFilePath))
							{
								configs.Add(configFilePath.LoadGenesisConfigFromFile());
							}
							else
							{
								_logger.Warning("Could not find config file at {ConfigFilePath}.", configFilePath);
							}
						}
					}

					_logger.Verbose($"Loaded {configs.Count} GenesisConfigs.");
					_logger.Verbose("Starting code generation.");

					try
					{
						for (var i = 0; i < configs.Count; i++)
						{
							_logger.Verbose("Generating code from {ConfigName}.", configs[i].Name);

							// Set the absolute project path per config
							var targetDirectoryConfig = configs[i].CreateAndConfigure<TargetDirectoryConfig>();
							var relativeOutputPath = targetDirectoryConfig.TargetDirectory;
							var absoluteOutputPath =
								Path.GetFullPath(Path.Combine(generateOptions.ProjectPath.Trim('"'), relativeOutputPath));
							targetDirectoryConfig.TargetDirectory = absoluteOutputPath;

							_logger.Verbose(
								"Setting absolute output path to {OutputPath}.",
								targetDirectoryConfig.TargetDirectory);

							// Create a code-gen runner per config and execute.
							var codeGenerator = CodeGeneratorTools.CodeGeneratorFromPreferences(configs[i]);
							codeGenerator.OnProgress += (title, info, progress) =>
							{
								_logger.Verbose($"{title} {info} {$"{progress:P}"}%");
							};

							if (generateOptions.IsDryRun)
							{
								codeGenerator.DryRun();
							}

							codeGenerator.Generate();
						}
					}
					catch (Exception ex)
					{
						_logger.Error(ex, "An unexpected error occured during code generation, stopping now...");

						result = 1;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "An unexpected error occurred during code generation.");

				result = 1;
			}

			return result;
		}

		private static int HandleErrors(IEnumerable<Error> errors)
		{
			foreach (var error in errors)
			{
				switch (error)
				{
					// Do nothing if these are the errors
					case HelpRequestedError helpRequestedError:
					case VersionRequestedError versionRequestedError:
						// No-op
						break;

					// Otherwise log to the console.
					default:
						_logger.Error(error.ToString());
						break;
				}
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
