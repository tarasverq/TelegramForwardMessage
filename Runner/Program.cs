using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProxyLib;
using ProxyLib.Proxy;
using Serilog;
using Serilog.Core;
using TelegramForwardMessage;
using TelegramForwardMessage.Exceptions;
using TLSharp.Core.Network;
using TLSharp.Core.Network.Exceptions;

namespace Runner
{
    class Program
    {
        static Logger _log;
        static ProxyClientFactory _proxyClientFactory = new ProxyClientFactory();

        static async Task Main(string[] args)
        {
            _log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("log.txt")
                .CreateLogger();
            _log.Information($"Program start.");


            //Клиент и секрет от него из телеграма
            // int clientId = 2496; string clientHash = "8da85b0d5bfe62527e5b244c209159c3"; //TG WEB
            //int clientId = 2428391; string clientHash = "fd7753c449cdd516e882a8b744277da1"; //MY
            int clientId = 3346028;
            string clientHash = "2daaeff02199ddca5481ce4ab3874298"; //Creamy
            //Телефон без +, иначе не будет работать кеш сессий
            string phone = "79058463205";

            var channels = File.ReadAllLines("channels_adult.txt").Distinct().ToList();
            var success = File.ReadAllLines("success.txt").Distinct().ToList();
            var noLinkedChannel = File.ReadAllLines("no_linked_chat.txt").Distinct().Except(success).ToList();
            var notFound = File.ReadAllLines("not_found.txt").Distinct().Except(success).ToList();
            channels = channels
                .Except(noLinkedChannel)
                .Except(notFound)
                //.Except(success)

                .OrderBy(x => Guid.NewGuid())

                .ToList();
            string[] replies = File.ReadAllLines("replies.txt");
            var delay = new Tuple<int, int>(1, 1);
            //var delay = new Tuple<int, int>(40, 200);

            TelegramMessageForwarder messageForwarder =
                await GetTelegramMessageForwarder(clientId, clientHash, phone, delay);

            await messageForwarder.SetOnline();
            for (int index = 0; index < channels.Count; index++)
            {
                string channel = channels[index];
                _log.Information(string.Empty);
                _log.Information($"{index + 1}/{channels.Count} - {channel}");
                try
                {
                    string message = RandomHelper.GetRandom(replies);
                    await messageForwarder.ReplyInDiscussion(channel, message);


                    _log.Information($"{channel}: {message}.");
                    
                    if (!success.Contains(channel))
                        File.AppendAllText("Success.txt", channel + Environment.NewLine);
                }
                catch (ChannelNotFoundException cnfe)
                {
                    _log.Error($"{channel}: not found.");
                    File.AppendAllText("not_found.txt", $"{channel}{Environment.NewLine}");
                }
                catch (NoLinkedChatException nlce)
                {
                    _log.Error($"{channel}: no linked chat.");
                    File.AppendAllText("no_linked_chat.txt", $"{channel}{Environment.NewLine}");
                }
                catch (NoDiscussionsException nde)
                {
                    _log.Error($"{channel}: NoDiscussion at message {nde.MessageId} \'{nde.MessageText}\'.");
                    File.AppendAllText("no_discussions.txt",
                        $"{channel}|{nde.MessageId}|{nde.MessageText}|{Environment.NewLine}");
                }
                catch (FloodException e)
                {
                    _log.Fatal(e.Demystify(), $"{channel}");
                    _log.Fatal($"Delay for {e.TimeToWait}. End at {DateTime.Now.Add(e.TimeToWait)}");
                    await Task.Delay(e.TimeToWait);
                    messageForwarder = await GetTelegramMessageForwarder(clientId, clientHash, phone, delay);
                    index--;
                }
                catch (InvalidOperationException invalidOperationException)
                    when (invalidOperationException.Message.Trim() ==
                          "CHAT_WRITE_FORBIDDEN")
                {
                    continue;
                }
                catch (Exception exception)
                {
                    //File.AppendAllText("Exceptions.txt", $"{channel}\r\n{exception.Demystify()}\r\n\r\n");
                    _log.Error(exception.Demystify(), $"{channel}");
                }

                int currentDelay = RandomHelper.GetRandomDelay(70, 120);
                _log.Information($"Sleep {currentDelay}");
                await Task.Delay(currentDelay);
            }

            _log.Information("End");
            Console.ReadLine();
        }

        private static async Task<TelegramMessageForwarder> GetTelegramMessageForwarder(int clientId, string clientHash,
            string phone, Tuple<int, int> delay)
        {
            IProxyClient proxyClient = _proxyClientFactory.CreateProxyClient(new ProxyDefinition()
                {
                    Type = ProxyType.Socks5,
                    ProxyHost = "127.0.0.1",
                    ProxyPort = 9150,
                    ProxyUsername = "",
                    ProxyPassword = ""
                }
            );

            TcpClientConnectionHandler handler = (address, port) =>
            {
                return proxyClient.CreateConnection(address, port);
            };

            TelegramMessageForwarder messageForwarder =
                new TelegramMessageForwarder(clientId, clientHash, phone, delay, handler);
            if (await messageForwarder.Initialize())
            {
                _log.Information("Введите код из телеграма");
                //TODO подставить тут код в дебаге
                string code = "1";
                await messageForwarder.SendCode(code);
            }

            return messageForwarder;
        }
    }
}