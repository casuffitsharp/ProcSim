using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcSim.Assets;
using ProcSim.Core.Configuration;
using ProcSim.Core.Enums;
using ProcSim.Core.Factories;
using ProcSim.Core.IO;
using ProcSim.Core.IO.Devices;
using ProcSim.Core.Logging;
using ProcSim.Core.Runtime;
using ProcSim.Core.Scheduling;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace ProcSim.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly StructuredLogger _logger;
    private readonly TickManager _tickManager;
    private readonly IoManager _ioManager;
    private readonly CpuScheduler _cpuScheduler;
    private readonly GlobalCancellationTokenService _globalCtsService = new();
    private Kernel _kernel;

    public MainViewModel()
    {
        _logger = new StructuredLogger();

        _tickManager = new(_logger);
        CpuTime = Settings.Default.CpuTime;

        _ioManager = new(_logger);
        _cpuScheduler = new(_ioManager, _logger);

        VmSettingsVm = new VmSettingsViewModel(new VmConfigRepository(), _ioManager);
        VmSettingsVm.PropertyChanged += VmSettingsVm_PropertyChanged;

        _tickManager.RunStateChanged += () => IsRunning = !_tickManager.IsPaused;

        ProcessesSettingsVm = new ProcessesSettingsViewModel(new ProcessesConfigRepository());
        ProcessesSettingsVm.PropertyChanged += ProcessesSettingsVm_PropertyChanged;
        Processes.CollectionChanged += Processes_CollectionChanged;
        ProcessesSettingsVm.ProcessStateChanged += OnProcessStateChanged;

        TaskManagerVm = new(_logger);

        RunPauseSchedulingCommand = new AsyncRelayCommand(RunPauseSchedulingAsync, CanRunPauseScheduling, AsyncRelayCommandOptions.AllowConcurrentExecutions);
        ResetSchedulingCommand = new AsyncRelayCommand(ResetSchedulingAsync, CanResetScheduling);

        ReadyProcessesView = new CollectionViewSource { Source = Processes }.View;
        ReadyProcessesView = CollectionViewSource.GetDefaultView(Processes);
        ReadyProcessesView.Filter = o => o is ProcessViewModel e && e.State == ProcessState.Ready;
        ((ICollectionViewLiveShaping)ReadyProcessesView).LiveFilteringProperties.Add(nameof(ProcessViewModel.State));
        ((ICollectionViewLiveShaping)ReadyProcessesView).IsLiveFiltering = true;

        RunningProcessesView = new CollectionViewSource { Source = Processes }.View;
        RunningProcessesView.Filter = o => o is ProcessViewModel e && e.State == ProcessState.Running;
        ((ICollectionViewLiveShaping)RunningProcessesView).LiveFilteringProperties.Add(nameof(ProcessViewModel.State));
        ((ICollectionViewLiveShaping)RunningProcessesView).IsLiveFiltering = true;

        BlockedProcessesView = new CollectionViewSource { Source = Processes }.View;
        BlockedProcessesView.Filter = o => o is ProcessViewModel e && e.State == ProcessState.Blocked;
        ((ICollectionViewLiveShaping)BlockedProcessesView).LiveFilteringProperties.Add(nameof(ProcessViewModel.State));
        ((ICollectionViewLiveShaping)BlockedProcessesView).IsLiveFiltering = true;

        CompletedProcessesView = new CollectionViewSource { Source = Processes }.View;
        CompletedProcessesView.Filter = o => o is ProcessViewModel e && e.State == ProcessState.Completed;
        ((ICollectionViewLiveShaping)CompletedProcessesView).LiveFilteringProperties.Add(nameof(ProcessViewModel.State));
        ((ICollectionViewLiveShaping)CompletedProcessesView).IsLiveFiltering = true;
    }

    public ObservableCollection<ProcessViewModel> Processes => ProcessesSettingsVm.Processes;
    public ICollectionView ReadyProcessesView { get; }
    public ICollectionView RunningProcessesView { get; }
    public ICollectionView BlockedProcessesView { get; }
    public ICollectionView CompletedProcessesView { get; }

    public ProcessesSettingsViewModel ProcessesSettingsVm { get; }
    public VmSettingsViewModel VmSettingsVm { get; }
    public TaskManagerViewModel TaskManagerVm { get; }

    public IAsyncRelayCommand RunPauseSchedulingCommand { get; }
    public IAsyncRelayCommand ResetSchedulingCommand { get; }

    [ObservableProperty]
    public partial int TotalTimeUnits { get; set; }

    public string StatusBarMessage => $"VM: {Path.GetFileNameWithoutExtension(VmSettingsVm.CurrentFile ?? "New")} | Processos: {Path.GetFileNameWithoutExtension(ProcessesSettingsVm.CurrentFile ?? "New")}";

    public bool IsRunning
    {
        get => field;
        private set
        {
            if (SetProperty(ref field, value))
            {
                VmSettingsVm.CanChangeAlgorithm = !value;
                RunPauseSchedulingCommand.NotifyCanExecuteChanged();
                ResetSchedulingCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public ushort CpuTime
    {
        get => field;
        set
        {
            if (SetProperty(ref field, value))
            {
                _tickManager.CpuTime = value;
                OnPropertyChanged(nameof(CpuTimeTs));
            }
        }
    }

    public TimeSpan CpuTimeTs => TimeSpan.FromMilliseconds(CpuTime);

    public void PauseScheduling()
    {
        _tickManager.Pause();
    }

    public async Task ResetSchedulingAsync()
    {
        await _globalCtsService.ResetAsync();
        _kernel = null;

        foreach (ProcessViewModel process in Processes)
            process.Model.Reset();

        RunPauseSchedulingCommand.NotifyCanExecuteChanged();
    }

    private bool CanRunPauseScheduling()
    {
        return IsRunning || Processes.Any(p => p.State != ProcessState.Completed);
    }

    private bool CanResetScheduling()
    {
        return _tickManager.IsPaused && _kernel is not null;
    }

    private async Task RunPauseSchedulingAsync()
    {
        if (IsRunning)
        {
            PauseScheduling();
        }
        else
        {
            await RunSchedulingAsync();
        }
    }

    private async Task RunSchedulingAsync()
    {
        if (_kernel is null)
            LoadKernel();

        try
        {
            await _kernel.RunAsync(_globalCtsService.TokenProvider);
        }
        catch (OperationCanceledException)
        {
            //_logger.Log("Simulação pausada pelo usuário.");
        }
        catch (Exception)
        {
            //_logger.Log($"Erro inesperado: {ex.Message}");
        }
    }

    private void LoadKernel()
    {
        _kernel = new(_tickManager, _cpuScheduler, VmSettingsVm.SelectedAlgorithmInstance, VmSettingsVm.CpuCores);
        foreach (ProcessViewModel process in Processes)
            _kernel.RegisterProcess(process.Model);

        foreach (DeviceViewModel vm in VmSettingsVm.AvailableDevices)
        {
            _ioManager.RemoveDevice(vm.Name);
            if (vm.IsEnabled)
            {
                IIoDevice ioDevice = IoDeviceFactory.CreateDevice(vm.DeviceType, vm.Name, vm.Channels, _tickManager.DelayFunc, _globalCtsService.TokenProvider);
                _ioManager.AddDevice(ioDevice);
            }
        }
    }

    private void OnProcessStateChanged(ProcessViewModel model)
    {
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => OnProcessStateChanged(model));
            return;
        }
    }

    private void Processes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (ProcessViewModel item in e.NewItems?.OfType<ProcessViewModel>())
            OnProcessStateChanged(item);

        RunPauseSchedulingCommand.NotifyCanExecuteChanged();
    }

    private void VmSettingsVm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VmSettingsVm.CurrentFile))
            OnPropertyChanged(nameof(StatusBarMessage));
    }

    private void ProcessesSettingsVm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProcessesSettingsVm.CurrentFile))
            OnPropertyChanged(nameof(StatusBarMessage));
    }

    public void Dispose()
    {
        _logger?.Dispose();
    }
}
