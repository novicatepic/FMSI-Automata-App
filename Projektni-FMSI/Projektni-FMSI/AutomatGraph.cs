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
            for(int i = 0; i < size; i++)
            {
                for(int j = 0; j < size; j++)
                {
                    ms[i, j] = 0;
                }
            }

        }

        public SortedSet<string> dfs(string startState)
        {
            bool[] visit = initVisit();
            SortedSet<string> set = new();
            void dfs_visit(int u)
            {
                int v;
                visit[u] = true;
                set.Add(nodes[u]);
                //Console.Write(nodes[u] + " ");
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
            return set;
        }

        public bool dfsForLongestWord(string startState)
        {
            bool isThereACycle = false;
            List<string> set = new();
            bool[] visit = initVisit();

            void dfs_visit(int u)
            {
                int v;
                visit[u] = true;
                //Console.Write(nodes[u] + " ");
                
                if(set.Contains(nodes[u]))
                {
                    isThereACycle = true;
                    return;
                }
                set.Add(nodes[u]);
                
               

                for (v = 0; v < size; v++)
                {
                    if (ms[u, v] == 1 && (!visit[v] || nodes[v] == startState))
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

            return isThereACycle;
        }

        public int bfsFindLongestWord(Automat visit)
        {
            int longestWord = 0;

            Queue<string> queue = new();
            HashSet < HashSet<string> > whichOnesWereVisited = new HashSet<HashSet<string>>();
            HashSet<string> rememberAllAlreadyVisited = new();
            
            for(int i = 0; i < visit.states.Count; i++)
            {
                whichOnesWereVisited.Add(new HashSet<string>());
            }

            whichOnesWereVisited.ElementAt(0).Add(visit.StartState);

            queue.Enqueue(visit.StartState);
            int counterForSortedSet = 1;

            while(queue.Count > 0)
            {
                string state = queue.Dequeue();

                rememberAllAlreadyVisited.Add(state);
                int position = findPositionOfState(state);

                    for(int j = 0; j < size; j++)
                    {
                        if(ms[position, j] == 1 && !rememberAllAlreadyVisited.Contains(findStateBasedOnPosition(j)))
                        {
                            string element = findStateBasedOnPosition(j);
                            queue.Enqueue(element);
                            rememberAllAlreadyVisited.Add(element);
                            for(int g = 0; g < whichOnesWereVisited.Count; g++) {
                                if(whichOnesWereVisited.ElementAt(g).Contains(state))
                                {
                                    whichOnesWereVisited.ElementAt(g + 1).Add(element);
                                    break;
                                }
                            }
                        }
                    }
                counterForSortedSet++;

                foreach(var element in queue)
                {
                    int wordLength = 0;
                    if(visit.finalStates.Contains(element))
                    {
                        for(int g = 0; g < whichOnesWereVisited.Count; g++)
                        {
                            if(whichOnesWereVisited.ElementAt(g).Contains(element))
                            {
                                wordLength = g;
                            }
                        }
                        if(wordLength > longestWord)
                        {
                            longestWord = wordLength;
                        }
                    }
                }

            }

            return longestWord;
        }

        private string findStateBasedOnPosition(int position)
        {
            return nodes.ElementAt(position);
        }

        private int findPositionOfState(string state)
        {
            for (int i = 0; i < size; i++) {
                if (nodes[i] == state)
                    return i;
            }
            return -1;
        }


        private bool[] initVisit()
        {
            bool[] visit = new bool[size];
            for (int i = 0; i < size; i++)
            {
                visit[i] = false;
            }

            return visit;
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

        public bool minimiseAutomataHelper(Automat a)
        {
            bool checkIfThereIsNone = false;
            for(int i = 0; i < a.states.Count; i++)
            {
                for(int j = 0; j < a.states.Count; j++)
                {
                    if(ms[i, j] != -1 && ms[i, j] != 1)
                    {
                        foreach(var symbol in a.alphabet)
                        {
                            //if(nodes.Contains(a.delta[(nodes.ElementAt(i), symbol)]) && nodes.Contains(a.delta[(nodes.ElementAt(j), symbol)]))
                            //{
                                string firstState = a.delta[(nodes.ElementAt(i), symbol)];
                                string secondState = a.delta[(nodes.ElementAt(j), symbol)];
                                int position1, position2;
                                for(position1 = 0; position1 < a.states.Count; position1++)
                                {
                                    if(nodes.ElementAt(position1) == firstState)
                                    {
                                        break;
                                    }
                                }
                                for (position2 = 0; position2 < a.states.Count; position2++)
                                {
                                    if (nodes.ElementAt(position2) == secondState)
                                    {
                                        break;
                                    }
                                }
                                if(ms[position1, position2] == 1 || ms[position2, position1] == 1)
                                {
                                    //Console.WriteLine(i + " " + j);
                                    checkIfThereIsNone = true;
                                    ms[i, j] = 1;
                                }
                            //}
                        }
                    }
                }
            }
            return checkIfThereIsNone;
        }
    }
}
