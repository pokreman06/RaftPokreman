# Test Case 26: Leader Steps Down on Higher Term Heartbeat

**Given:** you are a leader
**When:** you receive a heartbeat of a higher term
**Then:** you become a follower + increment your term

## Diagram

```mermaid
stateDiagram-v2
    Leader --> Follower: Heartbeat with\nhigher term received
    note right of Follower
        New leader detected with higher term.
        Step down to follower and update term.
    end note
```
