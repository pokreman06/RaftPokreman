using System.Collections.Concurrent;
using System.Data;
namespace raftLibrary;

public interface ITimer
{
    public int TimeoutMs { get; set; }
    public int CurrentTimeoutMs { get; set; }
    public int step { get; set; }
    public void ResetTimeout();
}
public class ResetTimer : ITimer
{
    private Random _random = new Random();
    public bool IsRandomized { get; set; } = false;
    public int TimeoutMs { get; set; } = 1000;
    public int CurrentTimeoutMs { get; set; } = 1000;
    public int step { get; set; } = 5;

    public ResetTimer(bool isRandomized = false)
    {
        IsRandomized = isRandomized;
    }
    public void ResetTimeout()
    {
        if (IsRandomized)
        {
            int randomVariation = _random.Next(-TimeoutMs / 5, TimeoutMs / 5);
            CurrentTimeoutMs = TimeoutMs + randomVariation;
            Console.WriteLine($"[ResetTimer.ResetTimeout] Randomized timeout: {CurrentTimeoutMs}ms (base: {TimeoutMs}ms, variation: {randomVariation}ms)");
        }
        else
        {
            CurrentTimeoutMs = TimeoutMs;

            Console.WriteLine($"[ResetTimer.ResetTimeout] Reset timeout to {CurrentTimeoutMs}ms");
        }
    }
}
public class LogEntry
{
    public int index { get; set; }
    public int term { get; set; }
    public string Command { get; set; } = "";
}
public class Node : INode
{
    public ConcurrentDictionary<string, string> stateMachine = new ConcurrentDictionary<string, string>();
    public string Name { get; set; } = Guid.NewGuid().ToString();
    public int CurrentTerm { get; set; }
    public INode? VotedFor { get; set; }
    public int CommitIndex { get; set; } = -1;
    public int LastApplied { get; set; } = -1;
    public int[]? NextIndex { get; set; }
    public int[]? MatchIndex { get; set; }
    public NodeState State { get; set; } = NodeState.Follower;
    public List<INode> Peers { get; set; } = new List<INode>();
    public List<INode> ResponseFollowers { get; set; } = new List<INode>();
    public List<LogEntry> Logs { get; set; } = new List<LogEntry>();
    public int TimeoutMs { get => timer.TimeoutMs; set => timer.TimeoutMs = value; }
    public int CurrentTimeoutMs { get => timer.CurrentTimeoutMs; set => timer.CurrentTimeoutMs = value; }
    public int leadertimeout = 500;
    public int step { get => timer.step; set => timer.step = value; }
    public int votes = 0;
    public INode? Leader;
    public ITimer timer = new ResetTimer();
    public bool IsRandomized { get; set; } = false;
    public bool IsUp { get; set; } = true;
    public Node(bool isRandomized = false)
    {
        IsRandomized = isRandomized;
        if (timer is ResetTimer resetTimer)
        {
            resetTimer.IsRandomized = isRandomized;
        }
    }
    public Task<string> RecieveCommand(string logEntry)
    {
        if(State == NodeState.Leader)
        {
            int oldLogsCount = Logs.Count;
            AddLog(new LogEntry { index = Logs.Count, term = CurrentTerm, Command = logEntry });
            // Update leader's own MatchIndex when it adds a log
            if (MatchIndex != null && MatchIndex.Length > 0)
            {
                MatchIndex[0] = Logs.Count - 1;
                Console.WriteLine($"[RecieveCommand] Leader={Name} added log. MatchIndex[0]={MatchIndex[0]}, Logs.Count={Logs.Count}");
            }
            return Task.FromResult("");
        }
        return Task.FromResult(Leader?.Name ?? "");
    }
    public Task<bool> AddLog(LogEntry logEntry)
    {
        Logs.Add(new LogEntry { index = logEntry.index, term = logEntry.term, Command = logEntry.Command });
        return Task.FromResult(true);
    }
    public Task<bool> VoteFor(int term, INode voter)
    {
        if(term < CurrentTerm)
        {
            return Task.FromResult(false);
        }
        votes += 1;
        if(votes > Peers.Count / 2)
        {
            State = NodeState.Leader;
            timer.CurrentTimeoutMs = 0;
            WinElection();  // Initialize MatchIndex and NextIndex for the new leader
        }
        return Task.FromResult(true);
    }
    public Task<bool> AppendEntriesResponse(int term, INode follower, int lastLogIndex, bool success)
    {
        Console.WriteLine($"[AppendEntriesResponse] Leader={Name} received response from {follower.Name}: success={success}, lastLogIndex={lastLogIndex}");
        
        int peerIndex = Peers.IndexOf(follower);
        if (peerIndex == -1)
        {
            int followerIndex = ResponseFollowers.IndexOf(follower);
            if (followerIndex == -1)
            {
                followerIndex = ResponseFollowers.Count;
                ResponseFollowers.Add(follower);
                
                if (NextIndex != null && MatchIndex != null && followerIndex + 1 >= NextIndex.Length)
                {
                    int[] newNextIndex = new int[followerIndex + 2];
                    int[] newMatchIndex = new int[followerIndex + 2];
                    Array.Copy(NextIndex, newNextIndex, NextIndex.Length);
                    Array.Copy(MatchIndex, newMatchIndex, MatchIndex.Length);
                    NextIndex = newNextIndex;
                    MatchIndex = newMatchIndex;
                }
            }
            peerIndex = Peers.Count + followerIndex;
        }
        
        int arrayIdx = peerIndex + 1;
        Console.WriteLine($"[AppendEntriesResponse] Leader={Name} follower {follower.Name} arrayIdx={arrayIdx}, MatchIndex.Length={MatchIndex?.Length}");
        
        if (!success && NextIndex != null && arrayIdx < NextIndex.Length)
        {
            if (NextIndex[arrayIdx] > 0)
            {
                NextIndex[arrayIdx]--;
                Console.WriteLine($"[AppendEntriesResponse] Decrementing NextIndex[{arrayIdx}] to {NextIndex[arrayIdx]}");
            }
        }
        else if (success && MatchIndex != null && NextIndex != null && arrayIdx < MatchIndex.Length)
        {
            int reportedLastIndex = lastLogIndex;
            MatchIndex[arrayIdx] = Math.Max(MatchIndex[arrayIdx], reportedLastIndex);
            if (NextIndex != null && arrayIdx < NextIndex.Length)
            {
                NextIndex[arrayIdx] = MatchIndex[arrayIdx] + 1;
            }
            Console.WriteLine($"[AppendEntriesResponse] Setting MatchIndex[{arrayIdx}] to {MatchIndex[arrayIdx]} (reportedLastIndex={reportedLastIndex})");
            
            if (State == NodeState.Leader && MatchIndex != null)
            {
                int totalNodes = Peers.Count + 1;
                int majorityNeeded = (totalNodes / 2) + 1;
                
                Console.WriteLine($"[AppendEntriesResponse] Leader={Name} checking replication: totalNodes={totalNodes}, majorityNeeded={majorityNeeded}, Logs.Count={Logs.Count}");
                Console.WriteLine($"[AppendEntriesResponse] MatchIndex array: {string.Join(",", MatchIndex.Select((m, i) => $"[{i}]={m}"))}");
                
                int newCommitIndex = CommitIndex;
                for (int logIndex = CommitIndex + 1; logIndex < Logs.Count; logIndex++)
                {
                    int replicatedCount = 0;
                    if (MatchIndex[0] >= logIndex)  // Leader's own log
                    {
                        replicatedCount++;
                    }
                    
                    for (int i = 1; i < MatchIndex.Length; i++)
                    {
                        if (MatchIndex[i] >= logIndex)
                        {
                            replicatedCount++;
                        }
                    }
                    
                    Console.WriteLine($"[AppendEntriesResponse] Log index {logIndex}: replicated on {replicatedCount}/{totalNodes} nodes");
                    
                    if (replicatedCount >= majorityNeeded && Logs[logIndex].term == CurrentTerm)
                    {
                        newCommitIndex = logIndex;
                        Console.WriteLine($"[AppendEntriesResponse] New log index {logIndex} replicated on majority!");
                    }
                }
                
                if (newCommitIndex > CommitIndex)
                {
                    CommitIndex = newCommitIndex;
                    Console.WriteLine($"[AppendEntriesResponse] Leader={Name} updated CommitIndex to {CommitIndex}");
                    LogCommitted();
                }
            }
        }
        
        return Task.FromResult(true);
    }
    public Task<bool> AppendEntries(int term, INode leaderId, int prevLogIndex, int prevLogTerm, LogEntry[] entries, int leaderCommit)
    {
        if (term >= CurrentTerm)
        {
            CurrentTerm = term;
            State = NodeState.Follower;
            Leader = leaderId;
        }
        if(term < CurrentTerm)
        {
            return Task.FromResult(false);
        }
        if(prevLogIndex >= Logs.Count + 1)
        {
            var _response = leaderId.AppendEntriesResponse(term, this, Logs.Count, false);
            return Task.FromResult(false);
        }
        if(entries.Length > 0)
        {
            var log = entries[0];
            if(log.index < Logs.Count)
            {
                if(log.term != Logs[log.index].term)
                {
                    var _response = leaderId.AppendEntriesResponse(term, this, Logs.Count, false);
                    return Task.FromResult(false);
                }
            }
        }
        Console.WriteLine($"Node {this} received AppendEntries from {leaderId} for term {term}");
        CurrentTimeoutMs = TimeoutMs; 
        AddLogs(entries);

        if(leaderCommit > CommitIndex)
        {
            CommitIndex = Math.Min(leaderCommit, Logs.Count - 1);
            LogCommitted();
        }

        var _ = leaderId.AppendEntriesResponse(term, this, Logs.Count, true);

        return Task.FromResult(true);
    }
    private void AddLogs(LogEntry[] entries)
    {
        foreach(var entry in entries)
        {
            if(entry.index < Logs.Count)
            {
                Logs[entry.index] = entry;
            }
            else
            {
                Logs.Add(entry);
            }
        }
    }

    public Task<bool> RequestVote(int term, INode candidateId, int lastLogIndex, int lastLogTerm)
    {
        if(term <= CurrentTerm)
        {
            return Task.FromResult(false);
        }
        CurrentTerm = term;
        var _ = candidateId.VoteFor(term, this);
        State = NodeState.Follower;
        VotedFor = candidateId;
        return Task.FromResult(true);

    }

    public void RoutineMaintenanceTasks()
    {
        foreach (var peer in Peers)
        {
            var entriesToSend = Logs.ToArray();
            _ = peer.AppendEntries(CurrentTerm, this, 0, 0, entriesToSend, CommitIndex);
        }
    }
    public void LogCommitted()
    {
        if (CommitIndex > LastApplied && CommitIndex < Logs.Count)
        {
            for (int i = LastApplied + 1; i <= CommitIndex; i++)
            {
                if (i >= 0 && i < Logs.Count)
                {
                    var logEntry = Logs[i];
                    var commandParts = logEntry.Command.Split(':');
                    if (commandParts.Length == 2)
                    {
                        string key = commandParts[0];
                        string value = commandParts[1];
                        stateMachine[key] = value;
                        Console.WriteLine($"Node {Name} applied log index {logEntry.index}: set {key} = {value}");
                    }
                }
            }
            LastApplied = CommitIndex;
        }
    }
        public void WinElection()
    {
        State = NodeState.Leader;
        // Initialize arrays sized for leader + all peers
        int arraySize = Peers.Count + 1;
        NextIndex = new int[arraySize];
        MatchIndex = new int[arraySize];
        
        for (int i = 0; i < arraySize; i++)
        {
            NextIndex[i] = Logs.Count;  // Next entry to send is the one after the last log entry
            MatchIndex[i] = -1;
        }
        
        // Leader's own MatchIndex is all logs it has replicated to itself
        MatchIndex[0] = Logs.Count > 0 ? Logs.Count - 1 : -1;
        Console.WriteLine($"[WinElection] Leader={Name} elected. Peers.Count={Peers.Count}, arraySize={arraySize}, Logs.Count={Logs.Count}, MatchIndex[0]={MatchIndex[0]}");
    }
    public void Timeout()
    {
        if(State == NodeState.Follower || State == NodeState.Candidate)
        {
            State = NodeState.Candidate;
            CurrentTerm += 1;
            votes = 1;
            VotedFor = this;
            
            Console.WriteLine($"[Timeout] Node={Name} became Candidate for term {CurrentTerm}");
            
            foreach(var peer in Peers)
            {
                _ = peer.RequestVote(CurrentTerm, this, 0, 0);
            }
        }
    }
    public void Shutdown()
    {
        IsUp = false;
        State = NodeState.Follower;
        CurrentTerm = 0;
        VotedFor = null;
        Logs.Clear();
        CommitIndex = -1;
        LastApplied = -1;
        Leader = null;
        votes = 0;
        Console.WriteLine($"[Shutdown] Node={Name} has been shut down.");
    }

    public void TurnOn()
    {
        IsUp = true;
        State = NodeState.Follower;
        CurrentTerm = 0;
        VotedFor = null;
        CommitIndex = -1;
        LastApplied = -1;
        Leader = null;
        votes = 0;
        timer.ResetTimeout();
        Console.WriteLine($"[TurnOn] Node={Name} has been turned on.");
    }

    public async Task RunElectionTimer()
    {
        while(true)
        {
            Console.WriteLine($"[RunElectionTimer] Node={Name} State={State} CurrentTimeoutMs={CurrentTimeoutMs} TimeoutMs={TimeoutMs}");

            if (State == NodeState.Leader)
            {
                CurrentTimeoutMs = leadertimeout;
                RoutineMaintenanceTasks();
            }
            else
            {
                // Call ResetTimeout to apply randomization if enabled
                timer.ResetTimeout();
            }

            while (CurrentTimeoutMs > 0)
            {
                await Task.Delay(step);
                CurrentTimeoutMs -= step;
            }

            Console.WriteLine($"[RunElectionTimer] Node={Name} timing out (State={State}). Calling Timeout().");
            Timeout();
        }
    }
}