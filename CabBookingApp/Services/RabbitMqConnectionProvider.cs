using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RideFast.Contracts;

namespace CabBookingApp.Services;

/// <summary>
/// One long-lived RabbitMQ connection for the whole app, opened lazily on first use so the
/// site still starts when the broker is down. The notification topology is declared once per
/// connection.
/// </summary>
public sealed class RabbitMqConnectionProvider : IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConnectionProvider> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;

    public RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnectionProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IConnection> GetConnectionAsync(CancellationToken ct = default)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _lock.WaitAsync(ct);
        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            if (string.IsNullOrWhiteSpace(_options.Uri))
                throw new InvalidOperationException("RabbitMq:Uri is not configured.");

            if (_connection != null)
                await _connection.DisposeAsync();

            var factory = new ConnectionFactory
            {
                Uri = new Uri(_options.Uri),
                ClientProvidedName = "ridefast-web",
            };
            _connection = await factory.CreateConnectionAsync(ct);

            await using (var channel = await _connection.CreateChannelAsync(cancellationToken: ct))
                await NotificationTopology.DeclareAsync(channel, ct);

            _logger.LogInformation("Connected to RabbitMQ at {Host}", factory.HostName);
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
            await _connection.DisposeAsync();
        _lock.Dispose();
    }
}
