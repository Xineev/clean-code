using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markdown
{
    internal class Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value = null)
        {
            Type = type;
            Value = value;
        }
    }
}
