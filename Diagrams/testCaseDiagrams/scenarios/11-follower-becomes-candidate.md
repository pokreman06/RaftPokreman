# Test Case 11: Follower Becomes Candidate on Election Timeout

**Given:** you are a follower
**When:** your election timeout triggers
**Then:** you become a candidate

## Diagram

```mermaid
stateDiagram-v2
    Follower --> Candidate: Election timeout\ntriggers
    note right of Candidate
        State transition to Candidate
    end note
```
