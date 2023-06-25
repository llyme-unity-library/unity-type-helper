using System;
using System.Reflection;

namespace TypeHelper
{
	public static class Generic
	{
		/// <summary>
		/// Captures both public and private fields/properties.
		/// </summary>
		public readonly static BindingFlags GENERIC_MEMBER_FLAG =
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance;
			
		/// <summary>
		/// Attempts to find the `PropertyInfo` in the class hierarchy,
		/// preferably the declaring type.
		/// </summary>
		public static PropertyInfo GetPropertyInHierarchy
			(this Type type,
			string name,
			BindingFlags flags)
		{
			PropertyInfo info =
				type.GetProperty(name, flags);

			if (info != null &&
				info.DeclaringType == type)
				return info;

			if (type.BaseType != null)
				return GetPropertyInHierarchy(type.BaseType, name, flags);

			return info;
		}

		/// <summary>
		/// Attempts to find the `FieldInfo` in the class hierarchy,
		/// preferably the declaring type.
		/// </summary>
		public static FieldInfo GetFieldInHierarchy
			(this Type type,
			string name,
			BindingFlags flags)
		{
			FieldInfo info = type.GetField(name, flags);

			if (info != null &&
				info.DeclaringType == type)
				return info;

			if (type.BaseType != null)
				return GetFieldInHierarchy(type.BaseType, name, flags);

			return info;
		}

		public static Type FromFullName<T>(string classFullName)
		{
			if (string.IsNullOrEmpty(classFullName))
				return null;

			Type root = typeof(T);
			Assembly assembly = Assembly.GetAssembly(root);
			classFullName = classFullName.ToUpper();

			foreach (Type type in assembly.GetTypes())
				if (type.IsClass &&
					!type.IsAbstract &&
					type.IsSubclassOf(root) &&
					type.FullName.ToUpper() == classFullName)
					return type;

			return null;
		}

		/// <summary>
		/// If the assembly does not exist,
		/// other assemblies are used instead.
		/// </summary>
		public static Type FromFullName
			(string assemblyFullName,
			string typeFullName)
		{
			if (string.IsNullOrEmpty(typeFullName))
				return null;

			Assembly assembly =
				Assembly.Load(assemblyFullName);

			if (assembly == null)
			{
				Assembly[] assemblies =
					AppDomain.CurrentDomain.GetAssemblies();

				foreach (Assembly assembly0 in assemblies)
				{
					Type type =
						assembly0.GetType(typeFullName);

					if (type != null)
						return type;
				}
			}
			else
				return assembly.GetType(typeFullName);

			return null;
		}
	}
}
