using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using TelegramForwardMessage;
using TLSharp.Core.Network.Exceptions;

namespace Runner
{
    class Program
    {
       
        static async Task Main(string[] args)
        {
            //Клиент и секрет от него из телеграма
            int clientId = 2496;
            string clientHash = "8da85b0d5bfe62527e5b244c209159c3";
            //Телефон без +, иначе не будет работать кеш сессий
            string phone = "79284571449";
           
            string[] channels =  File.ReadAllLines("channels.txt");
            string[] replies =  File.ReadAllLines("replies.txt");
            var delay = new Tuple<int, int>(8, 12);
            
            ITelegramMessageForwarder messageForwarder = new TelegramMessageForwarder(clientId, clientHash, phone, delay);
            if (await messageForwarder.Initialize())
            {
                Console.WriteLine("Введите код из телеграма");
                //TODO подставить тут код в дебаге
                string code = "1";
                await messageForwarder.SendCode(code);
            }

            foreach (string channel in channels)
            {
                try
                {
                    string message = RandomHelper.GetRandom(replies);
                    await messageForwarder.ReplyInDiscussion(channel, message);

                    Console.WriteLine($"{channel}: {message}.");
                }
                catch (NoDiscussionsException nde)
                {
                    Console.WriteLine($"{channel}: NoDiscussions.");
                    File.AppendAllText("no_discussions.txt", channel + Environment.NewLine);
                }
                catch (FloodException e)
                {
                    Console.WriteLine($"Delay for {e.TimeToWait}");
                    await Task.Delay(e.TimeToWait);
                }
                catch (Exception exception)
                {
                    File.AppendAllText("Exceptions.txt", $"{channel}\r\n{exception}\r\n\r\n");
                    Console.WriteLine(exception);
                }
                int currentDelay = RandomHelper.GetRandomDelay(50, 70);
                Console.WriteLine($" Sleep {currentDelay}");
                await Task.Delay(currentDelay);
            }
            
            Console.WriteLine("End");
            Console.ReadLine();
        }

       
       
    }
}