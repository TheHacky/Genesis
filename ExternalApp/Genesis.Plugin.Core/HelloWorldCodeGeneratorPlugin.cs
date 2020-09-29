namespace Genesis.Plugin.Core
{
	public sealed class HelloWorldCodeGeneratorPlugin : ICodeGenerationPlugin
	{
		#region Implementation of ICodeGenerationPlugin

		/// <summary>
		/// The name of the plugin.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The priority value this plugin should be given to execute with regards to other plugins,
		/// ordered by ASC value.
		/// </summary>
		public int Priority { get; }

		/// <summary>
		/// Returns true if this plugin should be executed in Dry Run Mode, otherwise false.
		/// </summary>
		public bool RunInDryMode { get; }

		#endregion
	}
}
