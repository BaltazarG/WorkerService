using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService
{
    public interface IAzBusService
    {
        public Task GetQueues(CancellationToken stoppingToken);
        public Task ProcessMessages();
        public Task MessageHandler(ProcessMessageEventArgs args);
        public Task ErrorHandler(ProcessErrorEventArgs arg);

    }
}
