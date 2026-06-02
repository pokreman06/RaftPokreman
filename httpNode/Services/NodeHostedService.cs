namespace httpNode.Services
{
    public class NodeHostedService : BackgroundService
    {
        private readonly NodeService _nodeService;
        private readonly ILogger<NodeHostedService> _logger;

        public NodeHostedService(NodeService nodeService, ILogger<NodeHostedService> logger)
        {
            _nodeService = nodeService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[NodeHostedService] Starting node election timer...");
            
            try
            {
                var node = _nodeService.GetCurrentNode();
                await node.RunElectionTimer();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[NodeHostedService] Node election timer was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NodeHostedService] Error running node election timer");
            }
        }
    }
}
