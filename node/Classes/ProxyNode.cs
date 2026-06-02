using System.Threading.Tasks;
using raftLibrary;

namespace node.Classes;

/// <summary>
/// A proxy node that wraps a real Node and simulates network latency
/// </summary>
public class ProxyNode : INode
{
    public string Name { get; set; }
    public Node InnerNode { get; }
    
    /// <summary>
    /// Simulated network delay in milliseconds
    /// </summary>
    public int NetworkDelayMs { get; set; } = 100;
    
    /// <summary>
    /// Election timeout in milliseconds (delegates to InnerNode)
    /// </summary>
    public int TimeoutMs
    {
        get => InnerNode.TimeoutMs;
        set => InnerNode.TimeoutMs = value;
    }
    
    /// <summary>
    /// Current timeout in milliseconds (delegates to InnerNode)
    /// </summary>
    public int CurrentTimeoutMs
    {
        get => InnerNode.CurrentTimeoutMs;
        set => InnerNode.CurrentTimeoutMs = value;
    }
    
    /// <summary>
    /// Current timeout progress (0-100%)
    /// </summary>
    public double TimeoutProgress { get; set; } = 0;
    
    /// <summary>
    /// Whether the node is currently timing out
    /// </summary>
    public bool IsTimingOut { get; set; } = false;

    public ProxyNode(string name, bool isRandomized = true)
    {
        Name = name;
        InnerNode = new Node(isRandomized)
        {
            State = NodeState.Follower,
            CurrentTerm = 0,
            Peers = new List<INode>()
        };
    }

    /// <summary>
    /// Start the election timer for this node. Call this after peers are configured.
    /// </summary>
    public void StartElectionTimer()
    {
        Console.WriteLine($"[ProxyNode.StartElectionTimer] Node={Name} starting election timer");
        _ = Task.Run(async () => await InnerNode.RunElectionTimer());
    }

    public NodeState State
    {
        get => InnerNode.State;
        set => InnerNode.State = value;
    }

    public int CurrentTerm
    {
        get => InnerNode.CurrentTerm;
        set => InnerNode.CurrentTerm = value;
    }

    /// <summary>
    /// Gets the state machine dictionary from the inner node
    /// </summary>
    public System.Collections.Concurrent.ConcurrentDictionary<string, string> StateMachine
    {
        get => InnerNode.stateMachine;
    }

    /// <summary>
    /// Gets the logs list from the inner node
    /// </summary>
    public List<LogEntry> Logs
    {
        get => InnerNode.Logs;
    }

    /// <summary>
    /// Gets the CommitIndex from the inner node
    /// </summary>
    public int CommitIndex
    {
        get => InnerNode.CommitIndex;
    }

    /// <summary>
    /// Gets the LastApplied from the inner node
    /// </summary>
    public int LastApplied
    {
        get => InnerNode.LastApplied;
    }
    public async Task<string> RecieveCommand(string logEntry)
    {
        Console.WriteLine($"[ProxyNode.RecieveCommand] Node={Name} received command: {logEntry} (delay {NetworkDelayMs}ms)");
        // Simulate network delay
        await Task.Delay(NetworkDelayMs);
        
        var result = await InnerNode.RecieveCommand(logEntry);
        Console.WriteLine($"[ProxyNode.RecieveCommand] Node={Name} command result: {result}");
        return result;
    }
    public async Task<bool> VoteFor( int term, INode voter)
    {
        Console.WriteLine($"[ProxyNode.VoteFor] Node={Name} voting in term {term} for {voter.Name} (delay {NetworkDelayMs}ms)");
        // Simulate network delay
        await Task.Delay(NetworkDelayMs);
        
        var result = await InnerNode.VoteFor(term, voter);
        Console.WriteLine($"[ProxyNode.VoteFor] Node={Name} vote result: {result}");
        return result;
    }
    public async Task<bool> AppendEntriesResponse(int term, INode follower, int lastLogIndex, bool success)
    {
        Console.WriteLine($"[ProxyNode.AppendEntriesResponse] Node={Name} from {follower.Name}: term={term} lastLogIndex={lastLogIndex} success={success} (delay {NetworkDelayMs}ms)");
        // Simulate network delay
        await Task.Delay(NetworkDelayMs);
        
        var result = await InnerNode.AppendEntriesResponse(term, follower, lastLogIndex, success);
        Console.WriteLine($"[ProxyNode.AppendEntriesResponse] Node={Name} response result: {result}");
        return result;
    }
    public async Task<bool> AppendEntries(int term, INode leaderId, int prevLogIndex, int prevLogTerm, LogEntry[] entries, int leaderCommit)
    {
        Console.WriteLine($"[ProxyNode.AppendEntries] Node={Name} from {leaderId.Name}: term={term} prevLogIndex={prevLogIndex} entries.Count={entries.Length} leaderCommit={leaderCommit} (delay {NetworkDelayMs}ms)");
        // Simulate network delay
        await Task.Delay(NetworkDelayMs);
        
        // Reset timeout when receiving AppendEntries from leader
        TimeoutProgress = 0;
        
        var result = await InnerNode.AppendEntries(term, leaderId, prevLogIndex, prevLogTerm, entries, leaderCommit);
        Console.WriteLine($"[ProxyNode.AppendEntries] Node={Name} append result: {result}");
        return result;
    }

    public async Task<bool> RequestVote(int term, INode candidateId, int lastLogIndex, int lastLogTerm)
    {
        Console.WriteLine($"[ProxyNode.RequestVote] Node={Name} from {candidateId.Name}: term={term} lastLogIndex={lastLogIndex} lastLogTerm={lastLogTerm} (delay {NetworkDelayMs}ms)");
        // Simulate network delay
        await Task.Delay(NetworkDelayMs);
        
        var result = await InnerNode.RequestVote(term, candidateId, lastLogIndex, lastLogTerm);
        Console.WriteLine($"[ProxyNode.RequestVote] Node={Name} vote request result: {result}");
        return result;
    }

    /// <summary>
    /// Increment the timeout progress based on elapsed time
    /// </summary>
    public void IncrementTimeout(double elapsedMs)
    {
        if (State != NodeState.Leader)
        {
            double oldProgress = TimeoutProgress;
            TimeoutProgress += (elapsedMs / TimeoutMs) * 100;
            if (TimeoutProgress >= 100)
            {
                TimeoutProgress = 100;
                IsTimingOut = true;
                Console.WriteLine($"[ProxyNode.IncrementTimeout] Node={Name} TIMEOUT! Progress went from {oldProgress:F1}% to {TimeoutProgress:F1}%");
            }
        }
        else
        {
            TimeoutProgress = 0;
        }
    }

    /// <summary>
    /// Reset the timeout progress
    /// </summary>
    public void ResetTimeout()
    {
        Console.WriteLine($"[ProxyNode.ResetTimeout] Node={Name} resetting timeout (was at {TimeoutProgress:F1}%)");
        TimeoutProgress = 0;
        IsTimingOut = false;
    }
}
