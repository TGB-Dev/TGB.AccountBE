using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Services;

namespace TGB.AccountBE.Test;

public class PasswordGeneratorTest
{
    private const int PASSWORD_LENGTH = 32;

    [Fact(DisplayName = "Single password generation")]
    public void SingleGenerationTest()
    {
        var generator = new RandomPasswordGenerator();
        var password = generator.Generate();

        Assert.Matches(UserInfoRules.PASSWORD_PATTERN, password);
        Assert.Equal(PASSWORD_LENGTH, password.Length);
    }

    [Fact(DisplayName = "10000 password generations to ensure uniqueness")]
    public void TenThousandGenerationTest()
    {
        const int NUMBER_OF_GENERATIONS = 10_000;

        var generator = new RandomPasswordGenerator();
        HashSet<string> passwordSet = [];

        for (var i = 0; i < NUMBER_OF_GENERATIONS; i++)
        {
            var password = generator.Generate();

            Assert.Matches(UserInfoRules.PASSWORD_PATTERN, password);
            Assert.Equal(PASSWORD_LENGTH, password.Length);
            Assert.DoesNotContain(password, passwordSet);
        }
    }
}
