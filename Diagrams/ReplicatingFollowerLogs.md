# Replicating Follower Logs (heartbeat + entry replication)

```mermaid
sequenceDiagram
  autonumber
  participant L as Leader
  participant F1 as Follower 1
  participant F2 as Follower 2

  Note over L: Leader has new log entry (index i, term T)
  L->>F1: AppendEntries(term=T, prevIndex=i-1, entries=[entry i])
  L->>F2: AppendEntries(term=T, prevIndex=i-1, entries=[entry i])

  F1-->>L: AppendEntriesResponse(success=true, matchIndex=i)
  F2-->>L: AppendEntriesResponse(success=true, matchIndex=i)

  Note right of L: Leader updates matchIndex and nextIndex for followers and advances commitIndex = i (leaderCommit)
  %% Leader advertises leaderCommit in next AppendEntries
  L->>F1: AppendEntries(term=T, leaderCommit=i)  heartbeat with commit
  L->>F2: AppendEntries(term=T, leaderCommit=i)

  F1-->>L: Applied entries up to i
  F2-->>L: Applied entries up to i
```

This sequence shows a leader replicating a single log entry to two followers (three-node cluster), followers accepting the entry, and the leader updating replication indexes and sending a heartbeat.
