# Test Case 16: Candidate Becomes Leader on Majority Votes

**Given:** you are a candidate that has sent out votes
**When:** you have received a majority of vote responses (voting for you)
**Then:** you become a leader

## Diagram

```mermaid
stateDiagram-v2
    Candidate --> Leader: Majority votes\nreceived
    note right of Leader
        Candidate received votes from
        majority of nodes (including self)
    end note
```
