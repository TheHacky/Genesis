using CommandLine;

namespace Genesis.CLI
{
	/// <summary>
	/// The command-line options for Genesis.CLI.
	/// </summary>
	internal sealed class DefaultOptions
	{
		[Option(
			"config",
			HelpText = "The path to the config file.",
			Default = "./config.json")]
		public string ConfigFilePath { get; set; }

		[Option(
			"plugin-path",
			HelpText = "The path to the plugin folder.",
			Default = "./plugins")]
		public string PluginPath { get; set; }

		[Option(
			"verbose",
			HelpText = "Sets the logging to be verbose if true, errors only if false.",
			Default = false)]
		public bool IsVerbose { get; set; }

		[Option(
			"dryrun",
			HelpText = "Performs a dry run of code-generation process, but does not output files.",
			Default = false)]
		public bool IsDryRun { get; set; }

		[Option(
			"output-path",
			HelpText = "The path to where generated code should be output to."
			//,Required = true
			)]
		public string OutputPath { get; set; }
	}
}
