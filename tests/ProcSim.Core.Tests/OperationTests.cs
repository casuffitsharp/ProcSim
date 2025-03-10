using ProcSim.Core.Enums;
using ProcSim.Core.Models.Operations;

namespace ProcSim.Core.Tests;

public class OperationTests
{
    [Fact]
    public void CpuOperation_ExecuteTick_DecreasesRemainingTime()
    {
        int duration = 5;
        IOperation operation = new CpuOperation(duration);

        for (int i = 0; i < duration; i++)
        {
            Assert.False(operation.IsCompleted);
            int previous = operation.RemainingTime;
            operation.ExecuteTick();
            Assert.Equal(previous - 1, operation.RemainingTime);
        }

        Assert.True(operation.IsCompleted);
        Assert.Equal(0, operation.RemainingTime);
    }

    [Fact]
    public void IoOperation_ExecuteTick_DecreasesRemainingTimeAndMaintainsDeviceType()
    {
        int duration = 3;
        IoDeviceType expected = IoDeviceType.Disk;
        IOperation operation = new IoOperation(duration, expected);

        IIoOperation ioOp = operation as IIoOperation;
        Assert.NotNull(ioOp);
        Assert.Equal(expected, ioOp.DeviceType);

        for (int i = 0; i < duration; i++)
        {
            Assert.False(operation.IsCompleted);
            int prev = operation.RemainingTime;
            operation.ExecuteTick();
            Assert.Equal(prev - 1, operation.RemainingTime);
        }

        Assert.True(operation.IsCompleted);
        Assert.Equal(0, operation.RemainingTime);
    }

    [Fact]
    public void Operation_RemainingTime_DoesNotGoBelowZero()
    {
        int duration = 2;
        IOperation operation = new CpuOperation(duration);

        for (int i = 0; i < duration + 2; i++)
        {
            operation.ExecuteTick();
        }

        Assert.Equal(0, operation.RemainingTime);
        Assert.True(operation.IsCompleted);
    }
}
