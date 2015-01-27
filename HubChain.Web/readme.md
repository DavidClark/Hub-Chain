#Hub-Chain

A signalR hub manages an observable sequence for every group across connections

Connections can subscribe (join) and unsubscribe (leave) from groups.
When all the connections have unsubscribed from a group (or disconnected) the source observable sequence is disposed.

Hubs will be scaled out using redis.

* If it has a chainhub it will subscribe to its chainhub for the values and not publish them.
* If it hasn't got a chain hub it will provide the source observable and publish it.

