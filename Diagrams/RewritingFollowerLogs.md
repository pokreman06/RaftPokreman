# Rewriting Follower Logs (AppendEntries retry)

```mermaid
%%{init: {"themeVariables": {"fontSize":"16px"}}}%%
flowchart TD
  Leader["Leader (term T) 
  tracks nextIndex[follower]
  =ni"]
  Attempt["Leader: send 
  AppendEntries (prevIndex 
  P; entries at ni)"]
  Follower["Follower 
  (current lastIndex = mi)"]
  Compare{"Does follower 
  accept AppendEntries?"}
  Fail["AppendEntries failed 
  (Conflict detected)"]
  Dec["Leader: decrement 
  nextIndex[follower]; ni = 
  ni - 1"]
  Retry["Leader: retry 
  AppendEntries with ni"]
  Success["AppendEntries 
  succeeded; follower 
  appends entries"]
  Update["Leader: set 
  matchIndex[follower]=ni; 
  set nextIndex[follower]=ni
  +1"]

  Leader --> Attempt
  Attempt --> Follower
  Follower --> Compare

  Compare -->|No| Fail
  Fail --> Dec
  Dec --> Retry
  Retry --> Attempt

  Compare -->|Yes| Success
  Success --> Update
  Update --> Leader

  classDef leader fill:#3a3f44,stroke:#9aa0a6,stroke-width:1px,color:#f0f0f0;
  classDef follower fill:#26292d,stroke:#9aa0a6,stroke-width:1px,color:#f0f0f0;
  classDef action fill:#2c2f33,stroke:#9aa0a6,stroke-width:1px,color:#f8f8f8;
  classDef decision fill:#1f6feb,stroke:#0b5bd7,stroke-width:1px,color:#ffffff;

  class Leader leader;
  class Follower follower;
  class Attempt,Fail,Dec,Retry,Success,Update action;
  class Compare decision;
```

This flowchart shows the leader's retry logic when an `AppendEntries` RPC fails due to log inconsistency: the leader decrements the follower's `nextIndex` and retries until the follower accepts the entries, then updates `matchIndex` and `nextIndex` accordingly.
