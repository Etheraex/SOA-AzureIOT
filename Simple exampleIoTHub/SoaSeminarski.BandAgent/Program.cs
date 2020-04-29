using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using SoaSeminarski.Common;

namespace SoaSeminarski.BandAgent
{
    class Program
    {
        private const string DeviceConnectionString = "HostName=soa-iot-hub.azure-devices.net;DeviceId=device-01;SharedAccessKey=+SCb+JnpB8iK6NyO71g5ZehUc8YqhLh+/Yc1zNWsNPg=";

        static async Task Main(string[] args)
        {
           
            var device = DeviceClient.CreateFromConnectionString(DeviceConnectionString);
            await device.OpenAsync();
            var receiveEventsTask = receiveEvents(device);
            await device.SetMethodDefaultHandlerAsync(OtherDeviceMethod, null);
            await device.SetMethodHandlerAsync("showMessage", ShowMessage, null);

           Console.WriteLine("Prost uredjaj je konektovan!");
            
            await UpdateTwin(device);


            Console.WriteLine("Unesi jedno od slova");
            Console.WriteLine("q: Nije specifirano");
            Console.WriteLine("h- Toplo je ");
            Console.WriteLine("c- Hladno je ");


            var random = new Random();
            var quitRequested = false;

            while(true)
            {
                Console.WriteLine("Akcija? ");
                var input = Console.ReadKey().KeyChar;
                Console.WriteLine();
                var status = StatusType.NotSpecified;
                var lat = random.Next(0, 100);
                var lon = random.Next(0, 100);

                switch (Char.ToLower(input)) { 
    
                    case 'q':
                        quitRequested = true;
                        break;
                    case 'h':
                        status = StatusType.Hot;
                        break;
                    case 'c':
                        status = StatusType.Cold;
                        break;
                }

                var tele = new Telemetry
                {
                    Latitude = lat,
                    Longitude = lon,
                    Status = status
                };

                var payload = JsonConvert.SerializeObject(tele);
                var message = new Message(Encoding.ASCII.GetBytes(payload));
                await device.SendEventAsync(message);
                Console.WriteLine("Poruka je poslata na cloud.");
            }
        }


        private static Task<MethodResponse> OtherDeviceMethod(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("Pozvana je metoda koja ne postoji.");
            Console.WriteLine(methodRequest.Name);
            Console.WriteLine(methodRequest.DataAsJson);
            var responsePayload = Encoding.ASCII.GetBytes("{\"Response\": \"Ova metoda ne postoji.\"}");
            return Task.FromResult(new MethodResponse(responsePayload, 404));

        }

        private static Task<MethodResponse> ShowMessage(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine("Pozvana show message metoda");
            Console.WriteLine(methodRequest.DataAsJson);
            var responsePayload = Encoding.ASCII.GetBytes("{\"Response\": \"Poruka je prikazana\"}");
            return Task.FromResult(new MethodResponse(responsePayload, 200));

        }

        private static async Task receiveEvents(DeviceClient device)
        {
            while (true)
            {
                var message = await device.ReceiveAsync();
               if (message == null)
                {
                    continue;
                }
                var messageBody = message.GetBytes();
                var payload = Encoding.ASCII.GetString(messageBody);
                Console.WriteLine($"Poruka sa cloud-a: '{payload}'");
                await device.CompleteAsync(message);

            }
        }

        private static async Task UpdateTwin(DeviceClient device)
        {
               var twinProperties = new TwinCollection();
            twinProperties["connectionType"] = "wi-fi";
            twinProperties["connectionStrength"] = "full";
            await device.UpdateReportedPropertiesAsync(twinProperties);
            

        }
    }
}
