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
        //everything is public because it was easier to test in main, if I had some extra time, I'd correct it!
        public Dictionary<(string, char), string> delta = new();
        public Dictionary<(string, char), List<string>> deltaForEpsilon = new();
        public HashSet<string> finalStates = new();
        public string StartState { get; set; }
        public HashSet<char> alphabet = new();
        public HashSet<string> states = new();
        //when we are working with E-NKA, we use lists, cuz it's possible to have more states to go to with one symbol!
        private List<string>[] listsOfStringsENKA;
        private List<string>[] listOfStringsOtherStates;

        //with id, it's possible to have states with same names, useful for chaining operations etc., every automata has unique id
        string id = "_";
        //counter2 is closely related to id
        static int counter2 = 0;
        private int counter = 0;

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

        //Check if input word is accepted by automata
        public void runAutomata()
        {
            string inputWord;
            Console.WriteLine("Enter a word, so that DKA/E-NKA can check if it is valid or not: ");
            inputWord = Console.ReadLine();
            try
            {
                if (!checkIfInputWordIsCorrect(inputWord))
                {
                    throw new Exception("Incorrect input word!");
                }
                if (!checkIfIsENKA())
                {
                    //Standard function 
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
                    //2 methods, convert to DKA and run, or run directly, you can choose, seconde one probably faster
                    Automat ENKATODKA = this.convertENKAtoDKA();
                    if (ENKATODKA.AcceptsDKA(inputWord))
                    {
                        Console.WriteLine("ENKA accepted this word ;)!");
                    }
                    else
                    {
                        Console.WriteLine("ENKA didn't accept this word :(!");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Function that checks if E-NKA accepts a word without conversion to DKA
        public bool acceptsENKA(string word)
        {
            //Automatgraph is useful, it allows me to make matrix and check E-closures of states
            AutomatGraph automatGraph = new AutomatGraph(states.Count);
            //Help function for making graph
            makeGraphFromAutomataWithEClosures(automatGraph);

            SortedSet<string> traversals = new();
            SortedSet<string> addOtherStates = new();
            //Get E-Closure for start state
            traversals = automatGraph.dfs(StartState);

            foreach (var symbol in word)
            {
                List<String> goToStates = new();
                foreach (var state in traversals)
                {
                    if (deltaForEpsilon.ContainsKey((state, symbol)))
                    {
                        foreach (var s in deltaForEpsilon[(state, symbol)])
                        {
                            //For each symbol in word, which states can we reach?
                            goToStates.Add(s);
                        }
                    }
                }
                traversals.Clear();
                foreach (var state in goToStates)
                {
                    //We reached some states with symbol from word, can we expand our set with E-closures
                    SortedSet<string> tempTraversal = automatGraph.dfs(state);
                    foreach (var tempState in tempTraversal)
                    {
                        traversals.Add(tempState);
                    }
                }
            }

            //Check if it accepts the word in the end
            foreach (var finalState in finalStates)
            {
                if (traversals.Contains(finalState))
                {
                    return true;
                }
            }

            return false;
        }

        //Find union for DKA-s
        public Automat findUnion(Automat other)
        {
            if (!this.checkIfAlphabetIsTheSame(other))
            {
                throw new Exception("Alphabet is not the same!");
            }

            Automat result = new Automat();

            //Add every symbol from alphabet!
            //result.alphabet = alphabet causes problems!
            foreach (var symbol in alphabet)
            {
                result.alphabet.Add(symbol);
            }

            foreach (var state1 in this.states)
            {
                foreach (var state2 in other.states)
                {
                    //Make start state
                    if (StartState == state1 && other.StartState == state2)
                    {
                        result.StartState = state1 + state2;
                    }

                    //Make new state
                    string newState = state1 + state2;
                    result.states.Add(newState);

                    //If at least one automata has that final state, add it to final states (union)
                    if (this.finalStates.Contains(state1) || other.finalStates.Contains(state2))
                    {
                        result.finalStates.Add(newState);
                    }

                    //Connect deltas from both automatas
                    foreach (var symbol in alphabet)
                    {
                        result.delta[(newState, symbol)] = this.delta[(state1, symbol)] + other.delta[(state2, symbol)];
                    }

                }
            }

            return result;
        }

        //Same logic I used for union, except for final states
        public Automat findIntersection(Automat other)
        {
            if (!checkIfAlphabetIsTheSame(other))
            {
                throw new Exception("Alphabet is not the same!");
            }

            Automat result = new Automat();
            foreach (var element in alphabet)
            {
                result.alphabet.Add(element);
            }


            foreach (var state1 in this.states)
            {
                foreach (var state2 in other.states)
                {
                    if (StartState == state1 && other.StartState == state2)
                    {
                        result.StartState = state1 + state2;
                    }

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

        //Same as two functions before this one, final states are the only difference!
        public Automat findDifference(Automat other)
        {
            if (!checkIfAlphabetIsTheSame(other))
            {
                throw new Exception("Alphabet is not the same!");
            }

            Automat result = new Automat();
            foreach (var element in alphabet)
            {
                result.alphabet.Add(element);
            }
            foreach (var state1 in this.states)
            {
                foreach (var state2 in other.states)
                {
                    if (StartState == state1 && other.StartState == state2)
                    {
                        result.StartState = state1 + state2;
                    }

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

        //"Merge" two automatas
        public Automat connectLanguages(Automat other)
        {
            Automat result = new();

            try
            {
                if (!checkIfAlphabetIsTheSame(other))
                {
                    throw new Exception("I can't merge two languages that don't have the same alphabet, sorry!");
                }
                result.StartState = this.StartState;
                
                //It's gonna contain 'E' by definiton
                if (!result.alphabet.Contains('E'))
                {
                    result.alphabet.Add('E');
                }
                foreach (var symbol in this.alphabet)
                {
                    result.alphabet.Add(symbol);
                }
                //It's gonna have same final states as second automata, by definition
                foreach (var finalState in other.finalStates)
                {
                    result.finalStates.Add(finalState);
                }
                foreach (var state in this.states)
                {
                    result.states.Add(state);
                }
                foreach (var state in other.states)
                {
                    result.states.Add(state);
                }

                foreach (var state in this.states)
                {
                    foreach (var symbol in this.alphabet)
                    {
                        //If it's not final state from first automata, delta function does the same as before!
                        if (!this.finalStates.Contains(state))
                        {
                            if (symbol != 'E')
                            {
                                helpForConnectingAndStuff(result, state, symbol);
                            }
                        }
                        else
                        {
                            //If it's final state, it's gonna connect to start state of second automata with E
                            result.ESwitching(state, 'E', other.StartState);
                            foreach (var symb in alphabet)
                            {
                                //If it's not E, do the same as before!
                                if (symb != 'E')
                                {
                                    helpForConnectingAndStuff(result, state, symbol);
                                }
                            }
                        }
                    }
                }

                //For second automata, everything stays the same!
                foreach (var state in other.states)
                {
                    foreach (var symbol in other.alphabet)
                    {
                        other.helpForConnectingAndStuff(result, state, symbol);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        private void helpForConnectingAndStuff(Automat result, string state, char symbol)
        {
            //If it was E-NKA, check in deltaForEpsilon...
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
            //else check in delta, because it was DKA
            else
            {
                if (delta.ContainsKey((state, symbol)))
                {
                    result.ESwitching(state, symbol, delta[(state, symbol)]);
                }
            }
        }

        //Function that allows us to chain operations
        //Everything is pretty much self-explanatory
        public static Automat chainOperations()
        {
            Automat result = new();

            Console.WriteLine("Options for chaining operations:\n1-Union\n2-Intersection\n3-Difference\n4-Complement\n5-Connection\n6-KleeneStar\n");
            string input;
            Console.WriteLine("Enter your options until it becomes boring for you, if it becomes boring, input --exit: ");

            Console.WriteLine("Make first automata: ");
            Automat a1 = new Automat();
            a1.makeAutomata();

            Automat a2 = null;

            Automat res = null;

            do
            {
                input = Console.ReadLine();
                if (input == "1")
                {
                    if (res == null)
                    {
                        Console.WriteLine("Make second automata: ");
                        a2 = new();
                        a2.makeAutomata();
                        res = a1.findUnion(a2);
                        res.printStatesAndAlphabet();
                    }

                    else
                    {
                        a1 = null;
                        a1 = new();
                        a1.makeAutomata();
                        res = res.findUnion(a1);
                        res.printStatesAndAlphabet();
                    }

                }
                else if (input == "2")
                {
                    if (res == null)
                    {
                        Console.WriteLine("Make second automata: ");
                        a2 = new();
                        a2.makeAutomata();
                        res = a1.findIntersection(a2);
                        res.printStatesAndAlphabet();
                    }

                    else
                    {
                        a1 = null;
                        a1 = new();
                        a1.makeAutomata();
                        res = res.findIntersection(a1);
                        res.printStatesAndAlphabet();
                    }
                }
                else if (input == "3")
                {
                    if (res == null)
                    {
                        Console.WriteLine("Make second automata: ");
                        a2 = new();
                        a2.makeAutomata();
                        res = a1.findDifference(a2);
                        res.printStatesAndAlphabet();
                    }
                    else
                    {
                        a1 = null;
                        a1 = new();
                        a1.makeAutomata();
                        res = res.findDifference(a1);
                        res.printStatesAndAlphabet();
                    }
                }
                else if (input == "4")
                {
                    if (res == null)
                    {
                        res = a1.constructComplement();
                        res.printStatesAndAlphabet();
                    }
                    else
                    {
                        Automat temp = null;
                        temp = new();
                        temp = res.constructComplement();
                        res = temp;
                        res.printStatesAndAlphabet();
                    }

                }
                else if (input == "5")
                {
                    if (res == null)
                    {
                        Console.WriteLine("Make second automata: ");
                        a2 = new();
                        a2.makeAutomata();
                        res = a1.connectLanguages(a2);
                    }
                    else
                    {
                        a1 = null;
                        a1 = new();
                        a1.makeAutomata();
                        res = res.connectLanguages(a1);
                        res.printStatesAndAlphabet();
                    }
                }
                else if (input == "6")
                {
                    if (res == null)
                    {
                        res = a1.applyKleeneStar();
                        res.printStatesAndAlphabet();
                    }
                    else
                    {
                        Automat temp = null;
                        temp = new();
                        temp = res.applyKleeneStar();
                        res = temp;
                        res.printStatesAndAlphabet();
                    }
                }
                else if (input == "--exit")
                {

                }
                else
                {
                    Console.WriteLine("Incorrent entry, try again!");
                }
            } while (input != "--exit");

            return result;
        }

        //Function that allows us to apply Kleene star
        public Automat applyKleeneStar()
        {
            Automat result = new();

            result.StartState = "NSS" + id; //new start state
            result.finalStates.Add("NFS" + id); //new final state
            result.states.Add(result.StartState);


            if (!result.alphabet.Contains('E'))
            {
                result.alphabet.Add('E');
            }

            foreach (var symbol in alphabet)
            {
                result.alphabet.Add(symbol);
            }

            foreach (var state in this.states)
            {
                result.states.Add(state);
            }

            result.states.Add("NFS" + id);

            foreach (var state in this.states)
            {
                foreach (var symbol in this.alphabet)
                {
                    helpForConnectingAndStuff(result, state, symbol);
                }
            }

            //By definiton, we go from start state to final state state directly and from start state to start state of old automata
            result.ESwitching(result.StartState, 'E', result.finalStates.ElementAt(0));
            result.ESwitching(result.StartState, 'E', StartState);

            //By definiton, we go from final states with 'E' to old start state and final state
            foreach (var finalState in this.finalStates)
            {
                result.ESwitching(finalState, 'E', "NFS" + id);
                result.ESwitching(finalState, 'E', StartState);
            }

            return result;
        }

        //Self-explanatory
        private bool checkIfAlphabetIsTheSame(Automat other)
        {
            if (this.alphabet.Count != other.alphabet.Count)
            {
                return false;
            }

            foreach (var symbol in this.alphabet)
            {
                if (!other.alphabet.Contains(symbol))
                {
                    return false;
                }
            }

            return true;
        }


        //Self-explanatory
        private bool checkIfInputWordIsCorrect(string word)
        {
            foreach (var symbol in word)
            {
                if (!alphabet.Contains(symbol))
                {
                    return false;
                }
            }
            return true;
        }

        //Check if alphabet contains 'E'
        private bool checkIfIsENKA()
        {
            return alphabet.Contains('E');
        }

        //Self-explanatory
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

        //Does DKA accept a certain word?
        public bool AcceptsDKA(string input)
        {
            var currentState = StartState;
            foreach (var symbol in input)
            {
                currentState = delta[(currentState, symbol)];
            }
            return finalStates.Contains(currentState);
        }

        //Construct complement -> switch all non-final states to final
        //And switch all final states to non-final
        public Automat constructComplement()
        {
            Automat result = new();
            result = this;
            result.finalStates.Clear();
            foreach (var state in states)
            {
                if (!this.finalStates.Contains(state))
                {
                    result.finalStates.Add(state);
                }
            }
            return result;
        }

        //Help function that helps user to enter alphabet
        private void enterAlphabet()
        {
            Console.WriteLine("--exit to exit input loop!\nEnter your alphabet (if you enter E as an symbol, it's E-NKA): ");
            string inputString;
            do
            {
                inputString = Console.ReadLine();
                if (inputString != "--exit")
                {
                    char symbol = char.Parse(inputString);
                    alphabet.Add(symbol);
                }
            } while (inputString != "--exit");
        }

        //Helps user to add states
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
                if (initState != "--exit")
                {
                    states.Add(state);
                    string extraInput;
                    Console.WriteLine("Do you want this state to be a final state? (yes/no): ");
                    extraInput = Console.ReadLine();
                    if (extraInput == "yes")
                    {
                        finalStates.Add(state);
                    }
                    else if (extraInput == "no") { }
                    else
                    {
                        Console.WriteLine("Incorrent word, I'll take that as a no!");
                    }

                }
            } while (initState != "--exit");

        }

        //Self-explanatory
        private bool checkIfStateExists(string state)
        {
            return states.Contains(state);
        }

        //Self-explanatory
        private bool checkIfSymbolIsInAlphabet(char symbol)
        {
            return alphabet.Contains(symbol);
        }

        //Allows user to add transitions when making automata
        private void addTransitions()
        {
            printStatesAndAlphabet();
            foreach (var state in states)
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

                    if (inputHelp != "--exit" && checkIfSymbolIsInAlphabet(char.Parse(inputHelp)))
                    {
                        symbol = char.Parse(inputHelp);
                        Console.WriteLine("Enter destination state: ");
                        nextState = Console.ReadLine();

                        if (checkIfStateExists(nextState + id))
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
                    else if (inputHelp != "--exit")
                    {
                        Console.WriteLine("That symbol doesn't exist in this alphabet!");
                    }
                } while (inputHelp != "--exit");
            }
        }

        //Help func for E-Switching
        private int ESwitchingHelper(char symbol)
        {
            for (int i = 0; i < alphabet.Count; i++)
            {
                if (symbol == alphabet.ElementAt(i))
                {
                    return i;
                }
            }
            return -1;
        }

        //Switch E-Automata
        //Complex logic, list for each state for E-transitions
        //And make list for each state for each symbol in alphabet
        //Then do transitions
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
                                    //ADDED
                                    if (pos == -1)
                                    {
                                        pos = 0;
                                    }
                                    deltaForEpsilon[(currentState, thing)] = listOfStringsOtherStates[pos];
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

        //Make all the lists for the function before
        //counter allows us to do it only once
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

        //Different symbols in alphabet, not counting 'E'
        private int howManyDifferentSymbolsInAlphabet()
        {
            int numOfSymbols = 0;
            foreach (var symbol in alphabet)
            {
                if (symbol != 'E')
                {
                    numOfSymbols++;
                }
            }
            return numOfSymbols;
        }

        //Self explanatory, also prints id 
        //There is a version that doesn't print id, will implement!
        public void printStatesAndAlphabet()
        {

            //Console.WriteLine("State: " + state.Substring(0, substring));
            Console.WriteLine("STATES: ");
            foreach (var state in states)
            {
                //int substring = state.IndexOf('_');
                //Console.Write(state.Substring(0, substring) + " ");
                Console.Write(state + " ");
            }
            Console.WriteLine();
            Console.WriteLine("ALPHABET: ");
            foreach (var symbol in alphabet)
            {
                Console.Write(symbol + " ");
            }
            Console.WriteLine();
            Console.WriteLine("DELTA TRANSITIONS: ");
            if (!checkIfIsENKA())
            {
                foreach (var symbol in alphabet)
                {
                    foreach (var state in states)
                    {
                        int substring = state.IndexOf('_');
                        if (delta.ContainsKey((state, symbol)))
                            Console.WriteLine(state/*.Substring(0, substring)*/ + " + " + symbol + " -> " + delta[(state, symbol)]/*.Substring(0, substring)*/);
                    }
                }
            }
            else
            {
                foreach (var symbol in alphabet)
                {
                    foreach (var state in states)
                    {
                        int substring = state.IndexOf('_');
                        //deltaForEpsilon
                        if (deltaForEpsilon.ContainsKey((state, symbol)))
                        {
                            List<string> goTo = deltaForEpsilon[(state, symbol)];
                            foreach (var s in goTo)
                            {
                                Console.WriteLine(state + " + " + symbol + " -> " + s/*.Substring(0, substring)*/);
                            }
                        }
                    }
                }
            }

        }

        //Checks if language is final
        //TODO: when we reach final state, check if it doesn't have any more transitions, then it's also final
        public bool isLanguageFinal()
        {
            Automat convert = new();

            if (checkIfIsENKA())
            {
                convert = convertENKAtoDKA();
            }
            else
            {
                convert = this;
            }

            if (convert.finalStates.Count == 0 || convert.finalStates.Count == convert.states.Count)
            {
                return false;
            }

            if (!isLanguageFinalHelpFunc())
            {
                return false;
            }

            AutomatGraph automatGraph = new(convert.states.Count);
            //Help function that puts 1 in graph if any of symbol from alphabet is present for delta
            convert.makeGraphFromAutomataWithAllConnections(automatGraph);

            foreach (var finalState in convert.finalStates)
            {
                //Help function in automat graph that allows us to check if there is a cycle
                //If it exists, language is not final!
                if (automatGraph.dfsForLongestWord(finalState))
                {
                    return false;
                }

            }

            //If we didn't find a cycle, language is final
            return true;
        }

        //If a final state, for any symbol from the alphabet, goes to itself, return false
        private bool isLanguageFinalHelpFunc()
        {
            bool result = true;
            foreach (var finalState in finalStates)
            {
                foreach (var symbol in alphabet)
                {
                    if (delta.ContainsKey((finalState, symbol)) && delta[(finalState, symbol)] == finalState)
                    {
                        return false;
                    }
                }
            }
            return result;
        }

        //Self-explanatory
        //BFS, when we get to a final state, we break, we remember path length
        public int findShortestPath()
        {
            int shortestPathLength = 0;

            if (finalStates.Contains(StartState))
            {
                return shortestPathLength;
            }

            if (finalStates.Count == 0)
            {
                Console.WriteLine("There is no shortest path because there is no final state, returning -1");
                return -1;
            }

            Queue<string> queue = new();
            queue.Enqueue(StartState);

            while (queue.Count > 0)
            {
                string temp = queue.Dequeue();
                foreach (char symbol in alphabet)
                {
                    if (delta[(temp, symbol)] != temp)
                    {
                        string nextState = delta[(temp, symbol)];
                        if (finalStates.Contains(nextState))
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

        //Self explanatory functio name
        public Automat convertENKAtoDKA()
        {
            Automat DKA = new();
            //ADDED
            //Ads dead state, easier to convert
            //Otherwise, a lot of errors
            this.addDeadState();
            AutomatGraph automatGraph = new(states.Count);
            makeGraphFromAutomataWithEClosures(automatGraph);
            DKA.StartState = this.StartState;
            DKA.states.Add(StartState);

            //Add every symbol except 'E'
            foreach (var symbol in alphabet)
            {
                if (symbol != 'E')
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
                    //If it is only one state, do E-closure only for that state
                    getDFStraversal = automatGraph.dfs(state);
                    helpMethodForConversion(DKA, automatGraph, state, getDFStraversal);
                }
                else
                {
                    //If there are more states, do E-closures for each one of them
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

        //Self explanatory name
        public int findLongestPath()
        {
            //int pathSize = 0;
            AutomatGraph automatGraph = new(states.Count);

            Automat possibleDKA = new();

            //Easier to work with DKA
            if (checkIfIsENKA())
            {
                possibleDKA = convertENKAtoDKA();
            }
            else
            {
                possibleDKA = this;
            }

            //If it's not final
            if (!isLanguageFinalHelpFunc())
            {
                Console.WriteLine("It's infinity, even thought here we don't have infinity, I'll just print out the biggest possible number");
                return int.MaxValue;
            }

            //Make standard graph
            possibleDKA.makeGraphFromAutomataWithAllConnections(automatGraph);

            //If there is a cycle, longest word is inifity
            foreach (var finalState in possibleDKA.finalStates)
            {
                if (automatGraph.dfsForLongestWord(finalState))
                {
                    Console.WriteLine("It's infinity, even thought here we don't have infinity, I'll just print out the biggest possible number");
                    return int.MaxValue;
                }
            }

            if (possibleDKA.finalStates.Count == 0)
            {
                Console.WriteLine("There is no final state, returning -1!");
                return -1;
            }

            return automatGraph.bfsFindLongestWord(possibleDKA);

            //return pathSize;
        }

        //As commented before, 
        //Function that makes a graph, ms[i][j] = 1 if any symbol connects two nodes
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
            for (i = 0; i < states.Count; i++)
            {
                for (j = 0; j < states.Count; j++)
                {
                    bool doesContain = false;
                    foreach (var symbol in alphabet)
                    {
                        if (!checkIfIsENKA())
                        {
                            if (delta.ContainsKey((nodes[i], symbol)) && delta[(nodes[i], symbol)].Contains(nodes[j]))
                            {
                                ms[i, j] = 1;
                                doesContain = true;
                            }
                        }
                    }
                    if (!doesContain)
                    {
                        ms[i, j] = 0;
                    }
                }
            }

            automatGraph.ms = ms;
            automatGraph.nodes = nodes;

        }

        //ms[i,j] = 1 only if we can go from one node to the other with 'E'
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

        //Convert E-NKA to DKA help method
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
                    //For each visited state
                    foreach (var stateVisited in getDFStraversal)
                    {
                        //Form a list of states I need to go to
                        List<string> goToStates = new();

                        //If there is a connection, add it
                        if (deltaForEpsilon.ContainsKey((stateVisited, symbol)))
                            goToStates = deltaForEpsilon[(stateVisited, symbol)];
                        //goToStates.Sort();

                        //For each state added, do DFS traversal
                        foreach (var goToState in goToStates)
                        {
                            SortedSet<string> tempTraversal = automatGraph.dfs(goToState);
                            foreach (var finalConnection in tempTraversal)
                            {
                                //Check if is final state
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
                if (statesSorted.Count > 1)
                {
                    int counter = 0;
                    foreach (var s in statesSorted)
                    {

                        temp += s;
                        counter++;
                        if (counter != statesSorted.Count)
                        {
                            //Add a delimiter, so it's easier afterwards
                            temp += ":";
                        }

                    }
                }
                else
                {
                    if (statesSorted.Count == 1)
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

        //Check if two automatas are the same
        //If automatas are E-NKA, convert them to DKA
        //Minimise automatas
        //Rename states
        //Check delta function, if there is a single thing that doesn't match return false
        public bool compareTwoAutomatas(Automat other)
        {
            Automat convertFirst = new();
            Automat convertSecond = new();
            Automat first;
            Automat second;
            if (this.checkIfIsENKA())
            {
                convertFirst = this.convertENKAtoDKA();
            }
            if (other.checkIfIsENKA())
            {
                convertSecond = other.convertENKAtoDKA();
            }

            AutomatGraph graph = new();

            if (!checkIfIsENKA())
            {
                first = graph.bfsTraversal(this);
            }
            else
            {
                first = graph.bfsTraversal(convertFirst);
            }

            if (!other.checkIfIsENKA())
            {
                second = graph.bfsTraversal(other);
            }
            else
            {
                second = graph.bfsTraversal(convertSecond);
            }

            if (first.states.Count != second.states.Count)
            {
                return false;
            }

            foreach (var state in first.states)
            {
                foreach (var symbol in first.alphabet)
                {
                    Console.WriteLine(first.delta[(state, symbol)] + " " + second.delta[(state, symbol)]);
                    if (first.delta[(state, symbol)] != second.delta[(state, symbol)])
                    {
                        return false;
                    }
                }
            }

            if (first.finalStates.Count != second.finalStates.Count)
            {
                return false;
            }

            foreach (var finalState in first.finalStates)
            {
                if (!second.finalStates.Contains(finalState))
                {
                    return false;
                }
            }

            //Console.WriteLine("Isti!");
            return true;
        }

        //Function that allows us to minimize automata
        //It's a very long function, long story short:
        //Form a matrix like we've learned
        //If there are multiple states (q0, q1), (q0, q2), (q1, q2) that have to be minimzed, merge them into one
        //If there are only two states to minimize, it's basic case
        //For each symbol, check if pair goes to other symbol or it stays within the same state (for example q0:q1)
        //Final states stay the same, or if we can minimize some final states, we will do it
        public Automat minimiseAutomata()
        {
            Automat minimized = new();

            //ADDED
            //Not sure if this is needed, added it anyways
            //Dead state is not added if it's not necessary
            this.addDeadState();
            //this.printStatesAndAlphabet();


            //Basic case
            if (finalStates.Count == states.Count)
            {
                minimized.finalStates.Add("q0" + minimized.id);
                helpForMinimization(minimized);
                return minimized;
            }

            //Another basic case
            if (finalStates.Count == 0)
            {
                helpForMinimization(minimized);
                return minimized;
            }

            //Find reachable states
            //Not the best name for a function
            SortedSet<string> getReachableStates = makeGraphForMinimization();
            this.states.Clear();
            //Add reachable states as new states
            foreach (var state in getReachableStates)
            {
                this.states.Add(state);
            }
            //Make new final states from reachable states
            HashSet<string> newFinalStates = new();
            foreach (var state in finalStates)
            {
                if (this.states.Contains(state))
                {
                    newFinalStates.Add(state);
                }
            }
            finalStates.Clear();
            //Update final states
            finalStates = newFinalStates;

            Console.WriteLine("STATES");
            foreach (var state in states)
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

            //Put 1 in graph ms if there is a pair with one final state
            //Otherwise put -1 if it's out of bounds
            //Or put 0 if there is a chance 
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
            //Run loop as long as you can update the matrix
            while (automatGraph.minimiseAutomataHelper(this))
            {
                //Console.WriteLine("YAS");
            }

            //Find states to minimize
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
                        //Should have used sorted set, more job this way
                        //Connect states with :
                        //Pretty much find each pair of states to minimize
                        if (String.Compare(states.ElementAt(i), states.ElementAt(j)) < 0)
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
            foreach (var state in statesToMinimize)
            {
                Console.Write(state + " ");
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            minimized.alphabet = this.alphabet;

            //For each pair, find if it is similar to other pair
            //If it is, "merge" those states
            //Doing this in case I have to merge more than two states, which is more than likely possible
            //Tested, works, at least for my examples
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
                        for (int g = 0; g < temp.Length - 1; g++)
                        {
                            temp2 += temp[g];
                        }
                        //Console.WriteLine(temp2);
                        fullMinimization.Add(temp2);
                    }
                }
            }

            //We got our states which we can fully minimize
            Console.WriteLine("States to fully minimize: ");
            foreach (var state in fullMinimization)
            {
                Console.Write(state + " ");
            }
            Console.WriteLine();

            //Update start state if it's necessary
            bool flagForStartState = false;
            foreach (var state in fullMinimization)
            {
                if (state.Contains(StartState))
                {
                    flagForStartState = true;
                    minimized.StartState = state;
                }
            }
            if (!flagForStartState)
            {
                minimized.StartState = StartState;
            }

            //Update final states if necessary
            bool flagForFinalState = false;
            foreach (var state in finalStates)
            {
                flagForFinalState = false;
                foreach (var s in fullMinimization)
                {
                    if (s.Contains(state))
                    {
                        flagForFinalState = true;
                        minimized.finalStates.Add(s);
                    }
                }
                if (!flagForFinalState)
                {
                    minimized.finalStates.Add(state);
                }
            }

            //Finally minimize states
            //If a state from first automata is not in fullMinimization states, add it as one state
            //Otherwise add minimized state
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
            foreach (var state in fullMinimization)
            {
                if (state != "")
                {
                    minimized.states.Add(state);
                }
            }

            //Update delta function
            //Explained before function declaration
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
                            //ADDED FIRST CONDITION
                            if (delta.ContainsKey((state, symbol)) && fullMinimization.ElementAt(i).Contains(delta[(state, symbol)]))
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
                            //Go into itself
                            minimized.delta[(state, symbol)] = state;
                            flag = false;
                        }
                        else
                        {
                            //If it doesn't go into itself for a particular symbol,
                            //It goes to the state at least one symbol goes to!
                            string temp = delta[(splitStates[0], symbol)];
                            bool flag2 = false;
                            foreach (var st in fullMinimization)
                            {
                                flag2 = false;
                                if (st.Contains(temp))
                                {
                                    flag2 = true;
                                    minimized.delta[(state, symbol)] = st;
                                    break;
                                }
                            }
                            if (!flag2)
                            {
                                minimized.delta[(state, symbol)] = delta[(splitStates[0], symbol)];
                            }
                        }
                    }
                }
            }

            //Check if it's good!
            minimized.printStatesAndAlphabet();
            foreach (var state in minimized.states)
            {
                foreach (var symbol in alphabet)
                {
                    Console.WriteLine(state + "->" + symbol + " = " + minimized.delta[(state, symbol)]);
                }
            }

            return minimized;
        }

        private void helpForMinimization(Automat minimized)
        {
            minimized.StartState = "q0" + id;
            minimized.states.Add("q0" + id);
            foreach (var symbol in alphabet)
            {
                minimized.alphabet.Add(symbol);
                minimized.delta[("q0" + id, symbol)] = "q0" + id;
            }
        }

        //Help function, makes a graph like we've done with professor
        //Also adds all the nodes and stuff
        private SortedSet<string> makeGraphForMinimization()
        {
            AutomatGraph automatGraph = new(states.Count);
            int[,] ms = new int[states.Count, states.Count];
            string[] nodes = new string[states.Count];
            int i = 0;
            foreach (var state in states)
            {
                nodes[i++] = state;
            }
            automatGraph.nodes = nodes;

            for (i = 0; i < states.Count; i++)
            {
                for (int j = 0; j < states.Count; j++)
                {
                    foreach (var symbol in alphabet)
                    {
                        //If there is a connection, add it!
                        if (delta.ContainsKey((nodes[i], symbol)) && delta[(nodes[i], symbol)] == nodes[j])//states.Contains(delta[(nodes[i], symbol)]))
                        {
                            ms[i, j] = 1;
                        }
                    }
                }
            }

            automatGraph.ms = ms;

            //UNNECESSARY
            Console.WriteLine("NODES: ");
            for (int k = 0; k < automatGraph.nodes.Length; k++)
            {
                Console.Write(automatGraph.nodes[k] + " ");
            }
            Console.WriteLine();

            Console.WriteLine("MS: ");
            for (int k = 0; k < nodes.Length; k++, Console.WriteLine())
            {
                for (int j = 0; j < nodes.Length; j++)
                {
                    Console.Write(ms[k, j] + " ");
                }
            }

            //Print out reachable states, test
            SortedSet<string> nodesVisited = automatGraph.dfs(StartState);
            Console.WriteLine("REACHABLE STATES");
            foreach (var state in nodesVisited)
            {
                Console.Write(state + " ");
            }
            Console.WriteLine();

            return nodesVisited;
        }

        //Help functions for regular expressions
        private int checkHowManyBrackets(string input, char symbol)
        {
            int bracketCounter = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == symbol)
                {
                    bracketCounter++;
                }
            }
            return bracketCounter;
        }

        private bool checkIfRegularExpressionIsCorrect(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '*' || input[i] == '+' || input[i] == '(' || input[i] == ')' || alphabet.Contains(input[i]))
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
            if (input[0] == ')' || input[input.Length - 1] == '(')
            {
                return false;
            }
            for (int i = 0; i < input.Length - 1; i++)
            {
                if (input[i] == '(' && input[i + 1] == ')')
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
            for (int i = 0; i < input.Length - 1; i++)
            {
                if (input[i] == '+' && input[i + 1] == ')')
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
            for (int i = 0; i < input.Length - 1; i++)
            {
                if (input[i] == '+' && input[i + 1] == '+')
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

        //Function to find union (a+b) for example
        //Done by definition
        private Automat findUnionBetweenTwoLanguages(Automat other)
        {

            Automat result = new();

            foreach (var element in alphabet)
            {
                result.alphabet.Add(element);
            }

            result.alphabet.Add('E');
            result.StartState = "NSS" + result.id;
            result.finalStates.Add("NFS" + result.id);


            result.states.Add(result.StartState);
            result.states.Add("NFS" + result.id);
            foreach (var state in states)
            {
                result.states.Add(state);
            }
            foreach (var state in other.states)
            {
                result.states.Add(state);
            }

            foreach (var symbol in alphabet)
            {
                foreach (var state in states)
                {
                    helpForConnectingAndStuff(result, state, symbol);
                }
            }

            foreach (var symbol in other.alphabet)
            {
                foreach (var state in other.states)
                {
                    other.helpForConnectingAndStuff(result, state, symbol);
                }
            }

            //Go from start state to start states of two automatas
            result.ESwitching(result.StartState, 'E', this.StartState);
            result.ESwitching(result.StartState, 'E', other.StartState);

            //Go from final states to new final state
            foreach (var finalState in finalStates)
            {
                result.ESwitching(finalState, 'E', result.finalStates.ElementAt(0));
            }

            //Go from final states to new final state
            foreach (var finalState in other.finalStates)
            {
                result.ESwitching(finalState, 'E', result.finalStates.ElementAt(0));
            }

            return result;
        }

        //Help function for transforming regular expression to automata
        private int findHowManyHashSetsAreNeeded(string regexp)
        {
            int min = 0, counter = 0;
            for (int i = 0; i < regexp.Length; i++)
            {
                if (regexp[i] == '(')
                {
                    counter++;
                    if (counter > min)
                    {
                        min = counter;
                    }
                }
                if (regexp[i] == ')')
                {
                    counter--;
                }
            }
            return (min + 1);
        }

        //Find out how many automatas are needed
        private int countNumOfAutomatas(string[] sets)
        {
            int counter = 0;
            for (int i = 1; i < sets.Length; i++)
            {
                string str = sets[i];
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == '/')
                    {
                        counter++;
                    }
                }
            }
            return counter;
        }

        //Hardest function ever!
        public Automat transformRegularExpressionToAutomata(string regularExpression)
        {
            //Find out if error exists!
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

            int num = findHowManyHashSetsAreNeeded(regularExpression);
            string[] sets = new string[num];
            int level = 0;

            //Split elements
            //When there is an opening bracket, go level down
            //When there is a closing bracket, go level up
            //'-' for level down, '/' for level up
            for (int i = 0; i < regularExpression.Length; i++)
            {
                if (regularExpression[i] == '(')
                {
                    //if we go level down, '-'
                    sets[level] += "-";
                    level++;
                }
                if (regularExpression[i] == ')')
                {
                    //if we go level up, '/'
                    sets[level] += "/";
                    level--;
                }
                if (regularExpression[i] != '(' && regularExpression[i] != ')')
                {
                    sets[level] += regularExpression[i];
                }
            }

            //UNNECESSARY PRINT
            foreach (var set in sets)
            {
                Console.WriteLine(set);
            }

            int countHowManyAutomatas = countNumOfAutomatas(sets) + 1;

            Automat[] realAutomatas = new Automat[countHowManyAutomatas];
            for (int i = 0; i < realAutomatas.Length; i++)
            {
                realAutomatas[i] = new();
            }

            //Where to push
            int pushIntoAutomatas = 0;
            //Helps us to know where to come back
            int[] savePositionOfThoseCreatedBefore = new int[sets.Length];

            for (int i = sets.Length - 1; i >= 0; i--)
            {
                string[] splitFirst = sets[i].Split('/');
                foreach(var element in splitFirst)
                {
                    if(element != "")
                    {
                        savePositionOfThoseCreatedBefore[i]++;
                    }
                }

                //Console.WriteLine("POSITION: " + i + " SAVED ON POSITION: " + savePositionOfThoseCreatedBefore[i]);
                //Console.WriteLine("Save positions " + savePositionOfThoseCreatedBefore[i]);
                int c = 0;
                foreach (var str in splitFirst)
                {
                    //Split by '+', easy to do
                    string[] plusSplit = str.Split('+');
                    int numberOfTempAutomatas = 0;
                    foreach (var element in plusSplit)
                    {
                        if (element != "")
                        {
                            //Console.WriteLine(element);
                            numberOfTempAutomatas++;
                        }
                    }

                    //number of temp automatas -> how many '+' splits we have
                    Automat[] tempAutomatas = new Automat[numberOfTempAutomatas];
                    for (int k = 0; k < tempAutomatas.Length; k++)
                    {
                        tempAutomatas[k] = new();
                    }
                    c = 0;
                    foreach (var automata in tempAutomatas)
                    {
                        foreach (var symbol in alphabet)
                        {
                            automata.alphabet.Add(symbol);
                        }
                    }
                    int helpCounter = 0;
                    foreach (var element in plusSplit)
                    {
                        if (element != "")
                        {
                            int stateNo = 0;
                            bool first = true;
                            for (int size = 0; size < element.Length; size++)
                            {
                                //If it's symbol from alphabet
                                if (alphabet.Contains(element[size]))
                                {
                                    string oldState = "", newState = "";
                                    HashSet<string> oldStates = new();
                                    //If it's first symbol, make two states
                                    if (first)
                                    {
                                        oldState = "q" + stateNo + tempAutomatas[c].id;
                                        stateNo++;
                                        tempAutomatas[c].StartState = oldState;
                                        first = false;
                                        oldStates.Add(oldState);
                                        newState = "q" + stateNo + tempAutomatas[c].id;
                                        tempAutomatas[c].states.Add(oldState);
                                        tempAutomatas[c].states.Add(newState);
                                        stateNo++;
                                        
                                        //NEW
                                        tempAutomatas[c].finalStates.Clear();
                                        tempAutomatas[c].finalStates.Add(newState);
                                    }
                                    //Else make one state and remember old final states
                                    //It works cuz we splited it with '+' before
                                    else
                                    {
                                        foreach(var state in tempAutomatas[c].finalStates)
                                        {
                                            oldStates.Add(state);
                                        }
                                        tempAutomatas[c].finalStates.Clear();

                                        newState = "q" + stateNo + tempAutomatas[c].id;
                                        tempAutomatas[c].finalStates.Add(newState);
                                        tempAutomatas[c].states.Add(newState);
                                        stateNo++;
                                    }
                                    
                                    //Make transitions if it's DKA or E-NKA, respectively
                                    if (!tempAutomatas[c].checkIfIsENKA())
                                    {
                                        if(size > 0 && element[size - 1] == '-')
                                        {
                                            foreach(var finalState in tempAutomatas[c].finalStates)
                                            {
                                                tempAutomatas[c].delta[(finalState, element[size])] = newState;
                                            }
                                        }
                                        else
                                        {
                                            foreach(var elem in oldStates)
                                                    tempAutomatas[c].delta[(elem, element[size])] = newState;
                                        }
                                    }
                                    else
                                    {
                                        tempAutomatas[c].counter = 0;
                                        //If symbol before was '*'
                                        if (size > 0 && element[size - 1] == '*')
                                        {
                                            //Connect old finalState to newState
                                            foreach(var elem in oldStates)
                                            { 
                                                if(elem != "")
                                                    tempAutomatas[c].ESwitching(elem, element[size], newState);
                                            }

                                        }
                                        //If symbol before was '-'
                                        else if (size > 0 && element[size - 1] == '-')
                                        {
                                            //Connect all old states to newState
                                            foreach (var elem in oldStates)
                                            {
                                                tempAutomatas[c].ESwitching(elem, element[size], newState);
                                            }
                                        }
                                        else
                                        {
                                            foreach(var elem in oldStates)
                                            {
                                                tempAutomatas[c].ESwitching(elem, element[size], newState);
                                            }
                                        }
                                    }
                                    //If it's the end
                                    //And it was a symbol, one final state
                                    //Increment counter
                                    if (size == element.Length - 1)
                                    {
                                        tempAutomatas[c].finalStates.Clear();
                                        tempAutomatas[c].finalStates.Add(newState);
                                        c++;
                                    }
                                    if(size != element.Length - 1 && element[size + 1] == '*')
                                    {
                                        tempAutomatas[c].finalStates.Add(newState);
                                    }
                                }
                                //If it was '*'
                                else if (element[size] == '*')
                                {
                                    //Apply Kleene star
                                    Automat tmp = tempAutomatas[c].applyKleeneStar();
                                    tempAutomatas[c] = tmp;
                                    tempAutomatas[c].alphabet.Add('E');
                                    //If we've come to an end, increment counter, otherwise error!
                                    if (size == element.Length - 1)
                                    {
                                        c++;
                                    }
                                }
                                //If we need to read from row below
                                else if (element[size] == '-')
                                {
                                    int pos = i + 1;
                                    int sum = 0;
                                    int total = 0;
                                    sum = savePositionOfThoseCreatedBefore[pos];
                                    while(pos < sets.Length)
                                    {
                                        total += savePositionOfThoseCreatedBefore[pos];
                                        pos++;
                                    }
                                    //FOR EXAMPLE: 3 in total - 2 on level = 1 -> WE GO FROM FIRST POSITION
                                    int startPosition = (total - sum) + helpCounter;
                                    helpCounter++;

                                    //Make E-closure for everything necessary
                                    foreach(var finalState in tempAutomatas[c].finalStates)
                                    {
                                        //Console.WriteLine("FS " + finalState + " ");
                                        //Console.WriteLine("SS " + realAutomatas[startPosition].StartState);
                                        if(tempAutomatas[c].checkIfIsENKA())
                                        {
                                            tempAutomatas[c].ESwitching(finalState, 'E', realAutomatas[startPosition].StartState);
                                        } 
                                        else
                                        {
                                            tempAutomatas[c].delta[(finalState, 'E')] = realAutomatas[startPosition].StartState;
                                        }
                                    }

                                    //Clear final states
                                    tempAutomatas[c].finalStates.Clear();

                                    //Add 'E' to temp automata
                                    if(realAutomatas[startPosition].alphabet.Contains('E'))
                                    {
                                        tempAutomatas[c].alphabet.Add('E');
                                    }

                                    foreach(var state in realAutomatas[startPosition].states)
                                    {
                                        tempAutomatas[c].states.Add(state);
                                    }

                                    //If it's not E-NKA, standard DKA transitions
                                    if(!realAutomatas[startPosition].checkIfIsENKA())
                                    {
                                        foreach(var symbol in realAutomatas[startPosition].alphabet)
                                        {
                                            foreach(var state in realAutomatas[startPosition].states)
                                            {
                                                if(realAutomatas[startPosition].delta.ContainsKey((state, symbol)))
                                                {
                                                    tempAutomatas[c].delta[(state, symbol)] = realAutomatas[startPosition].delta[(state, symbol)];
                                                }
                                            }
                                        }
                                    }
                                    //Else E-NKA transitions
                                    else
                                    {
                                        tempAutomatas[c].counter = 0;
                                        foreach (var symbol in realAutomatas[startPosition].alphabet)
                                        {
                                            foreach (var state in realAutomatas[startPosition].states)
                                            {
                                                if (realAutomatas[startPosition].deltaForEpsilon.ContainsKey((state, symbol)))
                                                {
                                                    foreach(var s in realAutomatas[startPosition].deltaForEpsilon[(state, symbol)])
                                                    {
                                                        tempAutomatas[c].ESwitching(state, symbol, s);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    string oldState = "", newState = "";
                                    //If '-' is first symbol
                                    if(first)
                                    {
                                        //Get it's start state
                                        tempAutomatas[c].StartState = realAutomatas[startPosition].StartState;
                                        stateNo++;
                                        tempAutomatas[c].states.Add(tempAutomatas[c].StartState);
                                        first = false;
                                    }
                                    else
                                    {
                                        //Else get old state
                                        //Set new state
                                        oldState = tempAutomatas[c].states.ElementAt(tempAutomatas[c].states.Count - 1);
                                        newState = realAutomatas[startPosition].StartState;
                                        //tempAutomatas[c].states.Add(newState);
                                        stateNo++;
                                    }
                                    //Add each final state from this state
                                    foreach(var finalState in realAutomatas[startPosition].finalStates)
                                    {
                                         tempAutomatas[c].finalStates.Add(finalState);
                                    }

                                    //Cases when counter is incremented
                                    if(size > 1 && element[size - 1] == '-')
                                    {
                                        c++;
                                    }
                                    if(element.Length == 1 && element[0] == '-')
                                    {
                                        c++;
                                    }
                                    if(element[element.Length - 1] == '-')
                                    {
                                        c++;
                                    }

                                }
                            }
                        }

                    }
                    //Connect with '+' -> UNION
                    if (tempAutomatas.Length == 1)
                    {
                        realAutomatas[pushIntoAutomatas] = tempAutomatas[c-1];
                        pushIntoAutomatas++;
                        //realAutomatas[pushIntoAutomatas - 1].printStatesAndAlphabet();
                        //c++;
                    }
                    else
                    {
                        for (int brRename = 0; brRename < tempAutomatas.Length; brRename++)
                        {
                            if (brRename == 0)
                            {
                                /*Console.WriteLine("TEMPAUTO1: ");
                                tempAutomatas[0].printStatesAndAlphabet();
                                Console.WriteLine("TEMPAUTO2: ");
                                tempAutomatas[1].printStatesAndAlphabet();
                                Console.WriteLine("FINALSTATES: ");
                                foreach(var finalState in tempAutomatas[1].finalStates)
                                {
                                    Console.Write(finalState + " ");
                                }*/

                                realAutomatas[pushIntoAutomatas] = tempAutomatas[0].findUnionBetweenTwoLanguages(tempAutomatas[1]);
                                /*Console.WriteLine("PUSH INTO AUTOMATA: ");
                                realAutomatas[pushIntoAutomatas].printStatesAndAlphabet();
                                Console.WriteLine("FINALSTATES: ");
                                foreach (var finalState in realAutomatas[pushIntoAutomatas].finalStates)
                                {
                                    Console.Write(finalState + " ");
                                }*/
                                brRename = brRename + 1;
                            }
                            else
                            {
                                realAutomatas[pushIntoAutomatas] = realAutomatas[pushIntoAutomatas].findUnionBetweenTwoLanguages(tempAutomatas[brRename]);
                            }
                        }
                        if(tempAutomatas.Length != 0)
                        {
                            pushIntoAutomatas++;
                        }

                    }
                }
            }
            int automatNum = countHowManyAutomatas - 1;

            return realAutomatas[automatNum];
        }

        //Help for minimization and stuff
        //Add dead state so minimization doesn't cause any errors
        private void addDeadState()
        {
            string deadState = "DEADSTATE" + id;
            bool first = true;
            bool isDeadStateThere = false;
            for(int i = 0; i < states.Count; i++)
            {
                foreach(var symbol in alphabet)
                {
                    if(!checkIfIsENKA())
                    {
                        if (!delta.ContainsKey((states.ElementAt(i), symbol)))
                        {
                            isDeadStateThere = true;
                            this.states.Add(deadState);
                            delta[(states.ElementAt(i), symbol)] = deadState;
                        }
                    }
                    else
                    {
                        if(!deltaForEpsilon.ContainsKey((states.ElementAt(i), symbol)))
                        {
                            if(first)
                            {
                                isDeadStateThere = true;
                                counter = 0;
                                this.states.Add(deadState);
                                first = false;
                            }
                            ESwitching(states.ElementAt(i), symbol, deadState);
                        }
                    }
                }
            }
            //NEWLY ADDED - TEST
            if(isDeadStateThere)
            {
                foreach(var symbol in alphabet)
                {
                    if(!checkIfIsENKA())
                    {
                        delta[(deadState, symbol)] = deadState;
                    }
                    else
                    {
                        if(symbol != 'E')
                        {
                            ESwitching(deadState, symbol, deadState);
                        }
                    }
                }
            }
            
        }

        //To check if two regular expressions are the same
        //Transform them to automatas
        //Compare those two automatas
        public bool checkIfTwoRegularExpressionsAreTheSame(string regexp1, string regexp2)
        {
            Automat convertFirstRegularExpression = transformRegularExpressionToAutomata(regexp1);
            Automat convertSecondRegularExpression = transformRegularExpressionToAutomata(regexp2);

            if(convertFirstRegularExpression.compareTwoAutomatas(convertSecondRegularExpression))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}
