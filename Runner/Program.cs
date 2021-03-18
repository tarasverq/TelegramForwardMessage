using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
        static  Tuple<int, int> delay = new Tuple<int, int>(0, 0);
        static async Task Main(string[] args)
        {
            _log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("log.txt")
                .CreateLogger();
            _log.Information($"Program start.");

           
            CircularList<Account> accounts = new CircularList<Account>(
                File.ReadAllLines("accounts.txt")
                    .Where(x=> !x.StartsWith("#"))
                    .Select(x=> new Account(x)).ToList());

            var channels = File.ReadAllLines("channels_adult.txt")
                .Where(x=> !x.StartsWith("_"))
                .Distinct().ToList();
            var success = File.ReadAllLines("success.txt").Distinct().ToList();
            var noLinkedChannel = File.ReadAllLines("no_linked_chat.txt").Distinct().Except(success).ToList();
            var notFound = File.ReadAllLines("not_found.txt").Distinct().Except(success).ToList();
            channels = channels
                .Except(noLinkedChannel)
                .Except(notFound)
                //.OrderBy(x => Guid.NewGuid())
                .Shuffle()
                .ToList();
            string[] replies = File.ReadAllLines("replies.txt");
            
            Dictionary<Account, TelegramMessageForwarder> forwarders = new ();

            for (int index = 0; index < channels.Count; index++)
            {
                Account account = accounts.GetNext();
                
                if(!forwarders.TryGetValue(account, out TelegramMessageForwarder messageForwarder))
                {
                    messageForwarder = await GetTelegramMessageForwarder(account);
                    forwarders.Add(account, messageForwarder);
                }

                string channel = channels[index];
                _log.Information(string.Empty);
                _log.Information($"{index + 1}/{channels.Count} - {channel}. Account: {account.Phone}");
                try
                {
                    string message = RandomHelper.GetRandom(replies);
                    //await messageForwarder.ReplyInDiscussion(channel, message);
                    await messageForwarder.ReplyInDiscussion("sex_educatiion", message);


                    _log.Information($"{account.Phone}: {channel}: {message}.");

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
                    messageForwarder = await GetTelegramMessageForwarder(account);
                    forwarders[account] = messageForwarder;
                    index--;
                }
                catch (InvalidOperationException invalidOperationException)
                    when (invalidOperationException.Message.Trim() ==
                          "CHAT_WRITE_FORBIDDEN")
                {
                    continue;
                }
                catch (WebException e)
                {
                    _log.Fatal(e.Demystify(), $"{channel}");
                    messageForwarder = await GetTelegramMessageForwarder(account);
                    forwarders[account] = messageForwarder;
                }
                catch (Exception exception)
                {
                    //File.AppendAllText("Exceptions.txt", $"{channel}\r\n{exception.Demystify()}\r\n\r\n");
                    _log.Error(exception.Demystify(), $"{channel}");
                }

                int currentDelay = RandomHelper.GetRandomDelay(80, 150);
                _log.Information($"Sleep {currentDelay}");
                await Task.Delay(currentDelay);
            }

            _log.Information("End");
            Console.ReadLine();
        }

        private static Task<TelegramMessageForwarder> GetTelegramMessageForwarder
            (Account account)
        {
            return GetTelegramMessageForwarder(account.ClientId, account.ClientHash, account.Phone,
                account.ProxyDefinition);
        }

        private static async Task<TelegramMessageForwarder> GetTelegramMessageForwarder
            (int clientId, string clientHash, string phone, ProxyDefinition definition = null)
        {
            TcpClientConnectionHandler handler = null;

            if (definition != null)
            {
                IProxyClient proxyClient = _proxyClientFactory.CreateProxyClient(definition);

                handler = (address, port) => { return proxyClient.CreateConnection(address, port); };
            }

            TelegramMessageForwarder messageForwarder =
                new TelegramMessageForwarder(clientId, clientHash, phone, delay, handler);
            if (await messageForwarder.Initialize())
            {
                _log.Information("Введите код из телеграма");
                //TODO подставить тут код в дебаге
                string code = "1";
                await messageForwarder.SendCode(code);
            }
            
            messageForwarder.RunOnlineSending();

            return messageForwarder;
        }
    }
}