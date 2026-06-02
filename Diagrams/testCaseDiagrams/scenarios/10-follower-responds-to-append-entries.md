# Test Case 10: Follower Responds to Current Term AppendEntries

**Given:** you are a follower
**When:** you receive an AppendEntries request for the current term
**Then:** you send a response to the leader node

## Diagram

```mermaid
sequenceDiagram
    participant Leader as Leader<br/>(Term N)
    participant Follower as Follower<br/>(Term N)
    
    Leader->>Follower: AppendEntries (term=N)
    rect rgb(150, 255, 150)
        Follower->>Follower: Process AppendEntries
        Follower->>Follower: Verify term matches
    end
    Follower->>Leader: AppendEntries Response (Success)
```
