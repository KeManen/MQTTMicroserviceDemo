// See https://aka.ms/new-console-template for more information

using System.Text;
using MQTTLib;
using MQTTnet.Client;

Console.WriteLine("Hello, Analyzer!");

Analyzer analyzer = new Analyzer("Analyzer");
await analyzer.StartMeasurement();
Console.WriteLine("Press enter to stop");
Console.ReadLine();
await analyzer.StopMeasurement();

sealed class Analyzer : Client
{
    private int errorCount = 0;

    public Analyzer(string clientId) : base(clientId)
    {
    }

    public async Task StartMeasurement()
    {
        this.Subscribe("Device");
    }



    public async Task StopMeasurement()
    {
        this.Dispose(true);
    }

    protected override Task handleMessage(MqttApplicationMessageReceivedEventArgs e)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        Console.WriteLine("Got Message");
        Console.WriteLine(encoding.GetString(e.ApplicationMessage.PayloadSegment));
        if (e.ProcessingFailed) errorCount++;
        return Publish(clientId, errorCount.ToString());
    }
}