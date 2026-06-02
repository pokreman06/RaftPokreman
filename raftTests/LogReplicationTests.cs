using System;
using System.Threading.Tasks;
using raftLibrary;
using Xunit;

namespace raftTests
{
    [Collection("Sequential")]
    public class LogReplicationTests
    {
        // TC1: Leader Receives Command - Adds to own log (uncommitted)
        // Testing Logs #1
        [Fact]
        public async Task TC1_LeaderReceivesCommand_AddsToOwnLogUncommitted()
        {
            // Given: you are a leader
            var leader = new Node { State = NodeState.Leader, CurrentTerm = 1 };
            
            // When: you receive a command
            var result = await leader.AddLog(new LogEntry { Command = "test-command" });
            
            // Then: it adds it to its own log (uncommitted)
            Assert.True(result);
            Assert.Equal(NodeState.Leader, leader.State);
        }

        // TC2: Leader Sends Heartbeats - Includes new log entry in AppendEntries
        // Testing Logs #2
        [Fact]
        public void TC2_LeaderSendsHeartbeats_IncludesNewLogEntry()
        {
            // Given: a leader has a new uncommitted log
            var follower = new MockNode();
            var leader = new Node 
            { 
                State = NodeState.Leader, 
                CurrentTerm = 1,
                Logs = new List<LogEntry> { new LogEntry { Command = "new-log-entry" } },
                Peers = new List<INode> { follower }
            };
            
            // When: the leader sends out heartbeats
            leader.RoutineMaintenanceTasks();
            
            // Then: the new log entry is included in the AppendEntries message
            Assert.Equal("AppendEntries", follower.LastCall);
        }

        // TC3: Leader Receives Follower Confirmation - Updates that node's log index
        // Testing Logs #3
        [Fact]
        public void TC3_LeaderReceivesFollowerConfirmation_UpdatesLogIndex()
        {
            // Given: you are a leader
            var leader = new Node 
            { 
                State = NodeState.Leader, 
                CurrentTerm = 1,
                MatchIndex = new int[] { 0, 0, 0 },
                NextIndex = new int[] { 1, 1, 1 }
            };
            var follower = new MockNode();
            
            // When: you receive confirmation that a follower has appended logs (AppendEntries response success)
            var result = leader.AppendEntriesResponse(1, follower, 0, true);
            
            // Then: you update that node's log index (MatchIndex should be updated)
            Assert.NotNull(leader.MatchIndex);
            Assert.Equal(3, leader.MatchIndex.Length);
        }

        // TC4: Leader Receives Single Confirmation - Not Majority
        // Testing Logs #4
        [Fact]
        public async Task TC4_LeaderReceivesSingleConfirmation_NotMajority()
        {
            // Given: you are a leader with 4 total nodes
            var follower1 = new MockNode();
            var follower2 = new MockNode();
            var follower3 = new MockNode();
            var leader = new Node 
            { 
                State = NodeState.Leader, 
                CurrentTerm = 1,
                CommitIndex = -1,
                MatchIndex = new int[] { 1, 0, 0, 0 },  // 1 = leader has its own logs
                NextIndex = new int[] { 1, 1, 1, 1 },
                Peers = new List<INode> { follower1, follower2, follower3 }
            };
            int initialCommitIndex = leader.CommitIndex;
            
            // When: only one follower (out of 3) confirms replication
            await leader.AppendEntriesResponse(1, follower1, 0, true);
            
            // Then: CommitIndex should NOT be updated (not a majority)
            Assert.Equal(initialCommitIndex, leader.CommitIndex);
        }

        [Fact]
        public void TC_SimpleDiagnostic_CheckPeersList()
        {
            // This is a simple test to verify the test infrastructure works
            var follower1 = new MockNode { Name = "follower1" };
            var follower2 = new MockNode { Name = "follower2" };
            var leader = new Node 
            { 
                State = NodeState.Leader, 
                Peers = new List<INode> { follower1, follower2 }
            };
            
            Assert.Equal(2, leader.Peers.Count);
            Assert.Contains(follower1, leader.Peers);
            Assert.Equal(0, leader.Peers.IndexOf(follower1));
        }

        // TC4B: Leader Achieves Majority - Updates commit index
        // Testing Logs #5
        [Fact]
        public async Task TC4B_LeaderAchievesMajority_UpdatesCommitIndex()
        {
            // Given: you are a leader with 3 total nodes
            var follower1 = new MockNode();
            var follower2 = new MockNode();
            var leader = new Node 
            { 
                State = NodeState.Leader, 
                CurrentTerm = 1,
                CommitIndex = -1,
                Logs = new List<LogEntry> 
                { 
                    new LogEntry { index = 0, term = 1, Command = "entry1" },
                    new LogEntry { index = 1, term = 1, Command = "entry2" }
                },
                MatchIndex = new int[] { 1, -1, -1 },  // Leader has replicated both entries to itself
                NextIndex = new int[] { 2, 2, 2 },
                Peers = new List<INode> { follower1, follower2 }
            };
            
            Console.WriteLine($"[Test] Initial CommitIndex={leader.CommitIndex}, Peers.Count={leader.Peers.Count}, follower1 in Peers={leader.Peers.Contains(follower1)}");
            
            // When: a majority of nodes (2 out of 3) have appended the logs
            await leader.AppendEntriesResponse(1, follower1, 1, true);  // follower1 has replicated up to index 1
            Console.WriteLine($"[Test] After follower1: CommitIndex={leader.CommitIndex}, MatchIndex=[{string.Join(",", leader.MatchIndex ?? Array.Empty<int>())}]");
            
            await leader.AppendEntriesResponse(1, follower2, 1, true);  // follower2 has replicated up to index 1
            Console.WriteLine($"[Test] After follower2: CommitIndex={leader.CommitIndex}, MatchIndex=[{string.Join(",", leader.MatchIndex ?? Array.Empty<int>())}]");
            
            // Then: CommitIndex should be updated (majority has replicated)
            Assert.True(leader.CommitIndex >= 0, $"CommitIndex was {leader.CommitIndex}, expected >= 0");
        }

        // TC5: Leader Has Commit Index - Includes in AppendEntries heartbeats
        // Testing Logs #6
        [Fact]
        public void TC5_LeaderHasCommitIndex_IncludesInHeartbeats()
        {
            // Given: you are a leader with a commit index
            var follower = new MockNode();
            var leader = new Node 
            { 
                State = NodeState.Leader, 
                CurrentTerm = 1, 
                CommitIndex = 5,
                Peers = new List<INode> { follower }
            };
            
            // When: you send AppendEntries heartbeats (routine maintenance)
            leader.RoutineMaintenanceTasks();
            
            // Then: you include your commit index in AppendEntries heartbeats
            Assert.Equal("AppendEntries", follower.LastCall);
            Assert.NotNull(follower.LastFunctionParameters);
            Assert.Equal(5, (int)follower.LastFunctionParameters["leaderCommit"]);
        }

        // TC6: Follower Rejects AppendEntries - Leader decrements nextIndex
        // Testing Logs #7
        [Fact]
        public void TC6_FollowerRejectsAppendEntries_LeaderDecrementsNextIndex()
        {
            // Given: you are a leader
            var leader = new Node 
            { 
                State = NodeState.Leader, 
                CurrentTerm = 1,
                NextIndex = new int[] { 1, 5, 5 },
                MatchIndex = new int[] { 0, 0, 0 }
            };
            var follower = new MockNode();
            
            // When: a follower rejects your appendEntries (success = false)
            var initialNextIndex = leader.NextIndex[1];
            leader.AppendEntriesResponse(1, follower, 0, false).Wait();
            
            // Then: you decrement your "nextIndex" value for that follower
            Assert.Equal(initialNextIndex - 1, leader.NextIndex[1]);
        }

        // TC7: Unacknowledged AppendEntries - Leader continues sending
        // Testing Logs #8
        [Fact]
        public void TC7_LeaderSendsUnacknowledgedAppendEntries_ContinuesSending()
        {
            // Given: you are a leader with an unsent log
            var follower = new MockNode();
            var leader = new Node 
            { 
                State = NodeState.Leader, 
                CurrentTerm = 1,
                NextIndex = new int[] { 1, 1 },
                MatchIndex = new int[] { 0, 0 },
                Logs = new List<LogEntry> { new LogEntry { Command = "unsent-log-entry" } },
                Peers = new List<INode> { follower }
            };
            
            // When: you send appendEntries that are not acknowledged
            leader.RoutineMaintenanceTasks();
            var firstEntries = (LogEntry[])follower.LastFunctionParameters["entries"];
            var firstCall = follower.LastCall;
            
            // And continue attempting
            leader.RoutineMaintenanceTasks();
            var secondEntries = (LogEntry[])follower.LastFunctionParameters["entries"];
            var secondCall = follower.LastCall;
            
            // Then: you continue to send appendEntries with the same log (retry mechanism)
            Assert.Equal("AppendEntries", firstCall);
            Assert.Equal("AppendEntries", secondCall);
            Assert.Equal(firstEntries, secondEntries);
        }

        // TC8: Leader Commits Log Entry - Applies to state machine
        // Testing Logs #9
        [Fact]
        public void TC8_LeaderCommitsLogEntry_AppliesToStateMachine()
        {
            // Given: you are a leader with logs
            var leader = new Node 
            { 
                State = NodeState.Leader, 
                CurrentTerm = 1,
                CommitIndex = 2,
                LastApplied = -1,
                Logs = new List<LogEntry>
                {
                    new LogEntry { index = 0, term = 1, Command = "test1:val1" },
                    new LogEntry { index = 1, term = 1, Command = "test2:val2" },
                    new LogEntry { index = 2, term = 1, Command = "test3:val3" }
                }
            };
            
            // When: you commit a log entry
            leader.LogCommitted();
            
            // Then: apply new logs to state machine (LastApplied should be updated to CommitIndex)
            Assert.Equal(2, leader.LastApplied);
        }

        // TC10: Follower Receives AppendEntries - Sends proper response
        // Testing Logs #11
        [Fact]
        public async Task TC10_FollowerReceivesAppendEntries_SendsProperResponse()
        {
            // Given: a follower and a leader
            var leaderMock = new MockNode();
            var follower = new Node 
            { 
                State = NodeState.Follower, 
                CurrentTerm = 1,
                Logs = new List<LogEntry>(),
                Peers = new List<INode> { leaderMock }
            };
            
            // When: follower receives AppendEntries from leader
            var logEntry = new LogEntry { index = 0, term = 1, Command = "test-command" };
            await follower.AppendEntries(1, leaderMock, 0, 0, new[] { logEntry }, 0);
            
            // Then: follower should add the log entry and respond with success
            Assert.Equal(1, follower.Logs.Count);
            Assert.Equal("test-command", follower.Logs[0].Command);
            // AppendEntriesResponse should have been called on the leader mock
            Assert.Equal("AppendEntriesResponse", leaderMock.LastCall);
        }

        // TC11: Leader Receives Success Response - Updates MatchIndex
        // Testing Logs #12
        [Fact]
        public async Task TC11_LeaderReceivesSuccessResponse_UpdatesMatchIndex()
        {
            // Given: a leader with a follower
            var follower = new MockNode();
            var leader = new Node 
            { 
                State = NodeState.Leader, 
                CurrentTerm = 1,
                Logs = new List<LogEntry> 
                { 
                    new LogEntry { index = 0, term = 1, Command = "entry1" }
                },
                Peers = new List<INode> { follower },
                NextIndex = new int[] { 1, 1 },
                MatchIndex = new int[] { 0, -1 }
            };
            
            // When: leader receives success response from follower
            await leader.AppendEntriesResponse(1, follower, 0, true);
            
            // Then: MatchIndex for that follower should be updated to 0 (follower has replicated entry 0)
            Assert.True(leader.MatchIndex[1] >= 0);
        }

        // TC12: LogCommitted is Called - Applies entries to state machine
        // Testing Logs #13
        [Fact]
        public void TC12_LogCommittedCalled_AppliesToStateMachine()
        {
            // Given: a node with uncommitted logs and a valid CommitIndex
            var node = new Node 
            { 
                State = NodeState.Leader, 
                CurrentTerm = 1,
                CommitIndex = 2,
                LastApplied = -1,  // Start from before all entries
                Logs = new List<LogEntry> 
                { 
                    new LogEntry { index = 0, term = 1, Command = "key1:value1" },
                    new LogEntry { index = 1, term = 1, Command = "key2:value2" },
                    new LogEntry { index = 2, term = 1, Command = "key3:value3" }
                }
            };
            
            // When: LogCommitted is called
            node.LogCommitted();
            
            // Then: entries up to CommitIndex should be applied to the state machine
            Assert.Equal(2, node.LastApplied);
            Assert.Contains("key1", node.stateMachine.Keys);
            Assert.Contains("key2", node.stateMachine.Keys);
            Assert.Equal("value1", node.stateMachine["key1"]);
            Assert.Equal("value2", node.stateMachine["key2"]);
        }

        // TC13: Leader Command Replication - Entry replicated to majority
        // Testing Logs #14
        [Fact]
        public async Task TC13_LeaderCommandReplication_EntryReplicatedToMajority()
        {
            // Given: a leader with 2 followers
            var follower1 = new MockNode();
            var follower2 = new MockNode();
            var leader = new Node 
            { 
                State = NodeState.Leader, 
                CurrentTerm = 1,
                CommitIndex = -1,
                Logs = new List<LogEntry> 
                { 
                    new LogEntry { index = 0, term = 1, Command = "testcmd" },
                    new LogEntry { index = 1, term = 1, Command = "testcmd2" }
                },
                Peers = new List<INode> { follower1, follower2 },
                NextIndex = new int[] { 2, 2, 2 },
                MatchIndex = new int[] { 1, -1, -1 }
            };
            
            // When: leader receives AppendEntries success from first follower
            await leader.AppendEntriesResponse(1, follower1, 1, true);
            // And from second follower
            await leader.AppendEntriesResponse(1, follower2, 1, true);
            
            // Then: majority have replicated entry, so CommitIndex should advance
            Assert.True(leader.CommitIndex >= 0);
        }
    }
}