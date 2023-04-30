using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Server;
using MQTTnet.Protocol;
using MQTTLib;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, Server!");

//Start the server
await Server.RunBroker();

public static class Server
{
    //Create the server for communciation
    public static async Task RunBroker(){
        /*
         * This sample starts a simple MQTT server which will accept any TCP connection.
         */

        var mqttFactory = new MqttFactory(new MQTTLib.ConsoleLogger());

        // The port for the default endpoint is 1883.
        // The default endpoint is NOT encrypted!
        // Use the builder classes where possible.
        var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

        // The port can be changed using the following API (not used in this example).
        // new MqttServerOptionsBuilder()
        //     .WithDefaultEndpoint()
        //     .WithDefaultEndpointPort(1234)
        //     .Build();

        using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
        {
            await mqttServer.StartAsync();

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            // Stop and dispose the MQTT server if it is no longer needed!
            await mqttServer.StopAsync();
        }
    }
}