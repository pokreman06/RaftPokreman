## WebSimulationProxy — appendMessage flow (leader -> follower via proxy)

This diagram shows an `AppendEntries` call flowing from one Raft node (leader) to another (follower) through a `WebSimulationProxy`. The proxy simulates network latency by awaiting before forwarding the request.

```mermaid
sequenceDiagram
    participant Leader as LeaderNode
    participant Proxy as WebSimulationProxy\n(simulates network)
    participant Follower as FollowerNode

    Leader->>Proxy: AppendEntries(term, leader, prevLogIndex, prevLogTerm, [message], leaderCommit)
    Note right of Proxy: Proxy simulates web delay before forwarding.
    Proxy->>Proxy: await SimulatedWebDelay()
    Proxy->>Follower: AppendEntries(term, leader, prevLogIndex, prevLogTerm, [message], leaderCommit)
    Note right of Follower: Follower applies entry to log and updates state.
    Follower-->>Proxy: bool (success/failure)
    Proxy-->>Leader: bool (result)

    %% Optional: RequestVote path for election simulation
    Leader->>Proxy: RequestVote(term, leader, lastLogIndex, lastLogTerm)
    Proxy->>Proxy: await SimulatedWebDelay()
    Proxy->>Follower: RequestVote(term, leader, lastLogIndex, lastLogTerm)
    Follower-->>Proxy: bool (granted)
    Proxy-->>Leader: bool (granted)
```

Notes:
- The `WebSimulationProxy` stands between two `INode` implementations and simulates network latency (e.g. `await Task.Delay(ms)`) before forwarding calls.
- Use the proxy to test timing-related Raft behaviors (log replication, election timeouts, split votes).

If you want, I can also add a small example `WebSimulationProxy` implementation in `raftLibrary` to demonstrate `Task.Delay`-based latency.
