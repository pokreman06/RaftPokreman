using System;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using raftLibrary;
using Xunit;

namespace raftTests;

[Collection("Sequential")]
public class LogReplicationFollowerTests
{
    // Testing logs #10
    [Fact]
    public async Task Follower_AppendsEntriesFromLeader()
    {
        // Arrange
        var leader = new MockNode();
        var follower = new Node();
        
        // Act
        var appendEntriesTask = await follower.AppendEntries(1, leader, 0, 0, new LogEntry[] { new LogEntry { index = 0, Command = "log1" }, new LogEntry { index = 1, Command = "log2" } }, 0);
        
        // Assert
        Assert.Equal("AppendEntriesResponse", leader.LastCall);
        Assert.NotNull(leader.LastFunctionParameters);
        Assert.Equal(2, leader.LastFunctionParameters["lastLogIndex"]);
    }
    // Testing logs #11
    [Fact]
    public async Task AppendEntriesAppendsLogsToFollower()
    {
        // Arrange
        var leader = new MockNode();
        var follower = new Node();
        
        // Act
        await follower.AppendEntries(1, leader, 0, 0, new LogEntry[] { new LogEntry { index = 0, Command = "log1" }, new LogEntry { index = 1, Command = "log2" } }, 0);
        
        // Assert
        Assert.Contains(follower.Logs, log => log.Command == "log1");
        Assert.Contains(follower.Logs, log => log.Command == "log2");
    }
    // Testing logs #12
    [Fact]
    public async Task FollowerRedirects_toLeader()
    {
        // Arrange
        var leader = new MockNode(){ Name = "LeaderNode" };
        var follower = new Node() { State = NodeState.Follower, Leader = leader };
        // Act
        var result = await follower.RecieveCommand("new command");
        // Assert
        Assert.Equal("LeaderNode", result);
    }
    // Testing logs #13
    [Fact]
    public async Task AppendEntriesFails_onLargerLog()
    {
        // Arrange
        var leader = new MockNode();
        var follower = new Node() { Logs = new List<LogEntry> { new LogEntry { index = 0, Command = "log1" } } };
        
        // Act
        var appendEntriesTask = await follower.AppendEntries(1, leader, 2, 0, new LogEntry[] { new LogEntry { index = 2, Command = "log3" } }, 0);
        
        // Assert
        Assert.Equal("AppendEntriesResponse", leader.LastCall);
        Assert.NotNull(leader.LastFunctionParameters);
        Assert.Equal(false, leader.LastFunctionParameters["success"]);
    }
    // Testing logs #14
    [Fact]
    public async Task AppendEntriesAppendsLogsToFollowerWithIncorrectTerm()
    {
        // Arrange
        var leader = new MockNode();
        var follower = new Node() { Logs = new List<LogEntry> { new LogEntry { index = 0, Command = "log1", term = 0 }, new LogEntry { index = 1, Command = "log2", term = 0 },  new LogEntry { index = 2, Command = "log3", term = 0 },} };
        
        // Act
        var appendEntriesTask = await follower.AppendEntries(1, leader, 1, 0, new LogEntry[] { new LogEntry { index = 2, Command = "log3", term = 1 } }, 0);
        
        // Assert
        Assert.Equal("AppendEntriesResponse", leader.LastCall);
        Assert.NotNull(leader.LastFunctionParameters);
        Assert.Equal(false, leader.LastFunctionParameters["success"]);
    }
    // Testing logs #15
    [Fact]
    public async Task OnLogMismatch_FollowerDeletesConflictingLogs()
    {
        // Arrange
        var leader = new MockNode();
        var follower = new Node() { Logs = new List<LogEntry> { new LogEntry { index = 0, Command = "log1", term = 0 }, new LogEntry { index = 1, Command = "log2", term = 0 },  new LogEntry { index = 2, Command = "log3", term = 0 },} };
        
        // Act
        var appendEntriesTask = await follower.AppendEntries(1, leader, 1, 0, new LogEntry[] { new LogEntry { index = 1, Command = "log2", term = 0 }, new LogEntry { index = 2, Command = "log3-new", term = 1 } }, 0);
        
        // Assert
        Assert.DoesNotContain(follower.Logs, log => log.Command == "log3");
        Assert.Contains(follower.Logs, log => log.Command == "log3-new");
    }
    // Testing logs #16
    [Fact]
    public async Task AppendEntriesUpdatesCommitIndex()
    {
        // Arrange
        var leader = new MockNode();
        var follower = new Node() {Logs = new List<LogEntry> { new LogEntry { index = 0, Command = "log1", term = 0 }, new LogEntry { index = 1, Command = "log2", term = 0 },  new LogEntry { index = 2, Command = "log3", term = 0 },} };
        
        // Act
        var appendEntriesTask = await follower.AppendEntries(1, leader, 2, 0, new LogEntry[] { new LogEntry { index = 3, Command = "log4", term = 0 } }, 2);
        
        // Assert
        Assert.Equal(2, follower.CommitIndex);
    }
    // Testing logs #17
    [Fact]
    public async Task OnCommitIndexUpdateApplyToStateMachine()
    {
        // Arrange
        var leader = new MockNode();
        var follower = new Node() { Logs = new List<LogEntry> { new LogEntry { index = 0, Command = "log1", term = 0 }, new LogEntry { index = 1, Command = "log2", term = 0 },  new LogEntry { index = 2, Command = "log3", term = 0 },} };
        
        // Act
        var appendEntriesTask = await follower.AppendEntries(1, leader, 2, 0, new LogEntry[] { new LogEntry { index = 3, Command = "log4", term = 0 } }, 2);
        
        // Assert
        Assert.Equal(2, follower.LastApplied);
    }
}

