using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI.Exceptions
{
    public class AlphabetNotContainsException : Exception
    {
        public AlphabetNotContainsException()
        {
            Console.WriteLine("Alphabet doesn't contain this symbol!");
        }
    }
}
