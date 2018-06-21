﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace libsdk
{
    class LibSdk
    {
        public LibSdk()
        {
        }

        // Public facing APIs
        // 1. Add a device
        public async Task<DeviceInfo> AddDeviceAsync(Endpoint endpoint)
        {
            string iotHubConnectionString = endpoint.ConnectionString;
            string deviceId = endpoint.DeviceId;
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);

            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }

            List<string> iotHubConnectionStringComponents = endpoint.ConnectionString.Split(';').ToList<string>();
            string primaryKeyConnectionString = iotHubConnectionStringComponents[0] + ";DeviceId=" + endpoint.DeviceId + ";SharedAccessKey=" + device.Authentication.SymmetricKey.PrimaryKey;
            
            return new DeviceInfo(primaryKeyConnectionString);
        }

        // 2. Send message from device to cloud
        public Result SendMessageD2CAsync(DeviceInfo deviceInfo, List<Telemetry> data)
        {
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(deviceInfo.PrimaryKeyConnectionString);

            if (deviceClient == null)
            {
                return new Result(false, "Failed to create DeviceClient!");
            }
            else
            {
                try
                {
                    SendEvent(deviceClient, data).Wait();
                    return new Result(true, "Message sending successful!");
                }
                
                catch(Exception e)
                {
                    return new Result(false, e.ToString());
                }
                
            }
        }

        // 3. Receive desired property change from cloud to device
        async void ReceiveC2DDesiredPropertyChangeAsync(DeviceInfo deviceInfo, Func<string> callback)
        {

        }

        // Supporting methods
        public async Task SendEvent(DeviceClient deviceClient, List<Telemetry> data)
        {
            string dataBuffer = "[";

            //for (int i = 0; i < 10; i++)
            for (int i = 0; i < data.Count; i++)
            {
                string dataBufferHere = "{";
                int keyValuePairCount = 0;
                foreach (KeyValuePair<string, string> entry in data[i].KeyValuePair)
                {
                    dataBufferHere += addProperty(entry.Key, entry.Value);

                    if (keyValuePairCount != (data[i].KeyValuePair.Count - 1))
                        dataBufferHere += ",";

                    keyValuePairCount++;
                }

                dataBufferHere += "}";
                dataBuffer += dataBufferHere;

                if (i != (data.Count - 1))
                    dataBuffer += ",";

                // Sending each JSON message in loop
                Microsoft.Azure.Devices.Client.Message eventMessage = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(dataBufferHere));
                Console.WriteLine("\t{0}> Sending message: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), 0, dataBufferHere);
                await deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);

                //// Works small piece
                //string dataBufferHardCoded = string.Format("{{\"deviceId\":\"{0}\",\"messageId\":{1},\"temperature\":{2},\"humidity\":{3}}}", 0, 1, 98.6, 99.9);
                //Microsoft.Azure.Devices.Client.Message eventMessage = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(dataBufferHardCoded));
                //Console.WriteLine("\t{0}> Sending message: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), i, dataBufferHardCoded);
                //await deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);

                //// Safe
                //var temperature = new Random().Next(20, 35);
                //var humidity = new Random().Next(60, 80);
                //dataBuffer = string.Format("{{\"deviceId\":\"{0}\",\"messageId\":{1},\"temperature\":{2},\"humidity\":{3}}}", "AftDeviceId1", i, temperature, humidity);
                //Microsoft.Azure.Devices.Client.Message eventMessage = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(dataBuffer));
                //eventMessage.Properties.Add("temperatureAlert", (temperature > 25) ? "true" : "false");
                //Console.WriteLine("\t{0}> Sending message: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), i, dataBuffer);
                //await deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);
            }
            dataBuffer += "]";

            //// Sending the JSON array
            //Microsoft.Azure.Devices.Client.Message eventMessage = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(dataBuffer));
            //Console.WriteLine("\t{0}> Sending message: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), 0, dataBuffer);
            //await deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);
        }

        public async Task SendEvent(DeviceInfo deviceInfo)
        {
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(deviceInfo.PrimaryKeyConnectionString);
            string dataBuffer;

            Console.WriteLine("Device sending {0} messages to IoTHub...\n", 5);

            Random rnd = new Random();
            for (int count = 0; count < 5; count++)
            {
                var temperature = rnd.Next(20, 35);
                var humidity = rnd.Next(60, 80);
                dataBuffer = string.Format("{{\"deviceId\":\"{0}\",\"messageId\":{1},\"temperature\":{2},\"humidity\":{3}}}", "AftDeviceId1", count, temperature, humidity);
                Microsoft.Azure.Devices.Client.Message eventMessage = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(dataBuffer));
                eventMessage.Properties.Add("temperatureAlert", (temperature > 25) ? "true" : "false");
                Console.WriteLine("\t{0}> Sending message: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), count, dataBuffer);

                await deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);
            }
        }

        public string addProperty(string key, string value)
        {
            return "\"" + key + "\":" + "\"" + value + "\"";
        }

    }

    // Complex types
    class DeviceInfo
    {
        public string PrimaryKeyConnectionString;

        public DeviceInfo(string primaryKeyConnectionString)
        {
            this.PrimaryKeyConnectionString = primaryKeyConnectionString;
        }
    }

    class Endpoint
    {
        public string ConnectionString;
        public string DeviceId;

        public Endpoint(string connectionString, string deviceId)
        {
            this.ConnectionString = connectionString;
            this.DeviceId = deviceId;
        }
    }

    class Telemetry
    {
        public Dictionary<string, string> KeyValuePair;

        public Telemetry(Dictionary<string, string> keyValuePair)
        {
            this.KeyValuePair = keyValuePair;
        }
    }

    class Result
    {
        public Boolean IsSuccessful;
        public string Reason;

        public Result(Boolean isSuccessful, string reason)
        {
            this.IsSuccessful = isSuccessful;
            this.Reason = reason;
        }
    }
}
