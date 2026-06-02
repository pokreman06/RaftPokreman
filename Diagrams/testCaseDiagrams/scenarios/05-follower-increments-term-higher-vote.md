# Test Case 5: Follower Increments Term for Higher Vote Request

**Given:** you are a follower
**When:** you receive a request to vote for a higher term
**Then:** you increment your term

## Diagram

```mermaid
sequenceDiagram
    participant Candidate as Candidate<br/>(Term N+1)
    participant Follower as Follower<br/>(Term N)
    
    Candidate->>Follower: RequestVote (term=N+1)
    rect rgb(200, 150, 255)
        Follower->>Follower: Update term from N to N+1
    end
    Follower->>Candidate: Term updated
```
