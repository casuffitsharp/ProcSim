using Microsoft.Extensions.DependencyInjection;
using ProcSim.Core.Configuration;
using ProcSim.Core.Models;
using ProcSim.Core.Simulation;
using ProcSim.ViewModels;
using ProcSim.Views;
using System.Windows;

namespace ProcSim;

public partial class App : Application
{
    private IServiceProvider _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ServiceCollection sc = new();
        sc.AddSingleton<ISimulationController, SimulationController>();
        sc.AddSingleton<IRepositoryBase<VmConfig>, VmConfigRepository>();
        sc.AddSingleton<IRepositoryBase<List<Process>>, ProcessesConfigRepository>();
        sc.AddSingleton<VmSettingsViewModel>();
        sc.AddSingleton<ProcessesSettingsViewModel>();
        sc.AddSingleton<TaskManagerViewModel>();
        sc.AddSingleton<MainViewModel>();
        sc.AddTransient<MainView>();

        _services = sc.BuildServiceProvider();

        MainView wnd = _services.GetRequiredService<MainView>();
        wnd.Show();
    }
}
