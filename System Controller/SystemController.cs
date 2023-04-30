using System.Buffers;
using System.Net;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MQTTLib;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;

Console.WriteLine("Hello, Controller!");

//Start client
SystemController controller = new SystemController("SystemController");
await controller.StartController();
Console.WriteLine("Press enter to stop");
Console.ReadLine();
await controller.StopController();

sealed class SystemController : Client
{
    private Queue<String> recentMeasurementData = new Queue<string>(100);
    private Queue<String> recentAnalyzerData = new Queue<string>(100);

    private HttpListener listener;
    private Thread serverThread;
    
    
    public SystemController(string clientId) : base(clientId)
    {
        this.serverThread = new Thread(serverLoop);
        serverThread.Name = "HTTP Server";
        listener = new HttpListener();
        listener.Prefixes.Add($"http://*:{1337}/");
    }

    private void serverLoop()
    {
        listener.Start();
        while (true)
        {
            try
            {
                HttpListenerContext context = listener.GetContext();
                if (context.Request.HttpMethod != "GET") return;
                switch (context.Request.RawUrl)
                {
                    case "/Analyzer":
                        string jsonAnalyzer = JsonSerializer.Serialize(recentAnalyzerData);
                        using(var streamWriter = new StreamWriter(context.Response.OutputStream))
                            streamWriter.Write(jsonAnalyzer);
                        break;
                    case "/Device":
                        string jsonMeasurement = JsonSerializer.Serialize(recentMeasurementData);
                        using(var streamWriter = new StreamWriter(context.Response.OutputStream))
                            streamWriter.Write(jsonMeasurement);
                        break;
                    default:
                        Console.WriteLine("Unrecognized");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }


    public Task StartController()
    {
        this.serverThread.Start();
        return this.Subscribe("Device", "Analyzer");
    }

    public Task StopController()
    {
        this.listener.Stop();
        this.serverThread.Join(500);
        this.Dispose(true);
        return Task.CompletedTask;
    }

    protected override Task handleMessage(MqttApplicationMessageReceivedEventArgs e)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        switch (e.ApplicationMessage.Topic)
        {
            case "Device":
                recentMeasurementData.Enqueue(encoding.GetString(e.ApplicationMessage.PayloadSegment));
                break;
            case "Analyzer":
                recentAnalyzerData.Enqueue(encoding.GetString(e.ApplicationMessage.PayloadSegment));
                break;
            default:
                break;
        }

        return Task.CompletedTask;
    }
}