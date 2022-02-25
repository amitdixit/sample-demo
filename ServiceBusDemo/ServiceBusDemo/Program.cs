// See https://aka.ms/new-console-template for more information
using Azure.Messaging.ServiceBus;



string connectionString = "Endpoint==";
string queueName = "test-queue";

// Because ServiceBusClient implements IAsyncDisposable, we'll create it 
// with "await using" so that it is automatically disposed for us.
await using var client = new ServiceBusClient(connectionString);
int counter = 1;
while (true)
{
    await Task.Delay(500);
    // The sender is responsible for publishing messages to the queue.
    ServiceBusSender sender = client.CreateSender(queueName);
    ServiceBusMessage message = new ServiceBusMessage($"Hello world! {counter}");
    Console.WriteLine($"Hello world! {counter}");

    await sender.SendMessageAsync(message);
    counter++;
}

/*
// The receiver is responsible for reading messages from the queue.
ServiceBusReceiver receiver = client.CreateReceiver(queueName);
while (true)
{
    ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();

    string? body = receivedMessage?.Body?.ToString();
    Console.WriteLine(body);

    // dead-letter the message, thereby preventing the message from being received again without receiving from the dead letter queue.
    await receiver.DeadLetterMessageAsync(receivedMessage);
    // receive the dead lettered message with receiver scoped to the dead letter queue.
    ServiceBusReceiver dlqReceiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions
    {
        SubQueue = SubQueue.DeadLetter
    });
    ServiceBusReceivedMessage dlqMessage = await dlqReceiver.ReceiveMessageAsync();
    // defer the message, thereby preventing the message from being received again without using
    // the received deferred message API.
    //  await receiver.DeferMessageAsync(receivedMessage);

    // receive the deferred message by specifying the service set sequence number of the original
    // received message
    // ServiceBusReceivedMessage deferredMessage = await receiver.ReceiveDeferredMessageAsync(receivedMessage.SequenceNumber);

    // await receiver.CompleteMessageAsync(receivedMessage);
}
*/
