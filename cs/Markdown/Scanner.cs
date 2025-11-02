using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markdown
{
    internal class Scanner
    {
        public static List<Token> TextToListOfTokens(string text)
        {
            var lines = TextToLines(text);
            var tokens = LinesToTokens(lines);
            throw new NotImplementedException();
        }

        public static List<string> TextToLines(string text)
        {
            throw new NotImplementedException();
        }

        public static List<Token> LinesToTokens(IEnumerable<string> lines)
        {
            var tokens = new List<Token>();

            foreach (var line in lines)
            {
                tokens.AddRange(ConvertLineToTokens(line));
            }

            return tokens;
        }

        public static List<Token> ConvertLineToTokens(string line)
        {
            throw new NotImplementedException();
        }
    }
}
