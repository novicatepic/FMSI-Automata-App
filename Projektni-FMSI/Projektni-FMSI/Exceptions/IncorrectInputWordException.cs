using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI.Exceptions
{
    public class IncorrectInputWordException : Exception
    {
        public IncorrectInputWordException()
        {
            Console.WriteLine("Incorrect input word!");
        }
    }
}
