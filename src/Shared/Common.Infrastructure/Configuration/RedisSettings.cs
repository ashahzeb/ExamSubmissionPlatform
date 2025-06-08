namespace Common.Infrastructure.Configuration;

public class RedisSettings
{
    public const string SectionName = "Redis";
    
    public string ConnectionString { get; set; } = "localhost:6379";
    public int Database { get; set; } = 0;
    public bool AbortOnConnectFail { get; set; } = false;
    public int ConnectRetry { get; set; } = 3;
    public int ConnectTimeout { get; set; } = 30000;
    public string? Password { get; set; }
    public bool Ssl { get; set; } = false;
}