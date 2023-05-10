namespace NexumNovus.AppSettings.Common.Test;

using System.Text;
using NexumNovus.AppSettings.Common.Secure;
using NexumNovus.AppSettings.Common.Utils;

public class AppSettingsParser_Tests
{
  [Fact]
  public void Should_Parse_Json_From_Stream()
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
  public void Should_Parse_Json_From_File()
  {
    // Arrange
    var jsonString = /*lang=json*/ @"
{
  ""Name"": ""test"",
  ""Child"": {
    ""Name"": ""test2""
  }
}";
    File.WriteAllText("test.json", jsonString);

    // Act
    var result = AppSettingsParser.Parse("test.json");

    // Assert
    result.Should().HaveCount(2);
    result["name"].Should().Be("test");
    result["child:name"].Should().Be("test2");

    // Cleanup
    File.Delete("test.json");
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
  public void Should_Flatten_Simple_Object()
  {
    // Arrange

    // Act
    var result = AppSettingsParser.Flatten("test", "testObj", SecretAttributeAction.Ignore);

    // Assert
    result.Should().HaveCount(1);
    result["testObj"].Should().Be("test");
  }

  [Fact]
  public void Should_Flatten_Object_With_Section()
  {
    // Arrange
    var testObj = new TestClass("test", "test2");

    // Act
    var result = AppSettingsParser.Flatten(testObj, "parent:testObj", SecretAttributeAction.Ignore);

    // Assert
    result.Should().HaveCount(2);
    result["parent:testObj:name"].Should().Be("test");
    result["parent:testObj:child:name"].Should().Be("test2");
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
  public void Should_Convert_Settings_Dictionary_To_Json()
  {
    // Arrange
    var dict = new Dictionary<string, string?>
    {
        { "Logging", "Warning" },
        { "AllowedHosts", null },
        { "ConnectionStrings:DefaultConnection", "MyDbConnection" },
        { "ConnectionStrings:Data:0", "A" },
        { "ConnectionStrings:Data:1", "B" },
        { "ConnectionStrings:Data:2", "C" },
    };

    var jsonString = /*lang=json*/
@"{
  ""Logging"": ""Warning"",
  ""AllowedHosts"": null,
  ""ConnectionStrings"": {
    ""DefaultConnection"": ""MyDbConnection"",
    ""Data"": [
      ""A"",
      ""B"",
      ""C""
    ]
  }
}";

    // Act
    var result = AppSettingsParser.ConvertSettingsDictionaryToJson(dict);

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
