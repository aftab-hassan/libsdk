using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace libsdk
{
    class Program
    {
        static void Main(string[] args)
        {
            Endpoint endpoint = new Endpoint("HostName=afhassan-iothub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ZqKX5O4OxGf2RNzzZwsIqgV4hYMO9qI3nMJI8SWGcM0=", "AftDeviceId1");

            LibSdk libSdk = new LibSdk();

            // Use case 1 - Add a device
            Task<DeviceInfo> deviceInfo = libSdk.AddDeviceAsync(endpoint);
            //deviceInfo.Wait();
            Console.WriteLine("printing result : " + deviceInfo.Result.PrimaryKeyConnectionString);

            // Use case 2 - Send message from device to cloud
            List<Telemetry> data = new List<Telemetry>();
            Dictionary<string, string> dictionary = new Dictionary<string, string>();//first message
            dictionary.Add("deviceId", "0");
            dictionary.Add("messageId", "1");
            dictionary.Add("temperature", "98.6");
            dictionary.Add("humidity", "99.9");
            Dictionary<string, string> dictionary1 = new Dictionary<string, string>();//second message
            dictionary1.Add("deviceId", "000");
            dictionary1.Add("messageId", "111");
            dictionary1.Add("temperature", "98.666");
            dictionary1.Add("humidity", "99.999");
            Dictionary<string, string> dictionary2 = new Dictionary<string, string>();//third message
            dictionary2.Add("deviceId", "___000");
            dictionary2.Add("messageId", "___111");
            dictionary2.Add("temperature", "___98.666");
            dictionary2.Add("humidity", "___99.999");
            Telemetry telemetry = new Telemetry(dictionary);
            Telemetry telemetry1 = new Telemetry(dictionary1);
            Telemetry telemetry2 = new Telemetry(dictionary2);
            data.Add(telemetry);
            data.Add(telemetry1);
            data.Add(telemetry2);
            Result sendMessageResult = libSdk.SendMessageD2CAsync(deviceInfo.Result, data);
            //sendMessageResult.Wait();
            //Console.WriteLine("boolean flag : " + sendMessageResult.Result.IsSuccessful + ", reason : " + sendMessageResult.Result.Reason);
            //libSdk.SendEvent(deviceInfo.Result).Wait();


            Console.Read();
        }

        
    }

    


}
