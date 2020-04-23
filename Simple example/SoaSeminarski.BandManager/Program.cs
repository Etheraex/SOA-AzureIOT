using Microsoft.Azure.Devices;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SoaSeminarski.BandManager
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceConnectionString = "HostName=soa-iot-hub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=rNdziZn9xIURSAwdpMASwt6z53E0qHvF0xRJ5TAqHrY=";
            var serviceClient = ServiceClient.CreateFromConnectionString(serviceConnectionString);
            var feedbackTask = ReceiveFeedback(serviceClient);
            while (true)
            {
                Console.WriteLine("Kom device da salje");
                Console.Write("> ");
                var deviceId = Console.ReadLine();
                //await sendCloudToDeviceMessage(serviceClient, deviceId);
                await CallDirectMethod(serviceClient, deviceId);
            }

                  
        }

        private static async Task CallDirectMethod(ServiceClient serviceClient, string deviceId)
        {
            var method = new CloudToDeviceMethod("showMessage");
            method.SetPayloadJson("'Helo from c#'");

            var response = await serviceClient.InvokeDeviceMethodAsync(deviceId, method);
            Console.WriteLine($"Response status:{response.Status} , payload: {response.GetPayloadAsJson()}");
                       
        }

        private static async Task ReceiveFeedback(ServiceClient serviceClient)
        {
            var feedbackReceiver = serviceClient.GetFeedbackReceiver();
            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if(feedbackBatch== null)
                {
                    continue;
                        
                }
                foreach(var record in feedbackBatch.Records)
                {
                    var messageId = record.OriginalMessageId;
                    var statusCode = record.StatusCode;
                    Console.WriteLine($"Feedback mess id: '{messageId}', status code '{statusCode}'");

                }

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }

        }

        private static async Task sendCloudToDeviceMessage(ServiceClient serviceClient, string deviceId)
        {
            Console.WriteLine("Sta da se salje");
            Console.Write(">  ");
            var payload = Console.ReadLine();
            var commandMessage = new Message(Encoding.ASCII.GetBytes(payload));
            commandMessage.MessageId = Guid.NewGuid().ToString();
            commandMessage.Ack = DeliveryAcknowledgement.Full;
            commandMessage.ExpiryTimeUtc = DateTime.UtcNow.AddSeconds(10);
            await serviceClient.SendAsync(deviceId, commandMessage);

        }
    }
}
