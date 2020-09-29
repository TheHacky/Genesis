namespace Genesis.CLI
{
	/// <summary>
	/// Shared constant fields for logging.
	/// </summary>
	internal static class LoggingConstants
	{
		public const string BRANCH_PROPERTY = "Branch";
		public const string VERSION_PROPERTY = "Version";

		public const string LOG_FILENAME = "./log.txt";

		public static readonly string VERBOSE_LOGGING_TEMPLATE =
			$"{{Timestamp:HH:mm}} [{{{VERSION_PROPERTY}}}] [{{{BRANCH_PROPERTY}}}] " +
			$"[{{Level}}] {{Message}}{{NewLine}}{{Exception}}";

		public static readonly string GENERAL_LOGGING_TEMPLATE =
			"{{Timestamp:HH:mm}} [{{Level}}] {{Message}}{{NewLine}}{{Exception}}";
	}
}
