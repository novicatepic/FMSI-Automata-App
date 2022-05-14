using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    //Abstract class cuz we have two possible tokens in this case
    abstract public class IToken
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public void printToken()
        {
            if(Value.Equals(null))
            {
                Console.WriteLine("\nToken type: " + Type + "\nToken value: null");
            }
            else
            {
                Console.WriteLine("\nToken type: " + Type + "\nToken value: " + Value);
            }
        }

    }
}
