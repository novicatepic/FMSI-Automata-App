using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    class AutomatGraph : Automat
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

        void bfsTraversal(Automat automata, int startingNode)
        {
            int shortestWord = 0, longestWord = 0;
            int helpCounter = 0;
            
            string rez = "";
            Queue<string> bfsQueue = new();
            Queue<int> otherBFSQueue = new();
            bool[] visit = new bool[size];
            for(int i = 0; i < size; i++)
            {
                visit[i] = false;
            }
            visit[startingNode] = true;
            bfsQueue.Enqueue(states[startingNode]);
            otherBFSQueue.Enqueue(startingNode);
            while(otherBFSQueue.Count > 0)
            {
                string temp = bfsQueue.Dequeue();
                int num = otherBFSQueue.Dequeue();
                rez += temp;
                for(int i = 0; i < size; i++)
                {
                    if(ms[num, i] == 1)
                    {
                        visit[i] = true;
                        bfsQueue.Enqueue(states[i]);
                        otherBFSQueue.Enqueue(i);
                    }
                }
            }
        }

    }
}
