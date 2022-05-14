using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI.Exceptions
{
    public class InvalidTransitionInputException : Exception
    {
        public InvalidTransitionInputException()
        {
            Console.WriteLine("This transition doesn't exist!");
        }
    }
}
