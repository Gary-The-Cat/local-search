using System;
using System.Collections.Generic;

namespace Game.ExtensionMethods
{
    public static class ListExtensions
    {
        private static Random random = new Random();

        // Shuffles the elements of the list in place
        public static void Shuffle<T>(this IList<T> list)
        {
            int count = list.Count;
            while (count > 1)
            {
                count--;
                int randomIndex = random.Next(count + 1);
                T temp = list[randomIndex];
                list[randomIndex] = list[count];
                list[count] = temp;
            }
        }

        // Swaps two elements in the list at specified indices
        public static void SwapInPlace<T>(this IList<T> list, int indexA, int indexB)
        {
            T temp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = temp;
        }

        // Returns distinct elements from a sequence by using a specified key selector to compare values
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}