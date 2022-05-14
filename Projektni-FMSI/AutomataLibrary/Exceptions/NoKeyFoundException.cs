using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI.Exceptions
{
    public class NoKeyFoundException : Exception
    {
        public NoKeyFoundException()
        {
            Console.WriteLine("Key not found!");
        }
    }
}
