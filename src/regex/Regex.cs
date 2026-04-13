using System.Text.RegularExpressions;

namespace PlaywrightSharp.RegularEx;

public static partial class RegExpressions
{
    //match alphanumeric string follower(optionaly) by slash alphabetic string, return the alphanumeric string
    [GeneratedRegex(@"([0-9a-zA-Z]+)(?:\/[a-zA-Z]+)?")]
    public static partial Regex GetShortTradeId();

    //match text with square brackets, return texts 
    [GeneratedRegex(@"\[([a-zA-Z0-9°]+)\]")]
    public static partial Regex GetTextInBrackets();
}
