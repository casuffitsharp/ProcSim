using ProcSim.Converters;
using ProcSim.Core;
using ProcSim.Core.Configuration;
using ProcSim.Core.IO;
using ProcSim.Core.Process;
using ProcSim.Core.Scheduler;
using ProcSim.Core.Syscall;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace ProcSim;

public class SimulationController
{
    private static readonly CpuOperationType[] CpuOperationTypes = [.. Enum.GetValues<CpuOperationType>().Where(c => c != CpuOperationType.Random)];

    private readonly PeriodicTimer _timer;
    private readonly ConcurrentDictionary<uint, ProcessConfigModel> _processes = [];
    private readonly ConcurrentDictionary<IoDeviceType, uint> _deviceIds = [];

    private Kernel _kernel;
    private Action _clockTick = () => { };

    public SimulationController()
    {
        _timer = new PeriodicTimer(Timeout.InfiniteTimeSpan);
        _ = Task.Run(TimerWorker);
    }

    public uint Clock
    {
        get;
        set
        {
            if (value != field)
            {
                field = value;
                if (Status != SimulationStatus.Paused)
                    _timer.Period = TimeSpan.FromMilliseconds(field);
            }
        }
    } = 100;

    public SimulationStatus Status { get; private set; } = SimulationStatus.Stopped;

    public void Resume()
    {
        if (Status == SimulationStatus.Paused)
        {
            Status = SimulationStatus.Running;
            _timer.Period = TimeSpan.FromMilliseconds(Clock);
        }
    }

    public void Pause()
    {
        if (Status == SimulationStatus.Running)
        {
            Status = SimulationStatus.Paused;
            _timer.Period = Timeout.InfiniteTimeSpan;
        }
    }

    public void Stop()
    {
        if (Status is SimulationStatus.Running or SimulationStatus.Paused)
        {
            Status = SimulationStatus.Stopped;
            _timer.Period = Timeout.InfiniteTimeSpan;
            _kernel?.Dispose();
            _kernel = null;
        }
    }

    public void Start(uint cores, uint quantum, SchedulerType schedulerType, List<ProcessConfigModel> processes, List<IoDeviceConfigModel> devices)
    {
        _kernel ??= new Kernel();
        _kernel.ProcessTerminated += OnProcessTerminated;
        _kernel.Initialize(cores, quantum, schedulerType, handler => _clockTick += handler);
        foreach (IoDeviceConfigModel device in devices)
            RegisterDevice(device);

        foreach (ProcessConfigModel process in processes)
            RegisterProcess(process);

        Status = SimulationStatus.Running;
    }

    public void RegisterProcess(ProcessConfigModel processConfig)
    {
        if (_kernel is null)
            throw new InvalidOperationException("Kernel not initialized.");

        if (processConfig.LoopConfig is RandomLoopConfig randomLoopConfig)
        {
            processConfig.LoopConfig = new FiniteLoopConfig
            {
                Iterations = (uint)Random.Shared.Next((int)randomLoopConfig.MinIterations, (int)(randomLoopConfig.MaxIterations + 1))
            };
        }

        ProcessDto program = new();
        foreach (IOperationConfigModel operation in processConfig.Operations)
        {
            Instruction instruction = operation switch
            {
                CpuOperationConfigModel cpuOperation => BuildCpuInstruction(cpuOperation),
                IoOperationConfigModel ioOperation => BuildIoInstruction(ioOperation),
                _ => throw new NotImplementedException()
            };
            program.Instructions.Add(instruction);
        }

        uint id = _kernel.CreateProcess(null, processConfig.Priority);
        _processes.TryAdd(id, processConfig);
    }

    private void OnProcessTerminated(uint pid)
    {
        _processes.TryGetValue(pid, out ProcessConfigModel processConfig);
        switch (processConfig.LoopConfig)
        {
            case InfiniteLoopConfig:
                RegisterProcess(processConfig);
                break;
            case FiniteLoopConfig finiteLoopConfig:
            {
                int runCount = _processes.Values.Count(p => p == processConfig);
                if (runCount < finiteLoopConfig.Iterations)
                    RegisterProcess(processConfig);
            }
            break;
        }
    }

    private void RegisterDevice(IoDeviceConfigModel deviceConfig)
    {
        uint deviceId = _kernel.RegisterDevice(deviceConfig.Name, deviceConfig.BaseLatency, deviceConfig.Channels);
        _deviceIds[deviceConfig.Type] = deviceId;
    }

    private Instruction BuildIoInstruction(IoOperationConfigModel ioOperation)
    {
        uint deviceId = _deviceIds[ioOperation.DeviceType];

        if (ioOperation.IsRandom)
            ioOperation.Duration = (uint)Random.Shared.Next((int)ioOperation.MinDuration, (int)ioOperation.MaxDuration + 1);

        return SyscallFactory.Create(SyscallType.IoRequest, deviceId, operationUnits: ioOperation.Duration);
    }

    private Instruction BuildCpuInstruction(CpuOperationConfigModel cpuOperation)
    {
        CpuOperationType cpuOperationType = cpuOperation.Type;
        if (cpuOperationType == CpuOperationType.Random)
            cpuOperationType = Random.Shared.GetItems(CpuOperationTypes, 1)[0];

        string mnemonic = EnumDescriptionConverter.GetEnumDescription(cpuOperationType);
        return cpuOperationType switch
        {
            CpuOperationType.Add => new(mnemonic,
            [
                new("SET_R1", cpu => cpu.RegisterFile["R1"] = cpuOperation.R1),
                new("SET_R2", cpu => cpu.RegisterFile["R2"] = cpuOperation.R2),
                new("FETCH_ADD", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] + cpu.RegisterFile["R2"])
            ]),
            CpuOperationType.Subtract => new(mnemonic,
            [
                new("SET_R1", cpu => cpu.RegisterFile["R1"] = cpuOperation.R1),
                new("SET_R2", cpu => cpu.RegisterFile["R2"] = cpuOperation.R2),
                new("FETCH_SUB", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] - cpu.RegisterFile["R2"])
            ]),
            CpuOperationType.Multiply => new(mnemonic,
            [
                new("SET_R1", cpu => cpu.RegisterFile["R1"] = cpuOperation.R1),
                new("SET_R2", cpu => cpu.RegisterFile["R2"] = cpuOperation.R2),
                new("FETCH_MUL", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] * cpu.RegisterFile["R2"])
            ]),
            CpuOperationType.Divide => new(mnemonic,
            [
                new("SET_R1", cpu => cpu.RegisterFile["R1"] = cpuOperation.R1),
                new("SET_R2", cpu => cpu.RegisterFile["R2"] = cpuOperation.R2),
                new("FETCH_DIV", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] / cpu.RegisterFile["R2"])
            ]),
            _ => throw new NotImplementedException(),
        };
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

public enum SimulationStatus
{
    Stopped,
    Running,
    Paused
}

