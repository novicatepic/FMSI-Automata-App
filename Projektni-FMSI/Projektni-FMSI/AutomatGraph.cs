using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    public class AutomatGraph
    {
        private int size;
        public int[,] ms { get; set; }
        public string[] nodes { get; set; }

        public AutomatGraph() { }

        public AutomatGraph(int size)
        {
            this.size = size;
            ms = new int[size, size];
            nodes = new string[size];
        }

        public SortedSet<string> dfs(string startState)
        {
            bool[] visit = new bool[size];
            for(int i = 0; i < size; i++)
            {
                visit[i] = false;
            }
            SortedSet<string> set = new();
            void dfs_visit(int u)
            {
                int v;
                visit[u] = true;
                set.Add(nodes[u]);
                for(v = 0; v < size; v++)
                {
                    if(ms[u,v] == 1 && !visit[v])
                    {
                        dfs_visit(v);
                    }
                }
            }
            for(int i = 0; i < size; i++)
            {
                if(startState.Equals(nodes[i]))
                {
                    dfs_visit(i);
                }
            }
            //dfs_visit(0);
            return set;
        }

        /*void bfsTraversal(Automat automata, int startingNode)
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
        }*/

    }
}
