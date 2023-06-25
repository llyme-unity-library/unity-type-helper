using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace TypeHelper
{
	public static class Objects
	{
		public static bool GenericMemberPredicate
			(string name,
			MemberInfo member)
		{
			if (member.GetCustomAttribute<NonSerializedAttribute>() != null)
				return false;

			if (member.GetCustomAttribute<ObsoleteAttribute>() != null)
				return false;

			if (member.GetCustomAttribute<XmlIgnoreAttribute>() != null)
				return false;

			if (member.GetCustomAttribute<HideInInspector>() != null)
				return false;

			if (member is PropertyInfo property)
			{
				if (!property.CanRead)
					return false;

				if (!property.CanWrite)
					return false;

				if (property.GetGetMethod() == null)
					return false;

				if (property.GetSetMethod() == null)
					return false;
			}

			return true;
		}

		public static object ValueOfProperty
			(this object @object,
			string name,
			BindingFlags flags =
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance) =>
			ValueOfProperty(@object, @object?.GetType(), name, flags);

		public static object ValueOfProperty<T>
			(this object @object,
			string name,
			BindingFlags flags =
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance) =>
			ValueOfProperty(@object, typeof(T), name, flags);

		public static object ValueOfProperty
			(this object @object,
			Type type,
			string name,
			BindingFlags flags =
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance)
		{
			ValueOfProperty(
				@object,
				type,
				name,
				out object value,
				flags
			);
			return value;
		}

		public static bool ValueOfProperty
			(this object @object,
			Type type,
			string name,
			out object value,
			BindingFlags flags =
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance)
		{
			if (@object == null)
			{
				value = null;
				return false;
			}

			PropertyInfo info =
				type.GetProperty(name, flags);

			if (info == null)
			{
				value = null;
				return false;
			}

			value = info.GetValue(@object);
			return true;
		}

		public static object ValueOfField
			(this object @object,
			string name,
			BindingFlags flags =
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance) =>
			ValueOfField(@object, @object?.GetType(), name, flags);

		public static object ValueOfField<T>
			(this object @object,
			string name,
			BindingFlags flags =
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance) =>
			ValueOfField(@object, typeof(T), name, flags);

		public static object ValueOfField
			(this object @object,
			Type type,
			string name,
			BindingFlags flags =
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance)
		{
			ValueOfField(@object, type, name, out object value, flags);
			return value;
		}

		public static bool ValueOfField
			(this object @object,
			Type type,
			string name,
			out object value,
			BindingFlags flags =
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance)
		{
			if (@object == null)
			{
				value = null;
				return false;
			}

			FieldInfo info =
				type.GetField(name, flags);

			if (info == null)
			{
				value = null;
				return false;
			}

			value = info.GetValue(@object);
			return true;
		}

		public static IEnumerable<KeyValuePair<string, FieldInfo>> AllFields
			(this object @object,
			BindingFlags flags =
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance,
			Func<string, FieldInfo, bool> predicate = null) =>
			AllFields(@object.GetType(), flags, predicate);

		public static IEnumerable<KeyValuePair<string, FieldInfo>> AllFields
			(this Type type,
			BindingFlags flags =
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance,
			Func<string, FieldInfo, bool> predicate = null)
		{
			HashSet<string> cache =
				new(StringComparer.OrdinalIgnoreCase);

			while (type != null)
			{
				IEnumerable<KeyValuePair<string, FieldInfo>> fields =
					AllFields_Internal(
						type,
						flags,
						predicate
					);

				foreach (KeyValuePair<string, FieldInfo> field in fields)
					if (!cache.Contains(field.Key))
					{
						cache.Add(field.Key);
						yield return field;
					}

				type = type.BaseType;
			}
		}

		private static IEnumerable<KeyValuePair<string, FieldInfo>> AllFields_Internal
			(this Type type,
			BindingFlags flags,
			Func<string, FieldInfo, bool> predicate)
		{
			FieldInfo[] fields =
				type.GetFields(flags);

			foreach (FieldInfo field in fields)
			{
				string name = field.Name;

				if (name.StartsWith("<"))
					name = name[1..name.IndexOf('>')];

				if (predicate == null || predicate(name, field))
					yield return new(name, field);
			}
		}

		/// <summary>
		/// Includes base types.
		/// </summary>
		public static IEnumerable<KeyValuePair<string, PropertyInfo>> AllProperties
			(this object @object) =>
			AllProperties(
				@object,
				TypeHelper.Generic.GENERIC_MEMBER_FLAG,
				GenericMemberPredicate
			);

		/// <summary>
		/// Includes base types.
		/// </summary>
		public static IEnumerable<KeyValuePair<string, PropertyInfo>> AllProperties
			(this object @object,
			BindingFlags flags =
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance,
			Func<string, PropertyInfo, bool> predicate = null) =>
			AllProperties(@object.GetType(), flags, predicate);

		public static IEnumerable<KeyValuePair<string, PropertyInfo>>
			AllProperties
			(this Type type,
			BindingFlags flags =
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance,
			Func<string, PropertyInfo, bool> predicate = null)
		{
			HashSet<string> cache = new(StringComparer.OrdinalIgnoreCase);

			while (type != null)
			{
				IEnumerable<KeyValuePair<string, PropertyInfo>> properties =
					AllProperties_Internal(
						type,
						flags,
						predicate
					);

				foreach (KeyValuePair<string, PropertyInfo> property in properties)
					if (!cache.Contains(property.Key))
					{
						cache.Add(property.Key);
						yield return property;
					}

				type = type.BaseType;
			}
		}

		/// <summary>
		/// Recursive (includes base types.)
		/// </summary>
		private static IEnumerable<KeyValuePair<string, PropertyInfo>> AllProperties_Internal
			(Type type,
			BindingFlags flags,
			Func<string, PropertyInfo, bool> predicate)
		{
			PropertyInfo[] properties =
				type.GetProperties(flags);

			foreach (PropertyInfo property in properties)
			{
				string name = property.Name;

				if (predicate == null ||
					predicate(name, property))
					yield return new(name, property);
			}
		}

		/// <summary>
		/// Copies the field values to another object.
		/// </summary>
		/// <param name="ignores">These fields will be ignored. Case-insensitive.</param>
		public static void CopyFields
			(this object from,
			object to,
			params string[] ignores)
		{
			FieldInfo[] fields =
				from
				.GetType()
				.GetFields(TypeHelper.Generic.GENERIC_MEMBER_FLAG);

			foreach (FieldInfo field in fields)
			{
				string name = field.Name;

				if (name.StartsWith("<"))
					name = name[1..name.IndexOf('>')];

				if (ignores.Any(v => v.ToUpper() == name.ToUpper()))
					continue;

				field.SetValue(to, field.GetValue(from));
			}
		}
	}
}
