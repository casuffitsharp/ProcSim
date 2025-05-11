namespace ProcSim.Core.New;

/// <summary>
/// TimerDevice simula o Local APIC Timer, rodando em thread separada.
/// Gera IRQ periódico (vetor 32) sem polling na CPU (OSDev Wiki).
/// </summary>
public class TimerDevice
{
    private readonly InterruptController _intc;
    private readonly uint _vector;
    private readonly uint _quantum;

    private uint _counter;

    public TimerDevice(uint vector, uint quantum, InterruptController intc, Action<Action> subscribeToTick)
    {
        _vector = vector;
        _quantum = quantum;
        _intc = intc;
        subscribeToTick(Tick);
    }

    private void Tick()
    {
        _counter++;
        if (_counter >= _quantum)
        {
            _intc.Raise(_vector);
            _counter = 0;
        }
    }
}
