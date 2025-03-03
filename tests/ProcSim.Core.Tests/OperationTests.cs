using ProcSim.Core.Models.Operations;
using ProcSim.Core.Enums;

namespace ProcSim.Core.Tests;

public class OperationTests
{
    [Fact]
    public void CpuOperation_ExecuteTick_DecreasesRemainingTime()
    {
        // Arrange: cria uma operação de CPU com duração 5.
        int duration = 5;
        IOperation operation = new CpuOperation(duration);

        // Act & Assert: a cada tick o tempo restante deve diminuir até chegar a zero.
        for (int i = 0; i < duration; i++)
        {
            Assert.False(operation.IsCompleted);
            int previousRemaining = operation.RemainingTime;
            operation.ExecuteTick();
            Assert.Equal(previousRemaining - 1, operation.RemainingTime);
        }

        // Após 5 ticks, a operação deve estar concluída.
        Assert.True(operation.IsCompleted);
        Assert.Equal(0, operation.RemainingTime);
    }

    [Fact]
    public void IoOperation_ExecuteTick_DecreasesRemainingTimeAndKeepsDeviceType()
    {
        // Arrange: cria uma operação de I/O com duração 3 e tipo Disk.
        int duration = 3;
        IoDeviceType expectedDeviceType = IoDeviceType.Disk;
        IOperation operation = new IoOperation(duration, expectedDeviceType);

        // Verifica se a operação implementa a interface IIoOperation e possui o tipo correto.
        var ioOperation = operation as IIoOperation;
        Assert.NotNull(ioOperation);
        Assert.Equal(expectedDeviceType, ioOperation.DeviceType);

        // Act & Assert: executa os ticks e verifica a redução do tempo restante.
        for (int i = 0; i < duration; i++)
        {
            Assert.False(operation.IsCompleted);
            int previousRemaining = operation.RemainingTime;
            operation.ExecuteTick();
            Assert.Equal(previousRemaining - 1, operation.RemainingTime);
        }

        // Após os ticks, a operação deve estar concluída.
        Assert.True(operation.IsCompleted);
        Assert.Equal(0, operation.RemainingTime);
    }

    [Fact]
    public void Operation_RemainingTime_DoesNotGoBelowZero()
    {
        // Arrange: cria uma operação de CPU com duração 2.
        int duration = 2;
        IOperation operation = new CpuOperation(duration);

        // Act: chama ExecuteTick mais vezes do que a duração.
        for (int i = 0; i < duration + 2; i++)
        {
            operation.ExecuteTick();
        }

        // Assert: o tempo restante nunca será negativo e a operação está concluída.
        Assert.Equal(0, operation.RemainingTime);
        Assert.True(operation.IsCompleted);
    }
}
