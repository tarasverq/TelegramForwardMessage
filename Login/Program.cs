using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ProxyLib;
using ProxyLib.Proxy;
using Serilog;
using Serilog.Core;
using TelegramForwardMessage;
using TelegramForwardMessage.Exceptions;
using TLSharp.Core.Network;
using TLSharp.Core.Network.Exceptions;

namespace Login
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
            
            
            
            string input = "79631525925:3502981:4b2fd1531b4e680f7f515fc9ff15c676:185.239.50.127:34513:tarasverqL8:S5p8EvX";
            string[] split = input.Split(':');
            
            string phone = split[0];
            int clientId = int.Parse(split[1]);
            string clientHash = split[2];
            
            string proxyIp = split[3];
            int proxyPort = int.Parse(split[4]);
            string proxyLogin = split[5];
            string proxyPassword = split[6];

            ProxyDefinition proxyDefinition = new ProxyDefinition()
            {
                Type = ProxyType.Socks5,
                ProxyHost = proxyIp,
                ProxyPort = proxyPort,
                ProxyUsername = proxyLogin,
                ProxyPassword = proxyPassword
            };
            
            

            TelegramMessageForwarder messageForwarder = await GetTelegramMessageForwarder(clientId, clientHash, phone, proxyDefinition);
            await messageForwarder.SetOnline();

            _log.Information("End");
        }

        private static async Task<TelegramMessageForwarder> GetTelegramMessageForwarder(int clientId, string clientHash,
            string phone, ProxyDefinition definition)
        {
            IProxyClient proxyClient = _proxyClientFactory.CreateProxyClient(definition);

            TcpClientConnectionHandler handler = (address, port) =>
            {
                return proxyClient.CreateConnection(address, port);
            };

            TelegramMessageForwarder messageForwarder = new TelegramMessageForwarder(clientId, clientHash, phone, delay, handler);
            if (await messageForwarder.Initialize())
            {
                _log.Information($"Введите код из телеграма. Аккаунт {phone}");
                string code = Console.ReadLine();
                await messageForwarder.SendCode(code);
            }

            return messageForwarder;
        }
    }
}