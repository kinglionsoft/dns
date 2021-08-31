using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DNS.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsProxy
{
    public class DnsWorker : IHostedService
    {
        private readonly ILogger<DnsWorker> _logger;
        private readonly DnsOptions _options;
        private DnsServer _server;

        public DnsWorker(ILogger<DnsWorker> logger, IOptions<DnsOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            MasterFile masterFile = new();

            foreach (var (domain, ip) in _options.Records)
            {
                masterFile.AddIPAddressResourceRecord(domain, ip);
            }

            _server = new DnsServer(masterFile, _options.EndServer);
            // _server.Responded += _server_Responded;
            _server.Listening += _server_Listening;
            _server.Errored += _server_Errored;

            await _server.Listen(_options.Port).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _server?.Dispose();
            return Task.CompletedTask;
        }

        private void _server_Errored(object sender, DnsServer.ErroredEventArgs e)
        {
            _logger.LogError(e.Exception, "sever error");
        }

        private void _server_Listening(object sender, EventArgs e)
        {
            _logger.LogInformation("listening at {0}", _options.Port);
        }

        private void _server_Responded(object sender, DnsServer.RespondedEventArgs e)
        {
            _logger.LogInformation("requested: {0} -> {1}", e.Request.Questions, e.Response.AnswerRecords);
        }
    }
}
