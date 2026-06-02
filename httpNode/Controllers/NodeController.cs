using Microsoft.AspNetCore.Mvc;
using httpNode.Services;
using raftLibrary;
using System.Text.Json;

namespace httpNode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NodeController : ControllerBase
    {
        private readonly NodeService _nodeService;
        private readonly ILogger<NodeController> _logger;

        public NodeController(NodeService nodeService, ILogger<NodeController> logger)
        {
            _nodeService = nodeService;
            _logger = logger;
        }

        private ActionResult CheckNodeStatus()
        {
            var node = _nodeService.GetCurrentNode();
            if (!node.IsUp)
            {
                return StatusCode(503, "Node is shut down and not available");
            }
            return Ok();
        }
        [HttpGet("details")]
        public ActionResult<Node> GetNodeDetails()
        {
            try
            {
                var statusCheck = CheckNodeStatus();
                if (statusCheck is not OkResult) return statusCheck;
                var node = _nodeService.GetCurrentNode();
                return Ok(node);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting node details");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("command")]
        public async Task<ActionResult<string>> ReceiveCommand([FromBody] CommandRequest request)
        {
            try
            {
                var statusCheck = CheckNodeStatus();
                if (statusCheck is not OkResult) return statusCheck;
                var node = _nodeService.GetCurrentNode();
                var result = await node.RecieveCommand(request.Command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving command");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("addlog")]
        public async Task<ActionResult<bool>> AddLog([FromBody] LogEntry logEntry)
        {
            try
            {
                var statusCheck = CheckNodeStatus();
                if (statusCheck is not OkResult) return statusCheck;
                var node = _nodeService.GetCurrentNode();
                var result = await node.AddLog(logEntry);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding log");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("votefor")]
        public async Task<ActionResult<bool>> VoteFor([FromBody] VoteForRequest request)
        {
            try
            {
                var statusCheck = CheckNodeStatus();
                if (statusCheck is not OkResult) return statusCheck;
                var node = _nodeService.GetCurrentNode();
                var voter = _nodeService.GetNodeByName(request.VoterId);
                var result = await node.VoteFor(request.Term, voter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("appendentriesresponse")]
        public async Task<ActionResult<bool>> AppendEntriesResponse([FromBody] AppendEntriesResponseRequest request)
        {
            try
            {
                var statusCheck = CheckNodeStatus();
                if (statusCheck is not OkResult) return statusCheck;
                var node = _nodeService.GetCurrentNode();
                var follower = _nodeService.GetNodeByName(request.FollowerId);
                var result = await node.AppendEntriesResponse(request.Term, follower, request.LastLogIndex, request.Success);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in append entries response");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("appendentries")]
        public async Task<ActionResult<bool>> AppendEntries([FromBody] AppendEntriesRequest request)
        {
            try
            {
                var statusCheck = CheckNodeStatus();
                if (statusCheck is not OkResult) return statusCheck;
                var node = _nodeService.GetCurrentNode();
                var leader = _nodeService.GetNodeByName(request.LeaderId);
                var result = await node.AppendEntries(request.Term, leader, request.PrevLogIndex, request.PrevLogTerm, request.Entries, request.LeaderCommit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in append entries");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("requestvote")]
        public async Task<ActionResult<bool>> RequestVote([FromBody] RequestVoteRequest request)
        {
            try
            {
                var statusCheck = CheckNodeStatus();
                if (statusCheck is not OkResult) return statusCheck;
                var node = _nodeService.GetCurrentNode();
                var candidate = _nodeService.GetNodeByName(request.CandidateId);
                var result = await node.RequestVote(request.Term, candidate, request.LastLogIndex, request.LastLogTerm);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting vote");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("routinemaintenance")]
        public ActionResult RoutineMaintenance()
        {
            try
            {
                var statusCheck = CheckNodeStatus();
                if (statusCheck is not OkResult) return statusCheck;
                var node = _nodeService.GetCurrentNode();
                node.RoutineMaintenanceTasks();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in routine maintenance");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("timeout")]
        public ActionResult Timeout()
        {
            try
            {
                var statusCheck = CheckNodeStatus();
                if (statusCheck is not OkResult) return statusCheck;
                var node = _nodeService.GetCurrentNode();
                node.Timeout();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in timeout");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("runelectiontimer")]
        public async Task<ActionResult> RunElectionTimer()
        {
            try
            {
                var statusCheck = CheckNodeStatus();
                if (statusCheck is not OkResult) return statusCheck;
                var node = _nodeService.GetCurrentNode();
                await node.RunElectionTimer();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running election timer");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("winelection")]
        public ActionResult WinElection()
        {
            try
            {
                var statusCheck = CheckNodeStatus();
                if (statusCheck is not OkResult) return statusCheck;
                var node = _nodeService.GetCurrentNode();
                node.WinElection();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in win election");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("logcommitted")]
        public ActionResult LogCommitted()
        {
            try
            {
                var statusCheck = CheckNodeStatus();
                if (statusCheck is not OkResult) return statusCheck;
                var node = _nodeService.GetCurrentNode();
                node.LogCommitted();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in log committed");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("status")]
        public ActionResult<NodeStatusResponse> GetStatus()
        {
            try
            {
                var node = _nodeService.GetCurrentNode();
                if (!node.IsUp)
                {
                    return StatusCode(503, "Node is shut down and not available");
                }
                return Ok(new NodeStatusResponse
                {
                    Name = node.Name,
                    State = node.State,
                    CurrentTerm = node.CurrentTerm,
                    CommitIndex = node.CommitIndex,
                    LastApplied = node.LastApplied,
                    LogsCount = node.Logs.Count,
                    MatchIndex = node.State == NodeState.Leader ? node.MatchIndex : null,
                    IsUp = node.IsUp,
                    CurrentTimeoutMs = node.CurrentTimeoutMs,
                    TimeoutMs = node.TimeoutMs,
                    StateMachine = new Dictionary<string, string>(node.stateMachine)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("shutdown")]
        public ActionResult Shutdown()
        {
            try
            {
                var node = _nodeService.GetCurrentNode();
                if (node.IsUp)
                {
                    node.Shutdown();
                    return Ok("Node shut down");
                }
                else
                {
                    node.TurnOn();
                    return Ok("Node turned on");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during shutdown/turnon");
                return BadRequest(ex.Message);
            }
        }
    }

    // Request DTOs
    public class CommandRequest
    {
        public string Command { get; set; } = "";
    }

    public class VoteForRequest
    {
        public int Term { get; set; }
        public string VoterId { get; set; } = "";
    }

    public class AppendEntriesResponseRequest
    {
        public int Term { get; set; }
        public string FollowerId { get; set; } = "";
        public int LastLogIndex { get; set; }
        public bool Success { get; set; }
    }

    public class AppendEntriesRequest
    {
        public int Term { get; set; }
        public string LeaderId { get; set; } = "";
        public int PrevLogIndex { get; set; }
        public int PrevLogTerm { get; set; }
        public LogEntry[] Entries { get; set; } = [];
        public int LeaderCommit { get; set; }
    }

    public class RequestVoteRequest
    {
        public int Term { get; set; }
        public string CandidateId { get; set; } = "";
        public int LastLogIndex { get; set; }
        public int LastLogTerm { get; set; }
    }

    public class NodeStatusResponse
    {
        public string Name { get; set; } = "";
        public NodeState State { get; set; }
        public int CurrentTerm { get; set; }
        public int CommitIndex { get; set; }
        public int LastApplied { get; set; }
        public int LogsCount { get; set; }
        public int[]? MatchIndex { get; set; }
        public bool IsUp { get; set; } = true;
        public int CurrentTimeoutMs { get; set; }
        public int TimeoutMs { get; set; }
        public Dictionary<string, string> StateMachine { get; set; } = new();
    }
}
