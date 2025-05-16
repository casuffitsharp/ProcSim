using ProcSim.Core.New;

Process cpuBoundProcess = new()
{
    Instructions =
    [
        new("ADD", [new("FETCH_ADD", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] + cpu.RegisterFile["R2"])]),
        new("SUB", [new("FETCH_SUB", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] - cpu.RegisterFile["R2"])]),
        new("MUL", [new("FETCH_MUL", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] * cpu.RegisterFile["R2"])]),
        new("DIV", [new("FETCH_DIV", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] / cpu.RegisterFile["R2"])]),
        new("JMP", [new("JUMP", cpu => cpu.PC = 0)]),
        SyscallFactory.Create(SyscallType.Exit),
    ],
    Registers = new()
    {
        ["R1"] = 10,
        ["R2"] = 5,
    }
};
cpuBoundProcess.Instructions.AddRange(SyscallFactory.Create(SyscallType.Exit));

Process ioBoundProcess = new()
{
    Instructions =
    [
        SyscallFactory.Create(SyscallType.IoRequest, deviceId: 0, operationUnits: 20),
        SyscallFactory.Create(SyscallType.IoRequest, deviceId: 1, operationUnits: 20),
        SyscallFactory.Create(SyscallType.IoRequest, deviceId: 2, operationUnits: 20),
        SyscallFactory.Create(SyscallType.Exit),
    ]
};

Process mixedProcess = new()
{
    Instructions =
    [
        new("ADD", [new MicroOp("FETCH_ADD", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] + cpu.RegisterFile["R2"])]),
        new("MUL", [new MicroOp("FETCH_MUL", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] * cpu.RegisterFile["R2"])]),

        SyscallFactory.Create(SyscallType.IoRequest, deviceId: 0, operationUnits: 20),

        new("SUB", [new MicroOp("FETCH_SUB", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] - cpu.RegisterFile["R2"])]),

        SyscallFactory.Create(SyscallType.IoRequest, deviceId: 1, operationUnits: 20),
        SyscallFactory.Create(SyscallType.Exit),
    ],
    Registers = new()
    {
        ["R1"] = 10,
        ["R2"] = 5,
    }
};

Action tick = () => { };

PeriodicTimer timer = new(TimeSpan.FromMilliseconds(500));
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
kernel.CreateProcess(mixedProcess);

Console.ReadKey();
