using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Newtonsoft.Json;
using SoaSeminarski.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace SoaSeminarski.MessageProcessor
{
    class LoggingEventProcessor : IEventProcessor
    {
      
        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            // throw new NotImplementedException();

            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {
            // throw new NotImplementedException();
            Console.WriteLine("LoggingEventProcessor opened, processing partition: " +
                             $"'{context.PartitionId}'");
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            //  throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public  Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            //throw new NotImplementedException();
            foreach (var eventData in messages)
            {
                var payload = Encoding.ASCII.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                var deviceId = eventData.SystemProperties["iothub-connection-device-id"];

                Console.WriteLine($"Message received on partition '{context.PartitionId}', " +
                                  $"device ID: '{deviceId}', " +
                                  $"payload: '{payload}'");

                var tele = JsonConvert.DeserializeObject<Telemetry>(payload);

                if (tele.Status == StatusType.Hot)
                {
                    Console.WriteLine("Toplo je.");
                  SendResponse(tele.Latitude, tele.Longitude);
                } else if (tele.Status == StatusType.Cold)
                {
                    Console.WriteLine("Hladno je.");
                    SendResponse(tele.Latitude, tele.Longitude);
                }

            }
            return context.CheckpointAsync();
        }

        private  async void SendResponse(decimal latitude, decimal longitude)
        { 
            Console.WriteLine($"Saljemo pomoc na( {latitude}, {longitude}).");
           
        }

   

    }
}
