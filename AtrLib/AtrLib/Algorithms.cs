using System;
using System.Collections.Generic;

namespace AtrLib
{
    public static class Algorithms
    {
        public static IDictionary<int, int> SumTwoNumbers(ICollection<int> collection, int x)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var temp = new Dictionary<int, bool>();
            int pairsCount = 0;
            foreach (var item in collection)
            {
                if (item < 0 && x > (int.MaxValue + item))
                    continue;
                if (item > 0 && x < (int.MinValue + item))
                    continue;

                int diff = x - item;
                if (temp.ContainsKey(diff))
                {
                    if (!temp[diff])
                    {
                        temp[diff] = true;
                        pairsCount++;
                    }
                }
                else
                {
                    if (!temp.ContainsKey(item))
                    {
                        temp.Add(item, false);
                    }
                }
            }
            var result = new Dictionary<int, int>(pairsCount);
            foreach (var item in temp)
            {
                if (item.Value)
                {
                    result.Add(item.Key, x - item.Key);
                }
            }
            return result;
        }
    }
}