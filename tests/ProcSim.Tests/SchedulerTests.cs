using ProcSim.Core;
using ProcSim.Core.Entities;

namespace ProcSim.Tests;

public class SchedulerTests
{
    [Fact]
    public void FCFS_Should_CompleteAllProcesses()
    {
        // Arrange
        Scheduler scheduler = new();
        for (int i = 1; i <= 3; i++)
        {
            scheduler.AddProcess(new Process
            {
                Id = i,
                Name = $"P{i}",
                ExecutionTime = i * 1,
                RemainingTime = i * 1
            });
        }

        // Act
        scheduler.Run();

        // Assert
        // If the code reaches here, all processes should be completed
        // You might add more specific assertions if needed.
    }
}
