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
        public Dictionary<(string, char), string> delta = new();
        private Dictionary<(string, char), List<string>> deltaForEpsilon = new();
        public HashSet<string> finalStates = new();
        public string StartState { get; set; }
        public HashSet<char> alphabet = new();
        public HashSet<string> states = new();
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
                    Automat ENKATODKA = this.convertENKAtoDKA();
                    if(ENKATODKA.AcceptsDKA(inputWord))
                    {
                        Console.WriteLine("ENKA accepted this word ;)!");
                    }
                    else
                    {
                        Console.WriteLine("ENKA didn't accept this word :(!");
                    }
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
                                helpForConnectingAndStuff(result, state, symbol);
                            }
                        }
                        else
                        {
                            result.ESwitching(state, 'E', other.StartState);
                        }
                    }
                }
                foreach(var state in other.states)
                {
                    foreach(var symbol in other.alphabet)
                    {
                        helpForConnectingAndStuff(result, state, symbol);                       
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        private void helpForConnectingAndStuff(Automat result, string state, char symbol)
        {
            if (this.checkIfIsENKA())
            {
                if (deltaForEpsilon.ContainsKey((state, symbol)))
                {
                    foreach (var s in deltaForEpsilon[(state, symbol)])
                    {
                        result.ESwitching(state, symbol, s);
                    }

                }
            }
            else
            {
                if (delta.ContainsKey((state, symbol)))
                {
                    result.ESwitching(state, symbol, delta[(state, symbol)]);
                }
            }
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
                foreach(var symbol in this.alphabet)
                {
                    helpForConnectingAndStuff(result, state, symbol);                   
                }              
            }

            result.ESwitching(result.StartState, 'E', result.finalStates.ElementAt(0));
            result.ESwitching(result.StartState, 'E', StartState);
        
            foreach(var finalState in this.finalStates)
            {
                result.ESwitching(finalState, 'E', "NFS");
                result.ESwitching(finalState, 'E', StartState);
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

        public bool AcceptsDKA(string input)
        {
            var currentState = StartState;
            foreach (var symbol in input)
            {
                currentState = delta[(currentState, symbol)];
            }
            return finalStates.Contains(currentState);
        }

        //DOBRA
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
        
        private bool checkIfStateExists(string state)
        {
            return states.Contains(state);
        }

        private bool checkIfSymbolIsInAlphabet(char symbol)
        {
            return alphabet.Contains(symbol);
        }
        
        private void addTransitions()
        {
            printStatesAndAlphabet();
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
                    
                    if(inputHelp != "--exit" && checkIfSymbolIsInAlphabet(char.Parse(inputHelp)))
                    {
                        symbol = char.Parse(inputHelp);
                        Console.WriteLine("Enter destination state: ");
                        nextState = Console.ReadLine();
                        
                        if(checkIfStateExists(nextState))
                        {
                            if (!checkIfIsENKA())
                            {
                                addTransitionDKA(state, symbol, nextState);
                            }
                            else
                            {
                                helpMethodForESwitching();
                                ESwitching(state, symbol, nextState);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Destination state doesn't exists, couldn't do what you wanted, sorry!");
                        }
                        
                    }
                    else if(inputHelp != "--exit")
                    {
                        Console.WriteLine("That symbol doesn't exist in this alphabet!");
                    }
                } while (inputHelp != "--exit");            
            }
        }

        private int ESwitchingHelper(char symbol)
        {
            for(int i = 0; i < alphabet.Count; i++)
            {
                if(symbol == alphabet.ElementAt(i))
                {
                    return i;
                }
            }
            return -1;
        }

        //HOW IT SHOULD GO!
        private void ESwitching(string currentState, char symbol, string nextState)
        {
            helpMethodForESwitching();

            int i;
            string[] array = states.ToArray<string>();
            int helpCounter = 0;
            for (i = 0; i < states.Count; i++)
            {
                if (currentState == array[i])
                {
                    if (symbol == 'E')
                    {
                        deltaForEpsilon[(currentState, 'E')] = listsOfStringsENKA[i];
                    }
                    else
                    {
                        for (int j = helpCounter; j < helpCounter + howManyDifferentSymbolsInAlphabet(); j += howManyDifferentSymbolsInAlphabet())
                        {
                            foreach (var thing in alphabet)
                            {
                                if (!thing.Equals('E') && thing.Equals(symbol))
                                {
                                    deltaForEpsilon[(currentState, thing)] = listOfStringsOtherStates[j + (ESwitchingHelper(symbol) - 1)];
                                    break;
                                }
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
                if (!deltaForEpsilon[(currentState, symbol)].Contains(nextState))
                    deltaForEpsilon[(currentState, symbol)].Add(nextState);
            }
        }

        private void helpMethodForESwitching()
        {
            if (counter == 0)
            {
                listsOfStringsENKA = new List<string>[states.Count];
                listOfStringsOtherStates = new List<string>[states.Count * (alphabet.Count - 1)];
                for (int j = 0; j < states.Count; j++)
                {
                    listsOfStringsENKA[j] = new List<string>();
                }
                for (int j = 0; j < states.Count * (alphabet.Count - 1); j++)
                {
                    listOfStringsOtherStates[j] = new();
                }
                counter++;
            }
        }

        //TEST FUNKCIJA, RADI
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

        public void printStatesAndAlphabet()
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

        //NIJE DOVOLJNO DOBRO
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
                    helpMethodForConversion(DKA, automatGraph, state, getDFStraversal);
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
                    helpMethodForConversion(DKA, automatGraph, state, dfsTraversalHelper);
                }
             
            }

            return DKA;
        }

        private void helpMethodForConversion(Automat DKA, AutomatGraph automatGraph, string state, SortedSet<string> getDFStraversal)
        {
            foreach (var symbol in alphabet)
            {
                bool isFinalState = false;
                string temp = "";
                int helpCounter = 0;
                if (symbol != 'E')
                {
                    foreach (var stateVisited in getDFStraversal)
                    {
                        //string goToState = delta[(stateVisited, symbol)];
                        List<string> goToStates = deltaForEpsilon[(stateVisited, symbol)];
                        foreach (var goToState in goToStates)
                        {
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

        public string getStartState()
        {
            return StartState;
        }

        public HashSet<char> getAlphabet()
        {
            return alphabet;
        }

        public SortedSet<char> sortAutomataAlphabet()
        {
            SortedSet<char> sortedSet = new SortedSet<char>(alphabet);
            return sortedSet;
        }

        public bool compareTwoAutomatas(Automat other)
        {
            Automat convertFirst = new();
            Automat convertSecond = new();
            Automat first;
            Automat second;
            if(this.checkIfIsENKA())
            {
                convertFirst = this.convertENKAtoDKA();
            }
            if(other.checkIfIsENKA())
            {
                convertSecond = other.convertENKAtoDKA();
            }

            AutomatGraph graph = new();

            if(!checkIfIsENKA())
            {
                first = graph.bfsTraversal(this);
            }
            else
            {
                first = graph.bfsTraversal(convertFirst);
            }

            if(!other.checkIfIsENKA())
            {
                second = graph.bfsTraversal(other);
            }
            else
            {
                second = graph.bfsTraversal(convertSecond);
            }
            //first.printStatesAndAlphabet();
            //second.printStatesAndAlphabet();

            if(first.states.Count != second.states.Count)
            {
                return false;
            }

            foreach(var state in first.states)
            {
                foreach(var symbol in first.alphabet)
                {
                    Console.WriteLine(first.delta[(state, symbol)] + " " + second.delta[(state, symbol)]);
                    if(first.delta[(state, symbol)] != second.delta[(state, symbol)])
                    {
                        return false;
                    }
                }
            }

            if(first.finalStates.Count != second.finalStates.Count)
            {
                return false;
            }

            foreach(var finalState in first.finalStates)
            {
                if(!second.finalStates.Contains(finalState))
                {
                    return false;
                }
            }

            Console.WriteLine("Isti!");
            return true;
        }
    }

}
