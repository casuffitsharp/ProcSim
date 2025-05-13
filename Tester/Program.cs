using ProcSim.Core.New;

Process cpuBoundProcess = new()
{
    Instructions =
    [
        new("ADD", [new("FETCH_ADD", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] + cpu.RegisterFile["R2"])]),
        new("SUB", [new("FETCH_SUB", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] - cpu.RegisterFile["R2"])]),
        new("MUL", [new("FETCH_MUL", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] * cpu.RegisterFile["R2"])]),
        new("DIV", [new("FETCH_DIV", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] / cpu.RegisterFile["R2"])]),
    ],
    Registers = new()
    {
        ["R1"] = 10,
        ["R2"] = 5,
    }
};
cpuBoundProcess.Instructions.AddRange(SyscallFactory.Create(SyscallType.Exit));

Process ioBoundProcess = new();
ioBoundProcess.Instructions = [];
ioBoundProcess.Instructions.AddRange(SyscallFactory.Create(SyscallType.IoRequest, deviceId: 0));
ioBoundProcess.Instructions.AddRange(SyscallFactory.Create(SyscallType.IoRequest, deviceId: 1));
ioBoundProcess.Instructions.AddRange(SyscallFactory.Create(SyscallType.IoRequest, deviceId: 2));
ioBoundProcess.Instructions.AddRange(SyscallFactory.Create(SyscallType.Exit));

Process mixedProcess = new()
{
    Instructions =
    [
        new("ADD", [new MicroOp("FETCH_ADD", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] + cpu.RegisterFile["R2"])]),
        new("MUL", [new MicroOp("FETCH_MUL", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] * cpu.RegisterFile["R2"])]),
    ],
    Registers = new()
    {
        ["R1"] = 10,
        ["R2"] = 5,
    }
};

mixedProcess.Instructions.AddRange(SyscallFactory.Create(SyscallType.IoRequest, deviceId: 0));
mixedProcess.Instructions.Add(new("SUB", [new MicroOp("FETCH_SUB", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] - cpu.RegisterFile["R2"])]));
mixedProcess.Instructions.AddRange(SyscallFactory.Create(SyscallType.IoRequest, deviceId: 1));
mixedProcess.Instructions.AddRange(SyscallFactory.Create(SyscallType.Exit));

Action tick = () => { };

PeriodicTimer timer = new(TimeSpan.FromSeconds(2));
_ = Task.Run(async () =>
{
    while (await timer.WaitForNextTickAsync())
    {
        tick();
    }
});

Kernel kernel = new();
kernel.Initialize(1, 5, 100, handler => tick += handler);

Thread.Sleep(2000);
kernel.CreateProcess(cpuBoundProcess);

Console.ReadKey();
