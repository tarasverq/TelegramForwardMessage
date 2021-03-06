using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
        
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                // ... except we don't really need to swap it fully, as we can
                // return it immediately, and afterwards it's irrelevant.
                int swapIndex = Random.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }
        
        public static void SuperMario()
    {
        Console.Beep(659, 125);
        Console.Beep(659, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(523, 125);
        Console.Beep(659, 125);
        Thread.Sleep(125);
        Console.Beep(784, 125);
        Thread.Sleep(375);
        Console.Beep(392, 125);
        Thread.Sleep(375);
        Console.Beep(523, 125);
        Thread.Sleep(250);
        Console.Beep(392, 125);
        Thread.Sleep(250);
        Console.Beep(330, 125);
        Thread.Sleep(250);
        Console.Beep(440, 125);
        Thread.Sleep(125);
        Console.Beep(494, 125);
        Thread.Sleep(125);
        Console.Beep(466, 125);
        Thread.Sleep(42);
        Console.Beep(440, 125);
        Thread.Sleep(125);
        Console.Beep(392, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(125);
        Console.Beep(784, 125);
        Thread.Sleep(125);
        Console.Beep(880, 125);
        Thread.Sleep(125);
        Console.Beep(698, 125);
        Console.Beep(784, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(125);
        Console.Beep(523, 125);
        Thread.Sleep(125);
        Console.Beep(587, 125);
        Console.Beep(494, 125);
        Thread.Sleep(125);
        Console.Beep(523, 125);
        Thread.Sleep(250);
        Console.Beep(392, 125);
        Thread.Sleep(250);
        Console.Beep(330, 125);
        Thread.Sleep(250);
        Console.Beep(440, 125);
        Thread.Sleep(125);
        Console.Beep(494, 125);
        Thread.Sleep(125);
        Console.Beep(466, 125);
        Thread.Sleep(42);
        Console.Beep(440, 125);
        Thread.Sleep(125);
        Console.Beep(392, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(125);
        Console.Beep(784, 125);
        Thread.Sleep(125);
        Console.Beep(880, 125);
        Thread.Sleep(125);
        Console.Beep(698, 125);
        Console.Beep(784, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(125);
        Console.Beep(523, 125);
        Thread.Sleep(125);
        Console.Beep(587, 125);
        Console.Beep(494, 125);
        Thread.Sleep(375);
        Console.Beep(784, 125);
        Console.Beep(740, 125);
        Console.Beep(698, 125);
        Thread.Sleep(42);
        Console.Beep(622, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(415, 125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Thread.Sleep(125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Console.Beep(587, 125);
        Thread.Sleep(250);
        Console.Beep(784, 125);
        Console.Beep(740, 125);
        Console.Beep(698, 125);
        Thread.Sleep(42);
        Console.Beep(622, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(698, 125);
        Thread.Sleep(125);
        Console.Beep(698, 125);
        Console.Beep(698, 125);
        Thread.Sleep(625);
        Console.Beep(784, 125);
        Console.Beep(740, 125);
        Console.Beep(698, 125);
        Thread.Sleep(42);
        Console.Beep(622, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(415, 125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Thread.Sleep(125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Console.Beep(587, 125);
        Thread.Sleep(250);
        Console.Beep(622, 125);
        Thread.Sleep(250);
        Console.Beep(587, 125);
        Thread.Sleep(250);
        Console.Beep(523, 125);
        Thread.Sleep(1125);
        Console.Beep(784, 125);
        Console.Beep(740, 125);
        Console.Beep(698, 125);
        Thread.Sleep(42);
        Console.Beep(622, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(415, 125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Thread.Sleep(125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Console.Beep(587, 125);
        Thread.Sleep(250);
        Console.Beep(784, 125);
        Console.Beep(740, 125);
        Console.Beep(698, 125);
        Thread.Sleep(42);
        Console.Beep(622, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(698, 125);
        Thread.Sleep(125);
        Console.Beep(698, 125);
        Console.Beep(698, 125);
        Thread.Sleep(625);
        Console.Beep(784, 125);
        Console.Beep(740, 125);
        Console.Beep(698, 125);
        Thread.Sleep(42);
        Console.Beep(622, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(415, 125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Thread.Sleep(125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Console.Beep(587, 125);
        Thread.Sleep(250);
        Console.Beep(622, 125);
        Thread.Sleep(250);
        Console.Beep(587, 125);
        Thread.Sleep(250);
        Console.Beep(523, 125);
        Thread.Sleep(625);
    }
    }
}