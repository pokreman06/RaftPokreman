# Test Case 17: Candidate Receives Higher Term Heartbeat

**Given:** you have a term as a candidate
**When:** you receive a heartbeat that is a larger term
**Then:** you update your term

## Diagram

```mermaid
sequenceDiagram
    participant Leader as Leader<br/>(Term N+1)
    participant Candidate as Candidate<br/>(Term N)
    
    Candidate->>Candidate: Running election for term N
    Leader->>Candidate: AppendEntries (term=N+1)
    rect rgb(200, 150, 255)
        Candidate->>Candidate: Term N+1 > current term
        Candidate->>Candidate: Update term to N+1
    end
    Candidate->>Leader: OK
```
