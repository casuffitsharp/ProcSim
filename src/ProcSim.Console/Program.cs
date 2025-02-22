using ProcSim.Core;
using ProcSim.Core.Entities;

namespace ProcSim.Console;

internal class Program
{
    private static void Main(string[] args)
    {
        Scheduler scheduler = new();

        // Example input
        for (int i = 1; i <= 3; i++)
        {
            scheduler.AddProcess(new Process
            {
                Id = i,
                Name = $"Process{i}",
                ExecutionTime = i * 2,
                RemainingTime = i * 2
            });
        }

        // Run scheduling
        scheduler.Run();

        System.Console.WriteLine("All processes completed");
    }
}
