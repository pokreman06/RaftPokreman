# Test Case 8: Follower Ignores Previous Term AppendEntries

**Given:** you are a follower
**When:** you receive an appendentries for previous term
**Then:** you do not reset your election timeout

## Diagram

```mermaid
sequenceDiagram
    participant OldLeader as Old Leader<br/>(Term N)
    participant Follower as Follower<br/>(Term N+1)
    
    Note over Follower: Election timeout running
    OldLeader->>Follower: AppendEntries (term=N)
    rect rgb(255, 200, 150)
        Follower->>Follower: Term N < current term
        Follower->>Follower: Ignore message
        Follower->>Follower: Keep election timeout
    end
    Note over Follower: Election timeout continues
```
