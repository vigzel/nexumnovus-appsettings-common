namespace NexumNovus.AppSettings.Common.Test;

using NexumNovus.AppSettings.Common.Secure;

public class DefaultSecretProtector_Tests
{
  [Fact]
  public void Should_Protect_Plain_Text()
  {
    var sut = DefaultSecretProtector.Instance;
    var plainText = "test";

    var protectedText = sut.Protect(plainText);
    var unprotectedText = sut.Unprotect(protectedText);

    protectedText.Should().NotBe(plainText);
    unprotectedText.Should().Be(plainText);
  }
}
