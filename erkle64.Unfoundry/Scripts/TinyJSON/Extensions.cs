using System;
using System.Collections.Generic;
using UnityEngine;


namespace TinyJSON
{
	public static class Extensions
	{
		public static bool AnyOfType<TSource>(this IEnumerable<TSource> source, Type expectedType)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (expectedType == null)
			{
				throw new ArgumentNullException("expectedType");
			}

			foreach (var item in source)
			{
				if (expectedType.IsInstanceOfType(item))
				{
					return true;
				}
			}

			return false;
		}

		public static string GetRelativePath(this Transform transform, Transform root)
		{
			if (transform == null || transform == root) return "";
			var parentPath = transform.parent.GetRelativePath(transform.parent.root);
            return string.IsNullOrEmpty(parentPath) ? transform.name : $"{parentPath}/{transform.name}";
        }
    }
}
