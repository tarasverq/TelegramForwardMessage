using System;
using System.Collections.Generic;

namespace TelegramForwardMessage
{
    public static class RandomHelper
    {
        private static Random Random = new Random();
        
        public static T GetRandom<T>(IList<T> collection)
        {
            int index = Random.Next(0, collection.Count);
            var item = collection[index];
            return item;
        }

        public static int GetRandomInt(int from, int to) => Random.Next(from, to);
        public static int GetRandomDelay(int from, int to) => GetRandomInt(from, to) * GetRandomInt(800, 1200);
    }
}