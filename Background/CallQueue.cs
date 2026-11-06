using System.Threading.Channels;

namespace CallManagement.Background;

public class CallQueue
{
    private readonly Channel<int> _channel = Channel.CreateUnbounded<int>();

    public ValueTask EnqueueAsync(int callId, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(callId, cancellationToken);

    public IAsyncEnumerable<int> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
