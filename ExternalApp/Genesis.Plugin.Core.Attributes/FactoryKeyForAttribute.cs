using System;

namespace Genesis.Plugin.Core.Attributes
{
	/// <summary>
	/// Indicates that a scriptable factory should be created using the decorated class as a key to
	/// <see cref="ValueType"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public sealed class FactoryKeyForAttribute : Attribute
	{
		public Type ValueType { get; }

		public FactoryKeyForAttribute(Type valueType)
		{
			ValueType = valueType;
		}
	}
}
