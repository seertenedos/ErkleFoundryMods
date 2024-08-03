using UnityEngine;

namespace PlanIt
{

    public static class Extensions
    {
        public static void DestroyAllChildren(this Transform transform, int startIndex = 0)
        {
            for (int i = transform.childCount - 1; i >= startIndex; i--)
            {
                Transform child = transform.GetChild(i);
                child.SetParent(null, false);
                Object.Destroy(child.gameObject);
            }
        }

        public static void Fill<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        public static void Fill<T>(this T[,] array, T value)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] = value;
                }
            }
        }
    }

}