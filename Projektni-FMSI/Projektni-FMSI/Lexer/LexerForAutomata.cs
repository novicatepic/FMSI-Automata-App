using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    public class LexerForAutomata : ILexer
    {
        private readonly string[] source;

        public LexerForAutomata(string[] input)
        {
            sourcePosition = 0;
            source = input;
        }

        public override IToken Next()
        {
            throw new NotImplementedException();
        }
    }
}
