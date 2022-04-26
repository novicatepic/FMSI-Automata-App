using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projektni_FMSI;

namespace Projektni_FMSI
{
    public class Automat
    {
        private Dictionary<(string, char), string> delta = new();
        private Dictionary<(string, char), List<string>> deltaForEpsilon = new();
        private HashSet<string> finalStates = new();
        private string StartState { get; set; }
        private HashSet<char> alphabet = new();
        private HashSet<string> states = new();
        private List<string>[] listsOfStringsENKA;
        private List<string>[] listOfStringsOtherStates;
        
        private static int counter = 0;

        public void makeAutomata()
        {
            enterAlphabet();
            addStates();
            addTransitions();
        }

        public void runAutomata()
        {
            string inputWord;
            Console.WriteLine("Enter a word, so that DKA/E-NKA can check if it is valid or not: ");
            inputWord = Console.ReadLine();
            try
            {
                if(!checkIfInputWordIsCorrect(inputWord))
                {
                    throw new Exception("Incorrect input word!");
                }
                if (!checkIfIsENKA())
                {
                    if (AcceptsDKA(inputWord))
                    {
                        Console.WriteLine("DKA accepted this word ;)!");
                    }
                    else
                    {
                        Console.WriteLine("DKA didn't accept this word :(!");
                    }
                } 
                else
                {
                    //PRETVORITI PRAVILNO U DKA PA IZVRSITI E-NKA
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public Automat findUnion(Automat other)
        {
            Automat result = new Automat();
            foreach (var state1 in this.states)
            {
                foreach (var state2 in other.states)
                {
                    string newState = state1 + state2;
                    result.states.Add(newState);

                    if (this.finalStates.Contains(state1) || other.finalStates.Contains(state2))
                    {
                        result.finalStates.Add(newState);
                    }

                    foreach (var symbol in alphabet)
                    {
                        result.delta[(newState, symbol)] = this.delta[(state1, symbol)] + other.delta[(state2, symbol)];
                    }

                }
            }

            return result;
        }

        public Automat findIntersection(Automat other)
        {
            Automat result = new Automat();
            foreach (var state1 in this.states)
            {
                foreach (var state2 in other.states)
                {
                    string newState = state1 + state2;
                    result.states.Add(newState);

                    if (this.finalStates.Contains(state1) && other.finalStates.Contains(state2))
                    {
                        result.finalStates.Add(newState);
                    }

                    foreach (var symbol in alphabet)
                    {
                        result.delta[(newState, symbol)] = this.delta[(state1, symbol)] + other.delta[(state2, symbol)];
                    }

                }
            }

            return result;
        }

        public Automat findDifference(Automat other)
        {
            Automat result = new Automat();
            foreach (var state1 in this.states)
            {
                foreach (var state2 in other.states)
                {
                    string newState = state1 + state2;
                    result.states.Add(newState);

                    if ((this.finalStates.Contains(state1) && !other.finalStates.Contains(state2)) || (!this.finalStates.Contains(state1) && other.finalStates.Contains(state2)))
                    {
                        result.finalStates.Add(newState);
                    }

                    foreach (var symbol in alphabet)
                    {
                        result.delta[(newState, symbol)] = this.delta[(state1, symbol)] + other.delta[(state2, symbol)];
                    }

                }
            }

            return result;
        }

        public Automat connectLanguages(Automat other)
        {
            Automat result = new();

            try
            {
                if(!checkIfAlphabetIsTheSame(other))
                {
                    throw new Exception("I can't merge two languages that don't have the same alphabet, sorry!");
                }
                result.StartState = this.StartState;
                result.alphabet = alphabet;
                foreach(var finalState in other.finalStates)
                {
                    result.finalStates.Add(finalState);
                }
                foreach(var state in this.states)
                {
                    result.states.Add(state);
                }
                foreach(var state in other.states)
                {
                    result.states.Add(state);
                }
                if(!result.alphabet.Contains('E'))
                {
                    result.alphabet.Add('E');
                }
                foreach(var state in this.states)
                {
                    foreach(var symbol in this.alphabet)
                    {
                        if(!this.finalStates.Contains(state))
                        {
                            if(symbol != 'E')
                            {
                                result.addTransitionENKA(state, symbol, delta[(state, symbol)]);
                            }                         
                        }
                        else
                        {
                            result.addTransitionENKA(state, 'E', other.StartState);
                        }
                    }
                }
                foreach(var state in other.states)
                {
                    foreach(var symbol in other.alphabet)
                    {
                        result.addTransitionENKA(state, symbol, other.delta[(state, symbol)]);
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        //PROBLEM SA FUNKCIJOM PRELAZA, KAD SLIKA IZ EPSILON U VISE STANJA, UVIJEK UZME ONO ZADNJE!
        //NAPRAVIO DA RADI, ISPROVJERAVATI!
        public Automat applyKleeneStar()
        {
            Automat result = new();

            result.StartState = "NSS"; //new start state
            result.finalStates.Add("NFS"); //new final state
            result.states.Add(result.StartState);
            result.states.Add("NFS");
            result.alphabet = alphabet;
            foreach(var state in this.states)
            {
                result.states.Add(state);
            }
            if(!result.alphabet.Contains('E'))
            {
                result.alphabet.Add('E');
            }
            
            foreach(var state in this.states)
            {
                foreach(var symbol in alphabet)
                {
                    if(symbol != 'E')
                    {
                        result.addTransitionENKA(state, symbol, this.delta[(state, symbol)]);
                    }
                     
                }              
            }

            result.addTransitionENKA(result.StartState, 'E', result.finalStates.ElementAt(0));
            result.addTransitionENKA(result.StartState, 'E', StartState);
        
            foreach(var finalState in this.finalStates)
            {
                result.addTransitionENKA(finalState, 'E', "NFS");
                result.addTransitionENKA(finalState, 'E', StartState);;
            }

            return result;
        }

        private bool checkIfAlphabetIsTheSame(Automat other)
        {
            if(this.alphabet.Count != other.alphabet.Count)
            {
                return false;
            }

            foreach(var symbol in this.alphabet)
            {
                if(!other.alphabet.Contains(symbol))
                {
                    return false;
                }
            }

            return true;
        }

        private bool checkIfInputWordIsCorrect(string word)
        {
            foreach(var symbol in word)
            {
                if(!alphabet.Contains(symbol))
                {
                    return false;
                }
            }
            return true;
        }

        private bool checkIfIsENKA()
        {
            return alphabet.Contains('E');
        }

        private void addTransitionDKA(string currentState, char symbol, string nextState)
        {
            try
            {
                if (!states.Contains(currentState) && !states.Contains(nextState) && !alphabet.Contains(symbol))
                {
                    throw new Exception("Invalid input!");
                }
                delta[(currentState, symbol)] = nextState;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void addFinalState(string state)
        {
            finalStates.Add(state);
        }

        public bool AcceptsDKA(string input)
        {
            var currentState = StartState;
            foreach (var symbol in input)
            {
                currentState = delta[(currentState, symbol)];
            }
            return finalStates.Contains(currentState);
        }

        public Automat constructComplement()
        {
            Automat result = new();
            result = this;
            result.finalStates.Clear();
            foreach(var state in states)
            {
                if(!this.finalStates.Contains(state))
                {
                    result.finalStates.Add(state);
                }
            }
            return result;
        }

        private void enterAlphabet()
        {
            Console.WriteLine("--exit to exit input loop!\nEnter your alphabet (if you enter E as an symbol, it's E-NKA): ");
            string inputString;
            do
            {
                inputString = Console.ReadLine();
                if(inputString != "--exit")
                {
                    char symbol = char.Parse(inputString);
                    alphabet.Add(symbol);
                }
            } while (inputString != "--exit");
        }

        private void addStates()
        {
            Console.Write("NOTE: First state you add is the initial state!");
            Console.WriteLine("--exit to exit input loop!\nEnter states: ");
            string state;
            bool isFirst = true;
            do
            {
                state = Console.ReadLine();
                if (isFirst)
                {
                    StartState = state;
                    isFirst = false;
                }
                if(state != "--exit")
                {
                    states.Add(state);
                    string extraInput;
                    Console.WriteLine("Do you want this state to be a final state? (yes/no): ");
                    extraInput = Console.ReadLine();
                    if(extraInput == "yes")
                    {
                        finalStates.Add(state);
                    }
                    else if(extraInput == "no") { }
                    else
                    {
                        Console.WriteLine("Incorrent word, I'll take that as a no!");
                    }
                    
                }
            } while (state != "--exit");

        }

        private void addTransitions()
        {
            listStatesAndAlphabet();
            foreach(var state in states)
            {
                char symbol;
                string inputHelp;
                string nextState;
                Console.WriteLine("State: " + state);
                do
                {
                    Console.WriteLine("Enter --exit to stop entering transitions for current state or enter symbol from alphabet for transition: ");
                    inputHelp = Console.ReadLine();
                    if(inputHelp != "--exit")
                    {
                        symbol = char.Parse(inputHelp);
                        Console.WriteLine("Enter destination state: ");
                        nextState = Console.ReadLine();
                        if(!checkIfIsENKA())
                        {
                            addTransitionDKA(state, symbol, nextState);
                        }
                        else
                        {
                            if (counter == 0) {
                                listsOfStringsENKA = new List<string>[states.Count];
                                listOfStringsOtherStates = new List<string>[states.Count * (alphabet.Count - 1)];
                                for(int i = 0; i < states.Count; i++)
                                {
                                    listsOfStringsENKA[i] = new List<string>();
                                }
                                for(int i = 0; i < states.Count * (alphabet.Count - 1); i++)
                                {
                                    listOfStringsOtherStates[i] = new List<string>();
                                }
                                counter++;
                            }
                            //RADI
                            //addTransitionENKA(state, symbol, nextState);
                            //RADI!!!
                            testFuncForESwitching(state, symbol, nextState);
                        }
                    }
                } while (inputHelp != "--exit");            
            }
        }

        private void addTransitionENKA(string currentState, char symbol, string nextState)
        {
            if (counter == 0)
            {
                listsOfStringsENKA = new List<string>[states.Count];
                for (int j = 0; j < states.Count; j++)
                {
                    listsOfStringsENKA[j] = new List<string>();
                }
                counter++;
            }

            int i;
            string[] array = states.ToArray<string>();
            for(i = 0; i < states.Count; i++)
            {
                if(currentState == array[i]) {
                    deltaForEpsilon[(currentState, symbol)] = listsOfStringsENKA[i];
                    break;
                }
            }

            if(symbol == 'E')
            {
                if(!deltaForEpsilon[(currentState, symbol)].Contains(nextState))
                    deltaForEpsilon[(currentState, symbol)].Add(nextState);
            }
            else
            {
                //POTREBNO IMPLEMENTIRATI DODATNU LOGIKU, I IZMIJENITI ODREDJENA MJESTA U KODU!
                delta[(currentState, symbol)] = nextState;
            }
        }

        //HOW IT SHOULD GO!
        private void testFuncForESwitching(string currentState, char symbol, string nextState)
        {
            if (counter == 0)
            {
                listsOfStringsENKA = new List<string>[states.Count];
                listOfStringsOtherStates = new List<string>[states.Count * (alphabet.Count - 1)];
                for (int j = 0; j < states.Count; j++)
                {
                    listsOfStringsENKA[j] = new List<string>();
                }
                for(int j = 0; j < states.Count * (alphabet.Count - 1); j++)
                {
                    listOfStringsOtherStates[j] = new();
                }
                counter++;
            }

            int i;
            string[] array = states.ToArray<string>();
            int helpCounter = 0;
            for (i = 0; i < states.Count; i++)
            {
                if (currentState == array[i])
                {
                    deltaForEpsilon[(currentState, 'E')] = listsOfStringsENKA[i];
                    for(int j = helpCounter; j < helpCounter + howManyDifferentSymbolsInAlphabet(); j++)
                    {
                        foreach(var thing in alphabet)
                        {
                            if(thing != 'E')
                            {
                                deltaForEpsilon[(currentState, thing)] = listOfStringsOtherStates[j];
                            }
                        }
                    }
                    break;
                }
                helpCounter += howManyDifferentSymbolsInAlphabet();
            }

            if (symbol == 'E')
            {
                if (!deltaForEpsilon[(currentState, symbol)].Contains(nextState))
                    deltaForEpsilon[(currentState, symbol)].Add(nextState);
            }
            else
            {
                //STARA IMPLEMENTACIJA
                //delta[(currentState, symbol)] = nextState;
                if(!deltaForEpsilon[(currentState, symbol)].Contains(nextState))
                    deltaForEpsilon[(currentState, symbol)].Add(nextState);
            }
        }

        public void printENKAEverything()
        {
            Console.WriteLine("E prelazi: ");
            foreach(var state in states)
            {
                if(deltaForEpsilon.ContainsKey((state, 'E')))
                {
                    foreach(var s in deltaForEpsilon[(state, 'E')])
                    {
                        Console.Write(s + " ");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine("Ostali prelazi: ");
            foreach(var state in states)
            {
                foreach(var symbol in alphabet)
                {
                    if(symbol != 'E')
                    {
                        if(deltaForEpsilon.ContainsKey((state, symbol)))
                        {
                            foreach(var s in deltaForEpsilon[(state, symbol)])
                            {
                                Console.Write(state + "+" + symbol + "->" + s);
                            }
                        }
                    }
                }
                Console.WriteLine();
            }
        }

        private int howManyDifferentSymbolsInAlphabet()
        {
            int numOfSymbols = 0;
            foreach(var symbol in alphabet)
            {
                if(symbol != 'E')
                {
                    numOfSymbols++;
                }
            }
            return numOfSymbols;
        }

        private void listStatesAndAlphabet()
        {
            Console.WriteLine("STATES: ");
            foreach(var state in states)
            {
                Console.Write(state + " ");
            }
            Console.WriteLine();
            Console.WriteLine("ALPHABET: ");
            foreach(var symbol in alphabet)
            {
                Console.Write(symbol + " ");
            }
            Console.WriteLine();
        }

        public void printStates()
        {
            Console.WriteLine("STATES: ");
            foreach(var state in states)
            {
                Console.Write(state + " ");
                
            }
            Console.WriteLine();
            /*Console.WriteLine("E STUFF: ");
            for(int i = 0; i < states.Count; i++)
            {
                foreach(var s in listsOfStrings[i])
                {
                    Console.Write(s + " ");
                }
                Console.WriteLine();
            }*/
        }

        public bool isLanguageFinal()
        {
            bool result = true;
            foreach(var finalState in finalStates)
            {
                foreach(var symbol in alphabet)
                {
                    if(delta[(finalState, symbol)] == finalState)
                    {
                        return false;
                    }
                }
            }
            return result;
        }

        public int findShortestPath()
        {
            int shortestPathLength = 0;

            if(finalStates.Contains(StartState))
            {
                return shortestPathLength;
            }

            Queue<string> queue = new();
            queue.Enqueue(StartState);

            while(queue.Count > 0)
            {
                string temp = queue.Dequeue();
                foreach(char symbol in alphabet)
                {
                    if(delta[(temp, symbol)] != temp)
                    {
                        string nextState = delta[(temp, symbol)];
                        if(finalStates.Contains(nextState))
                        {
                            shortestPathLength++;
                            return shortestPathLength;
                        }
                        else
                        {
                            queue.Enqueue(nextState);
                        }
                    }                 
                }
                shortestPathLength++;
            }

            return -1;
        }

        //TEST FUNCTION
        public void callGraph()
        {
            AutomatGraph automatGraph = new(states.Count);

            int[,] ms = new int[states.Count, states.Count];
            string[] nodes = new string[states.Count];

            int i = 0, j = 0;
            foreach(var state in states)
            {
                nodes[i++] = state;
            }
            i = 0;
            for(i = 0; i < states.Count; i++)
            {
                for(j = 0; j < states.Count; j++)
                {
                    if(deltaForEpsilon.ContainsKey((nodes[i], 'E')) && deltaForEpsilon[(nodes[i], 'E')].Contains(nodes[j]))
                    {
                        ms[i, j] = 1;
                    }
                    else
                    {
                        ms[i, j] = 0;
                    }
                }
            }
            automatGraph.ms = ms;
            automatGraph.nodes = nodes;

            //RADI
            /*SortedSet<string> dfsSet = automatGraph.dfs("q1");
            foreach(var state in dfsSet)
            {
                Console.Write(state + " ");
            }*/

        }

        public Automat convertENKAtoDKA()
        {
            Automat DKA = new();

            AutomatGraph automatGraph = new(states.Count);

            int[,] ms = new int[states.Count, states.Count];
            string[] nodes = new string[states.Count];

            int i = 0, j = 0;
            foreach (var state in states)
            {
                nodes[i++] = state;
            }
            i = 0;
            for (i = 0; i < states.Count; i++)
            {
                for (j = 0; j < states.Count; j++)
                {
                    if (deltaForEpsilon.ContainsKey((nodes[i], 'E')) && deltaForEpsilon[(nodes[i], 'E')].Contains(nodes[j]))
                    {
                        ms[i, j] = 1;
                    }
                    else
                    {
                        ms[i, j] = 0;
                    }
                }
            }
            automatGraph.ms = ms;
            automatGraph.nodes = nodes;

            for (int a = 0; a < states.Count; a++, Console.WriteLine())
            {
                for (int b = 0; b < states.Count; b++)
                {
                    Console.Write(ms[a, b] + " ");
                }
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            DKA.StartState = this.StartState;
            DKA.states.Add(StartState);

            for (int g = 0; g < DKA.states.Count; g++)
            {
                var state = DKA.states.ElementAt(g);

                SortedSet<string> getDFStraversal = new();

                string[] splitStates = DKA.states.ElementAt(g).Split(':');

                if(splitStates.Length == 1)
                {
                    getDFStraversal = automatGraph.dfs(state);
                    foreach (var symbol in alphabet)
                    {
                        bool isFinalState = false;
                        string temp = "";
                        int helpCounter = 0;
                        if (symbol != 'E')
                        {
                            foreach (var stateVisited in getDFStraversal)
                            {
                                string goToState = delta[(stateVisited, symbol)];
                                SortedSet<string> tempTraversal = automatGraph.dfs(goToState);
                                foreach (var finalConnection in tempTraversal)
                                {
                                    if (finalStates.Contains(finalConnection))
                                    {
                                        isFinalState = true;
                                    }
                                    if (!temp.Contains(finalConnection))
                                    {
                                        helpCounter++;
                                        if (helpCounter > 1)
                                        {
                                            temp += ":";
                                            temp += finalConnection;
                                        }
                                        else
                                        {
                                            temp += finalConnection;
                                        }
                                    }
                                }
                            }
                        }
                        if (isFinalState)
                        {
                            DKA.finalStates.Add(temp);
                        }
                        if (temp != "")
                        {
                            DKA.states.Add(temp);
                            DKA.delta[(state, symbol)] = temp;
                        }
                    }
                }
                else
                {
                    SortedSet<string> dfsTraversalHelper = new();
                    foreach(var s in splitStates)
                    {
                        getDFStraversal = automatGraph.dfs(s);
                        foreach(var elem in getDFStraversal)
                        {
                            dfsTraversalHelper.Add(elem);
                        }
                    }
                    foreach (var symbol in alphabet)
                    {
                        bool isFinalState = false;
                        string temp = "";
                        int helpCounter = 0;
                        if (symbol != 'E')
                        {
                            foreach (var stateVisited in dfsTraversalHelper)
                            {
                                string goToState = delta[(stateVisited, symbol)];
                                Console.WriteLine("goToState: " + goToState);
                                SortedSet<string> tempTraversal = automatGraph.dfs(goToState);
                                foreach (var finalConnection in tempTraversal)
                                {
                                    if (finalStates.Contains(finalConnection))
                                    {
                                        isFinalState = true;
                                    }
                                    if (!temp.Contains(finalConnection))
                                    {
                                        helpCounter++;
                                        if (helpCounter > 1)
                                        {
                                            temp += ":";
                                            temp += finalConnection;
                                        }
                                        else
                                        {
                                            temp += finalConnection;
                                        }
                                    }
                                }
                            }
                        }
                        if (isFinalState)
                        {
                            DKA.finalStates.Add(temp);
                        }
                        if (temp != "")
                        {
                            if(!DKA.states.Contains(temp))
                            {
                                DKA.states.Add(temp);
                                
                            }
                            DKA.delta[(state, symbol)] = temp;
                        }
                    }
                }
             
            }

            return DKA;
        }

    }

}
