using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    public class TokenAutomata : IToken
    {
        public TokenAutomata(string type, string value)
        {
            Type = type;
            Value = value;
        }

        //Two functions to check validity of tokens for automata
        public static bool isStateTokenValid(string stateToken)
        {
            foreach(var symbol in stateToken)
            {
                if(!char.IsDigit(symbol) && !char.IsLetter(symbol))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool isAlphabetTokenValid(string alphabetToken)
        {
            if(alphabetToken.Length != 1 || (alphabetToken.Length == 1 && alphabetToken[0] == ':'))
            {
                return false;
            }
            return true;
        }

    }
}
