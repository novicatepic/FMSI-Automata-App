using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    public class TokenRegEx : IToken
    {
        public TokenRegEx(string type, string value)
        {
            Type = type;
            Value = value;
        }

        //Check validity of token for regexp
        public static bool isTokenValid(string token)
        {
            if (token.Length == 1 && (char.IsDigit(token[0]) || char.IsLetter(token[0])))
            {
                return true;
            }
            return false;
        }
    }
}
