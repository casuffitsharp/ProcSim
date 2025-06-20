using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Assets;
using ProcSim.Core.Configuration;
using ProcSim.Core.IO;
using ProcSim.Core.Monitoring;
using ProcSim.Core.Simulation;
using ProcSim.New.ViewModels;
using ProcSim.Views;

namespace ProcSim.ViewModels;

public partial class SimulationControlViewModel : ObservableObject
{
    private readonly SimulationController _controller;
    private readonly VmConfigViewModel _vmConfig;
    private readonly ProcessesConfigViewModel _processesConfig;

    public SimulationControlViewModel(SimulationController controller, MonitoringService monitoringService, VmConfigViewModel vmConfig, ProcessesConfigViewModel processesConfig)
    {
        _controller = controller;
        _vmConfig = vmConfig;
        _processesConfig = processesConfig;
        _controller.SimulationStatusChanged += OnSimulationStatusChanged;

        RunPauseCommand = new AsyncRelayCommand(RunPauseAsync, CanRunPause, AsyncRelayCommandOptions.None);
        ResetCommand = new RelayCommand(Reset, CanReset);

        Clock = Settings.Default.Clock;
        MinClock = 10;
#if DEBUG
        MinClock = 200;
#endif
        MaxClock = 1000;

        Clock = Math.Clamp(Clock, MinClock, MaxClock);
    }

    public IAsyncRelayCommand RunPauseCommand { get; }
    public IRelayCommand ResetCommand { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunPauseCommand), nameof(ResetCommand))]
    public partial bool IsRunning { get; private set; }

    [ObservableProperty]
    public partial ushort MinClock { get; private set; }
    [ObservableProperty]
    public partial ushort MaxClock { get; private set; }

    public ushort Clock
    {
        get => _controller.Clock;
        set
        {
            if (value != _controller.Clock)
            {
                _controller.Clock = value;
                OnPropertyChanged(nameof(Clock));
            }
        }
    }

    private bool CanRunPause()
    {
        return _controller.Status >= SimulationStatus.Created;
    }

    private bool CanReset()
    {
        return _controller.Status == SimulationStatus.Paused;
    }

    private async Task RunPauseAsync()
    {
        switch (_controller.Status)
        {
            case SimulationStatus.Created:
            {
                await StartSimulationAsync();
                break;
            }
            case SimulationStatus.Running:
                _controller.Pause();
                break;
            case SimulationStatus.Paused:
                _controller.Resume();
                break;
        }
    }

    private async Task StartSimulationAsync()
    {
        if (!await ValidateAsync())
            return;

        _controller.Initialize(_vmConfig.MapToModel());
        foreach (ProcessConfigModel process in _processesConfig.MapToModel(p => p.IsSelectedForSimulation))
            _controller.RegisterProcess(process);
    }

    private async Task<bool> ValidateAsync()
    {
        if (!await _vmConfig.ValidateAsync())
            return false;

        List<IoDeviceType> enabledDevices = _vmConfig.IoDevices.Where(d => d.IsEnabled).Select(d => d.Type).ToList();
        List<string> errors = [];

        foreach (ProcessConfigViewModel process in _processesConfig.Processes)
        {
            if (!process.Validate(out IEnumerable<string> processErrors))
                errors.AddRange(processErrors.Select(e => $"Processo '{process.Name}': {e}"));

            if (process.Operations.Any(o => o.Type == OperationType.Io && !enabledDevices.Contains(o.IoOperationConfig.DeviceType)))
                errors.Add($"Processo '{process.Name}' utiliza dispositivo desabilitado.");
        }

        if (errors.Count > 0)
        {
            await TextDialog.ShowValidationErrorsAsync("Erros de validação", errors);
            return false;
        }

        return true;
    }

    private void Reset()
    {
        _controller.Reset();
        RunPauseCommand.NotifyCanExecuteChanged();
    }

    private void OnSimulationStatusChanged()
    {
        IsRunning = _controller.Status == SimulationStatus.Running;
        _processesConfig.IsSimulationRunning = _controller.Status > SimulationStatus.Created;
        _vmConfig.CanChangeConfigs = _controller.Status is SimulationStatus.Created;
    }
}