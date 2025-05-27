namespace ProcSim.Core.Old.Runtime;

public class GlobalCancellationTokenService
{
    private CancellationTokenSource _cts = new();

    public CancellationToken CurrentToken => _cts.Token;

    public Func<CancellationToken> TokenProvider => () => _cts.Token;

    public async Task ResetAsync()
    {
        await _cts.CancelAsync();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
    }
}