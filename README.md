# raft-pokreman06


# Proposed test scenarios

## First Start

1. Given: a single node [x]
   When: the node has just started
   Then: the node is a follower
   
2. Given: a single node [x]
   When: the node has just started
   Then: the node sets its election timeout

## Start as Follower

3. Given: you have a term as a follower [x]
   When:  you receive a heartbeat that is a larger term
   Then:  you update your term

4. Given: you are a follower that has already voted [x]
   When: you receive a request for vote (for the same term)
   Then: you deny that request/vote

5. Given: you are a follower [x]
   When: you receive a request to vote for a higher term
   Then: you increment your term

6. Given: you are a follower [x]
   When: you receive a request to vote for a higher term
   Then: vote for the requestor node

7. Given: you are a follower [x]
   When: you receive an appendentries for current term or future term
   Then: you reset your election timeout

8. Given: you are a follower [x]
   When: you receive an appendentries for previous term
   Then: you do not reset your election timeout

9. Given: you are a follower [x]
   When: you receive a request for vote for lower term
   Then: you ignore it

10. Given: you are a follower [x]
    When: you receive an AppendEntries request for the current term
    Then: you send a response to the leader node

## Follower -> Candidate

11. Given: you are a follower [x]
    When: your election timeout triggers
    Then: you become a candidate

12. Given: you are a follower [x]
    When: you just became a candidate
    Then: you reset your election timeout

13. Given: you are a follower [x]
    When: you just became a candidate
    Then: you increment your term

## Start as Candidate

14. Given: you became a candidate [x]
    When: 
    Then: you vote for yourself

15. Given: you became a candidate [x]
    When:
    Then: you send vote requests to all other nodes

16. Given: you are a candidate that has sent out votes [x]
    When: you have recieved a majority of vote responses (voting for you)
    Then: you become a leader

17. Given: you have a term as a candidate [x]
    When:  you receive a heartbeat that is a larger term
    Then:  you update your term

18. Given: you have a term as a candidate [x]
    When:  you receive a heartbeat that is the same term
    Then:  become a follower (you lost)

19. Given: a candidate is in term N [x]
    When: it receives a request to vote in a higher term
    Then: you become a follwer

20. Given: a candidate is in term N [x]
    When: it receives a request to vote in a higher term
    Then: you vote for the requestor node

21. Given: a candidate is in term N [x]
    When: it receives a request to vote in a higher term
    Then: you update your term


## Candidate -> Leader

<!-- 3 candidates all receive a minority votes -->
22. Given: you area candidate [x]
    When: your election timeout triggers
    Then: you start a new election

23. Given: a candidate wins an election [x]
    When:
    Then: it immediately sends out a heartbeat

24. Given: you are a candidate [x]
    When: you receive a request for vote for lower term
    Then: you ignore it



## Start as a Leader

25. Given: you are a leader [x]
    When: 
    Then: you send a heartbeat to all other nodes (at regular intervals)

26. Given: you are a leader [x]
    When: you receive a heartbeat of a higher term
    Then: you become a follower + increment your term

27. Given: you are a leader [x]
    When: you receive a heartbeat of a lower term
    Then: you ignore it

28. Given: you are a leader [x]
    When: you receive a request for vote for current or lower term
    Then: you ignore it


# Log Replication scenarios

## as a leader

1. Given: you are a leader [x]
when: you receive a command
then: it adds it to its own log (uncommitted)

2. Given: a leader has a new uncommitted log [x]
when: the leader sends out heartbeats
then: the new log entry is included in the AppendEntries message

3. Given: you are a leader [x]
When: you receive confirmation that a follower has appended logs (response to AppendEntries)
Then: you update that nodes log index

4. Given: you are a leader [x]
When: you receive confirmation that a majority of nodes have appended the log index
Then: you update your commit index to include the new log

5. Given: you are a leader [x]
When: you have a commit index
Then: you include your commit index in AppendEntries heartbeats

6. Given: you are a leader [x]
When: a follower rejects your appendEntries
Then: you decrement your "nextIndex" value for that follower (maybe rejection message has desired index)

7. Given: you are a leader [x]
When: you send appendEntries that are not acknowledged
Then: you continue to send appendEntries

8. Given: you are a leader [x]
When: you commit a log entry
Then: apply new logs to state machine


9. Given: you just won an election [x]
Then: you initialize a "nextIndex" value for each follower

## as a follower

10. Given: you receive an appendEntries [x]
when: it has a new log entry
then: send back an acknowledgement (include: latest index you have in your log, and what term you are responding to)

11. Given: you receive an appendEntries [x]
when: it has a new log entry
then: you add the logs to your own log

12. Given: you are a follower [x]
When: you receive a command from the client
Then: and ignore the request (you respond with the current leader ID) 

13. Given: you are a follower [x]
When: you receive an appendEntries where the first log index of the logs is larger than your log (you are on log 3, got request for log 5)
Then: you send a rejection response to the leader (include your log index)

14. Given: you are a follower [x]
When: you receive an appendEntries where the the first AppendEntrie log entry key (term and index) has a different term than your log at that index
Then: you send a rejection response to the leader (include your log index)
- your logs: (term, index) [(1, 0), (1, 1), (1, 2)], appendEntries: [(2, 2), (2, 3)] -> (1, 2) =/= (2, 2) -> reject

15. Given: you are a follower [x]
When: you receive an appendEntries where the the first log index matches a log you have, but follow up logs mismatch your coppies
Then: you delete your local logs and replace them with the appendEntries logs
- your logs: (term, index) [(1, 0), (1, 1), (1, 2)], appendEntries: [(1, 1), (2, 2), (2, 3)] -> (1, 1) === (1, 1) -> accept, then delete your local (1, 2)

16. Given: you are a follower [x]
When: you receive an appendEntries that you accept (logs line up)
Then: you make your commit index match the AppendEntries data
 
17. Given: you are a follower [x]
When: you increase your commit index
Then: apply new logs to state machine
