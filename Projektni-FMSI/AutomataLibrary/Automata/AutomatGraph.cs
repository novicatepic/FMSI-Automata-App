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
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    ms[i, j] = 0;
                }
            }

        }

        //CLASSIC DFS TRAVERSAL
        public SortedSet<string> dfs(string startState)
        {
            bool[] visit = initVisit();
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
            return set;
        }

        //HELP FUNCTION FOR FINDING LONGEST WORD
        //If there is a cycle from any of the final state, max word is infinity
        public bool dfsForLongestWord(string startState, Automat automata)
        {
            bool isThereACycle = false;
            List<string> set = new();
            bool[] visit = initVisit();

            void dfs_visit(int u)
            {
                int v;
                visit[u] = true;

                //CYCLE FOUND
                if (set.Contains(nodes[u]))
                {
                    isThereACycle = true;
                    //return;
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

        //Help function to check if cycle was found somewhere before
        private static int[] findPositionForLongestWord(string state, HashSet<HashSet<string>> whichOnesWereVisited)
        {
            int[] result = new int[2];

            for(int i = 0; i < whichOnesWereVisited.Count; i++)
            {
                for(int j = 0; j < whichOnesWereVisited.ElementAt(i).Count; j++)
                {
                    if(whichOnesWereVisited.ElementAt(i).ElementAt(j) == state)
                    {
                        result[0] = i;
                        result[1] = j;
                    }
                }
            }
            return result;
        }

        //IF THERE WAS NO CYCLE
        //FIND LONGEST WORD WITH BFS
        //REMEMBER LEVELS VISITED
        //SO WE CAN REMEMBER LONGEST WORD 
        //AND REPLACE IT AS NEEDED!
        public int bfsFindLongestWord(Automat visit)
        {
            int longestWord = 0;

            Queue<string> queue = new();
            HashSet<HashSet<string>> whichOnesWereVisited = new HashSet<HashSet<string>>();
            HashSet<string> rememberAllAlreadyVisited = new();
            List<List<bool>> checkIfCycleExistsSomewhere = new List<List<bool>>();

            for(int i = 0; i < visit.getStates().Count; i++)
            {
                checkIfCycleExistsSomewhere.Add(new List<bool>());
            } 

            for (int i = 0; i < visit.getStates().Count; i++)
            {
                whichOnesWereVisited.Add(new HashSet<string>());
            }

            whichOnesWereVisited.ElementAt(0).Add(visit.getStartState());
            if(visit.checkIfStateHasCycle(visit.getStartState()))
            {
                checkIfCycleExistsSomewhere.ElementAt(0).Add(true);
            }
            else
            {
                checkIfCycleExistsSomewhere.ElementAt(0).Add(false);
            }

            queue.Enqueue(visit.getStartState());
            int counterForSortedSet = 1;

            while (queue.Count > 0)
            {
                string state = queue.Dequeue();

                rememberAllAlreadyVisited.Add(state);
                int position = findPositionOfState(state);

                for (int j = 0; j < size; j++)
                {
                    if (ms[position, j] == 1 && !rememberAllAlreadyVisited.Contains(findStateBasedOnPosition(j)))
                    {
                        int[] pos = findPositionForLongestWord(state, whichOnesWereVisited);
                        string element = findStateBasedOnPosition(j);
                        queue.Enqueue(element);
                        rememberAllAlreadyVisited.Add(element);
                        for (int g = 0; g < whichOnesWereVisited.Count; g++)
                        {
                            if (whichOnesWereVisited.ElementAt(g).Contains(state))
                            {
                                whichOnesWereVisited.ElementAt(g + 1).Add(element);

                                if(checkIfCycleExistsSomewhere.ElementAt(g).Contains(false))
                                {
                                    if(visit.checkIfStateHasCycle(element) || checkIfCycleExistsSomewhere.ElementAt(pos[0]).ElementAt(pos[1]))
                                    {
                                        checkIfCycleExistsSomewhere.ElementAt(g + 1).Add(true);
                                    }
                                    else
                                    {
                                        checkIfCycleExistsSomewhere.ElementAt(g + 1).Add(false);
                                    }
                                }
                                else
                                {
                                    checkIfCycleExistsSomewhere.ElementAt(g + 1).Add(true);
                                }

                                break;
                            }
                        }
                    }
                }
                counterForSortedSet++;

                /*Console.WriteLine("Which ones were visited: ");
                for(int i = 0; i < nodes.Length; i++)
                {
                    foreach(var element in whichOnesWereVisited.ElementAt(i))
                    {
                        Console.Write(element + " ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();

                Console.WriteLine("Check if cycle exists: ");
                for (int i = 0; i < nodes.Length; i++)
                {
                    foreach (var element in checkIfCycleExistsSomewhere.ElementAt(i))
                    {
                        Console.Write(element + " ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();*/

                /*Console.WriteLine("Remember all already visited: ");
                foreach(var elem in rememberAllAlreadyVisited)
                {
                    Console.Write(elem + " ");
                }
                Console.WriteLine();*/

                /*Console.WriteLine("QUEUE: ");
                foreach (var element in queue)
                {
                    Console.Write(element + " ");
                }
                Console.WriteLine();*/

                foreach (var element in queue)
                {
                    int wordLength = 0;
                    if (visit.getFinalStates().Contains(element))
                    {
                        for (int g = 0; g < whichOnesWereVisited.Count; g++)
                        {
                            int[] pos = findPositionForLongestWord(element, whichOnesWereVisited);
                            if (checkIfCycleExistsSomewhere.ElementAt(pos[0]).ElementAt(pos[1]))
                            {
                                Console.WriteLine("There is no longest word, returning MAX length possible!");
                                return int.MaxValue;
                            }
                            if (whichOnesWereVisited.ElementAt(g).Contains(element))
                            {
                                wordLength = g;
                            }
                        }
                        if (wordLength > longestWord)
                        {
                            longestWord = wordLength;
                        }
                    }
                }

            }

            return longestWord;
        }

        //SOME HELP FUNCTIONS
        private string findStateBasedOnPosition(int position)
        {
            return nodes.ElementAt(position);
        }

        private int findPositionOfState(string state)
        {
            for (int i = 0; i < size; i++)
            {
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

        //WHEN WE WANT TO CHECK IF AUTOMATAS ARE THE SAME
        //WE MINIMISE IT
        //RENAME THE STATE AS SHOWED HERE
        //AND WE GET NEW AUTOMATA 
        //WE DO IT FOR SECOND AUTOMATA AS WELL
        //-> WE GET THE EXPECTED RESULT
        public Automat bfsTraversal(Automat automata)
        {
            Automat resultAutomat = new();
            HashSet<string> resultSet = new();
            HashSet<string> originalSet = new();
            Queue<string> queue = new();
            queue.Enqueue(automata.getStartState());
            SortedSet<char> alphabetSorted = automata.sortAutomataAlphabet();
            HashSet<char> alphabetS = new HashSet<char>(alphabetSorted);
            //resultAutomat.alphabet = alphabetS;
            resultAutomat.setAlphabet(alphabetS);
            //resultAutomat.StartState = "a0";
            resultAutomat.setStartState("a0");
            //resultAutomat.states.Add("a0");
            resultAutomat.addState("a0");
            int counter = 0;

            while (queue.Count > 0)
            {
                string temp = queue.Dequeue();
                string newStateName = "a" + counter;
                if (newStateName != "a0")
                {
                    //resultAutomat.states.Add(newStateName);
                    resultAutomat.addState(newStateName);
                }
                originalSet.Add(temp);
                if (automata.getFinalStates().Contains(temp))
                {
                    resultAutomat.setFinalState(newStateName);
                    //resultAutomat.finalStates.Add(newStateName);
                }
                counter++;
                resultSet.Add(newStateName);
                foreach (char symbol in alphabetSorted)
                {
                    /*if (!queue.Contains(automata.delta[(temp, symbol)]) && !originalSet.Contains(automata.delta[(temp, symbol)]))
                        queue.Enqueue(automata.delta[(temp, symbol)]);*/
                    if (!queue.Contains(automata.getDelta()[(temp, symbol)]) && !originalSet.Contains(automata.getElementBasedOnDelta(temp, symbol)))
                        queue.Enqueue(automata.getDelta()[(temp, symbol)]);
                }
            }

            for (int i = 0; i < resultSet.Count; i++)
            {
                foreach (var symbol in alphabetSorted)
                {
                    int j;
                    string tempState = automata.getDelta()[(originalSet.ElementAt(i), symbol)];//delta[(originalSet.ElementAt(i), symbol)];
                    for (j = 0; j < originalSet.Count; j++)
                    {
                        if (tempState == originalSet.ElementAt(j))
                        {
                            break;
                        }
                    }
                    //resultAutomat.delta[(resultSet.ElementAt(i), symbol)] = resultSet.ElementAt(j);
                    resultAutomat.setDelta(resultSet.ElementAt(i), symbol, resultSet.ElementAt(j));
                }
            }


            return resultAutomat;
        }

        //FOR ANY SYMBOL
        //IF TWO STATES GO TO A STATE THAT'S ALREADY PRESENT IN MATRIX
        //ADD IT
        //AS WE LEARNED!
        public bool minimiseAutomataHelper(Automat a)
        {
            bool checkIfThereIsNone = false;
            for (int i = 0; i < a.getStates().Count; i++)
            {
                for (int j = 0; j < a.getStates().Count; j++)
                {
                    if (ms[i, j] != -1 && ms[i, j] != 1)
                    {
                        foreach (var symbol in a.getAlphabet())
                        {
                            //ADDED FIRST ROW
                            //if (a.delta.ContainsKey((nodes.ElementAt(i), symbol)) && a.delta.ContainsKey((nodes.ElementAt(j), symbol)) &&
                                //nodes.Contains(a.delta[(nodes.ElementAt(i), symbol)]) && nodes.Contains(a.delta[(nodes.ElementAt(j), symbol)]))
                            if(a.getDelta().ContainsKey((nodes.ElementAt(i), symbol)) && a.getDelta().ContainsKey((nodes.ElementAt(j), symbol)) &&
                                nodes.Contains(a.getDelta()[(nodes.ElementAt(i), symbol)]) && nodes.Contains(a.getDelta()[(nodes.ElementAt(j), symbol)]))
                            {
                                //string firstState = a.delta[(nodes.ElementAt(i), symbol)];
                                //string secondState = a.delta[(nodes.ElementAt(j), symbol)];
                                string firstState = a.getDelta()[(nodes.ElementAt(i), symbol)];
                                string secondState = a.getDelta()[(nodes.ElementAt(j), symbol)];
                                int position1, position2;
                                for (position1 = 0; position1 < a.getStates().Count; position1++)
                                {
                                    if (nodes.ElementAt(position1) == firstState)
                                    {
                                        break;
                                    }
                                }
                                for (position2 = 0; position2 < a.getStates().Count; position2++)
                                {
                                    if (nodes.ElementAt(position2) == secondState)
                                    {
                                        break;
                                    }
                                }
                                if (ms[position1, position2] == 1 || ms[position2, position1] == 1)
                                {
                                    //Console.WriteLine(i + " " + j);
                                    checkIfThereIsNone = true;
                                    ms[i, j] = 1;
                                }
                            }
                        }
                    }
                }
            }
            return checkIfThereIsNone;
        }
    }
}
