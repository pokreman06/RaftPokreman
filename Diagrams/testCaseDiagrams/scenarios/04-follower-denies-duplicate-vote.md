# Test Case 4: Follower Denies Duplicate Vote Request

**Given:** you are a follower that has already voted
**When:** you receive a request for vote (for the same term)
**Then:** you deny that request/vote

## Diagram

```mermaid
sequenceDiagram
    participant Candidate1 as Candidate 1<br/>(Term N)
    participant Follower as Follower<br/>(Term N)
    participant Candidate2 as Candidate 2<br/>(Term N)
    
    Candidate1->>Follower: RequestVote (term=N)
    rect rgb(150, 255, 150)
        Follower->>Follower: Vote for Candidate1
    end
    Follower->>Candidate1: VOTE GRANTED
    
    Candidate2->>Follower: RequestVote (term=N)
    rect rgb(255, 150, 150)
        Follower->>Follower: Already voted in term N
    end
    Follower->>Candidate2: VOTE DENIED
```
