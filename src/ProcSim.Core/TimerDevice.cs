using ProcSim.Core.Interruptions;

namespace ProcSim.Core;

public class TimerDevice
{
    private readonly uint _vector;
    private readonly uint _quantum;
    private readonly uint _coreId;
    private readonly InterruptController _intc;
    private uint _count;

    public TimerDevice(uint coreId, uint vector, uint quantum, InterruptController intc, Action<Action> subscribeToTick)
    {
        _coreId = coreId;
        _vector = vector;
        _quantum = quantum;
        _intc = intc;

        subscribeToTick(Tick);
    }

    private void Tick()
    {
        _count++;
        if (_count >= _quantum)
        {
            _intc.RaiseLocal(_vector, _coreId);
            _count = 0;
        }
    }
}
