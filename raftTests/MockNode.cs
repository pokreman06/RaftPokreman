using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using raftLibrary;

namespace raftTests
{
    public class MockNode : INode
    {
        public string Name { get; set; } = Guid.NewGuid().ToString();
        private readonly TaskCompletionSource<bool> _callTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public string? LastCall { get; private set; }
        public bool? isFailed { get; set; } = null;
        
        // Store the parameters of the last function call as a dictionary
        public Dictionary<string, object?> LastFunctionParameters { get; private set; } = new Dictionary<string, object?>();
        public Task<string> RecieveCommand(string logEntry)
        {
            LastCall = "RecieveCommand";
            LastFunctionParameters = new Dictionary<string, object?>
            {
                { "logEntry", logEntry }
             };
            isFailed = false;
            return Task.FromResult("test");
        }

        public Task<bool> AppendEntriesResponse(int term, INode follower, int lastLogIndex, bool success)
        {
            LastCall = "AppendEntriesResponse";
            LastFunctionParameters = new Dictionary<string, object?>
            {
                { "term", term },
                { "follower", follower },
                { "lastLogIndex", lastLogIndex },
                { "success", success }
            };
            isFailed = false;
            return Task.FromResult(true);
        }

        public Task<bool> AppendEntries(int term, INode leaderId, int prevLogIndex, int prevLogTerm, LogEntry[] entries, int leaderCommit)
        {
            LastCall = "AppendEntries";
            LastFunctionParameters = new Dictionary<string, object?>
            {
                { "term", term },
                { "leaderId", leaderId },
                { "prevLogIndex", prevLogIndex },
                { "prevLogTerm", prevLogTerm },
                { "entries", entries },
                { "leaderCommit", leaderCommit }
            };
            isFailed = false;
            return Task.FromResult(true);
        }

        public Task<bool> RequestVote(int term, INode candidateId, int lastLogIndex, int lastLogTerm)
        {
            LastCall = "RequestVote";
            LastFunctionParameters = new Dictionary<string, object?>
            {
                { "term", term },
                { "candidateId", candidateId },
                { "lastLogIndex", lastLogIndex },
                { "lastLogTerm", lastLogTerm }
            };
            isFailed = false;
            return Task.FromResult(false);
        }

        public Task<bool> VoteFor(int term, INode voter)
        {
            LastCall = "VoteResponse";
            LastFunctionParameters = new Dictionary<string, object?>
            {
                { "term", term },
                { "voter", voter }
            };
            isFailed = false;
            return Task.FromResult(true);
        }
    }
}
