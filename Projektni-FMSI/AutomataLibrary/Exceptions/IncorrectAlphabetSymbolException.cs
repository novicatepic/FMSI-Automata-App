using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI.Exceptions
{
    public class IncorrectAlphabetSymbolException : Exception
    {
        public IncorrectAlphabetSymbolException()
        {
            Console.WriteLine(": not allowed in alphabet");
        }
    }
}
