using System;
using System.Collections.Generic;
using Projektni_FMSI;

namespace Project_SecondApp
{

    //Just calling func
    class ProgramSecond
    {
        static void Main(string[] args)
        {
            Automat a = new();
            a.makeAutomata();
            CodeGenerator gen2 = new(a);
            gen2.generate();
        }
    }
}
