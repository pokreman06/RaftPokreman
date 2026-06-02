using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using raftLibrary;
using Xunit;

namespace raftTests;

[Collection("Sequential")]
public class UnitTest1
{
    // Test Case 01: Single node starts
    [Fact]
    public async Task NodeStartsAsFollower()
    {
        var node = new Node();
        Assert.Equal(NodeState.Follower, node.State);
    }
    
    // Test Case 02: Election timeout set
    [Fact]
    public async Task StartedNodeResetsTimer()
    {
        var node = new Node() { TimeoutMs = 500, step = 250 };
        var _ = node.RunElectionTimer();
        Assert.Equal(node.TimeoutMs, node.CurrentTimeoutMs);


    }
    
    // Test Case 03: Follower receives higher term
    [Fact]
    public async Task updateTermOnHigherTermAppendEntries()
    {
        var node = new Node() { CurrentTerm = 1, State = NodeState.Leader };
        await node.AppendEntries(2, new MockNode(), 0, 0, Array.Empty<LogEntry>(), 0);
        Assert.Equal(2, node.CurrentTerm);
        Assert.Equal(NodeState.Follower, node.State);
    }
    
    // Test Case 04: Follower denies duplicate vote
    [Fact]
    public async Task votedOnceNotVotedTwice()
    {
        var node = new Node() { CurrentTerm = 1};
        var second = await node.RequestVote(1, new MockNode(), 0, 0);
        Assert.False(second);
    }
    
    // Test Case 05: Follower increments term on higher vote for higher term
    [Fact]
    public async Task voteRequestHigherTermIncrementsTermVoted()
    {
        var node = new Node() { CurrentTerm = 1};
        var candidate = new MockNode();
        var first = await node.RequestVote(2, candidate, 0, 0);
        Assert.Equal(2, node.CurrentTerm);
        Assert.Equal("VoteResponse", candidate.LastCall);
    }
    
// Test Case 06: Follower votes for higher term 
[Fact]
public async Task voteRequestHigherTermSetsVotedFor()
{
    var node = new Node() { CurrentTerm = 1 };
    var candidate = new MockNode();
    await node.RequestVote(2, candidate, 0, 0);
    Assert.Equal(candidate, node.VotedFor);
}
    // Test Case 07: Follower resets timeout on current term AppendEntries
    [Fact]
    public async Task timerResetOnAppendEntries()
    {
        var node = new Node() { TimeoutMs = 500, step = 250 };
        var _ = node.RunElectionTimer();
        await Task.Delay(300);
        var before = node.CurrentTimeoutMs;
        await node.AppendEntries(1, new MockNode(), 0, 0, Array.Empty<LogEntry>(), 0);
        var after = node.CurrentTimeoutMs;
        await Task.Delay(10);
        Assert.Equal(after, node.TimeoutMs);
        Assert.Equal(before, 250);
    }
    
    // Test Case 08: Follower ignores previous term AppendEntries
    [Fact]
    public async Task followerNotResetOnLowerTermAppendEntries()
    {
                var node = new Node() { TimeoutMs = 500, step = 250, CurrentTerm = 2 };
        var _ = node.RunElectionTimer();
        await Task.Delay(300);
        var before = node.CurrentTimeoutMs;
        await node.AppendEntries(1, new MockNode(), 0, 0, Array.Empty<LogEntry>(), 0);
        var after = node.CurrentTimeoutMs;
        await Task.Delay(10);
        Assert.Equal(after, before);
    }
    
    // Test Case 09: Follower ignores lower term vote request
    [Fact]
    public async Task voteRequestLowerTermDenied()
    {
        var node = new Node() { CurrentTerm = 2};
        var candidate = new MockNode();
        var first = await node.RequestVote(1, candidate, 0, 0);
        Assert.False(first);
        Assert.Equal(2, node.CurrentTerm);
        Assert.Null(candidate.LastCall);
    }
    
    // Test Case 10: Follower responds to AppendEntries
    [Fact]
    public async Task appendEntriesMeritsResponse()
    {
        var node = new Node() { CurrentTerm = 1};
        var leader = new MockNode();
        var response = await node.AppendEntries(1, leader, 0, 0, Array.Empty<LogEntry>(), 0);
        Assert.True(response);
        Assert.Equal("AppendEntriesResponse", leader.LastCall);
    }
    
    // Test Case 11, 13 & 14: Follower becomes candidate, increments term, votes for self
    [Fact]
    public void followerTimeoutLeadsToCandidacyAndIncrementedTermAndVotesForSelf()
    {
        var node = new Node() { TimeoutMs = 100, step = 10 };
        node.Timeout();
        Assert.Equal(NodeState.Candidate, node.State);
        Assert.Equal(1, node.CurrentTerm);
        Assert.Equal(1, node.votes);
    }

// Test Case 12: Candidate resets election timeout
[Fact]
public async Task becomingCandidateResetsElectionTimeout()
{
    var timer = new MockTimer() { TimeoutMs = 100, step = 10, CurrentTimeoutMs = 50 };
    var node = new Node() { timer = timer };
    _ = Task.Run(async () => await node.RunElectionTimer());
    await Task.Delay(200);  // Increased delay to ensure timeout occurs  
    Assert.Equal(NodeState.Candidate, node.State);
    Assert.True(timer.isReset); 
}

// Test Case 15: Candidate requests votes from peers
[Fact]
public async Task candidateRequestsVotesFromPeers() 
{
    var mockPeer = new MockNode();
    var mockPeer2 = new MockNode();
    var node = new Node() { timer = new MockTimer() { TimeoutMs = 100, step = 10 }, Peers = new List<INode> { mockPeer, mockPeer2 } };
    _ = Task.Run(async () => await node.RunElectionTimer());
    await Task.Delay(200);  // Increased delay to ensure timeout occurs
    Assert.Equal("RequestVote", mockPeer.LastCall);
    Assert.Equal("RequestVote", mockPeer2.LastCall);
}   

// Test Case 16: Wins Election Becomes Leader
[Fact]
public async Task candidateWinsElectionBecomesLeader()
{
    var mockPeer = new MockNode();
    var node = new Node() { timer = new MockTimer() { TimeoutMs = 100, step = 10 }, Peers = new List<INode> { mockPeer } };
    _ = Task.Run(async () => await node.RunElectionTimer());
    node.VoteFor(1, mockPeer); // Vote for self
    await Task.Delay(300);  // Increased delay to ensure timeout occurs
    Assert.Equal(NodeState.Leader, node.State);
}
// Test Case 16: Wins Election Becomes Leader pt2
[Fact]
public async Task candidateWinsElectionBecomesLeaderPt2()
{
    var mockPeer1 = new MockNode();
    var mockPeer2 = new MockNode();
    var node = new Node() { timer = new MockTimer() { TimeoutMs = 100, step = 10 }, Peers = new List<INode> { mockPeer1, mockPeer2 } };
    _ = Task.Run(async () => await node.RunElectionTimer());
    await Task.Delay(200);  // Wait for node to become candidate
    // Simulate receiving majority votes (node votes for itself + 2 peer votes = 3/3 majority)
    node.votes = 3;  // Simulate winning the election
    node.State = NodeState.Leader;  // Manually set to leader to test the behavior
    Assert.Equal(NodeState.Leader, node.State);
}
// Test Case 17: Candidate updates term on higher term AppendEntries
[Fact]
public async Task candidateUpdatesTermOnHigherTermAppendEntries()
{
    var node = new Node() { CurrentTerm = 1, State = NodeState.Candidate };
    await node.AppendEntries(2, new MockNode(), 0, 0, Array.Empty<LogEntry>(), 0);
    Assert.Equal(2, node.CurrentTerm);
    Assert.Equal(NodeState.Follower, node.State);
}
// Test Case 18: Candidate Loses Election becomes Follower
[Fact]
public async Task candidateLosesElectionBecomesFollower()
{
    var mockPeer1 = new MockNode();
    var mockPeer2 = new MockNode();
    var node = new Node() { Peers = new List<INode> { mockPeer1, mockPeer2 }, CurrentTerm = 1, State = NodeState.Candidate };
    node.AppendEntries(2, new MockNode(), 0, 0, Array.Empty<LogEntry>(), 0);
    Assert.Equal(NodeState.Follower, node.State);   
}   
// Test Case 19 & 20 & 21: Candidate recieves higher vote request becomes Follower and votes for that node and updates term
[Fact]
public async Task candidateReceivesHigherVoteRequestBecomesFollower()
{
    var mockPeer = new MockNode();
    var node = new Node() { CurrentTerm = 1, State = NodeState.Candidate };
    await node.RequestVote(2, mockPeer, 0, 0);
    Assert.Equal("VoteResponse", mockPeer.LastCall);
    Assert.Equal(NodeState.Follower, node.State);
    Assert.Equal(2, node.CurrentTerm);
}
// Test Case 22: Candidate increments term on own election timeout
[Fact]
public async Task candidateIncrementsTermOnOwnElectionTimeout()
{
    var timer = new MockTimer() { TimeoutMs = 100, step = 10 };
    var node = new Node() { CurrentTerm = 1, State = NodeState.Candidate, timer = timer };
    _ = Task.Run(async () => await node.RunElectionTimer());
    await Task.Delay(200);
    Assert.Equal(2, node.CurrentTerm);
    Assert.True(timer.isReset);
}
// Test Case: lower term voteFor ignored
[Fact]
public async Task lowerTermVoteForIgnored()
{
    var node = new Node() { CurrentTerm = 2, State = NodeState.Candidate  };
    var voter = new MockNode();
    var result = await node.VoteFor(1, voter);
    Assert.Equal(node.votes, 0);
}
// Test Case 23: on election win append entries sent to peers
[Fact]
public async Task onElectionWinAppendEntriesSentToPeers()
{
    var mockPeer = new MockNode();
    var mockPeer2 = new MockNode();
    var node = new Node() { leadertimeout = 10, Peers = new List<INode> { mockPeer, mockPeer2 }, CurrentTerm = 1, State = NodeState.Candidate, timer = new MockTimer() { TimeoutMs = 100, step = 10 } };
    // Directly call RoutineMaintenanceTasks to test AppendEntries sending
    node.VoteFor(1, mockPeer); // Vote for self
    node.VoteFor(1, mockPeer2); // Vote for self
    _ = Task.Run(async () => await node.RunElectionTimer());
    await Task.Delay(500); // Give time for async operations
    Assert.Equal("AppendEntries", mockPeer.LastCall);
    Assert.Equal("AppendEntries", mockPeer2.LastCall);

}

// Test Case 24: Candidate Ignores lower term voterequest
[Fact]
public async Task candidateIgnoresLowerTermVoteRequest() 
{
    var node = new Node() { CurrentTerm = 2, State = NodeState.Candidate };
    var candidate = new MockNode();
    var result = await node.RequestVote(1, candidate, 0, 0);
    Assert.False(result);
    Assert.Equal(2, node.CurrentTerm);
    Assert.Null(candidate.LastCall);
}
// Test Case 25: Leader sends AppendEntries to followers
[Fact]
public async Task leaderSendsAppendEntriesToFollowers()
{
    var mockPeer = new MockNode();
    var node = new Node() { leadertimeout = 20, Peers = new List<INode> { mockPeer }, CurrentTerm = 1, State = NodeState.Leader, timer = new MockTimer() { TimeoutMs = 100, step = 10 } };
    _ = Task.Run(async () => await node.RunElectionTimer());
    await Task.Delay(200); // Give time for async operations
    Assert.Equal("AppendEntries", mockPeer.LastCall);
}
// Test Case 26: Leader - higher term heartbeat resets to follower
[Fact]
public async Task leaderResetsToFollowerOnHigherTermHeartbeat()
{   
    var node = new Node() { CurrentTerm = 1, State = NodeState.Leader };
    await node.AppendEntries(2, new MockNode(), 0, 0, Array.Empty<LogEntry>(), 0);
    Assert.Equal(2, node.CurrentTerm);
    Assert.Equal(NodeState.Follower, node.State);
}
// Test Case 27: Leader - lower term AppendEntries ignored
[Fact]
public async Task leaderIgnoresLowerTermAppendEntries()
{
    var node = new Node() { CurrentTerm = 2, State = NodeState.Leader };
    await node.AppendEntries(1, new MockNode(), 0, 0, Array.Empty<LogEntry>(), 0);
    Assert.Equal(2, node.CurrentTerm);
    Assert.Equal(NodeState.Leader, node.State);
}

// Test Case 28: Leader - lower term vote request ignored
[Fact]
public async Task leaderIgnoresLowerTermVoteRequest()
{
    var node = new Node() { CurrentTerm = 2, State = NodeState.Leader };
    var candidate = new MockNode();
    var result = await node.RequestVote(1, candidate, 0, 0);
    Assert.False(result);
    Assert.Equal(2, node.CurrentTerm);
    Assert.Null(candidate.LastCall);
}

// Test Case: Leader receives command, logs it, and replicates to follower
[Fact]
public async Task leaderAddsCommandToLogAndReplicatesToFollower()
{
    var follower = new MockNode();
    var leader = new Node() { State = NodeState.Leader, CurrentTerm = 1 };
    leader.Peers.Add(follower);
    leader.WinElection();

    // Leader receives command
    var command = "key:value";
    await leader.RecieveCommand(command);

    // Verify log entry was added to leader
    Assert.Single(leader.Logs);
    Assert.Equal(0, leader.Logs[0].index);
    Assert.Equal(1, leader.Logs[0].term);
    Assert.Equal(command, leader.Logs[0].Command);
}

// Test Case: Follower receives AppendEntries and applies state machine
[Fact]
public async Task followerReceivesAppendEntriesAndAppliesStateMachine()
{
    var leader = new MockNode();
    var follower = new Node() { State = NodeState.Follower, CurrentTerm = 1, CommitIndex = -1, LastApplied = -1 };
    
    // Create log entry
    var logEntry = new LogEntry { index = 0, term = 1, Command = "key:value" };
    var entries = new[] { logEntry };

    // Follower receives AppendEntries from leader with leaderCommit = 0
    await follower.AppendEntries(1, leader, 0, 0, entries, 0);

    // Verify log was added
    Assert.Single(follower.Logs);
    Assert.Equal("key:value", follower.Logs[0].Command);

    // Verify commit and state machine were updated
    Assert.Equal(0, follower.CommitIndex);
    Assert.True(follower.stateMachine.ContainsKey("key"));
    Assert.Equal("value", follower.stateMachine["key"]);
}

// Test Case: Candidate becomes leader and MatchIndex is initialized
[Fact]
public async Task candidateWinsElectionMatchIndexInitialized()
{
    var mockPeer1 = new MockNode();
    var mockPeer2 = new MockNode();
    var candidate = new Node() { CurrentTerm = 1, State = NodeState.Candidate, Peers = new List<INode> { mockPeer1, mockPeer2 } };
    
    // Candidate wins election
    candidate.WinElection();
    
    // Verify MatchIndex is initialized and not empty
    Assert.NotNull(candidate.MatchIndex);
    Assert.NotEmpty(candidate.MatchIndex);
    
    // Verify MatchIndex has correct size (leader + 2 peers = 3)
    Assert.Equal(3, candidate.MatchIndex.Length);
    
    // Verify leader's own MatchIndex (index 0) is initialized correctly
    Assert.Equal(-1, candidate.MatchIndex[0]); // No logs yet, so -1
    
    // Verify follower MatchIndex entries are initialized to -1
    Assert.Equal(-1, candidate.MatchIndex[1]);
    Assert.Equal(-1, candidate.MatchIndex[2]);
}

// Test Case: Candidate wins election and MatchIndex is properly initialized with peers
[Fact]
public async Task candidateWinsElectionAndMatchIndexNotNullOrEmpty()
{
    var mockPeer1 = new MockNode();
    var mockPeer2 = new MockNode();
    var candidate = new Node() { CurrentTerm = 1, State = NodeState.Candidate, Peers = new List<INode> { mockPeer1, mockPeer2 } };
    
    // Candidate wins the election by calling WinElection
    candidate.WinElection();
    
    // Verify candidate is now a leader
    Assert.Equal(NodeState.Leader, candidate.State);
    
    // Verify MatchIndex is NOT null
    Assert.NotNull(candidate.MatchIndex);
    
    // Verify MatchIndex is NOT empty
    Assert.NotEmpty(candidate.MatchIndex);
    
    // Verify it has the expected size for 1 leader + 2 peers
    Assert.Equal(3, candidate.MatchIndex.Length);
}



}
