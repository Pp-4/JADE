using System.Text.RegularExpressions;

namespace JADE.RegularEx;

public static partial class RegExpressions
{
    //match string made of digits, characters dashes and dots followed (optionaly) by slash alphabetic string, return the string
    [GeneratedRegex(@"([0-9a-zA-Z-.]+)(?:\/[a-zA-Z]+)?")]
    public static partial Regex GetShortTradeId();

    //match text with square brackets, return texts 
    [GeneratedRegex(@"\[([a-zA-Z0-9°]+)\]")]
    public static partial Regex GetTextInBrackets();
}
