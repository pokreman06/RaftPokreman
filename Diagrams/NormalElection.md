# Normal Election

```mermaid
sequenceDiagram
  autonumber
  participant C as Candidate
  participant F1 as Follower 1
  participant F2 as Follower 2

  Note over C: Candidate timed out (term T)\nstarts election
  C->>C: VoteSelf(term=T)  candidate votes for itself
  C->>F1: RequestVote(term=T, lastLogIndex=ic, lastLogTerm=tc)
  C->>F2: RequestVote(term=T, lastLogIndex=ic, lastLogTerm=tc)


  F1-->>C: VoteGranted(term=T)

  Note right of C: Candidate has majority (self + F1 = 2/3)\nand becomes leader for term T

  F2-->>C: VoteGranted(term=T)

  C->>F1: AppendEntries(term=T) single heartbeat
  C->>F2: AppendEntries(term=T)
```

This sequence shows a normal election: the candidate times out, requests votes from all servers, wins the election (majority), and sends a single heartbeat (AppendEntries) to establish leadership.
