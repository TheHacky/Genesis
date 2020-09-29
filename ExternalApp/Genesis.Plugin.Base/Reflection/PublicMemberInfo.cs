﻿using System;
using System.Reflection;

namespace Genesis.Plugin
{
	/// <summary>
	/// Represents information about a public member of an object.
	/// </summary>
	public class PublicMemberInfo
	{
		private readonly FieldInfo _fieldInfo;
		private readonly PropertyInfo _propertyInfo;
		public readonly AttributeInfo[] attributes;
		public readonly string name;
		public readonly Type type;

		public PublicMemberInfo(FieldInfo info)
		{
			_fieldInfo = info;
			type = _fieldInfo.FieldType;
			name = _fieldInfo.Name;
			attributes = getAttributes(_fieldInfo.GetCustomAttributes(false));
		}

		public PublicMemberInfo(PropertyInfo info)
		{
			_propertyInfo = info;
			type = _propertyInfo.PropertyType;
			name = _propertyInfo.Name;
			attributes = getAttributes(_propertyInfo.GetCustomAttributes(false));
		}

		public PublicMemberInfo(Type type, string name, AttributeInfo[] attributes = null)
		{
			this.type = type;
			this.name = name;
			this.attributes = attributes;
		}

		public object GetValue(object obj)
		{
			return _fieldInfo == null ? _propertyInfo.GetValue(obj, null) : _fieldInfo.GetValue(obj);
		}

		public void SetValue(object obj, object value)
		{
			if (_fieldInfo != null)
			{
				_fieldInfo.SetValue(obj, value);
			}
			else
			{
				_propertyInfo.SetValue(obj, value, null);
			}
		}

		private static AttributeInfo[] getAttributes(object[] attributes)
		{
			var attributeInfoArray = new AttributeInfo[attributes.Length];
			for (var index = 0; index < attributes.Length; ++index)
			{
				var attribute = attributes[index];
				attributeInfoArray[index] = new AttributeInfo(attribute, attribute.GetType().GetPublicMemberInfos());
			}

			return attributeInfoArray;
		}
	}
}
