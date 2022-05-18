using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataLibrary.Exceptions
{
    public class ParsingException : Exception
    {
        public ParsingException()
        {
            Console.WriteLine("Error while parsing!");
        }
    }
}
