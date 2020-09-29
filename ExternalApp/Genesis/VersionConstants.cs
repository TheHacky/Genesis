using System.Diagnostics;
using System.Reflection;

namespace Genesis.CLI
{
	/// <summary>
	/// A constants class whose values are calculated and baked into an internal class at compile-time.
	/// </summary>
	internal static class VersionConstants
	{
		public static readonly string MAJOR;
		public static readonly string MINOR;
		public static readonly string PATCH;
		public static readonly string PRE_RELEASE_TAG;
		public static readonly string PRE_RELEASE_TAG_WITH_DASH;
		public static readonly string PRE_RELEASE_LABEL;
		public static readonly string PRE_RELEASE_NUMBER;
		public static readonly string BUILD_META_DATA;
		public static readonly string BUILD_META_DATA_PADDED;
		public static readonly string FULL_BUILD_META_DATA;
		public static readonly string MAJOR_MINOR_PATCH;
		public static readonly string SEM_VER;
		public static readonly string LEGACY_SEM_VER;
		public static readonly string LEGACY_SEM_VER_PADDED;
		public static readonly string ASSEMBLY_SEM_VER;
		public static readonly string ASSEMBLY_SEM_FILE_VER;
		public static readonly string FULL_SEM_VER;
		public static readonly string INFORMATIONAL_VERSION;
		public static readonly string BRANCH_NAME;
		public static readonly string SHA;
		public static readonly string SHORT_SHA;
		public static readonly string NU_GET_VERSION_V2;
		public static readonly string NU_GET_VERSION;
		public static readonly string NU_GET_PRE_RELEASE_TAG_V2;
		public static readonly string NU_GET_PRE_RELEASE_TAG;
		public static readonly string COMMITS_SINCE_VERSION_SOURCE;
		public static readonly string COMMITS_SINCE_VERSION_SOURCE_PADDED;
		public static readonly string COMMIT_DATE;

		private const string COMPILE_TIME_VERSION_CLASS = "GitVersionInformation";

		static VersionConstants()
		{
			var assembly = Assembly.GetAssembly(typeof(VersionConstants));

			Debug.Assert(assembly != null);

			var gitVersionInformationType = assembly.GetType(COMPILE_TIME_VERSION_CLASS);

			Debug.Assert(gitVersionInformationType != null);

			MAJOR = (string)gitVersionInformationType.GetField("Major").GetValue(null);
			MINOR = (string)gitVersionInformationType.GetField("Minor").GetValue(null);
			PATCH = (string)gitVersionInformationType.GetField("Patch").GetValue(null);
			PRE_RELEASE_TAG = (string)gitVersionInformationType.GetField("PreReleaseTag").GetValue(null);
			PRE_RELEASE_TAG_WITH_DASH = (string)gitVersionInformationType.GetField("PreReleaseTagWithDash").GetValue(null);
			PRE_RELEASE_LABEL = (string)gitVersionInformationType.GetField("PreReleaseLabel").GetValue(null);
			PRE_RELEASE_NUMBER = (string)gitVersionInformationType.GetField("PreReleaseNumber").GetValue(null);
			BUILD_META_DATA = (string)gitVersionInformationType.GetField("BuildMetaData").GetValue(null);
			BUILD_META_DATA_PADDED = (string)gitVersionInformationType.GetField("BuildMetaDataPadded").GetValue(null);
			FULL_BUILD_META_DATA = (string)gitVersionInformationType.GetField("FullBuildMetaData").GetValue(null);
			MAJOR_MINOR_PATCH = (string)gitVersionInformationType.GetField("MajorMinorPatch").GetValue(null);
			SEM_VER = (string)gitVersionInformationType.GetField("SemVer").GetValue(null);
			LEGACY_SEM_VER = (string)gitVersionInformationType.GetField("LegacySemVer").GetValue(null);
			LEGACY_SEM_VER_PADDED = (string)gitVersionInformationType.GetField("LegacySemVerPadded").GetValue(null);
			ASSEMBLY_SEM_VER = (string)gitVersionInformationType.GetField("AssemblySemVer").GetValue(null);
			ASSEMBLY_SEM_FILE_VER = (string)gitVersionInformationType.GetField("AssemblySemFileVer").GetValue(null);
			FULL_SEM_VER = (string)gitVersionInformationType.GetField("FullSemVer").GetValue(null);
			INFORMATIONAL_VERSION = (string)gitVersionInformationType.GetField("InformationalVersion").GetValue(null);
			BRANCH_NAME = (string)gitVersionInformationType.GetField("BranchName").GetValue(null);
			SHA = (string)gitVersionInformationType.GetField("Sha").GetValue(null);
			SHORT_SHA = (string)gitVersionInformationType.GetField("ShortSha").GetValue(null);
			NU_GET_VERSION_V2 = (string)gitVersionInformationType.GetField("NuGetVersionV2").GetValue(null);
			NU_GET_VERSION = (string)gitVersionInformationType.GetField("NuGetVersion").GetValue(null);
			NU_GET_PRE_RELEASE_TAG_V2 = (string)gitVersionInformationType.GetField("NuGetPreReleaseTagV2").GetValue(null);
			NU_GET_PRE_RELEASE_TAG = (string)gitVersionInformationType.GetField("NuGetPreReleaseTag").GetValue(null);
			COMMITS_SINCE_VERSION_SOURCE = (string)gitVersionInformationType.GetField("CommitsSinceVersionSource").GetValue(null);
			COMMITS_SINCE_VERSION_SOURCE_PADDED = (string)gitVersionInformationType.GetField("CommitsSinceVersionSourcePadded").GetValue(null);
			COMMIT_DATE = (string)gitVersionInformationType.GetField("CommitDate").GetValue(null);
		}
	}
}
