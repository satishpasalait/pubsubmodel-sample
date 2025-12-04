# PubSub.Sample - RabbitMQ Messaging Patterns

A comprehensive .NET 10 sample solution demonstrating different RabbitMQ messaging patterns, from basic queues to advanced error handling and real-world event-driven architectures.

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Solution Structure](#solution-structure)
- [Setup](#setup)
- [Messaging Patterns](#messaging-patterns)
  - [Basic Queue](#1-basic-queue)
  - [Fanout Exchange](#2-fanout-exchange)
  - [Topic Exchange](#3-topic-exchange)
  - [Durable Messaging](#4-durable-messaging)
  - [Acknowledgment and QoS](#5-acknowledgment-and-qos)
  - [Error Handling](#6-error-handling)
  - [Order Events System](#7-order-events-system)
- [Running the Projects](#running-the-projects)
- [Common Utilities](#common-utilities)
- [Troubleshooting](#troubleshooting)

## Overview

This solution demonstrates fundamental and advanced RabbitMQ messaging patterns:

1. **Basic Queue** - Simple point-to-point messaging between a single producer and consumer
2. **Fanout Exchange** - Broadcasting messages to multiple consumers simultaneously
3. **Topic Exchange** - Routing messages to consumers based on routing key patterns
4. **Durable Messaging** - Ensuring messages and queues survive broker restarts
5. **Acknowledgment and QoS** - Implementing message acknowledgment and controlling consumer message flow
6. **Error Handling** - Demonstrating strategies for handling message processing failures
7. **Order Events System** - A comprehensive real-world example using Topic Exchange with multiple microservices

Each pattern includes a separate Producer/Publisher and Consumer project to demonstrate the pattern's behavior.

## Prerequisites

- **.NET 10 SDK** - The solution targets .NET 10.0
- **RabbitMQ Server** - Required for message queuing
- **Visual Studio 2022** or **VS Code** - For development

## Solution Structure

```
PubSub.Sample/
├── Basic/                                    # Basic Queue Pattern
│   ├── PubSub.Sample.Basic.Producer         # Producer application
│   └── PubSub.Sample.Basic.Consumer         # Consumer application
├── Fanout/                                   # Fanout Exchange Pattern
│   ├── PubSub.Sample.Fanout.Publisher       # Publisher application
│   └── PubSub.Sample.Fanout.Consumer        # Consumer application
├── Topic/                                    # Topic Exchange Pattern
│   ├── PubSub.Sample.Topic.Publisher        # Publisher application
│   └── PubSub.Sample.Topic.Consumer         # Consumer application
├── Durable/                                  # Durable Messaging Pattern
│   ├── PubSub.Sample.Durable.Publisher      # Publisher application
│   └── PubSub.Sample.Durable.Consumer       # Consumer application
├── AckQoS/                                   # Acknowledgment and QoS Pattern
│   ├── PubSub.Sample.AckQoS.Publisher       # Publisher application
│   └── PubSub.Sample.AckQoS.Consumer        # Consumer application
├── ErrorHandling/                            # Error Handling Pattern
│   ├── PubSub.Sample.ErrorHandling.Publisher # Publisher application
│   └── PubSub.Sample.ErrorHandling.Consumer # Consumer application
├── OrderEventsSystem/                        # Real-world Event-Driven System
│   ├── PubSub.Sample.OrderEventsSystem.Common           # Common utilities
│   ├── PubSub.Sample.OrderEventsSystem.OrderEvents      # Event definitions
│   ├── PubSub.Sample.OrderEventsSystem.Publisher        # Event publisher
│   ├── PubSub.Sample.OrderEventsSystem.EmailService     # Email service consumer
│   ├── PubSub.Sample.OrderEventsSystem.AnalyticsService # Analytics consumer
│   └── PubSub.Sample.OrderEventsSystem.AuditService     # Audit log consumer
└── PubSub.Sample.Common/                    # Shared utilities and helpers
    └── RabbitMqConnectionHelper.cs          # Common RabbitMQ connection helper
```

## Setup

### 1. Install RabbitMQ

#### Option A: Using Docker (Recommended)

```powershell
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

This will:
- Start RabbitMQ on port **5672** (AMQP)
- Start Management UI on port **15672** (HTTP)

#### Option B: Install Locally

1. Download and install RabbitMQ from [rabbitmq.com/download.html](https://www.rabbitmq.com/download.html)
2. Start the RabbitMQ service:
   ```powershell
   Start-Service RabbitMQ
   ```

### 2. Verify RabbitMQ is Running

Open your browser and navigate to: **http://localhost:15672**

- Default username: `guest`
- Default password: `guest`

If the management UI loads, RabbitMQ is running correctly.

### 3. Restore Dependencies

```powershell
dotnet restore
```

### 4. Build the Solution

```powershell
dotnet build
```

## Messaging Patterns

### 1. Basic Queue

**Purpose**: Simple point-to-point messaging where one producer sends messages to one consumer.

**Key Characteristics**:
- Direct producer-to-consumer communication
- Messages are consumed by only one consumer
- Queue acts as a buffer between producer and consumer
- First-in-first-out (FIFO) message delivery

**Use Cases**:
- Task distribution
- Simple request-response patterns
- Load balancing across workers

**How It Works**:
- Producer sends messages directly to a named queue (`basic.queue`)
- Consumer connects to the same queue and receives messages
- Each message is delivered to only one consumer

**Projects**:
- `PubSub.Sample.Basic.Producer` - Sends messages to the queue
- `PubSub.Sample.Basic.Consumer` - Receives messages from the queue

**Queue Configuration**:
- Queue Name: `basic.queue`
- Durable: `false` (queue deleted when RabbitMQ restarts)
- Auto-delete: `false`
- Exclusive: `false`

**Running the Example**:
1. Start the Consumer: `dotnet run --project PubSub.Sample/Basic/PubSub.Sample.Basic.Consumer`
2. Start the Producer: `dotnet run --project PubSub.Sample/Basic/PubSub.Sample.Basic.Producer`
3. Type messages in the Producer console and watch them appear in the Consumer

**Notes**:
- Messages sent before the consumer starts are buffered in the queue
- Only one consumer will receive each message (if multiple consumers are running, messages are distributed)
- Use this pattern when you need reliable message delivery with single consumer processing

---

### 2. Fanout Exchange

**Purpose**: Broadcast messages to all bound queues, regardless of routing keys. Perfect for pub/sub scenarios.

**Key Characteristics**:
- Messages are broadcast to ALL bound queues
- Routing key is ignored (all bound queues receive every message)
- Each consumer gets a unique queue
- Supports multiple subscribers

**Use Cases**:
- News feeds and notifications
- Event broadcasting
- Multi-service notifications
- Cache invalidation

**How It Works**:
- Publisher sends messages to a Fanout exchange
- Exchange copies the message to all bound queues
- Each consumer binds its own queue to the exchange
- All consumers receive a copy of every message

**Projects**:
- `PubSub.Sample.Fanout.Publisher` - Publishes messages to the fanout exchange
- `PubSub.Sample.Fanout.Consumer` - Subscribes to the exchange and receives all messages

**Exchange Configuration**:
- Exchange Name: `fanout.exchange`
- Exchange Type: `Fanout`
- Durable: `false`
- Auto-delete: `false`

**Running the Example**:
1. Start multiple Consumers (in separate terminals):
   ```powershell
   dotnet run --project PubSub.Sample/Fanout/PubSub.Sample.Fanout.Consumer
   ```
2. Start the Publisher:
   ```powershell
   dotnet run --project PubSub.Sample/Fanout/PubSub.Sample.Fanout.Publisher
   ```
3. Send a message and watch it appear in ALL consumer windows

**Notes**:
- Each consumer gets its own auto-generated queue name
- All consumers receive every message (true broadcast)
- Routing keys are ignored in Fanout exchanges
- Use this pattern when you need to notify multiple services simultaneously
- Useful for event-driven architectures where multiple services need to react to the same event

---

### 3. Topic Exchange

**Purpose**: Route messages to queues based on routing key patterns. Provides flexible message routing based on topics.

**Key Characteristics**:
- Messages routed based on routing key patterns
- Supports wildcard matching (`*` for single word, `#` for multiple words)
- Flexible routing rules
- Enables selective message consumption

**Use Cases**:
- Event routing based on event types
- Categorizing messages by topics
- Selective subscription based on interests
- Microservices event routing

**How It Works**:
- Publisher sends messages with routing keys (e.g., `order.created`, `user.deleted`)
- Consumers bind queues with routing key patterns (e.g., `order.*`, `*.created`, `#`)
- Exchange routes messages to queues whose pattern matches the routing key
- Consumers only receive messages matching their binding pattern

**Projects**:
- `PubSub.Sample.Topic.Publisher` - Publishes messages with routing keys to the topic exchange
- `PubSub.Sample.Topic.Consumer` - Subscribes with a routing key pattern

**Exchange Configuration**:
- Exchange Name: `topic.exchange`
- Exchange Type: `Topic`
- Durable: `false`
- Auto-delete: `false`

**Routing Key Patterns**:
- `*` (star) - Matches exactly one word (e.g., `order.*` matches `order.created` but not `order.created.paid`)
- `#` (hash) - Matches zero or more words (e.g., `order.#` matches `order.created`, `order.created.paid`, etc.)
- Exact match - `order.created` only matches `order.created`

**Running the Example**:
1. Start Consumers with different routing key patterns:
   ```powershell
   # Consumer for order events
   dotnet run --project PubSub.Sample/Topic/PubSub.Sample.Topic.Consumer -- "order.*"
   
   # Consumer for all created events
   dotnet run --project PubSub.Sample/Topic/PubSub.Sample.Topic.Consumer -- "*.created"
   
   # Consumer for all events
   dotnet run --project PubSub.Sample/Topic/PubSub.Sample.Topic.Consumer -- "#"
   ```
2. Start the Publisher:
   ```powershell
   dotnet run --project PubSub.Sample/Topic/PubSub.Sample.Topic.Publisher
   ```
3. Enter messages in format: `<routingKey> <message>`
   - Example: `order.created New Order 123`
   - Example: `user.deleted User ID 456`
   - Example: `payment.processed Payment ID 789`

**Notes**:
- Routing keys typically use dot notation (e.g., `order.created`, `user.updated`)
- Consumers can subscribe to specific topics or use wildcards for broader subscriptions
- One message can be delivered to multiple queues if multiple patterns match
- Use this pattern for event-driven architectures where services need to react to specific event types
- Provides better message filtering compared to Fanout exchange

**Common Routing Key Examples**:
- `order.created` - New order created
- `order.*` - All order events
- `*.created` - All "created" events
- `user.#` - All user-related events
- `payment.processed.success` - Successful payment processing

---

### 4. Durable Messaging

**Purpose**: Ensure messages and queues survive RabbitMQ broker restarts. Essential for production systems that require message persistence.

**Key Characteristics**:
- Durable exchanges and queues survive broker restarts
- Messages are persisted to disk
- Critical for production environments
- Slightly slower performance due to disk I/O

**Use Cases**:
- Critical business messages that must not be lost
- Production systems requiring message persistence
- Financial transactions and audit logs
- Important notifications and alerts

**How It Works**:
- Exchange declared with `durable: true`
- Queue declared with `durable: true`
- Messages are persisted to disk
- After broker restart, durable queues and their messages are restored

**Projects**:
- `PubSub.Sample.Durable.Publisher` - Publishes messages to a durable exchange
- `PubSub.Sample.Durable.Consumer` - Consumes messages from a durable queue

**Exchange Configuration**:
- Exchange Name: `durable.exchange`
- Exchange Type: `Direct`
- Durable: `true`
- Auto-delete: `false`

**Queue Configuration**:
- Queue Name: `durable.queue`
- Durable: `true`
- Auto-delete: `false`
- Exclusive: `false`

**Running the Example**:
1. Start the Consumer:
   ```powershell
   dotnet run --project PubSub.Sample/Durable/PubSub.Sample.Durable.Consumer
   ```
2. Start the Publisher (in another terminal):
   ```powershell
   dotnet run --project PubSub.Sample/Durable/PubSub.Sample.Durable.Publisher
   ```
3. Send some messages, then stop RabbitMQ and restart it
4. Observe that messages are still available after restart (if consumer wasn't running when messages were sent)

**Notes**:
- Durable queues and exchanges survive broker restarts
- Messages in durable queues are persisted to disk
- Use durable messaging for critical messages that must not be lost
- Non-durable queues/exchanges are faster but lose all data on restart
- In production, always use durable queues for important messages

---

### 5. Acknowledgment and QoS

**Purpose**: Control message delivery and ensure reliable processing by using manual acknowledgments and quality of service (QoS) settings.

**Key Characteristics**:
- Manual message acknowledgment (explicit ACK/NACK)
- Quality of Service (QoS) controls message prefetch
- Messages are only removed after acknowledgment
- Failed messages can be requeued
- Prevents message loss if consumer crashes

**Use Cases**:
- Critical message processing that must not be lost
- Long-running tasks where processing may fail
- Ensuring exactly-once processing
- Controlling message flow to prevent consumer overload

**How It Works**:
- Consumer sets QoS to control how many unacknowledged messages it can receive
- Messages are delivered to consumer but not removed from queue
- Consumer processes message and sends ACK when successful
- If processing fails, consumer sends NACK with requeue option
- Message is only removed from queue after ACK

**Projects**:
- `PubSub.Sample.AckQoS.Publisher` - Publishes 30 messages to demonstrate QoS
- `PubSub.Sample.AckQoS.Consumer` - Consumes messages with manual ACK and QoS

**Queue Configuration**:
- Queue Name: `ackqos.queue`
- Durable: `true`
- Auto-delete: `false`
- QoS Prefetch Count: `1` (process one message at a time)

**Running the Example**:
1. Start the Publisher (sends 30 messages):
   ```powershell
   dotnet run --project PubSub.Sample/AckQoS/PubSub.Sample.AckQoS.Publisher
   ```
2. Start the Consumer:
   ```powershell
   dotnet run --project PubSub.Sample/AckQoS/PubSub.Sample.AckQoS.Consumer
   ```
3. Observe messages being processed one at a time (due to QoS=1)
4. Messages containing "error" will be rejected and requeued
5. Stop consumer mid-processing to see messages remain in queue

**Notes**:
- `autoAck: false` enables manual acknowledgment mode
- QoS `prefetchCount: 1` limits consumer to one unacknowledged message
- `BasicAckAsync` confirms successful processing
- `BasicNackAsync` with `requeue: true` puts message back in queue for retry
- Messages are only removed after acknowledgment
- Use this pattern for critical messages that require reliable processing

---

### 6. Error Handling

**Purpose**: Implement robust error handling strategies including dead letter queues (DLQ) for failed messages.

**Key Characteristics**:
- Dead Letter Exchange (DLX) for failed messages
- Dead Letter Queue (DLQ) to store failed messages
- Automatic routing of rejected/nacked messages to DLQ
- Retry mechanisms and error recovery
- Audit trail of failed messages

**Use Cases**:
- Handling transient failures with automatic retry
- Storing permanently failed messages for analysis
- Implementing retry policies
- Monitoring and debugging message processing issues
- Preventing poison messages from blocking processing

**How It Works**:
- Main queue configured with Dead Letter Exchange (DLX)
- Failed messages (NACK without requeue) are routed to DLX
- DLX routes messages to Dead Letter Queue (DLQ)
- DLQ stores failed messages for analysis or manual reprocessing
- Can implement retry logic with message TTL and delayed queues

**Projects**:
- `PubSub.Sample.ErrorHandling.Publisher` - Publishes messages to main exchange
- `PubSub.Sample.ErrorHandling.Consumer` - Consumes messages with error handling and DLQ routing

**Exchange Configuration**:
- Main Exchange: `main.exchange` (Direct, non-durable)
- DLQ Exchange: `dlq.exchange` (Direct, non-durable)

**Queue Configuration**:
- Main Queue: `main.queue` (with DLX configured)
- DLQ Queue: `dlq.queue` (durable)

**Running the Example**:
1. Start the Consumer:
   ```powershell
   dotnet run --project PubSub.Sample/ErrorHandling/PubSub.Sample.ErrorHandling.Consumer
   ```
2. Start the Publisher (in another terminal):
   ```powershell
   dotnet run --project PubSub.Sample/ErrorHandling/PubSub.Sample.ErrorHandling.Publisher
   ```
3. Send messages - some will fail and be routed to DLQ
4. Check the DLQ queue in RabbitMQ Management UI to see failed messages

**Notes**:
- Dead Letter Exchange is configured via queue arguments (`x-dead-letter-exchange`)
- Failed messages are automatically routed to DLQ without requeue
- DLQ allows analysis of why messages failed
- Can implement retry logic by republishing from DLQ after delay
- Use this pattern for production systems requiring robust error handling

---

### 7. Order Events System

**Purpose**: A comprehensive real-world example demonstrating event-driven architecture using Topic Exchange with multiple microservices consuming order-related events.

**Key Characteristics**:
- Event-driven architecture with multiple consumers
- Topic Exchange for flexible event routing
- Multiple microservices (Email, Analytics, Audit)
- JSON-serialized event objects
- Durable exchanges and queues for production-like setup
- Selective event subscription using routing key patterns

**Use Cases**:
- E-commerce order processing
- Microservices communication
- Event sourcing patterns
- Multi-service notification systems
- Analytics and audit logging

**How It Works**:
- Publisher creates different order events (Created, Shipped, Cancelled, Payment Received)
- Events published to Topic Exchange with routing keys like `order.created.{orderId}`
- Multiple services subscribe with different routing key patterns:
  - Email Service: `order.*` (all order events)
  - Analytics Service: `#` (all events)
  - Audit Service: `#` (all events)
- Each service processes events independently and asynchronously

**Projects**:
- `PubSub.Sample.OrderEventsSystem.OrderEvents` - Event type definitions (records)
- `PubSub.Sample.OrderEventsSystem.Common` - Shared connection helper
- `PubSub.Sample.OrderEventsSystem.Publisher` - Publishes order events
- `PubSub.Sample.OrderEventsSystem.EmailService` - Sends emails for order events
- `PubSub.Sample.OrderEventsSystem.AnalyticsService` - Tracks events for analytics
- `PubSub.Sample.OrderEventsSystem.AuditService` - Logs all events for audit

**Event Types**:
- `OrderCreated(OrderId, CustomerEmail, Amount)`
- `OrderShipped(OrderId, TrackingNumber, ShippingProvider)`
- `OrderCancelled(OrderId, Reason)`
- `PaymentReceived(OrderId, Amount, PaymentMethod)`

**Exchange Configuration**:
- Exchange Name: `order.events.exchange`
- Exchange Type: `Topic`
- Durable: `true`

**Queue Configuration**:
- Email Queue: `order.events.email.queue` (binding: `order.*`)
- Analytics Queue: `order.events.analytics.queue` (binding: `#`)
- Audit Queue: `order.events.audit.queue` (binding: `#`)

**Running the Example**:
1. Start all service consumers (in separate terminals):
   ```powershell
   # Email Service
   dotnet run --project PubSub.Sample/OrderEventsSystem/PubSub.Sample.OrderEventsSystem.EmailService
   
   # Analytics Service
   dotnet run --project PubSub.Sample/OrderEventsSystem/PubSub.Sample.OrderEventsSystem.AnalyticsService
   
   # Audit Service
   dotnet run --project PubSub.Sample/OrderEventsSystem/PubSub.Sample.OrderEventsSystem.AuditService
   ```
2. Start the Publisher (in another terminal):
   ```powershell
   dotnet run --project PubSub.Sample/OrderEventsSystem/PubSub.Sample.OrderEventsSystem.Publisher
   ```
3. Choose event type (1-4) and enter event details
4. Observe how different services react to the same events

**Event Routing**:
- Publisher sends events with routing keys: `order.created.{orderId}`, `order.shipped.{orderId}`, etc.
- Email Service receives all order events (`order.*`)
- Analytics Service receives all events (`#`)
- Audit Service receives all events (`#`)

**Notes**:
- Demonstrates real-world event-driven architecture
- Multiple services can process the same events independently
- Services can subscribe to specific event types using routing key patterns
- Events are JSON-serialized for interoperability
- Durable queues ensure events are not lost
- Each service can scale independently
- Use this pattern for microservices architectures and event-driven systems

---

## Running the Projects

### Build All Projects

```powershell
dotnet build
```

### Run Individual Projects

**Basic Queue:**
```powershell
# Consumer
dotnet run --project PubSub.Sample/Basic/PubSub.Sample.Basic.Consumer

# Producer (in another terminal)
dotnet run --project PubSub.Sample/Basic/PubSub.Sample.Basic.Producer
```

**Fanout Exchange:**
```powershell
# Multiple Consumers (start as many as you want)
dotnet run --project PubSub.Sample/Fanout/PubSub.Sample.Fanout.Consumer

# Publisher (in another terminal)
dotnet run --project PubSub.Sample/Fanout/PubSub.Sample.Fanout.Publisher
```

**Topic Exchange:**
```powershell
# Consumer with routing key pattern (routing key is required as argument)
dotnet run --project PubSub.Sample/Topic/PubSub.Sample.Topic.Consumer -- "order.*"

# Publisher (in another terminal)
dotnet run --project PubSub.Sample/Topic/PubSub.Sample.Topic.Publisher
```

**Durable Messaging:**
```powershell
# Consumer
dotnet run --project PubSub.Sample/Durable/PubSub.Sample.Durable.Consumer

# Publisher (in another terminal)
dotnet run --project PubSub.Sample/Durable/PubSub.Sample.Durable.Publisher
```

**Acknowledgment and QoS:**
```powershell
# Publisher (sends 30 messages)
dotnet run --project PubSub.Sample/AckQoS/PubSub.Sample.AckQoS.Publisher

# Consumer (in another terminal)
dotnet run --project PubSub.Sample/AckQoS/PubSub.Sample.AckQoS.Consumer
```

**Error Handling:**
```powershell
# Consumer
dotnet run --project PubSub.Sample/ErrorHandling/PubSub.Sample.ErrorHandling.Consumer

# Publisher (in another terminal)
dotnet run --project PubSub.Sample/ErrorHandling/PubSub.Sample.ErrorHandling.Publisher
```

**Order Events System:**
```powershell
# Start all services (in separate terminals)
dotnet run --project PubSub.Sample/OrderEventsSystem/PubSub.Sample.OrderEventsSystem.EmailService
dotnet run --project PubSub.Sample/OrderEventsSystem/PubSub.Sample.OrderEventsSystem.AnalyticsService
dotnet run --project PubSub.Sample/OrderEventsSystem/PubSub.Sample.OrderEventsSystem.AuditService

# Publisher (in another terminal)
dotnet run --project PubSub.Sample/OrderEventsSystem/PubSub.Sample.OrderEventsSystem.Publisher
```

**Note**: The Topic Consumer requires a routing key pattern as a command-line argument. Examples:
- `"order.*"` - All order events
- `"*.created"` - All created events  
- `"#"` - All events

## Common Utilities

### RabbitMqConnectionHelper

Located in `PubSub.Sample.Common`, this helper class provides a centralized way to create RabbitMQ connections.

**Features**:
- Centralized connection configuration
- Connection timeout settings (10 seconds)
- Automatic recovery enabled
- Error handling with helpful messages
- Connection to `localhost:5672` by default

**Configuration**:
- Host: `localhost`
- Port: `5672` (AMQP port)
- Username: `guest`
- Password: `guest`
- Connection Timeout: 10 seconds
- Automatic Recovery: Enabled

**Usage**:
```csharp
using PubSub.Sample.Common;

var connection = RabbitMqConnectionHelper.CreateConnection();
```

**Note**: The `OrderEventsSystem` folder has its own `PubSub.Sample.OrderEventsSystem.Common` project that contains a separate `RabbitMqConnectionHelper` specifically for that system. This allows for independent configuration if needed in a multi-system architecture.

## Troubleshooting

### Connection Issues

**Error**: `BrokerUnreachableException: None of the specified endpoints were reachable`

**Solutions**:
1. Verify RabbitMQ is running: Check http://localhost:15672
2. Verify port 5672 is accessible: `Test-NetConnection localhost -Port 5672`
3. If using Docker, ensure port 5672 is mapped: `docker ps` should show `0.0.0.0:5672->5672/tcp`
4. Check firewall settings

### Common Issues

- **Port 15672 accessible but 5672 not working**: The management UI port is mapped, but AMQP port is not. Restart Docker container with proper port mapping.
- **Messages not appearing**: Ensure consumers are started before sending messages (for Basic Queue), or check routing key patterns (for Topic Exchange).
- **Multiple consumers not receiving messages**: For Fanout exchange, ensure each consumer has its own queue bound to the exchange.

## Dependencies

- **RabbitMQ.Client** (Version 7.2.0) - RabbitMQ .NET client library
- **.NET 10.0** - Target framework

## Notes

### Pattern Characteristics

- **Basic, Fanout, Topic**: Non-durable exchanges/queues (deleted on RabbitMQ restart) with auto-acknowledgment
- **Durable**: Durable exchanges and queues (survive broker restarts) - production-ready configuration
- **AckQoS**: Manual acknowledgment with Quality of Service (QoS) control
- **ErrorHandling**: Dead Letter Queue (DLQ) pattern for failed messages
- **OrderEventsSystem**: Durable Topic Exchange with multiple microservices - real-world architecture

### General Notes

- Connection errors include helpful diagnostic messages
- Each project can be run independently
- Multiple instances of consumers can be run simultaneously to demonstrate message distribution
- Durable patterns are recommended for production environments
- Manual acknowledgment patterns ensure message reliability but require explicit ACK/NACK handling

## License

This is a sample project for educational purposes.

## Additional Resources

- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [RabbitMQ .NET Client](https://www.rabbitmq.com/dotnet.html)
- [RabbitMQ Tutorials](https://www.rabbitmq.com/getstarted.html)

