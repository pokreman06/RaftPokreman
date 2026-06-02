# Test Case 19: Candidate Becomes Follower on Higher Term Vote Request

**Given:** a candidate is in term N
**When:** it receives a request to vote in a higher term
**Then:** you become a follower

## Diagram

```mermaid
stateDiagram-v2
    Candidate --> Follower: RequestVote\nfor higher term received
    note right of Follower
        Higher term detected.
        Step down to follower.
    end note
```
