using System.Text.RegularExpressions;

namespace Blog.Shared.Utils;

public static class EmailUtil
{
    private static readonly Regex EmailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public static bool ValidateEmail(string email)
    {
        return EmailRegex.IsMatch(email);
    }
}