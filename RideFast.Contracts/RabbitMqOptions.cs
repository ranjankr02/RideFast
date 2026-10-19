namespace RideFast.Contracts;

public class RabbitMqOptions
{
    public const string Section = "RabbitMq";

    /// <summary>AMQP URI, e.g. amqp://guest:guest@localhost:5672 or a CloudAMQP amqps:// URL.</summary>
    public string Uri { get; set; } = string.Empty;
}
