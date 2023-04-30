namespace MQTTLib;
using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Client;
using MQTTnet.Protocol;
public abstract class Client: IDisposable
{
    private bool disposed = false;
    private static readonly MqttFactory factory = new MqttFactory(new ConsoleLogger());
    protected readonly string clientId;
    private readonly IMqttClient client = factory.CreateMqttClient();
    private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
    
    
    
    public Client(string clientId, string ip = "localhost", int port = 1883){
        this.clientId = clientId;

        var clientOptions = new MqttClientOptionsBuilder()
            .WithClientId(clientId)
        .WithTcpServer(ip, port) // Port is optional
        .Build();
        client.ApplicationMessageReceivedAsync += handleMessage;
        var response = client.ConnectAsync(clientOptions, tokenSource.Token).Result;
        Console.WriteLine($"Client connected: {clientId}");
        Console.WriteLine(response);
    }

    public void Dispose(){
        this.Dispose(true);
        GC.SuppressFinalize(this);        
    }

    protected void Dispose(bool force){
        if(disposed) return;
        if(force){
            tokenSource.Cancel();
            client?.DisconnectAsync().Wait();
            client?.Dispose();
        }
        disposed = true;
    }

    public async Task Subscribe(string topic){
        MqttClientSubscribeOptions subscriberOptions = new MqttClientSubscribeOptionsBuilder()
        .WithTopicFilter(filter => { filter.WithTopic(topic);}).Build();
        var response = await client.SubscribeAsync(subscriberOptions, tokenSource.Token);
        Console.WriteLine($"Subscribed to topic: {topic}");
        Console.WriteLine(response);
    }

    public async Task Subscribe(string topic1, string topic2)
    {
        MqttClientSubscribeOptions subscriberOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(filter => { filter.WithTopic(topic1);})
                .WithTopicFilter(filter => { filter.WithTopic(topic2);}).Build();
        
                var response = await client.SubscribeAsync(subscriberOptions, tokenSource.Token);
                Console.WriteLine($"Subscribed to topic: {topic1} {topic2}");
                Console.WriteLine(response);
    }

    protected abstract Task handleMessage(MqttApplicationMessageReceivedEventArgs e);


    public async Task Publish(string topic, string payload){
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithRetainFlag()
            .Build();
        var response = await client.PublishAsync(message, tokenSource.Token);
        Console.WriteLine($"Published message: {payload}");
        Console.WriteLine(response);
    } 
}
