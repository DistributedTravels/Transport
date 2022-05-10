# Transport

Database.TransportContext:
* Context for accessing SQL database

Database.Tables:
* Directory for tables specific for service

Consumers:
* Consuming events and acting accordingly to the type of event

Models:
* EventModel.cs - base class for events
* <EventName>Event.cs - event classes inheriting from EventModel, defining all data to be transferred via queues for specific event, separated into directories of services that handle them
* <EventName>ReplyEvent.cs - event classes sent in reply to events, received by other services

Init:
* Files for initial configuration (here data from JSON files to be input into SQL database)
