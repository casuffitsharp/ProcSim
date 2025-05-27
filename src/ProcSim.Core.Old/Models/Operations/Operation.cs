using System.Text.Json.Serialization;

namespace ProcSim.Core.Old.Models.Operations;

public abstract class Operation(int duration) : IOperation
{
    public event Action RemainingTimeChanged;
    public event Action ChannelChanged;

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
    public int? Channel
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                ChannelChanged?.Invoke();
            }
        }
    }

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
        Channel = null;
    }
}
