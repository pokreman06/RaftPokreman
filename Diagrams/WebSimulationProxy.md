## WebSimulationProxy — appendMessage flow

This diagram shows how an `appendMessage` call (modeled as `AppendEntries`) flows through a WebSimulation proxy that implements `INode` and contains an actual `Node` instance which simulates web delay by awaiting before forwarding requests.

```mermaid
sequenceDiagram
    participant Client
    participant Proxy as WebSimulationProxy\n(implements INode)
    participant Node as Node\n(simulates web delay)

    Client->>Proxy: appendMessage(message)
    Note right of Proxy: Proxy implements `INode` and wraps a `Node` instance.
    Proxy->>Proxy: await SimulatedWebDelay()
    Proxy->>Node: AppendEntries(term, proxy, prevLogIndex, prevLogTerm, [message], leaderCommit)
    Note right of Node: Node applies the entry to its log and updates state.
    Node-->>Proxy: bool (success/failure)
    Proxy-->>Client: bool (result)

    %% Optional: RequestVote path for completeness
    Client->>Proxy: requestVote(request)
    Proxy->>Proxy: await SimulatedWebDelay()
    Proxy->>Node: RequestVote(term, proxy, lastLogIndex, lastLogTerm)
    Node-->>Proxy: bool (granted)
    Proxy-->>Client: bool (granted)
```

Notes:
- The `WebSimulationProxy` implements `INode` and delegates calls to the contained `Node` after simulating network latency.
- `SimulatedWebDelay()` can be implemented with `Task.Delay(milliseconds)` to model variable web delays.
