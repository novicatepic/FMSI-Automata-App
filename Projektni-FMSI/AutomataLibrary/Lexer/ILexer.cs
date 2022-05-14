using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    //Parent class to two other classes, was OOP to do this
    abstract public class ILexer
    {
        protected int sourcePosition;

        public abstract IToken Next();

        public int getSourcePosition()
        {
            return sourcePosition;
        }
    }
}
