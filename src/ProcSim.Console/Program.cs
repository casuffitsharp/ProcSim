using ProcSim.Core;
using ProcSim.Core.Enums;
using ProcSim.Core.Models;

namespace ProcSim.Console;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Scheduler scheduler = new();
        List<Process> processes = [.. Enumerable.Range(1, 3).Select(i => new Process(i, $"P{i}", i * 1, 0, ProcessType.CpuBound))];

        // Run scheduling
        await scheduler.RunAsync(processes);

        System.Console.WriteLine("All processes completed");
    }
}
