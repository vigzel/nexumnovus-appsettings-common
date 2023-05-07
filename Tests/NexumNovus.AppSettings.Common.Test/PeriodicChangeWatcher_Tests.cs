namespace NexumNovus.AppSettings.Common.Test;
public class PeriodicChangeWatcher_Tests
{
  private const int RefreshInterval = 50;

  [Fact]
  public async Task Should_Detect_Change_When_New_State_Is_Returned_Async()
  {
    // Arrange
    var tester = new Mock<ITest>();
    tester.Setup(x => x.GetNewState()).Returns("newState");
    var initialState = "initial";
    var refreshInterval = TimeSpan.FromMilliseconds(RefreshInterval);
    var sut = new PeriodicChangeWatcher(tester.Object.GetNewState, initialState, refreshInterval);

    // Act
    var changeToken = sut.Watch();
    await Task.Delay(RefreshInterval * 4).ConfigureAwait(false);

    // Assert
    changeToken.HasChanged.Should().BeTrue();
    tester.Verify(x => x.GetNewState(), Times.AtLeast(3));
  }

  [Fact]
  public async Task Should_Detect_No_Change_When_Identical_State_Is_Returned_Async()
  {
    // Arrange
    var tester = new Mock<ITest>();
    tester.Setup(x => x.GetNewState()).Returns("oldState");
    var initialState = "oldState";
    var refreshInterval = TimeSpan.FromMilliseconds(RefreshInterval);
    var sut = new PeriodicChangeWatcher(tester.Object.GetNewState, initialState, refreshInterval);

    // Act
    var changeToken = sut.Watch();
    await Task.Delay(RefreshInterval * 2).ConfigureAwait(false);

    // Assert
    changeToken.HasChanged.Should().BeFalse();
    tester.Verify(x => x.GetNewState(), Times.AtLeastOnce);
  }

  [Fact]
  public void Should_Trigger_Change_When_Called_With_New_State()
  {
    // Arrange
    var tester = new Mock<ITest>();
    tester.Setup(x => x.GetNewState()).Returns("oldState");
    var initialState = "oldState";
    var refreshInterval = TimeSpan.FromMilliseconds(RefreshInterval);
    var sut = new PeriodicChangeWatcher(tester.Object.GetNewState, initialState, refreshInterval);

    // Act
    var changeToken = sut.Watch();
    sut.TriggerChange("newState");

    // Assert
    changeToken.HasChanged.Should().BeTrue();
    tester.Verify(x => x.GetNewState(), Times.Never);
  }

  [Fact]
  public void Should_Not_Trigger_Change_When_Called_With_Identical_State()
  {
    // Arrange
    var tester = new Mock<ITest>();
    tester.Setup(x => x.GetNewState()).Returns("oldState");
    var initialState = "oldState";
    var refreshInterval = TimeSpan.FromMilliseconds(RefreshInterval);
    var sut = new PeriodicChangeWatcher(tester.Object.GetNewState, initialState, refreshInterval);

    // Act
    var changeToken = sut.Watch();
    sut.TriggerChange("oldState");

    // Assert
    changeToken.HasChanged.Should().BeFalse();
    tester.Verify(x => x.GetNewState(), Times.Never);
  }

  public interface ITest
  {
    string GetNewState();
  }
}
