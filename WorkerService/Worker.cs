namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IAzBusService _azBusService;
        private readonly PeriodicTimer _timer =
            new(TimeSpan.FromMilliseconds(Int32.Parse(
                System.Configuration.ConfigurationManager.AppSettings["timer"])));

        public Worker(ILogger<Worker> logger, IAzBusService azBusService)
        {
            _logger = logger;
            _azBusService = azBusService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await _azBusService.GetQueues(stoppingToken);
            }
        }
    }
}