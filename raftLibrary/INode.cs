using System.Threading.Tasks;

namespace raftLibrary
{
    public enum NodeState
    {
        Leader,
        Candidate,
        Follower
    }


    public interface INode
    {
        string Name { get; set; }
        Task<string> RecieveCommand(string logEntry);
        Task<bool> AppendEntries(int term, INode leaderId, int prevLogIndex, int prevLogTerm, LogEntry[] entries, int leaderCommit);

        Task<bool> RequestVote(int term, INode candidateId, int lastLogIndex, int lastLogTerm);
        Task<bool> VoteFor(int term, INode voter);
        Task<bool> AppendEntriesResponse(int term, INode follower, int lastLogIndex, bool success);
    }
}
