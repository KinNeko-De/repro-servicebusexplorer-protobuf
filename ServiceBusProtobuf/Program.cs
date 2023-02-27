using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;
using Azure.Messaging.ServiceBus;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Message;

namespace ServiceBusProtobuf;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var serviceBusConnectionString = "enter your credential here";
        var queueName = "enter your queue name here";

        IMessage message = CreateProtobufMessage();
        
        
        await using ServiceBusClient serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
        await using (var sender = serviceBusClient.CreateSender(queueName))
        {
            var serviceBusMessage = new ServiceBusMessage()
            {
                Body = BinaryData.FromBytes(message.ToByteArray()),
                ContentType = message.Descriptor.FullName,
            };
            await sender.SendMessageAsync(serviceBusMessage);
        }
        Console.WriteLine("Message send");

        await using ServiceBusReceiver receiver = serviceBusClient.CreateReceiver(queueName);

        var receivedServiceBusMessage = await ReceiveServiceBusMessage(receiver);
        var receivedMessage = MyMessage.Parser.ParseFrom(receivedServiceBusMessage.Body);
        
        Console.WriteLine("Message successfully parsed.");
        Console.WriteLine(JsonFormatter.Default.Format(receivedMessage));

        Console.WriteLine("Moving service bus message to dead letter queue");
        await receiver.DeadLetterMessageAsync(receivedServiceBusMessage);

        var brokenServiceBusMessage = await ReceiveServiceBusMessage(receiver);
        try
        {
            var brokenReceivedMessage = MyMessage.Parser.ParseFrom(brokenServiceBusMessage.Body);
            Console.WriteLine("ServiceBusExplorer now supports binary protobuf messages. Thank you :)");
            Console.WriteLine(JsonFormatter.Default.Format(brokenReceivedMessage));
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.ToString());
        }
    }

    private static async Task<ServiceBusReceivedMessage> ReceiveServiceBusMessage(ServiceBusReceiver receiver)
    {
        ServiceBusReceivedMessage? receivedMessage;
        do
        {
            Console.WriteLine("Receiving message");
            receivedMessage = await receiver.ReceiveMessageAsync();
            
        } while (receivedMessage != null);

        return receivedMessage!;
    }

    private static IMessage CreateProtobufMessage()
    {
        var message = new Message.MyMessage()
        {
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
            Submessage =
            {
                new MySubMessage()
                {
                    Content = "a string that a human can read",
                    TypeOf = MyEnum.Normal,
                    Taxes = new Taxes()
                    {
                        Number = 12
                    }
                },
                new MySubMessage()
                {
                    Content = "readable string",
                    TypeOf = MyEnum.Test,
                    MorePrecise = new MorePrecise()
                    {
                        Amount = 12.45
                    }
                }
            }
        };
        return message;
    }
}