namespace NexumNovus.AppSettings.Common.Test;

using System.Text;
using NexumNovus.AppSettings.Common.Secure;
using NexumNovus.AppSettings.Common.Utils;

public class AppSettingsParser_Tests
{
  [Fact]
  public void Should_Parse_Json()
  {
    var jsonString = /*lang=json*/ @"
{
  ""Name"": ""test"",
  ""Child"": {
    ""Name"": ""test2""
  }
}";

    var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
    var result = AppSettingsParser.Parse(stream);
    stream.Dispose();

    result.Should().HaveCount(2);
    result["name"].Should().Be("test");
    result["child:name"].Should().Be("test2");
  }

  [Fact]
  public void Should_Flatten_Object()
  {
    var testObj = new TestClass("test", "test2");

    var result = AppSettingsParser.Flatten(testObj, "testObj", SecretAttributeAction.Ignore);

    result.Should().HaveCount(2);
    result["testObj:name"].Should().Be("test");
    result["testObj:child:name"].Should().Be("test2");
  }

  [Fact]
  public void Should_Mark_SecretSetting_With_Star_When_Flattening_Object()
  {
    var testObj = new TestClass("test", "test2");

    var result = AppSettingsParser.Flatten(testObj, "testObj", SecretAttributeAction.MarkWithStar);

    result.Should().HaveCount(2);
    result["testObj:name"].Should().Be("test");
    result["testObj:child:name*"].Should().Be("test2");
  }

  [Fact]
  public void Should_Serialize_Object()
  {
    var testObj = new TestClass("test", "test2");
    var jsonString = /*lang=json*/ @"
{
  ""Name"": ""test"",
  ""Child"": {
    ""Name"": ""test2""
  }
}";

    var result = AppSettingsParser.SerializeObject(testObj, SecretAttributeAction.Ignore);

    result.Should().Be(jsonString.Trim());
  }

  [Fact]
  public void Should_Mark_SecretSetting_With_Star_When_Serializing_Object()
  {
    var testObj = new TestClass("test", "test2");
    var jsonString = /*lang=json*/ @"
{
  ""Name"": ""test"",
  ""Child"": {
    ""Name*"": ""test2""
  }
}";

    var result = AppSettingsParser.SerializeObject(testObj, SecretAttributeAction.MarkWithStar);

    result.Should().Be(jsonString.Trim());
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
