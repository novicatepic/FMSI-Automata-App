using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    class AutomatGraph
    {
        private int size;
        int[,] ms; //= new int[MAX, MAX];
        string[] states;// = new string[MAX];

        AutomatGraph() { }

        AutomatGraph(int size)
        {
            this.size = size;
            ms = new int[size, size];
            states = new string[size];
        }


    }
}
