# PubSub.Sample - RabbitMQ Messaging Patterns

A comprehensive .NET 10 sample solution demonstrating different RabbitMQ messaging patterns, including Basic Queue, Fanout Exchange, and Topic Exchange patterns.

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Solution Structure](#solution-structure)
- [Setup](#setup)
- [Messaging Patterns](#messaging-patterns)
  - [Basic Queue](#1-basic-queue)
  - [Fanout Exchange](#2-fanout-exchange)
  - [Topic Exchange](#3-topic-exchange)
- [Running the Projects](#running-the-projects)
- [Common Utilities](#common-utilities)

## Overview

This solution demonstrates three fundamental RabbitMQ messaging patterns:

1. **Basic Queue** - Simple point-to-point messaging between a single producer and consumer
2. **Fanout Exchange** - Broadcasting messages to multiple consumers simultaneously
3. **Topic Exchange** - Routing messages to consumers based on routing key patterns

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

- All exchanges and queues are non-durable (will be deleted on RabbitMQ restart)
- Auto-acknowledgment is enabled for simplicity (messages are automatically removed after delivery)
- Connection errors include helpful diagnostic messages
- Each project can be run independently
- Multiple instances of consumers can be run simultaneously to demonstrate message distribution

## License

This is a sample project for educational purposes.

## Additional Resources

- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [RabbitMQ .NET Client](https://www.rabbitmq.com/dotnet.html)
- [RabbitMQ Tutorials](https://www.rabbitmq.com/getstarted.html)

