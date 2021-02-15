using ProxyLib.Proxy;

namespace ProxyLib
{
    public class ProxyDefinition
    {
        public ProxyType Type { get; init; }
        public string ProxyHost { get; init; }
        public int ProxyPort { get; init; }
        public string ProxyUsername { get; init; }
        public string ProxyPassword { get; init; }
    }
}