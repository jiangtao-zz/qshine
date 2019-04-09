EventSourcing Aggregate root process::

1. service/controller received a request

2. service convert it to a domain command and send command to command handler

3. command handler create Aggregate Root and replay all events from event store (event stream)

4. command handler call AR behaviour (or action) to raise domain events

5. AR event handlers Handle (play) each event to change AR proeprties

6. command handler persist and clear AR uncommitted events from AR event queue. Ensure no concurrency conflicts.
(conflicts resolver could lookback the existing events and comparing current event to determine)

7. command handler publish events to read model within CQRS context

8. command handler publish domain event across Boundary Context.


