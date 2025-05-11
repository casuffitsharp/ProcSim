using ProcSim.Core.New;

List<Instruction> cpuBoundProgram =
[
    new("ADD", [new("FETCH_ADD", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] + cpu.RegisterFile["R2"])]),
    new("SUB", [new("FETCH_SUB", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] - cpu.RegisterFile["R2"])]),
    new("MUL", [new("FETCH_MUL", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] * cpu.RegisterFile["R2"])]),
    new("DIV", [new("FETCH_DIV", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] / cpu.RegisterFile["R2"])]),
];

List<Instruction> ioBoundProgram =
[
    SyscallFactory.Create(SyscallType.IoRequest, deviceId: 0),
    SyscallFactory.Create(SyscallType.IoRequest, deviceId: 1),
    SyscallFactory.Create(SyscallType.IoRequest, deviceId: 2),
];


List<Instruction> mixedProgram =
[
    new("ADD", [new MicroOp("FETCH_ADD", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] + cpu.RegisterFile["R2"])]),
    new("MUL", [new MicroOp("FETCH_MUL", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] * cpu.RegisterFile["R2"])]),

    SyscallFactory.Create(SyscallType.IoRequest, deviceId: 0),

    new("SUB", [new MicroOp("FETCH_SUB", cpu => cpu.RegisterFile["R0"] = cpu.RegisterFile["R1"] - cpu.RegisterFile["R2"])]),

    SyscallFactory.Create(SyscallType.IoRequest, deviceId: 1),
    SyscallFactory.Create(SyscallType.Exit),
];

Action tick = () => { };

PeriodicTimer timer = new(TimeSpan.FromSeconds(5));
_ = Task.Run(async () =>
{
    while (await timer.WaitForNextTickAsync())
    {
        tick();
    }
});

Kernel kernel = new();
kernel.Initialize(1, 20, 100, handler => tick += handler);

Thread.Sleep(5000);
kernel.CreateProcess(cpuBoundProgram);

Console.ReadKey();
