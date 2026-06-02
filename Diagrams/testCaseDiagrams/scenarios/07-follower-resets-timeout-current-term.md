# Test Case 7: Follower Resets Timeout on Current/Future Term AppendEntries

**Given:** you are a follower
**When:** you receive an appendentries for current term or future term
**Then:** you reset your election timeout

## Diagram

```mermaid
sequenceDiagram
    participant Leader as Leader
    participant Follower as Follower
    
    Note over Follower: Election timeout running
    Leader->>Follower: AppendEntries (term >= current)
    rect rgb(150, 200, 255)
        Follower->>Follower: Reset election timeout
        Follower->>Follower: Restart timeout timer
    end
    Follower->>Leader: OK
    Note over Follower: New election timeout set
```
