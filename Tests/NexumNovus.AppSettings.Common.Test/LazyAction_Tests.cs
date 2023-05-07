namespace NexumNovus.AppSettings.Common.Test;

using NexumNovus.AppSettings.Common.Utils;

public class LazyAction_Tests
{
  [Fact]
  public void Should_Protect_Plain_Text()
  {
    // Arrange
    var initialized = false;
    var syncLock = new object();
    var tester = new Mock<ITest>();

    // Act
    LazyAction.EnsureInitialized(ref initialized, ref syncLock, tester.Object.Test);
    LazyAction.EnsureInitialized(ref initialized, ref syncLock, tester.Object.Test);

    // Assert
    tester.Verify(x => x.Test(), Times.Once);
  }

  public interface ITest
  {
    void Test();
  }
}
