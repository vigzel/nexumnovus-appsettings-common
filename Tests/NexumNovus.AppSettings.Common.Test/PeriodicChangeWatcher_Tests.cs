namespace NexumNovus.AppSettings.Common.Test;

using Microsoft.Reactive.Testing;
using NexumNovus.AppSettings.Common.Utils;

public class PeriodicChangeWatcher_Tests
{
  [Fact]
  public void Should_Detect_Change_When_New_State_Is_Returned()
  {
    // Arrange
    var tester = new Mock<ITest>();
    tester.Setup(x => x.GetNewState()).Returns("newState");
    var testScheduler = new TestScheduler();
    var sut = new PeriodicChangeWatcher(tester.Object.GetNewState, "initialState", TimeSpan.FromSeconds(1), scheduler: testScheduler);

    // Act
    var changeToken = sut.Watch();
    testScheduler.AdvanceBy(TimeSpan.FromSeconds(3.5).Ticks);

    // Assert
    changeToken.HasChanged.Should().BeTrue();
    tester.Verify(x => x.GetNewState(), Times.Exactly(3));
  }

  [Fact]
  public void Should_Detect_No_Change_When_Identical_State_Is_Returned()
  {
    // Arrange
    var tester = new Mock<ITest>();
    tester.Setup(x => x.GetNewState()).Returns("initialState");
    var testScheduler = new TestScheduler();
    var sut = new PeriodicChangeWatcher(tester.Object.GetNewState, "initialState", TimeSpan.FromSeconds(1), scheduler: testScheduler);

    // Act
    var changeToken = sut.Watch();
    testScheduler.AdvanceBy(TimeSpan.FromSeconds(1.5).Ticks);

    // Assert
    changeToken.HasChanged.Should().BeFalse();
    tester.Verify(x => x.GetNewState(), Times.AtLeastOnce);
  }

  [Fact]
  public void Should_Trigger_Change_When_Called_With_New_State()
  {
    // Arrange
    var tester = new Mock<ITest>();
    var testScheduler = new TestScheduler();
    var sut = new PeriodicChangeWatcher(tester.Object.GetNewState, "initialState", TimeSpan.Zero, scheduler: testScheduler);

    // Act
    var changeToken = sut.Watch();
    testScheduler.AdvanceBy(TimeSpan.FromMinutes(10).Ticks);
    sut.TriggerChange("newState");

    // Assert
    changeToken.HasChanged.Should().BeTrue();
    tester.Verify(x => x.GetNewState(), Times.Never);
  }

  [Fact]
  public void Should_Trigger_Change_When_Called_With_Identical_State()
  {
    // Arrange
    var tester = new Mock<ITest>();
    var testScheduler = new TestScheduler();
    var sut = new PeriodicChangeWatcher(tester.Object.GetNewState, "initialState", TimeSpan.Zero, scheduler: testScheduler);

    // Act
    var changeToken = sut.Watch();
    testScheduler.AdvanceBy(TimeSpan.FromMinutes(10).Ticks);
    sut.TriggerChange("initialState");

    // Assert
    changeToken.HasChanged.Should().BeTrue();
    tester.Verify(x => x.GetNewState(), Times.Never);
  }

  public interface ITest
  {
    string GetNewState();
  }
}
