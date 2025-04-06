using System.Text.Json.Serialization;

namespace ProcSim.Core.Models.Operations;

public abstract class Operation(int duration) : IOperation
{
    public event Action RemainingTimeChanged;

    public int Duration { get; } = duration;

    [JsonIgnore]
    public int RemainingTime
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                RemainingTimeChanged?.Invoke();
            }
        }
    } = duration;

    [JsonIgnore]
    public int Channel { get; set; }

    [JsonIgnore]
    public bool IsCompleted => RemainingTime <= 0;

    public void ExecuteTick()
    {
        if (RemainingTime > 0)
            RemainingTime--;
    }

    public void Reset()
    {
        RemainingTime = Duration;
        Channel = 0;
    }
}
