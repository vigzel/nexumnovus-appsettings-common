namespace NexumNovus.AppSettings.Common.Test;

using System.Text;
using NexumNovus.AppSettings.Common.Secure;
using NexumNovus.AppSettings.Common.Utils;

public class AppSettingsParser_Tests
{
  [Fact]
  public void Should_Parse_Json()
  {
    // Arrange
    var jsonString = /*lang=json*/ @"
{
  ""Name"": ""test"",
  ""Child"": {
    ""Name"": ""test2""
  }
}";

    // Act
    var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
    var result = AppSettingsParser.Parse(stream);
    stream.Dispose();

    // Assert
    result.Should().HaveCount(2);
    result["name"].Should().Be("test");
    result["child:name"].Should().Be("test2");
  }

  [Fact]
  public void Should_Flatten_Object()
  {
    // Arrange
    var testObj = new TestClass("test", "test2");

    // Act
    var result = AppSettingsParser.Flatten(testObj, "testObj", SecretAttributeAction.Ignore);

    // Assert
    result.Should().HaveCount(2);
    result["testObj:name"].Should().Be("test");
    result["testObj:child:name"].Should().Be("test2");
  }

  [Fact]
  public void Should_Mark_SecretSetting_With_Star_When_Flattening_Object()
  {
    // Arrange
    var testObj = new TestClass("test", "test2");

    // Act
    var result = AppSettingsParser.Flatten(testObj, "testObj", SecretAttributeAction.MarkWithStar);

    // Assert
    result.Should().HaveCount(2);
    result["testObj:name"].Should().Be("test");
    result["testObj:child:name*"].Should().Be("test2");
  }

  [Fact]
  public void Should_Mark_And_Protect_SecretSetting_When_Flattening_Object()
  {
    // Arrange
    var mockProtector = new Mock<ISecretProtector>();
    mockProtector.Setup(x => x.Protect(It.IsAny<string>())).Returns("***");
    var testObj = new TestClass("test", "test2");

    // Act
    var result = AppSettingsParser.Flatten(testObj, "testObj", SecretAttributeAction.MarkWithStarAndProtect, mockProtector.Object);

    // Assert
    result.Should().HaveCount(2);
    result["testObj:name"].Should().Be("test");
    result["testObj:child:name*"].Should().Be("***");
  }

  [Fact]
  public void Should_Mark_And_Protect_SecretSetting_When_Serializing_Object()
  {
    // Arrange
    var mockProtector = new Mock<ISecretProtector>();
    mockProtector.Setup(x => x.Protect(It.IsAny<string>())).Returns("***");
    var testObj = new TestClass("test", "test2");
    var jsonString = /*lang=json*/
@"{
  ""Name"": ""test"",
  ""Child"": {
    ""Name*"": ""***""
  }
}";

    // Act
    var result = AppSettingsParser.SerializeObject(testObj, mockProtector.Object);

    // Assert
    result.Should().Be(jsonString);
  }

  private sealed class TestClass
  {
    public string Name { get; set; }

    public TestChildClass Child { get; set; }

    public TestClass(string name, string childName)
    {
      Name = name;
      Child = new TestChildClass(childName);
    }
  }

  private sealed class TestChildClass
  {
    [SecretSetting]
    public string Name { get; set; }

    public TestChildClass(string name) => Name = name;
  }
}
