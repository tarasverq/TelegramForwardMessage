using ProxyLib;
using ProxyLib.Proxy;

namespace Runner
{
    public class Account
    {
        private readonly string _input;
        public ProxyDefinition ProxyDefinition { get; }
        public string Phone { get; }
        public int ClientId { get; }
        public string ClientHash { get; }

        public Account(string input)
        {
            _input = input;
            string[] split = input.Split(':');

            Phone = split[0];
            ClientId = int.Parse(split[1]);
            ClientHash = split[2];

            if (split.Length > 3)
            {
                string proxyIp = split[3];
                int proxyPort = int.Parse(split[4]);
                string proxyLogin = split[5];
                string proxyPassword = split[6];

                ProxyDefinition = new ProxyDefinition()
                {
                    Type = ProxyType.Socks5,
                    ProxyHost = proxyIp,
                    ProxyPort = proxyPort,
                    ProxyUsername = proxyLogin,
                    ProxyPassword = proxyPassword
                };
            }
        }

        public override string ToString()
        {
            return _input;
        }
    }
}