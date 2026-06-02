# Test Case 20: Candidate Votes for Higher Term Requestor

**Given:** a candidate is in term N
**When:** it receives a request to vote in a higher term
**Then:** you vote for the requestor node

## Diagram

```mermaid
sequenceDiagram
    participant Candidate as Candidate<br/>(Term N)
    participant Candidate2 as New Candidate<br/>(Term N+1)
    
    Candidate->>Candidate: Running election for term N
    Candidate2->>Candidate: RequestVote (term=N+1)
    rect rgb(150, 255, 150)
        Candidate->>Candidate: Term N+1 > current term
        Candidate->>Candidate: Vote for Candidate2
    end
    Candidate->>Candidate2: VOTE GRANTED
```
