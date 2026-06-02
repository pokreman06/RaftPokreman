using System.Xml.Serialization;
using raftLibrary;
public class NodeProxy(Node node) : INode
{
        public string Name { get => node.Name; set => node.Name = value; }

    public Task<string> RecieveCommand(string logEntry)
    {
        Thread.Sleep(100);
        return node.RecieveCommand(logEntry);
    }
    public Task<bool> AppendEntries(int term, INode leaderId, int prevLogIndex, int prevLogTerm, LogEntry[] entries, int leaderCommit)
    {
        Thread.Sleep(100);
        return node.AppendEntries(term, (Node)leaderId, prevLogIndex, prevLogTerm, entries, leaderCommit);
    }
    public Task<bool> AppendEntriesResponse(int term, INode follower, int lastLogIndex, bool success)
    {
        Thread.Sleep(100);
        return node.AppendEntriesResponse(term, (Node)follower, lastLogIndex, success);
    }
    public Task<bool> RequestVote(int term, INode candidateId, int lastLogIndex, int lastLogTerm)
    {
        Thread.Sleep(100);
        return node.RequestVote(term, (Node)candidateId, lastLogIndex, lastLogTerm);
    }
    public Task<bool> VoteFor(int term, INode voter) 
    {
        Thread.Sleep(100);
        return node.VoteFor(term, (Node)voter);
    }
    public void register(INode addnode)
    {
        node.Peers.Add((Node)addnode);
    }
}