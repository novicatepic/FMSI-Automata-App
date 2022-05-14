using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI.Exceptions
{
    public class StateNotPresentException : Exception
    {
        public StateNotPresentException()
        {
            Console.WriteLine("This state doesn't exist!");
        }
    }
}
