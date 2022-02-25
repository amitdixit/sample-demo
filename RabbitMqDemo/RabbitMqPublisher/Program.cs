// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

while (true)
{
    Console.WriteLine("Write Message");
    var msg = Console.ReadLine();
    RabbitMqPublisher.Communiator.Send("akd-queue",msg);
}
Console.WriteLine("Done");