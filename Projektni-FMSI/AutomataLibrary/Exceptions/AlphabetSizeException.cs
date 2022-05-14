using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI.Exceptions
{
    public class AlphabetSizeException : Exception
    {
        public AlphabetSizeException()
        {
            Console.WriteLine("Element of alphabet must be a character!");
        }
    }
}
