### Wbskt Client for Real-Time Communication

The Wbskt.Client.CSharp project is a .NET-based client library designed for seamless real-time communication with the Wbskt platform. It provides robust features for managing WebSocket connections, sending and receiving messages, and handling authentication via JWT tokens. The library is highly configurable, supporting dynamic settings through `IOptionsMonitor` or custom configurations. Key functionalities include:

- **WebSocket Communication**: Establishes and maintains WebSocket connections with retry mechanisms.
- **Authentication**: Secure token-based authentication using JWT.
- **Message Handling**: Processes incoming and outgoing messages with extensible payload handling.
- **Configuration Support**: Flexible configuration options for server address, client details, and channel subscriptions.
- **Logging**: Integrated logging for debugging and monitoring.

---

This project is ideal for applications requiring real-time updates, such as chat systems, live notifications, collaborative tools, and IoT services.

---

#### Setup

Go to [wbskt.com](https://wbskt.com), log in, and create a channel. You will need to provide a `ChannelSecret`, and after creation, your channel will be assigned a `SubscriberId`.

##### Now in your .NET application:

Add a reference to the `Wbskt.Client.CSharp` library from [nuget.org](https://nuget.org), and then in your code:

```csharp
var config = new WbsktConfigurationCustom
{
    ChannelDetails = new ChannelDetails()
    {
        Secret = "{ChannelSecret}", // The secret you provided when creating the channel.
        SubscriberId = "{SubscriberId}" // The ID you received when the channel was created.
    },
    ClientDetails = new ClientDetails
    {
        Name = "Bob's Application",
        UniqueId = Guid.NewGuid()
    },
};
```

Once you have the configuration, you can start listening:

````csharp
// Your custom method. This will be triggered when an event is fired on the Wbskt server.
void BobsFunction(ClientPayload payload)
{
    // Your custom logic here.
}

// Initialize a listener (you can even have multiple listeners listening to different channels).
var listener = new WbsktListener(config);

// Register your method to the event.
listener.ReceivedPayloadEvent += BobsFunction;

// You can also have multiple custom methods registered to the same event.
listener.ReceivedPayloadEvent += AlicesFunction;

// Start listening.
listener.StartListening();

// Stop listening.
listener.StopListening();
````