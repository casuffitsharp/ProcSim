using ProcSim.Core.Logging;
using ProcSim.Core.Runtime;

namespace ProcSim.Core.Tests;

public class TickManagerTests
{
    [Fact]
    public async Task TickManager_FiresTickOccurred_WhenResumed()
    {
        StructuredLogger logger = new();
        TickManager tickManager = new(logger)
        {
            TickInterval = 10
        };
        bool tickFired = false;
        tickManager.TickOccurred += () => tickFired = true;
        using CancellationTokenSource cts = new(500);

        tickManager.Resume();
        await tickManager.WaitNextTickAsync(cts.Token);

        Assert.True(tickFired);
    }

    [Fact]
    public async Task TickManager_DoesNotFireTick_WhenPaused()
    {
        StructuredLogger logger = new();
        TickManager tickManager = new(logger);
        bool tickFired = false;
        tickManager.TickOccurred += () => tickFired = true;
        tickManager.Pause();
        using CancellationTokenSource cts = new(200);

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await tickManager.WaitNextTickAsync(cts.Token);
        });
        Assert.False(tickFired);
    }
}
