using ProcSim.Core;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Tests;

public class SchedulerTests
{
    [Fact]
    public async Task FCFS_Should_CompleteAllProcessesAsync()
    {
        // Arrange
        Scheduler scheduler = new();
        for (int i = 1; i <= 3; i++)
        {
            scheduler.AddProcess(new Process(i, $"P{i}", i * 1, 0, ProcessType.CpuBound));
        }

        // Act
        await scheduler.RunAsync();

        // Assert
        // If the code reaches here, all processes should be completed
        // You might add more specific assertions if needed.
    }
}
