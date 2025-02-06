using System.Security.Cryptography;
using TGB.AccountBE.API.Interfaces.Services;

namespace TGB.AccountBE.API.Services;

public class RandomPasswordGenerator : IRandomPasswordGenerator
{
    public string Generate()
    {
        // The password requirements are:
        // 1. Has at least 1 number
        // 2. Has at least 1 letter (A-Z or a-z)
        // 3. Has the minimum length of 8

        // To prevent brute-forcing attacks against accounts that are registered with OAuth providers,
        // we generate a password that has the following parameters

        const int MINIMUM_PASSWORD_LENGTH = 32;
        const int MINIMUM_NUMBER_OF_SPECIAL_CHARS = 1;
        const int MAXIMUM_NUMBER_OF_SPECIAL_CHARS = 8;
        const int MINIMUM_NUMBER_OF_NUMBERS = 1;
        const int MAXIMUM_NUMBER_OF_NUMBERS = 8;

        // The datasets are inspired from https://github.com/bitwarden/clients/blob/main/libs/tools/generator/core/src/engine/data.ts
        const string LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        const string NUMBERS = "0123456789";
        const string SPECIAL_CHARS = "!@#$%^&*";

        // The main password generation
        // The idea is get a random set of special characters, a random set of numbers, and a random
        // set of letters, concatenate them, and then shuffle them up

        var numberOfSpecialChars = RandomNumberGenerator.GetInt32(MINIMUM_NUMBER_OF_SPECIAL_CHARS,
            MAXIMUM_NUMBER_OF_SPECIAL_CHARS + 1);
        var numberOfNumbers =
            RandomNumberGenerator.GetInt32(MINIMUM_NUMBER_OF_NUMBERS,
                MAXIMUM_NUMBER_OF_NUMBERS + 1);
        var numberOfLetters = MINIMUM_PASSWORD_LENGTH - numberOfSpecialChars - numberOfNumbers;

        var password = "";
        var result = "";

        password += RandomNumberGenerator.GetString(SPECIAL_CHARS, numberOfSpecialChars);
        password += RandomNumberGenerator.GetString(NUMBERS, numberOfNumbers);
        password += RandomNumberGenerator.GetString(LETTERS, numberOfLetters);

        Span<char> tmp = stackalloc char[password.Length];
        password.AsSpan().CopyTo(tmp);
        RandomNumberGenerator.Shuffle(tmp);

        result = tmp.ToString();
        return result;
    }
}
