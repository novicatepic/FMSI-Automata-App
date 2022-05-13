using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    abstract public class ILexer
    {
        protected int sourcePosition;

        public abstract IToken Next();
    }
}
