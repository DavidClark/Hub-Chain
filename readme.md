#Hub-Chain

A collection of 'endpoints' that can be subscribed to by signalr clients.

The server they subscribe to is either the data source or it is chained to it.  
If it is chained to the source it subscribes to the endoint on the source and maintains a cache of the latest value.
All servers can be on redis or the server will relay published items too.

Subscribing to an endpoint creates a published observable on the server.  
This can have multiple subscriptions and is removed on disposal.
