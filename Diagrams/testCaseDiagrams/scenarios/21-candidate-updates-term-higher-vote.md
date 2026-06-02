# Test Case 21: Candidate Updates Term on Higher Term Vote Request

**Given:** a candidate is in term N
**When:** it receives a request to vote in a higher term
**Then:** you update your term

## Diagram

```mermaid
sequenceDiagram
    participant Candidate as Candidate<br/>(Term N)
    participant Candidate2 as New Candidate<br/>(Term N+1)
    
    Candidate->>Candidate: Current term = N
    Candidate2->>Candidate: RequestVote (term=N+1)
    rect rgb(200, 150, 255)
        Candidate->>Candidate: Update term from N to N+1
        Candidate->>Candidate: Current term = N+1
    end
```
