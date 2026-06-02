# Test Case 18: Candidate Becomes Follower on Same Term Heartbeat

**Given:** you have a term as a candidate
**When:** you receive a heartbeat that is the same term
**Then:** become a follower (you lost)

## Diagram

```mermaid
stateDiagram-v2
    Candidate --> Follower: Heartbeat received\nfor same term
    note right of Follower
        Another node won the election
        for this term. Candidate lost.
    end note
```
