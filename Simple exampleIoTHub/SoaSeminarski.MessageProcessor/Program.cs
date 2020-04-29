using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Threading.Tasks;

namespace SoaSeminarski.MessageProcessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            var hubName = "iothub-ehub-soa-iot-hu-3285897-9599b6b7c1";
            var iotHubConnectionString = "Endpoint=sb://ihsuprodblres100dednamespace.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=kogsdygIoIXeMHygFKBXfj9J57fkmWUb3KoRr/ximdE=;EntityPath=iothub-ehub-soa-iot-hu-3285897-9599b6b7c1";
            var storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=soaiotstorage01;AccountKey=DJKsrMDp1HO/r8Qx8FelAV7Uh0gK4FHy5ZZb9wo8B7Ip4BIx8ZMC+894DL+5xo6uM39uGS1xgxriwantAw+TiQ==;EndpointSuffix=core.windows.net";
            var storageContainerName = "message-processor-host";
            var consumerGroupName = PartitionReceiver.DefaultConsumerGroupName;

            var processor = new EventProcessorHost(hubName, consumerGroupName, iotHubConnectionString, storageConnectionString, storageContainerName);
            await processor.RegisterEventProcessorAsync<LoggingEventProcessor>();

            Console.WriteLine("Obrada poruka je startovana.");
            Console.ReadLine();
            await processor.UnregisterEventProcessorAsync();


        }
    }
}
