# TC7: Leader Continues Sending

```mermaid
%%{init: {'theme': 'dark'}}%%
graph TD
    A["AppendEntries not<br/>acknowledged by<br/>follower"] --> B["Continue sending<br/>AppendEntries"]
    B --> C["Persistent retry<br/>until success"]
    
    style A fill:#f57c00,color:#fff
    style B fill:#01579b,color:#fff
    style C fill:#0288d1,color:#fff
```
