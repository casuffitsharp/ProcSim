using ProcSim.Core.Configuration;
using ProcSim.Core.IO;
using ProcSim.Core.Monitoring;
using ProcSim.Core.Process;
using ProcSim.Core.Process.Factories;
using ProcSim.Core.Syscall;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProcSim.Core.Simulation;

public class SimulationController
{
    private static readonly CpuOperationType[] CpuOperationTypes = [.. Enum.GetValues<CpuOperationType>().Where(c => c > CpuOperationType.Random)];

    private readonly ConcurrentDictionary<int, ProcessConfigModel> _processes = [];
    private readonly ConcurrentDictionary<IoDeviceType, uint> _deviceIds = [];
    private readonly MonitoringService _monitoringService;

    private Kernel _kernel;
    private Action _clockTick = () => { };
    private readonly PeriodicTimer _timer;

    public SimulationController(MonitoringService monitoringService)
    {
        Status = SimulationStatus.Created;
        _timer = new PeriodicTimer(Timeout.InfiniteTimeSpan);
        _ = Task.Run(TimerWorker);

        Debug.WriteLine("SimulationController initialized.");
        _monitoringService = monitoringService;
    }

    public event Action SimulationStatusChanged = () => { };

    public ushort Clock
    {
        get;
        set
        {
            if (value != field)
            {
                field = value;
                Debug.WriteLine($"Clock set to {field} ms.");

                if (Status == SimulationStatus.Running)
                {
                    _timer.Period = TimeSpan.FromMilliseconds(field);
                    Debug.WriteLine($"Timer period updated to {field} ms.");
                }
            }
        }
    } = 100;

    public SimulationStatus Status
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                SimulationStatusChanged?.Invoke();
                Debug.WriteLine($"Simulation status changed to: {field}");
            }
        }
    }

    public void Resume()
    {
        if (Status is SimulationStatus.Paused or SimulationStatus.Created)
        {
            Status = SimulationStatus.Running;
            _timer.Period = TimeSpan.FromMilliseconds(Clock);
            _monitoringService.Resume();

            Debug.WriteLine("Simulation resumed.");
        }
    }

    public void Pause()
    {
        if (Status == SimulationStatus.Running)
        {
            Status = SimulationStatus.Paused;
            _timer.Period = Timeout.InfiniteTimeSpan;
            _monitoringService.Pause();

            Debug.WriteLine("Simulation paused.");
        }
    }

    public void Reset()
    {
        if (Status > SimulationStatus.Created)
        {
            _timer.Period = Timeout.InfiniteTimeSpan;
            _clockTick = () => { };
            _monitoringService.SetKernel(null);
            _kernel?.Dispose();
            _kernel = null;

            _processes.Clear();
            _deviceIds.Clear();

            Status = SimulationStatus.Created;
            Debug.WriteLine("Simulation reset to Created state.");
        }
    }

    public void Initialize(VmConfigModel vmConfig)
    {
        if (Status != SimulationStatus.Created)
            throw new InvalidOperationException("Simulation must be in Created state to start.");

        _kernel = new Kernel();
        _kernel.Initialize(vmConfig.CpuCores, vmConfig.Quantum, vmConfig.SchedulerType, handler => _clockTick += () => Task.Run(handler));

        foreach (IoDeviceConfigModel device in vmConfig.Devices)
        {
            if (device.IsEnabled)
                RegisterDevice(device);
        }

        _monitoringService.SetKernel(_kernel);

        Resume();
        Debug.WriteLine("Simulation initialized and running.");
    }

    public ProcessDto BuildProgram(ProcessConfigModel processConfig, bool simulate = false)
    {
        if (processConfig.LoopConfig is RandomLoopConfig randomLoopConfig)
        {
            processConfig.LoopConfig = new FiniteLoopConfig
            {
                Iterations = (uint)Random.Shared.Next((int)randomLoopConfig.MinIterations, (int)(randomLoopConfig.MaxIterations + 1))
            };
        }

        ProcessDto program = new()
        {
            Name = processConfig.Name,
            Priority = processConfig.Priority
        };
        foreach (IOperationConfigModel operation in processConfig.Operations)
        {
            IEnumerable<Instruction> instructions = operation switch
            {
                CpuOperationConfigModel cpuOperation => BuildCpuInstructions(cpuOperation),
                IoOperationConfigModel ioOperation => BuildIoInstruction(ioOperation, simulate),
                _ => throw new NotImplementedException()
            };
            program.Instructions.AddRange(instructions);
        }

        if (processConfig.LoopConfig is InfiniteLoopConfig)
        {
            program.Instructions.Add(InstructionFactory.Jmp(0));
        }
        else if (processConfig.LoopConfig is FiniteLoopConfig finiteLoopConfig)
        {
            int limit = (int)finiteLoopConfig.Iterations;
            program.Instructions.Add(InstructionFactory.AddImmediate("R7", 1));
            program.Instructions.Add(InstructionFactory.MovImmediate("R6", limit));
            program.Instructions.Add(InstructionFactory.Blt("R7", "R6", 0));
        }

        program.Instructions.Add(InstructionFactory.Syscall(SyscallType.Exit));
        return program;
    }

    public void RegisterProcess(ProcessConfigModel processConfig)
    {
        if (_kernel is null)
            throw new InvalidOperationException("Kernel not initialized.");

        ProcessDto program = BuildProgram(processConfig);

        int id = _kernel.CreateProcess(program);
        _processes.TryAdd(id, processConfig);

        Debug.WriteLine($"Process '{processConfig.Name}' registered with ID {id}.");
    }

    private void RegisterDevice(IoDeviceConfigModel deviceConfig)
    {
        uint deviceId = _kernel.RegisterDevice(deviceConfig.Name, deviceConfig.BaseLatency, deviceConfig.Channels);
        _deviceIds[deviceConfig.Type] = deviceId;

        Debug.WriteLine($"Device '{deviceConfig.Name}' registered with ID {deviceId} and type {deviceConfig.Type}.");
    }

    private IEnumerable<Instruction> BuildIoInstruction(IoOperationConfigModel ioOperation, bool simulate)
    {
        uint deviceId = simulate ? 0 : _deviceIds[ioOperation.DeviceType];

        for (int i = 0; i < ioOperation.RepeatCount; i++)
        {
            if (ioOperation.IsRandom)
                ioOperation.Duration = (uint)Random.Shared.Next((int)ioOperation.MinDuration, (int)ioOperation.MaxDuration + 1);

            yield return InstructionFactory.Syscall(SyscallType.IoRequest, deviceId, ioOperation.Duration);
        }
    }

    private IEnumerable<Instruction> BuildCpuInstructions(CpuOperationConfigModel cpuOperation)
    {
        for (int i = 0; i < cpuOperation.RepeatCount; i++)
        {
            CpuOperationType cpuOperationType = cpuOperation.Type;
            if (cpuOperationType == CpuOperationType.Random)
                cpuOperationType = Random.Shared.GetItems(CpuOperationTypes, 1)[0];

            string mnemonic = cpuOperationType.ToString().ToLowerInvariant();

            int r1 = Random.Shared.Next(cpuOperation.Min, cpuOperation.Max + 1);
            int r2 = Random.Shared.Next(cpuOperation.Min, cpuOperation.Max + 1);
            if (r1 == 0) r1++;
            if (r2 == 0) r2++;
            if (r1 == r2) r2++;

            yield return cpuOperationType switch
            {
                CpuOperationType.Add => InstructionFactory.Add(r1, r2),
                CpuOperationType.Subtract => InstructionFactory.Sub(r1, r2),
                CpuOperationType.Multiply => InstructionFactory.Mul(r1, r2),
                CpuOperationType.Divide => InstructionFactory.Div(r1, r2),
                _ => throw new NotImplementedException(),
            };
        }
    }

    private async Task TimerWorker()
    {
        while (await _timer.WaitForNextTickAsync())
        {
            if (Status == SimulationStatus.Running)
                _clockTick();
        }
    }
}

