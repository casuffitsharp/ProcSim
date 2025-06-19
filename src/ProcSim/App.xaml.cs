using Microsoft.Extensions.DependencyInjection;
using ProcSim.Core.Configuration;
using ProcSim.Core.Monitoring;
using ProcSim.Core.Simulation;
using ProcSim.New.ViewModels;
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
        sc.AddSingleton<IRepositoryBase<VmConfigModel>, VmConfigRepository>();
        sc.AddSingleton<IRepositoryBase<List<ProcessConfigModel>>, ProcessesConfigRepository>();
        sc.AddSingleton<MonitoringService>();
        sc.AddSingleton<SimulationController>();
        sc.AddSingleton<SimulationControlViewModel>();
        sc.AddSingleton<VmConfigViewModel>();
        sc.AddSingleton<ProcessesConfigViewModel>();
        sc.AddSingleton<TaskManagerDetailsViewModel>();
        sc.AddSingleton<CpuMonitoringViewModel>();
        sc.AddSingleton<DevicesMonitoringViewModel>();
        sc.AddSingleton<ProcessAnalysisViewModel>();
        sc.AddSingleton<TaskManagerViewModel>();
        sc.AddSingleton<MainViewModel>();
        sc.AddTransient<MainView>();

        _services = sc.BuildServiceProvider();

        MainView wnd = _services.GetRequiredService<MainView>();
        wnd.Show();
    }
}
