using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI.Exceptions
{
    public class LexicalException : Exception
    {
        public LexicalException()
        {
            Console.WriteLine("Lexical analysis failed, you are informed about lines with errors!");
        }
    }
}
