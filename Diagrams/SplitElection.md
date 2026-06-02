# Split Election (5 nodes)

```mermaid
sequenceDiagram
  autonumber
  participant N1 as Node 1
  participant N2 as Node 2
  participant N3 as Node 3
  participant N4 as Node 4
  participant N5 as Node 5

  Note over N1: Nodes 1, 2 and 3 timeout simultaneously (term T)

  N1->>N1: VoteSelf(term=T)
  N2->>N2: VoteSelf(term=T)
  N3->>N3: VoteSelf(term=T)

  N1->>N4: RequestVote(term=T)
  N1->>N5: RequestVote(term=T)

  N2->>N4: RequestVote(term=T)
  N2->>N5: RequestVote(term=T)

  N3->>N4: RequestVote(term=T)
  N3->>N5: RequestVote(term=T)

  %% Followers split their votes producing no majority
  N4-->>N1: VoteGranted(term=T)
  N5-->>N2: VoteGranted(term=T)

  Note right of N1: Results after first round: N1=2 (self + N4), N2=2 (self + N5), N3=1 (self). No majority -> split vote

  %% Election stalls; later Node 3 times out again and retries
  Note over N3: Timeout again -> new election (term T+1)
```

This diagram shows a split election among five nodes: three nodes time out simultaneously and each votes for itself; followers split their votes so no candidate reaches a majority. Later, Node 3 times out again in a higher term, requests votes, wins the majority, and issues a heartbeat to establish leadership.
