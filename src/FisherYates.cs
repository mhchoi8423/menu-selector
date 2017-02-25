using System;

namespace MenuSelector
{
    /// <see href="https://en.wikipedia.org/wiki/Fisher–Yates_shuffle"/>
    public class FisherYates
    {
        private static Random Random { get; set; }

        static FisherYates()
        {
            Random = new Random(Guid.NewGuid().GetHashCode() ^ Guid.NewGuid().GetHashCode());
        }

        public static void Shuffle<T>(T[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                var j = i + Random.Next(array.Length - i);
                var temp = array[j];
                array[j] = array[i];
                array[i] = temp;
            }
        }
    }
}
