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
        //bool connectFlag = false;
        static int counter2 = 0;
        string id = "_";
        private static int counter = 0;

        public Automat()
        {
            id += counter2.ToString();
            counter2++;
        }

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
                if (!result.alphabet.Contains('E'))
                {
                    result.alphabet.Add('E');
                }
                foreach(var symbol in this.alphabet)
                {
                    result.alphabet.Add(symbol);
                }
               
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
                            foreach(var symb in alphabet)
                            {
                                if(symb != 'E')
                                {
                                    helpForConnectingAndStuff(result, state, symbol);
                                }
                            }
                        }
                    }
                }

                foreach(var state in other.states)
                {
                    foreach(var symbol in other.alphabet)
                    {
                        other.helpForConnectingAndStuff(result, state, symbol);                       
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

        public static Automat chainOperations()
        {
            Automat result = new();

            Console.WriteLine("Options for chaining operations:\n1-Union\n2-Intersection\n3-Difference\n4-Complement\n5-Connection\n6-KleeneStar\n");
            string input;
            Console.WriteLine("Enter your options until it becomes boring for you, if it becomes boring, input --exit: ");

            Automat a1 = new Automat();
            a1.makeAutomata();
            Automat a2 = new();
            a2.makeAutomata();
            Automat res = null;

            do
            {
                input = Console.ReadLine();
                if (input == "1")
                {
                    if(res == null)
                        res = a1.findUnion(a2);
                    else
                    {
                        a1 = null;
                        a1 = new();
                        res = res.findUnion(a1);
                    } 
                        
                }
                else if(input == "2")
                {
                    if (res == null)
                        res = a1.findIntersection(a2);
                    else
                    {
                        a1 = null;
                        a1 = new();
                        res = res.findIntersection(a1);
                    }
                }
                else if(input == "3")
                {
                    if (res == null)
                        res = a1.findDifference(a2);
                    else
                    {
                        a1 = null;
                        a1 = new();
                        res = res.findDifference(a1);
                    }
                }
                else if(input == "4")
                {
                    if (res == null)
                        res = a1.constructComplement();
                    else
                        res = res.constructComplement();
                }
                else if(input == "5")
                {
                    if(res == null)
                        res = a1.connectLanguages(a2);
                    else
                    {
                        a1 = null;
                        a1 = new();
                        res = res.connectLanguages(a1);
                    }
                }
                else if(input == "6")
                {
                    if(res == null)
                        res = a1.applyKleeneStar();
                    else
                    {
                        res = res.applyKleeneStar();
                    }
                }
                else if(input == "--exit")
                {

                }
                else
                {
                    Console.WriteLine("Incorrent entry, try again!");
                }
            } while (input != "--exit");

            return result;
        }

        public Automat applyKleeneStar()
        {
            Automat result = new();

            result.StartState = "NSS"; //new start state
            result.finalStates.Add("NFS"); //new final state
            result.states.Add(result.StartState);
            result.states.Add("NFS");

            if (!result.alphabet.Contains('E'))
            {
                result.alphabet.Add('E');
            }

            foreach(var symbol in alphabet)
            {
                result.alphabet.Add(symbol);
            }

            foreach(var state in this.states)
            {
                result.states.Add(state);
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
            string initState;
            bool isFirst = true;
            do
            {
                state = Console.ReadLine();
                initState = state;
                state += id;
                if (isFirst)
                {
                    StartState = state;
                    isFirst = false;
                }
                if(initState != "--exit")
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
            } while (initState != "--exit");

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
                int substring = state.IndexOf('_');
                Console.WriteLine("State: " + state.Substring(0, substring));
                do
                {
                    Console.WriteLine("Enter --exit to stop entering transitions for current state or enter symbol from alphabet for transition: ");
                    inputHelp = Console.ReadLine();
                    
                    if(inputHelp != "--exit" && checkIfSymbolIsInAlphabet(char.Parse(inputHelp)))
                    {
                        symbol = char.Parse(inputHelp);
                        Console.WriteLine("Enter destination state: ");
                        nextState = Console.ReadLine();

                        if(checkIfStateExists(nextState + id))
                        //if(checkIfStateExists(nextState))
                        {
                            if (!checkIfIsENKA())
                            {
                                addTransitionDKA(state, symbol, nextState + id);
                                //addTransitionDKA(state, symbol, nextState);
                            }
                            else
                            {
                                helpMethodForESwitching();
                                ESwitching(state, symbol, nextState + id);
                                //ESwitching(state, symbol, nextState);
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
                                    int pos = j + ESwitchingHelper(symbol) - 1;
                                    //Console.WriteLine(currentState + " + " + thing + "->" + pos);
                                    //Console.WriteLine(listOfStringsOtherStates.Length);
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
            
            //Console.WriteLine("State: " + state.Substring(0, substring));
            Console.WriteLine("STATES: ");
            foreach(var state in states)
            {
                int substring = state.IndexOf('_');
                Console.Write(state.Substring(0, substring) + " ");
            }
            Console.WriteLine();
            Console.WriteLine("ALPHABET: ");
            foreach(var symbol in alphabet)
            {
                Console.Write(symbol + " ");
            }
            Console.WriteLine();
            Console.WriteLine("DELTA TRANSITIONS: ");
            if(!checkIfIsENKA())
            {
                foreach (var symbol in alphabet)
                {
                    foreach (var state in states)
                    {
                        int substring = state.IndexOf('_');
                        if (delta.ContainsKey((state, symbol)))
                            Console.WriteLine(state.Substring(0, substring) + " + " + symbol + " -> " + delta[(state, symbol)].Substring(0, substring));
                    }
                }
            }
            else
            {
                foreach(var symbol in alphabet)
                {
                    foreach(var state in states)
                    {
                        int substring = state.IndexOf('_');
                        //deltaForEpsilon
                        if (deltaForEpsilon.ContainsKey((state, symbol)))
                        {
                            List<string> goTo = deltaForEpsilon[(state, symbol)];
                            foreach (var s in goTo)
                            {
                                Console.WriteLine(state + " + " + symbol + " -> " + s.Substring(0, substring));
                            }
                        }
                    }
                }
            }
            
        }

        public bool isLanguageFinal()
        {
            Automat convert = new();

            if(checkIfIsENKA())
            {
                convert = convertENKAtoDKA();    
            }
            else
            {
                convert = this;
            }

            if(convert.finalStates.Count == 0 || convert.finalStates.Count == convert.states.Count)
            {
                return false;
            }

            if(!isLanguageFinalHelpFunc())
            {
                return false;
            }

            AutomatGraph automatGraph = new(convert.states.Count);
            convert.makeGraphFromAutomataWithAllConnections(automatGraph);

            foreach(var finalState in convert.finalStates)
            {
                if (automatGraph.dfsForLongestWord(finalState))
                {
                    return false;
                }

            }


            return true;
        }

        private bool isLanguageFinalHelpFunc()
        {
            bool result = true;
            foreach(var finalState in finalStates)
            {
                foreach(var symbol in alphabet)
                {
                    if(delta.ContainsKey((finalState, symbol)) && delta[(finalState, symbol)] == finalState)
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

            if(finalStates.Count == 0)
            {
                Console.WriteLine("There is no shortest path because there is no entry state, returning -1");
                return -1;
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
            makeGraphFromAutomataWithEClosures(automatGraph);
            DKA.StartState = this.StartState;
            DKA.states.Add(StartState);
            foreach(var symbol in alphabet)
            {
                if(symbol != 'E')
                {
                    DKA.alphabet.Add(symbol);
                }
            }

            for (int g = 0; g < DKA.states.Count; g++)
            {
                var state = DKA.states.ElementAt(g);

                SortedSet<string> getDFStraversal = new();

                string[] splitStates = DKA.states.ElementAt(g).Split(':');

                if (splitStates.Length == 1)
                {
                    getDFStraversal = automatGraph.dfs(state);
                    helpMethodForConversion(DKA, automatGraph, state, getDFStraversal);
                }
                else
                {
                    SortedSet<string> dfsTraversalHelper = new();
                    foreach (var s in splitStates)
                    {
                        getDFStraversal = automatGraph.dfs(s);
                        foreach (var elem in getDFStraversal)
                        {
                            dfsTraversalHelper.Add(elem);
                        }
                    }
                    helpMethodForConversion(DKA, automatGraph, state, dfsTraversalHelper);
                }

            }

            //DKA.minimiseAutomata();

            return DKA;
        }

        public int findLongestPath()
        {
            //int pathSize = 0;
            AutomatGraph automatGraph = new(states.Count);

            Automat possibleDKA = new();

            if(checkIfIsENKA())
            {
                possibleDKA = convertENKAtoDKA();
            }
            else
            {
                possibleDKA = this;
            }

            if(!isLanguageFinalHelpFunc())
            {
                Console.WriteLine("It's infinity, even thought here we don't have infinity, I'll just print out the biggest possible number");
                return int.MaxValue;
            }

            possibleDKA.makeGraphFromAutomataWithAllConnections(automatGraph);

            foreach(var finalState in possibleDKA.finalStates)
            {
                if(automatGraph.dfsForLongestWord(finalState))
                {
                    Console.WriteLine("It's infinity, even thought here we don't have infinity, I'll just print out the biggest possible number");
                    return int.MaxValue;
                }
            }

            if(possibleDKA.finalStates.Count == 0)
            {
                Console.WriteLine("There is no final state, returning -1!");
                return -1;
            }

            return automatGraph.bfsFindLongestWord(possibleDKA);

            //return pathSize;
        }

        private void makeGraphFromAutomataWithAllConnections(AutomatGraph automatGraph)
        {
            int[,] ms = new int[states.Count, states.Count];
            string[] nodes = new string[states.Count];
            int i = 0, j = 0;
            foreach (var state in states)
            {
                nodes[i++] = state;
            }
            i = 0;
            for(i = 0; i < states.Count; i++)
            {
                for(j = 0; j < states.Count; j++)
                {
                    bool doesContain = false;
                    foreach(var symbol in alphabet)
                    {
                        if(!checkIfIsENKA())
                        {
                            if(delta.ContainsKey((nodes[i], symbol)) && delta[(nodes[i], symbol)].Contains(nodes[j]))
                            {
                                ms[i, j] = 1;
                                doesContain = true;
                            }
                        }
                    }
                    if(!doesContain)
                    {
                        ms[i, j] = 0;
                    }
                }
            }

            automatGraph.ms = ms;
            automatGraph.nodes = nodes;

        }

        private void makeGraphFromAutomataWithEClosures(AutomatGraph automatGraph)
        {
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
        }

        private void helpMethodForConversion(Automat DKA, AutomatGraph automatGraph, string state, SortedSet<string> getDFStraversal)
        {
            foreach (var symbol in alphabet)
            {
                bool isFinalState = false;
                string temp = "";
                SortedSet<string> statesSorted = new();
                int helpCounter = 0;
                if (symbol != 'E')
                {
                    foreach (var stateVisited in getDFStraversal)
                    {
                        List<string> goToStates = new();

                        //string goToState = delta[(stateVisited, symbol)];
                        if(deltaForEpsilon.ContainsKey((stateVisited, symbol)))
                            goToStates = deltaForEpsilon[(stateVisited, symbol)];
                        //goToStates.Sort();
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
                                        statesSorted.Add(finalConnection);
                                    }
                                    else
                                    {
                                        statesSorted.Add(finalConnection);
                                    }
                                }
                            }
                        }

                    }
                }
                temp = "";
                if(statesSorted.Count > 1)
                {
                    int counter = 0;
                    foreach (var s in statesSorted) {
                        
                        temp += s;
                        counter++;
                        if (counter != statesSorted.Count)
                        {
                            temp += ":";
                        }

                    }
                }
                else
                {
                    if(statesSorted.Count == 1)
                        temp = statesSorted.ElementAt(0);
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

        public Automat minimiseAutomata()
        {
            Automat minimized = new();

            if (finalStates.Count == states.Count)
            {
                minimized.finalStates.Add("q0");
                helpForMinimization(minimized);
                return minimized;
            }

            if (finalStates.Count == 0)
            {
                helpForMinimization(minimized);
                return minimized;
            }

            SortedSet<string> getReachableStates = makeGraphForMinimization();
            this.states.Clear();
            foreach(var state in getReachableStates)
            {
                this.states.Add(state);
            }
            HashSet<string> newFinalStates = new();
            foreach(var state in finalStates)
            {
                if(this.states.Contains(state))
                {
                    newFinalStates.Add(state);
                }
            }
            finalStates.Clear();
            finalStates = newFinalStates;

            Console.WriteLine("STATES");
            foreach(var state in states)
            {
                Console.Write(state + " ");
            }
            Console.WriteLine();
            Console.WriteLine("FINALSTATES");
            foreach (var state in finalStates)
            {
                Console.Write(state + " ");
            }
            Console.WriteLine();

            int[,] ms = new int[states.Count, states.Count];
            string[] nodes = new string[states.Count];
            for (int i = 0; i < states.Count; i++)
            {
                nodes[i] = states.ElementAt(i);
                for (int j = 0; j < states.Count; j++)
                {
                    if (i <= j || i == 0 || j == states.Count - 1)
                    {
                        ms[i, j] = -1;
                    }
                    else
                    {
                        if ((finalStates.Contains(states.ElementAt(i)) && !finalStates.Contains(states.ElementAt(j))) ||
                            (!finalStates.Contains(states.ElementAt(i)) && finalStates.Contains(states.ElementAt(j))))
                        {
                            ms[i, j] = 1;
                        }
                        else
                        {
                            ms[i, j] = 0;
                        }
                    }
                }
            }
            AutomatGraph automatGraph = new(states.Count);
            automatGraph.ms = ms;
            automatGraph.nodes = nodes;
            while (automatGraph.minimiseAutomataHelper(this))
            {
                //Console.WriteLine("YAS");
            }

            HashSet<string> statesToMinimize = new();
            HashSet<string> fullMinimization = new();
            SortedSet<string> sortedMinimization = new();
            for (int i = 0; i < states.Count; i++)
            {
                for (int j = 0; j < states.Count; j++)
                {
                    string temp = "";
                    if (automatGraph.ms[i, j] == 0)
                    {
                        if(String.Compare(states.ElementAt(i), states.ElementAt(j)) < 0) 
                        {
                            temp = states.ElementAt(i) + ":" + states.ElementAt(j);
                        }
                        else
                        {
                            temp = states.ElementAt(j) + ":" + states.ElementAt(i);
                        }
                        statesToMinimize.Add(temp);
                    }
                    //Console.WriteLine(temp);
                }
            }

            Console.WriteLine("STATES TO MINIMIZE: ");
            foreach(var state in statesToMinimize)
            {
                Console.Write(state + " ");
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            minimized.alphabet = this.alphabet;


            for (int i = 0; i < statesToMinimize.Count; i++)
            {
                string temp = "";
                SortedSet<string> matches = new();
                string[] splitNewStates = statesToMinimize.ElementAt(i).Split(':');
                foreach (var splitState in splitNewStates)
                {
                    temp = "";
                    for (int j = 0; j < statesToMinimize.Count; j++)
                    {
                        if (j != i)
                        {

                            if (statesToMinimize.ElementAt(j).Contains(splitState))
                            {
                                //Console.WriteLine("YES");
                                foreach (var splSt in splitNewStates)
                                {
                                    matches.Add(splSt);
                                }
                                string[] splitMatches = statesToMinimize.ElementAt(j).Split(':');
                                foreach (var splitMatch in splitMatches)
                                {
                                    matches.Add(splitMatch);
                                }
                            }
                        }

                    }
                    foreach (var match in matches)
                    {
                        temp += match;
                        temp += ":";
                    }
                    if (matches.Count == 0)
                    {
                        Console.WriteLine(statesToMinimize.ElementAt(i));
                        fullMinimization.Add(statesToMinimize.ElementAt(i));
                    }
                    else
                    {
                        string temp2 = "";
                        for(int g = 0; g < temp.Length - 1; g++)
                        {
                            temp2 += temp[g];
                        }
                        //Console.WriteLine(temp2);
                        fullMinimization.Add(temp2);
                    }
                }
            }

            Console.WriteLine("States to fully minimize: ");
            foreach(var state in fullMinimization)
            {
                Console.Write(state + " ");
            }
            Console.WriteLine();

            bool flagForStartState = false;
            foreach(var state in fullMinimization)
            {
                if(state.Contains(StartState))
                {
                    flagForStartState = true;
                    minimized.StartState = state;
                }
            }
            if(!flagForStartState)
            {
                minimized.StartState = StartState;
            }

            bool flagForFinalState = false;
            foreach(var state in finalStates)
            {
                flagForFinalState = false;
                foreach (var s in fullMinimization)
                {
                    if(s.Contains(state))
                    {
                        flagForFinalState = true;
                        minimized.finalStates.Add(s);
                    }
                }
                if(!flagForFinalState)
                {
                    minimized.finalStates.Add(state);
                }
            }

            foreach (var state in states)
            {
                bool flag = false;
                for (int i = 0; i < fullMinimization.Count; i++)
                {
                    if (fullMinimization.ElementAt(i).Contains(state))
                    {
                        flag = true;
                    }
                }

                if (!flag && state != "")
                {
                    minimized.states.Add(state);
                }
            }
            foreach(var state in fullMinimization)
            {
                if(state != "")
                {
                    minimized.states.Add(state);
                }
            }

            foreach (var state in minimized.states)
            {
                if (!fullMinimization.Contains(state))
                {
                    string stateToGoTo = "";
                    bool flag = false;
                    foreach (var symbol in alphabet)
                    {
                        for (int i = 0; i < fullMinimization.Count; i++)
                        {
                            if (fullMinimization.ElementAt(i).Contains(delta[(state, symbol)]))
                            {
                                stateToGoTo = fullMinimization.ElementAt(i);
                                flag = true;
                                break;
                            }
                        }

                        if (!flag)
                        {
                            minimized.delta[(state, symbol)] = delta[(state, symbol)];
                        }
                        else
                        {
                            minimized.delta[(state, symbol)] = stateToGoTo;
                            flag = false;
                        }
                    }
                }
                else
                {
                    string[] splitStates = state.Split(':');
                    foreach (var symbol in alphabet)
                    {
                        bool flag = false;
                        foreach (var splitState in splitStates)
                        {
                            if (splitState != "" && state.Contains(delta[(splitState, symbol)]))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (flag)
                        {
                            //PRESLIKAVA SE SAM U SEBE
                            minimized.delta[(state, symbol)] = state;
                            flag = false;
                        }
                        else
                        {
                            //AKO SE NE PRESLIKAVA SAM U SEBE, MORA U SLIKANJE OD BILO KOJEG
                            string temp = delta[(splitStates[0], symbol)];
                            bool flag2 = false;
                            foreach(var st in fullMinimization)
                            {
                                flag2 = false;
                                if(st.Contains(temp))
                                {
                                    flag2 = true;
                                    minimized.delta[(state, symbol)] = st;
                                    break;
                                }
                            }
                            if(!flag2)
                            {
                                minimized.delta[(state, symbol)] = delta[(splitStates[0], symbol)];
                            }
                        }
                    }
                }
            }

            minimized.printStatesAndAlphabet();
            foreach(var state in minimized.states)
            {
                foreach(var symbol in alphabet)
                {
                    Console.WriteLine(state + "->" + symbol + " = " + minimized.delta[(state, symbol)]);
                }
            }

            return minimized;
        }

        private void helpForMinimization(Automat minimized)
        {
            minimized.StartState = "q0";
            minimized.states.Add("q0");
            foreach (var symbol in alphabet)
            {
                minimized.alphabet.Add(symbol);
                minimized.delta[("q0", symbol)] = "q0";
            }
        }

        private SortedSet<string> makeGraphForMinimization()
        {
            AutomatGraph automatGraph = new(states.Count);
            int[,] ms = new int[states.Count, states.Count];
            string[] nodes = new string[states.Count];
            int i = 0;
            foreach(var state in states)
            {
                nodes[i++] = state;
            }
            automatGraph.nodes = nodes;

            for(i = 0; i < states.Count; i++)
            {
                for(int j = 0; j < states.Count; j++)
                {
                    foreach(var symbol in alphabet)
                    {
                        if(delta.ContainsKey((nodes[i], symbol)) && delta[(nodes[i], symbol)] == nodes[j])//states.Contains(delta[(nodes[i], symbol)]))
                        {
                            ms[i, j] = 1;
                        }
                    }
                }
            }

            automatGraph.ms = ms;

            Console.WriteLine("NODES: ");
            for(int k = 0; k < automatGraph.nodes.Length; k++)
            {
                Console.Write(automatGraph.nodes[k] + " ");
            }
            Console.WriteLine();

            Console.WriteLine("MS: ");
            for(int k = 0; k < nodes.Length; k++, Console.WriteLine())
            {
                for(int j = 0; j < nodes.Length; j++)
                {
                    Console.Write(ms[k, j] + " ");
                }
            }


            SortedSet<string> nodesVisited = automatGraph.dfs(StartState);
            Console.WriteLine("REACHABLE STATES");
            foreach(var state in nodesVisited)
            {
                Console.Write(state + " ");
            }
            Console.WriteLine();
            
            return nodesVisited;
        }

        private int checkHowManyBrackets(string input, char symbol)
        {
            int bracketCounter = 0;
            for(int i = 0; i < input.Length; i++)
            {
                if(input[i] == symbol)
                {
                    bracketCounter++;
                }
            }
            return bracketCounter;
        }

        private bool checkIfRegularExpressionIsCorrect(string input)
        {
            for(int i = 0; i < input.Length; i++)
            {
                if(input[i] == '*' || input[i] == '+' || input[i] == '(' || input[i] == ')' || alphabet.Contains(input[i]))
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private bool checkPositionOfBrackets(string input)
        {
            if(input[0] == ')' || input[input.Length - 1] == '(')
            {
                return false;
            }
            for (int i = 0; i < input.Length - 1; i++) {
                if(input[i] == '(' && input[i + 1] == ')')
                {
                    return false;
                }
            }
            return true;
        }

        private bool checkIfStartsWithPlus(string input)
        {
            return input[0] == '+';
        }

        private bool checkIfPlusIsBeforeClosedBracket(string input)
        {
            for(int i = 0; i < input.Length - 1; i++)
            {
                if(input[i] == '+' && input[i + 1] == ')')
                {
                    return true;
                }
            }
            return false;
        }

        private bool checkIfEndsWithPlus(string input)
        {
            return (input[input.Length - 1] == '+');
        }

        private bool checkIfTwoPlusesAreNextToEachOther(string input)
        {
            for(int i = 0; i < input.Length - 1; i++) {
                if(input[i] == '+' && input[i + 1] == '+')
                {
                    return true;
                }
            }
            return false;
        }

        private static Automat errorInTransformatingRegExpToAutomata()
        {
            Console.WriteLine("Automata not created, returning null");
            return null;
        }

        private Automat findUnionBetweenTwoLanguages(Automat other)
        {
            Automat result = new();

            result.alphabet = this.alphabet;
            result.alphabet.Add('E');
            result.StartState = "NSS" + id;
            result.finalStates.Add("NFS" + id);
            foreach(var state in states)
            {
                result.states.Add(state);
            }
            foreach(var state in other.states)
            {
                result.states.Add(state);
            }

            foreach(var symbol in alphabet)
            {
                foreach(var state in states)
                {
                    helpForConnectingAndStuff(result, state, symbol);
                }
            }

            foreach(var symbol in other.alphabet)
            {
                foreach(var state in other.states)
                {
                    other.helpForConnectingAndStuff(result, state, symbol);
                }
            }

            result.deltaForEpsilon[(result.StartState, 'E')].Add(this.StartState);
            result.deltaForEpsilon[(result.StartState, 'E')].Add(other.StartState);
            foreach(var finalState in finalStates)
            {
                result.deltaForEpsilon[(finalState, 'E')].Add(result.finalStates.ElementAt(0));
            }
            foreach(var finalState in other.finalStates)
            {
                result.deltaForEpsilon[(finalState, 'E')].Add(result.finalStates.ElementAt(0));
            }

            return result;
        }

        private int howManyBeforeInMiddleAndAfter(char[] expression)
        {
            int howMany = 0;
            int bracketsOpened = 0;
            bool doneItOnce = false;
            for(int i = 1; i < expression.Length - 1; i++)
            {
                if(expression[i] != '(' && expression[i] != ')' && bracketsOpened == 0 && !doneItOnce)
                {
                    //while(expression[i++] != '(') { }
                    howMany++;
                    doneItOnce = true;
                }
                if(expression[i] == '(')
                {
                    doneItOnce = false;
                    bracketsOpened++;
                }
                if(expression[i] == ')')
                {
                    doneItOnce = false;
                    bracketsOpened--;
                }
            }
            return howMany;
        }

        private int numberOfValidExpressions(string regexp)
        {
            int number = 0;
            int bracketCounter = 0;
            int i = 0;
            bool foundOne = false;
            do
            {
                foundOne = false;
                if(regexp[i] == '(')
                {
                    bracketCounter++;
                    number++;
                    i++;
                    while(bracketCounter > 0)
                    {
                        if(regexp[i] == '(')
                        {
                            bracketCounter++;
                        }
                        if(regexp[i] == ')')
                        {
                            bracketCounter--;
                        }
                        i++;
                    }
                }
                else
                {
                    if(!foundOne) { 
                        while(i != regexp.Length - 1 && regexp[i] != '(')
                        {
                            i++;
                        }
                        number++;
                    }
                    foundOne = true;
                }

            } while (i != regexp.Length - 1);

            return number;
        }

        public Automat transform(string regularExpression)
        {
            Automat result = new();

            if (checkHowManyBrackets(regularExpression, '(') != checkHowManyBrackets(regularExpression, ')'))
            {
                return errorInTransformatingRegExpToAutomata();
            }

            if (!checkIfRegularExpressionIsCorrect(regularExpression) || !checkPositionOfBrackets(regularExpression) ||
                checkIfStartsWithPlus(regularExpression) || checkIfPlusIsBeforeClosedBracket(regularExpression) ||
                checkIfEndsWithPlus(regularExpression) || checkIfTwoPlusesAreNextToEachOther(regularExpression))
            {
                return errorInTransformatingRegExpToAutomata();
            }

            Stack<char> stack = new();

            foreach (var symbol in regularExpression)
            {
                stack.Push(symbol);
            }

            int numberOfValidExp = numberOfValidExpressions(regularExpression);
            string[] regExp = new string[numberOfValidExp];
            int bracketsNumber = 0;
            int helpCounter = 0;
            while (numberOfValidExp > 0)
            {
                string temp = "";
                int index = numberOfValidExp - 1;
                while (stack.Count != 0 && stack.Peek() != ')' && bracketsNumber == 0)
                {
                    helpCounter++;
                    regExp[index] += stack.Pop();
                }
                if(helpCounter > 0)
                {
                    numberOfValidExp--;
                }
                if (stack.Count != 0 && stack.Peek() == ')')
                {
                    helpCounter = 0;
                    temp += stack.Pop();
                    bracketsNumber++;
                    while (bracketsNumber > 0)
                    {
                        if (stack.Peek() == ')')
                        {
                            bracketsNumber++;
                        }
                        if (stack.Peek() == '(')
                        {
                            bracketsNumber--;
                        }
                        temp += stack.Pop();
                    }
                    regExp[index] = temp;
                    numberOfValidExp--;
                }
            }

            for(int i = 0; i < numberOfValidExp; i++)
            {
                Console.WriteLine(regExp[i]);
            }

            return result;
        }

        public Automat transformRegularExpressionToAutomata(string regularExpression)
        {
            Automat result = new();

            if(checkHowManyBrackets(regularExpression, '(') != checkHowManyBrackets(regularExpression, ')'))
            {
                return errorInTransformatingRegExpToAutomata();
            }

            if (!checkIfRegularExpressionIsCorrect(regularExpression) || !checkPositionOfBrackets(regularExpression) || 
                checkIfStartsWithPlus(regularExpression) || checkIfPlusIsBeforeClosedBracket(regularExpression) || 
                checkIfEndsWithPlus(regularExpression) || checkIfTwoPlusesAreNextToEachOther(regularExpression))
            {
                return errorInTransformatingRegExpToAutomata();
            }

            Stack<char> stack = new();
            //bool areThereBrackets = false;
            int countBrackets = 0;
            int countBracketsInLoop = 0;

            for (int i = 0; i < regularExpression.Length; i++)
            {
                if(regularExpression[i] == '(')
                {
                    countBracketsInLoop++;
                    bool firstEncounter = false;
                    while(countBracketsInLoop > 0)
                    {
                        if (i == regularExpression.Length) break;
                        stack.Push(regularExpression[i]);
                        if(regularExpression[i] == '(' && firstEncounter)
                        {
                            countBracketsInLoop++;
                        }
                        if(regularExpression[i] == ')')
                        {
                            //stack.Push(regularExpression[i]);
                            countBracketsInLoop--;
                        }
                        firstEncounter = true;
                        i++;
                    }
                }

            }

            foreach(var symbol in stack)
            {
                Console.Write(symbol + " ");
            }
            Console.WriteLine();

            foreach(var symbol in stack)
            {
                if(symbol == '(')
                {
                    countBrackets++;
                }
            }

            int howManyNestedBrackets = 0;
            int howManyNestedBracketsForReal = 0;
            foreach(var symbol in stack)
            {
                if(symbol == '(')
                {
                    if(howManyNestedBrackets > 1)
                    {
                        howManyNestedBracketsForReal++;
                    }
                    howManyNestedBrackets--;
                }

                if(symbol == ')')
                {
                    howManyNestedBrackets++;
                }
            }

            Console.WriteLine(howManyNestedBracketsForReal);

            string tmp = "";
            foreach(var symbol in stack)
            {
                tmp += symbol;
            }
            char[] arr = tmp.ToCharArray();
            Array.Reverse(arr);
            tmp = "";

            Console.WriteLine(arr);
            //Console.WriteLine("Howmany: " + howManyBeforeInMiddleAndAfter(arr));
            
            int num = howManyNestedBracketsForReal + howManyBeforeInMiddleAndAfter(arr);
            Console.WriteLine("NUM: " + num);
            string[] newExpressions = new string[num];

            bool firstGo = true;
            string tempString = "";

            while (num > 0)
            {

                //tempString = "";
                if (firstGo)
                {
                    stack.Pop();
                    firstGo = false;
                }
                else
                {
                    if(stack.Peek() == ')')
                    {
                        stack.Pop();
                        num--;
                        while(stack.Peek() != '(')
                        {
                            if (stack.Peek() != ')')
                                newExpressions[num] += stack.Pop();
                            else
                                break;
                        }
                        stack.Pop();
                        if (stack.Count == 1)
                        {
                            stack.Pop();
                        }
                    }
                    else
                    {
                        tempString = "";
                        while(stack.Peek() != ')' && stack.Peek() != '(')
                        {
                            char pop = stack.Pop();
                            tempString += pop;
                        }
                        num--;
                        newExpressions[num] = tempString;
                        //if(stack.Peek() != '(')
                            //tempString += ":";
                        if (stack.Count != 0 && stack.Peek() == '(')
                        {
                            stack.Pop();
                        }
                        if (stack.Count == 1)
                        {
                            stack.Pop();
                        }
                    }
                }
            }


            Console.WriteLine();

            //Console.WriteLine(tempString);
            Console.WriteLine(newExpressions[0]);
            Console.WriteLine(newExpressions[1]);
            //Console.WriteLine(newExpressions[2]);

            char[] charArray = newExpressions[0].ToCharArray();
            Array.Reverse(charArray);
            string str = new string(charArray);
            //Console.WriteLine(str);

            Console.WriteLine(numberOfValidExpressions(regularExpression));

            return result;
        }


    }

}
