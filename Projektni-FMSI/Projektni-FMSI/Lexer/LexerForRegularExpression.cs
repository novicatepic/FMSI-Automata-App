using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    public class LexerForRegularExpression : ILexer
    {
        private readonly string[] source;

        public LexerForRegularExpression(string[] input)
        {
            sourcePosition = 0;
            source = input;
        }

        public override TokenRegEx Next()
        {
            if(sourcePosition >= source.Length)
            {
                return new TokenRegEx("EOF", null);
            }
            else if(sourcePosition > source.Length)
            {
                return null;
            }

            while(sourcePosition < source.Length && source[sourcePosition] == "")
            {
                sourcePosition++;
            }

            if(TokenRegEx.isTokenValid(source[sourcePosition]))
            {
                sourcePosition++;
                return new TokenRegEx("symbol", source[sourcePosition - 1]);
            }

            if(source[sourcePosition].Length == 1 && char.Parse(source[sourcePosition]) == '+')
            {
                sourcePosition++;
                return new TokenRegEx("+", null);
            }

            if(source[sourcePosition].Length == 1 && char.Parse(source[sourcePosition]) == '*')
            {
                sourcePosition++;
                return new TokenRegEx("*", null);
            }

            if (source[sourcePosition].Length == 1 && char.Parse(source[sourcePosition]) == '(')
            {
                sourcePosition++;
                return new TokenRegEx("(", null);
            }

            if (source[sourcePosition].Length == 1 && char.Parse(source[sourcePosition]) == ')')
            {
                sourcePosition++;
                return new TokenRegEx(")", null);
            }

            sourcePosition++;

            

            return null;

        }
    }
}
