using System.Timers;
using MQTTLib;
using MQTTnet.Client;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, Device!");


//Start measurement
Device device = new Device("Device");
await device.StartMeasurement();
Console.WriteLine("Press enter to stop");
Console.ReadLine();
await device.StopMeasurement();


sealed public class Device: Client
{
    private int value = 0;
    private System.Timers.Timer timer;

    public Device(String clientId) : base(clientId)
    {
        timer = new System.Timers.Timer(2000);
        timer.Elapsed += timerCallback;
    }

    protected new void Dispose(bool force)
    {
        timer.Elapsed -= timerCallback;
        timer.Dispose();
        base.Dispose(force);
    }

    protected override Task handleMessage(MqttApplicationMessageReceivedEventArgs e)
    {
        return Task.CompletedTask;
    }

    public async Task StartMeasurement(){
        //Send the start command
        await Publish(base.clientId, "start");
        timer.Start();
    }

    public async Task StopMeasurement()
    {
        this.Dispose(true);
    }
    
    private void timerCallback(Object? sender, ElapsedEventArgs e)
    {
        Publish(base.clientId, value.ToString()).Wait();
        value++;
    }
}