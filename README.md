# Transport

Template Service as of 28.04.2022

Database:
* Context for accessing database

Database.Tables:
* Directory for tables specific for service

Handlers:
* IHandler.cs - interface class for handling events
* <EventName>Handler.cs - class for handling <EventName> event

Models:
* EventModel.cs - base class for events
* <EventName>Event.cs - classes inheriting from EventModel, defining all data to be transferred via queues for specific event, separated into directories of services that handle them

EventManager.cs - class managing RabbitMQ queues for service and publishing events