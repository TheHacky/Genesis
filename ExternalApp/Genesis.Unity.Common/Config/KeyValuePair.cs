using System;

namespace Genesis.Common
{
	/// <summary>
	/// A serializable key-value pair for configuration.
	/// </summary>
	[Serializable]
	internal class KeyValuePair
	{
		public string key;
		public string value;
	}
}
