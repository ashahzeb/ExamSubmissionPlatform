namespace Common.Infrastructure.Configuration;

public class RabbitMQSettings
{
    public const string SectionName = "RabbitMQ";
    
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public bool EnableSsl { get; set; } = false;
    public int ConnectionTimeout { get; set; } = 30000;
    public int NetworkRecoveryInterval { get; set; } = 10000;
    public int RequestedHeartbeat { get; set; } = 60;
}