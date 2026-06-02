## First Start

1. Given: a single node
   When: the node has just started
   Then: the node is a follower
   
2. Given: a single node
   When: the node has just started
   Then: the node sets its election timeout

## Start as Follower

3. Given: you have a term as a follower
   When:  you receive a heartbeat that is a larger term
   Then:  you update your term

4. Given: you are a follower that has already voted
   When: you receive a request for vote (for the same term)
   Then: you deny that request/vote

5. Given: you are a follower
   When: you receive a request to vote for a higher term
   Then: you increment your term

6. Given: you are a follower
   When: you receive a request to vote for a higher term
   Then: vote for the requestor node

7. Given: you are a follower
   When: you receive an appendentries for current term or future term
   Then: you reset your election timeout

8. Given: you are a follower
   When: you receive an appendentries for previous term
   Then: you do not reset your election timeout

9. Given: you are a follower
   When: you receive a request for vote for lower term
   Then: you ignore it

10. Given: you are a follower
    When: you receive an AppendEntries request for the current term
    Then: you send a response to the leader node

## Follower -> Candidate

11. Given: you are a follower
    When: your election timeout triggers
    Then: you become a candidate

12. Given: you are a follower
    When: you just became a candidate
    Then: you reset your election timeout

13. Given: you are a follower
    When: you just became a candidate
    Then: you increment your term

## Start as Candidate

14. Given: you became a candidate
    When: 
    Then: you vote for yourself

15. Given: you became a candidate
    When:
    Then: you send vote requests to all other nodes

16. Given: you are a candidate that has sent out votes
    When: you have recieved a majority of vote responses (voting for you)
    Then: you become a leader

17. Given: you have a term as a candidate
    When:  you receive a heartbeat that is a larger term
    Then:  you update your term

18. Given: you have a term as a candidate
    When:  you receive a heartbeat that is the same term
    Then:  become a follower (you lost)

19. Given: a candidate is in term N
    When: it receives a request to vote in a higher term
    Then: you become a follwer

20. Given: a candidate is in term N
    When: it receives a request to vote in a higher term
    Then: you vote for the requestor node

21. Given: a candidate is in term N
    When: it receives a request to vote in a higher term
    Then: you update your term


## Candidate -> Leader

<!-- 3 candidates all receive a minority votes -->
22. Given: you area candidate
    When: your election timeout triggers
    Then: you start a new election

23. Given: a candidate wins an election
    When:
    Then: it immediately sends out a heartbeat

24. Given: you are a candidate
    When: you receive a request for vote for lower term
    Then: you ignore it



## Start as a Leader

25. Given: you are a leader
    When: 
    Then: you send a heartbeat to all other nodes (at regular intervals)

26. Given: you are a leader
    When: you receive a heartbeat of a higher term
    Then: you become a follower + increment your term

27. Given: you are a leader
    When: you receive a heartbeat of a lower term
    Then: you ignore it

28. Given: you are a leader
    When: you receive a request for vote for current or lower term
    Then: you ignore it
