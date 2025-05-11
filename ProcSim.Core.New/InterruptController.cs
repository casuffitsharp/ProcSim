using System.Collections.Concurrent;

namespace ProcSim.Core.New;

/// <summary>
/// Controlador de interrupções simulando PIC/I/O APIC.
/// Em sistemas reais, o dispositivo de interrupção gera vetores que apontam para a IDT (Intel SDM Vol.3).
/// </summary>
public class InterruptController
{
    private readonly ConcurrentQueue<uint> _pending = new();

    public void Raise(uint vector)
    {
        _pending.Enqueue(vector);
    }

    public uint? FetchReady()
    {
        return _pending.TryDequeue(out uint vec) ? vec : null;
    }
}
