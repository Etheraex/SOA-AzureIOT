
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;


namespace SoaSeminarski.BandManager
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceConnectionString = "HostName=soa-iot-hub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=rNdziZn9xIURSAwdpMASwt6z53E0qHvF0xRJ5TAqHrY=";
            var serviceClient = ServiceClient.CreateFromConnectionString(serviceConnectionString);
            var registryManager = RegistryManager.CreateFromConnectionString(serviceConnectionString);
            Console.WriteLine("Menadzer je startovan.");
            Console.WriteLine("u- Za update temperature");
            Console.WriteLine("r- Za reset brojaca poruka");
            Console.WriteLine("m- Za slanje poruke uredjaju");

            var feedbackTask = ReceiveFeedback(serviceClient);
            while (true)
            {
              
                Console.WriteLine("Akcija? ");
                var input = Console.ReadKey().KeyChar;
                Console.WriteLine();
                switch (Char.ToLower(input))
                {
                    case 'u':
                        {
                            Console.WriteLine("Unesite id iot edge uredjaja");
                            Console.Write("> ");
                            var deviceId = Console.ReadLine();
                            Console.WriteLine("Unesite granicnu temperaturu.");
                            Console.Write("> ");
                            int temperature =Convert.ToInt32(Console.ReadLine());
                            await UpdateDeviceTemperature(registryManager, deviceId , temperature);
                       

                        }
                        break;
                    case 'r':
                        {
                            Console.WriteLine("Unesite id iot edge uredjaja za reset njegovog brojaca.");
                              Console.Write("> ");
                             var deviceId = Console.ReadLine();
                            await CallDirectMethod(serviceClient, deviceId);


                        }
                        break;
                    case 'm':
                        {
                            Console.WriteLine("Unesite id prostog uredjaja");
                            Console.Write("> ");
                            var deviceId = Console.ReadLine();
                            await sendCloudToDeviceMessage(serviceClient, deviceId);

                        }
                        break;
                }


            }

                  
        }


        private static async Task UpdateDeviceTemperature(RegistryManager registryManager, string deviceId, int temperature)
        {

            var moduleTwin = await registryManager.GetTwinAsync(deviceId, "EdgeFilterMessages");

            moduleTwin.Properties.Desired["TemperatureThreshold"] = temperature;
            var updatedTwin = await registryManager.UpdateTwinAsync(deviceId, "EdgeFilterMessages", moduleTwin, moduleTwin.ETag);
            Console.WriteLine("Uspesno promenjena temperatura.");

        }


        private static async Task CallDirectMethod(ServiceClient serviceClient, string deviceId)
        {
            var method = new CloudToDeviceMethod("resetCounter");
            var response= await serviceClient.InvokeDeviceMethodAsync(deviceId, "EdgeFilterMessages", method);
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
                    Console.WriteLine($"Feedback for message: '{messageId}', status code '{statusCode}'");

                }

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }

        }


        private static async Task sendCloudToDeviceMessage(ServiceClient serviceClient, string deviceId)
        {
            Console.WriteLine("Unesite tekst za slanje:");
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
