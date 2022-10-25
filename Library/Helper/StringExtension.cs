using System.Text.RegularExpressions;

namespace Helper;

public static class StringExtension
{
    public static string MakeValidFileName(this string name)
    {
        var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return Regex.Replace(name, invalidRegStr, "_");
    }
}