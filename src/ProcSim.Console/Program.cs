using ProcSim.Core;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Console;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Scheduler scheduler = new();

        // Example input
        for (int i = 1; i <= 3; i++)
        {
            scheduler.AddProcess(new Process(i, $"Process{i}", i * 2, 0, ProcessType.CpuBound));
        }

        // Run scheduling
        await scheduler.RunAsync();

        System.Console.WriteLine("All processes completed");
    }
}
