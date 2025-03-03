using ProcSim.Core.Logging;
using ProcSim.Core.Runtime;

namespace ProcSim.Core.Tests;

public class TickManagerTests
{
    [Fact]
    public async Task TickManager_FiresTickOccurred_WhenResumed()
    {
        // Arrange
        var logger = new StructuredLogger();
        var tickManager = new TickManager(logger);
        tickManager.CpuTime = 10;
        bool tickFired = false;
        tickManager.TickOccurred += () => tickFired = true;
        using var cts = new CancellationTokenSource(500);

        // Act
        tickManager.Resume();
        await tickManager.WaitNextTickAsync(cts.Token);

        // Assert
        Assert.True(tickFired);
    }

    [Fact]
    public async Task TickManager_DoesNotFireTick_WhenPaused()
    {
        // Arrange
        var logger = new StructuredLogger();
        var tickManager = new TickManager(logger);
        bool tickFired = false;
        tickManager.TickOccurred += () => tickFired = true;
        tickManager.Pause();

        using var cts = new CancellationTokenSource(200);

        // Act & Assert: espera um tick e espera cancelamento (não ocorrer tick).
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await tickManager.WaitNextTickAsync(cts.Token);
        });
        Assert.False(tickFired);
    }
}
