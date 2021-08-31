using System.Collections.Generic;

namespace DnsProxy
{
    public sealed class DnsOptions
    {
        public int Port { get; set; } = 53;

        public string EndServer { get; set; } = "114.114.114.114";

        public Dictionary<string, string> Records { get; set; } = new();
    }
}