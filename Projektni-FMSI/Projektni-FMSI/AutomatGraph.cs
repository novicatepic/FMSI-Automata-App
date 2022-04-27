﻿using System;
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
            for (int i = 0; i < size; i++)
            {
                visit[i] = false;
            }
            SortedSet<string> set = new();
            void dfs_visit(int u)
            {
                int v;
                visit[u] = true;
                set.Add(nodes[u]);
                for (v = 0; v < size; v++)
                {
                    if (ms[u, v] == 1 && !visit[v])
                    {
                        dfs_visit(v);
                    }
                }
            }
            for (int i = 0; i < size; i++)
            {
                if (startState.Equals(nodes[i]))
                {
                    dfs_visit(i);
                }
            }
            //dfs_visit(0);
            return set;
        }

        public Automat bfsTraversal(Automat automata)
        {
            Automat resultAutomat = new();
            HashSet<string> resultSet = new();
            HashSet<string> originalSet = new();
            Queue<string> queue = new();
            queue.Enqueue(automata.getStartState());
            SortedSet<char> alphabetSorted = automata.sortAutomataAlphabet();
            HashSet<char> alphabetS = new HashSet<char>(alphabetSorted);
            resultAutomat.alphabet = alphabetS;
            resultAutomat.StartState = "a0";
            resultAutomat.states.Add("a0");
            int counter = 0;

            while (queue.Count > 0)
            {
                string temp = queue.Dequeue();
                string newStateName = "a" + counter;
                if(newStateName != "a0")
                {
                    resultAutomat.states.Add(newStateName);
                }
                originalSet.Add(temp);
                if(automata.finalStates.Contains(temp))
                {
                    resultAutomat.finalStates.Add(newStateName);
                }
                counter++;
                resultSet.Add(newStateName);
                foreach (char symbol in alphabetSorted)
                {
                    if(!queue.Contains(automata.delta[(temp, symbol)]) && !originalSet.Contains(automata.delta[(temp, symbol)]))
                        queue.Enqueue(automata.delta[(temp, symbol)]);
                }
            }

            for(int i = 0; i < resultSet.Count; i++)
            {
                foreach(var symbol in alphabetSorted)
                {
                    int j;
                    string tempState = automata.delta[(originalSet.ElementAt(i), symbol)];
                    for (j = 0; j < originalSet.Count; j++)
                    {
                        if(tempState == originalSet.ElementAt(j))
                        {
                            break;
                        }
                    }
                    resultAutomat.delta[(resultSet.ElementAt(i), symbol)] = resultSet.ElementAt(j);
                }
            }

 
            return resultAutomat;
        }
    }
}
