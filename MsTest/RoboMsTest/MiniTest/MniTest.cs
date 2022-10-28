using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Shouldly;

namespace RoboMsTest.MiniTest;

[TestClass]
public class MniTest
{
    [TestMethod]
    public void RegExTest_OneWord()
    {
        string pattern = @"^/\b[a-zA-Z0-9_]+\b$";
        var text = "/Start";
        Match m = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        if (m.Success)
        {
            var msg = m.Value;
        }

        text.ShouldStartWith("/");
    }
}