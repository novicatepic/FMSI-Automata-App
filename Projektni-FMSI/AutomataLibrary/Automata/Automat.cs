using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projektni_FMSI;
using System.IO;
using Projektni_FMSI.Exceptions;

namespace Projektni_FMSI
{
    public class Automat
    {
        //everything is public because it was easier to test in main, if I had some extra time, I'd correct it!
        //corrected to private
        private Dictionary<(string, char), string> delta = new();
        private Dictionary<(string, char), List<string>> deltaForEpsilon = new();
        private HashSet<string> finalStates = new();
        private string StartState { get; set; }
        private HashSet<char> alphabet = new();
        private HashSet<string> states = new();
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

        //Name is self explanatory
        //Calls three help functions
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
            if (!checkIfInputWordIsCorrect(inputWord))
            {
                throw new IncorrectInputWordException();
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
                //Automat ENKATODKA = this.convertENKAtoDKA();
                //if (ENKATODKA.AcceptsDKA(inputWord))
                if(acceptsENKA(inputWord))
                {
                    Console.WriteLine("ENKA accepted this word ;)!");
                }
                else
                {
                    Console.WriteLine("ENKA didn't accept this word :(!");
                }
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
                if (!alphabet.Contains(symbol))
                {
                    throw new AlphabetNotContainsException();
                }
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

        //Find union for any language representation
        //Name contains DKA because it's not other union which separates two languages with E
        public Automat findUnionDKA(Automat other)
        {
            Console.WriteLine("This representation will return a new automata, even if you input two regular expressions or any other combination!");

            Automat first = new();
            first = convertToDKAIfNecessary();

            other = other.convertToDKAIfNecessary();

            first.addDeadState();
            other.addDeadState();

            if (!first.checkIfAlphabetIsTheSame(other))
            {
                throw new Exception("Alphabet is not the same, therefore union can't be done!");
            }

            Automat result = new Automat();

            //Add every symbol from alphabet!
            //result.alphabet = alphabet causes problems!
            foreach (var symbol in first.alphabet)
            {
                result.alphabet.Add(symbol);
            }

            foreach (var state1 in first.states)
            {
                foreach (var state2 in other.states)
                {
                    //Make start state
                    if (first.StartState == state1 && other.StartState == state2)
                    {
                        result.StartState = state1 + state2;
                    }

                    //Make new state
                    string newState = state1 + state2;
                    result.states.Add(newState);

                    //Connect deltas from both automatas
                    foreach (var symbol in first.alphabet)
                    {
                        result.delta[(newState, symbol)] = first.delta[(state1, symbol)] + other.delta[(state2, symbol)];
                    }

                    //If at least one automata has that final state, add it to final states (union)
                    if (first.finalStates.Contains(state1) || other.finalStates.Contains(state2))
                    {
                        result.finalStates.Add(newState);
                    }
                }
            }

            result = result.minimiseAutomata();

            return result;
        }

        //Help function which allows us to make language representation
        //It's gonna be converted to DKA, easier to work
        public static void makeLanguagesForUnionIntersectionDifference(ref Automat other)
        {
            Console.WriteLine("Make your choice for language representation: ");
            Console.WriteLine("a for automata, r for regular expression: ");
            string userInput = Console.ReadLine();
            if (userInput == "a")
            {
                other.makeAutomata();
                if (other.checkIfIsENKA())
                {
                    Automat temp = other.convertENKAtoDKA();
                    other = temp;
                }
            }
            else if (userInput == "r")
            {
                Console.WriteLine("Input your regular expression: ");
                string regularExpression = Console.ReadLine();
                if (checkIfTwoPlusesAreNextToEachOther(regularExpression) || checkIfEndsWithPlus(regularExpression) || checkIfPlusIsBeforeClosedBracket(regularExpression) ||
                    !checkPositionOfBrackets(regularExpression) || !checkIfRegularExpressionIsCorrect(regularExpression))
                {
                    Console.WriteLine("You've made (a) silly mistake(s), try it again!");
                }
                else if (checkHowManyBrackets(regularExpression, '(') != checkHowManyBrackets(regularExpression, ')'))
                {
                    Console.WriteLine("Not the same number of opening and closed brackets, try again!");
                }

                else
                {
                    other = makeAutomataFromRegularExpression(regularExpression);
                    if (other.checkIfIsENKA())
                    {
                        Automat temp = other.convertENKAtoDKA();
                        other = temp;
                    }
                }
            }
            else
            {
                throw new Exception("Incorrent input, have to throw an exception!");
            }
        }

        //Same logic I used for union, except for final states
        public Automat findIntersection(Automat other)
        {
            Console.WriteLine("This representation will return a new automata, even if you input two regular expressions or any other combination!");

            this.addDeadState();
            other.addDeadState();

            Automat first = new();
            first = convertToDKAIfNecessary();

            other = other.convertToDKAIfNecessary();

            if (!first.checkIfAlphabetIsTheSame(other))
            {
                throw new Exception("Alphabet is not the same, therefore union can't be done!");
            }

            Automat result = new Automat();
            foreach (var element in first.alphabet)
            {
                result.alphabet.Add(element);
            }


            foreach (var state1 in first.states)
            {
                foreach (var state2 in other.states)
                {
                    if (first.StartState == state1 && other.StartState == state2)
                    {
                        result.StartState = state1 + state2;
                    }

                    string newState = state1 + state2;
                    result.states.Add(newState);

                    if (first.finalStates.Contains(state1) && other.finalStates.Contains(state2))
                    {
                        result.finalStates.Add(newState);
                    }

                    foreach (var symbol in first.alphabet)
                    {
                        result.delta[(newState, symbol)] = first.delta[(state1, symbol)] + other.delta[(state2, symbol)];
                    }
                }
            }

            result = result.minimiseAutomata();

            return result;
        }

        //Same as two functions before this one, final states are the only difference!
        public Automat findDifference(Automat other)
        {
            Console.WriteLine("This representation will return a new automata, even if you input two regular expressions or any other combination!");

            Automat first = new();
            first = convertToDKAIfNecessary();

            other = other.convertToDKAIfNecessary();

            if (!first.checkIfAlphabetIsTheSame(other))
            {
                throw new Exception("Alphabet is not the same, therefore union can't be done!");
            }

            first.addDeadState();
            other.addDeadState();

            Automat result = new Automat();
            foreach (var element in first.alphabet)
            {
                result.alphabet.Add(element);
            }
            foreach (var state1 in first.states)
            {
                foreach (var state2 in other.states)
                {
                    if (first.StartState == state1 && other.StartState == state2)
                    {
                        result.StartState = state1 + state2;
                    }

                    string newState = state1 + state2;
                    result.states.Add(newState);

                    if ((first.finalStates.Contains(state1) && !other.finalStates.Contains(state2)) || (!first.finalStates.Contains(state1) && other.finalStates.Contains(state2)))
                    {
                        result.finalStates.Add(newState);
                    }

                    foreach (var symbol in first.alphabet)
                    {
                        result.delta[(newState, symbol)] = first.delta[(state1, symbol)] + other.delta[(state2, symbol)];
                    }

                }
            }

            result = result.minimiseAutomata();

            return result;
        }

        //"Merge" two representations of language
        public Automat connectLanguages(Automat other)
        {
            Automat result = new();

            Automat first = new();
            first = convertToDKAIfNecessary();

            other = other.convertToDKAIfNecessary();

            try
            {
                if (!first.checkIfAlphabetIsTheSame(other))
                {
                    throw new Exception("I can't merge two languages that don't have the same alphabet, sorry!");
                }
                result.StartState = first.StartState;

                //It's gonna contain 'E' by definiton
                if (!result.alphabet.Contains('E'))
                {
                    result.alphabet.Add('E');
                }
                foreach (var symbol in first.alphabet)
                {
                    result.alphabet.Add(symbol);
                }
                //It's gonna have same final states as second automata, by definition
                foreach (var finalState in other.finalStates)
                {
                    result.finalStates.Add(finalState);
                }
                foreach (var state in first.states)
                {
                    result.states.Add(state);
                }
                foreach (var state in other.states)
                {
                    result.states.Add(state);
                }

                foreach (var state in first.states)
                {
                    foreach (var symbol in first.alphabet)
                    {
                        //If it's not final state from first automata, delta function does the same as before!
                        if (!first.finalStates.Contains(state))
                        {
                            if (symbol != 'E')
                            {
                                first.helpForConnectingAndStuff(result, state, symbol);
                            }
                        }
                        else
                        {
                            //If it's final state, it's gonna connect to start state of second automata with E
                            result.ESwitching(state, 'E', other.StartState);
                            foreach (var symb in first.alphabet)
                            {
                                //If it's not E, do the same as before!
                                if (symb != 'E')
                                {
                                    first.helpForConnectingAndStuff(result, state, symbol);
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

        //Help function
        private Automat convertToDKAIfNecessary()
        {
            Automat first;
            if (this.checkIfIsENKA())
            {
                first = this.convertENKAtoDKA();
            }
            else
            {
                first = this;
            }

            return first;
        }

        //Help function
        //Allows us to make E-Switch, no matter if other automata is DKA or ENKA
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
        //Multiple things when doing unions, intersections n stuff does weird print outs, too many states etc
        //Looks very ugly to be honest
        public static Automat chainOperations()
        {
            Automat result = new();
            Console.WriteLine("Options for chaining operations:\n1-Union\n2-Intersection\n3-Difference\n4-Complement\n5-Connection\n6-KleeneStar\n");
            string input;
            Console.WriteLine("Enter your options until it becomes boring for you, if it becomes boring, input --exit: ");
            Console.WriteLine("Make first language: ");
            Automat a1 = new Automat();
            Automat a2 = new();
            makeLanguagesForUnionIntersectionDifference(ref a1);
            Automat res = null;
            Console.WriteLine("Input option: ");
            do
            {
                input = Console.ReadLine();
                if (input == "1")
                {
                    if (res == null)
                    {
                        Console.WriteLine("Make second language: ");
                        makeLanguagesForUnionIntersectionDifference(ref a2);
                        res = a1.findUnionDKA(a2);
                        res.printStatesAndAlphabet();
                    }

                    else
                    {
                        a2 = null;
                        a2 = new();
                        makeLanguagesForUnionIntersectionDifference(ref a2);
                        res = res.findUnionDKA(a2);
                        res.printStatesAndAlphabet();
                    }

                }
                else if (input == "2")
                {
                    if (res == null)
                    {
                        Console.WriteLine("Make second language: ");
                        makeLanguagesForUnionIntersectionDifference(ref a2);
                        res = a1.findIntersection(a2);
                        res.printStatesAndAlphabet();
                    }

                    else
                    {
                        a2 = null;
                        a2 = new();
                        makeLanguagesForUnionIntersectionDifference(ref a2);
                        res = res.findIntersection(a2);
                        res.printStatesAndAlphabet();
                    }
                }
                else if (input == "3")
                {
                    if (res == null)
                    {
                        Console.WriteLine("Make second language: ");
                        makeLanguagesForUnionIntersectionDifference(ref a2);
                        res = a1.findDifference(a2);
                        res.printStatesAndAlphabet();
                    }
                    else
                    {
                        a2 = null;
                        a2 = new();
                        makeLanguagesForUnionIntersectionDifference(ref a2);
                        res = res.findDifference(a2);
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
                        Console.WriteLine("Make second language: ");
                        makeLanguagesForUnionIntersectionDifference(ref a2);
                        res = a1.connectLanguages(a2);
                        res.printStatesAndAlphabet();
                    }
                    else
                    {
                        a2 = null;
                        a2 = new();
                        makeLanguagesForUnionIntersectionDifference(ref a2);
                        Automat temp = new();
                        temp = res.connectLanguages(a2);
                        res = temp;
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
        public bool checkIfIsENKA()
        {
            return alphabet.Contains('E');
        }

        //Self-explanatory
        public void setDelta(string currentState, char symbol, string nextState)
        {

            if (!states.Contains(currentState) && !states.Contains(nextState) && !alphabet.Contains(symbol))
            {
                throw new InvalidTransitionInputException();
            }
            delta[(currentState, symbol)] = nextState;

        }

        //Does DKA accept a certain word?
        public bool AcceptsDKA(string input)
        {
            var currentState = StartState;
            foreach (var symbol in input)
            {
                if (!alphabet.Contains(symbol))
                {
                    throw new AlphabetNotContainsException();
                }
                currentState = delta[(currentState, symbol)];
            }
            return finalStates.Contains(currentState);
        }

        //Construct complement -> switch all non-final states to final
        //And switch all final states to non-final
        public Automat constructComplement()
        {
            Automat result = new();
            result.StartState = StartState;

            foreach(var symbol in alphabet)
            {
                result.alphabet.Add(symbol);
            }

            foreach(var state in states)
            {
                result.states.Add(state);
            }

            if(checkIfIsENKA())
            {
                foreach(var symbol in alphabet)
                {
                    foreach(var state in states)
                    {
                        if(deltaForEpsilon.ContainsKey((state, symbol)))
                        {
                            foreach(var s in deltaForEpsilon[(state, symbol)])
                            {
                                result.ESwitching(state, symbol, s);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach(var symbol in alphabet)
                {
                    foreach(var state in states)
                    {
                        if(delta.ContainsKey((state, symbol)))
                        {
                            result.delta[(state, symbol)] = delta[(state, symbol)];
                        }
                    }
                }
            }

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
            Console.WriteLine("You can't enter strings, only char symbols for alphabet!");
            Console.WriteLine("--exit to exit input loop!\nEnter your alphabet (if you enter E as an symbol, it's E-NKA): ");
            string inputString = "";
            do
            {
                inputString = Console.ReadLine();
                if (inputString != "--exit")
                {
                    if (inputString.Length > 1)
                    {
                        throw new AlphabetSizeException();
                    }
                    char symbol = char.Parse(inputString);
                    if (symbol.Equals(':'))
                    {
                        throw new IncorrectAlphabetSymbolException();
                    }
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
                Console.WriteLine("Enter state: ");
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
            //printStatesAndAlphabet();
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
                                setDelta(state, symbol, nextState + id);
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
                            Console.WriteLine("Destination state doesn't exists, couldn't do what you wanted, sorry, try again!");
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
        public void ESwitching(string currentState, char symbol, string nextState)
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
                if(!deltaForEpsilon.ContainsKey((currentState, symbol)))
                {
                    throw new NoKeyFoundException();
                }

                if (!deltaForEpsilon[(currentState, symbol)].Contains(nextState))
                    deltaForEpsilon[(currentState, symbol)].Add(nextState);
            }
            else
            {
                if(!deltaForEpsilon.ContainsKey((currentState, symbol)))
                {
                    throw new NoKeyFoundException();
                }

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
        //There is a version that doesn't print id!
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
            Console.WriteLine("FINAL STATES: ");
            foreach (var finalState in finalStates)
            {
                Console.Write(finalState + " ");
            }
            Console.WriteLine();
        }

        //For each final state, if it's stuck in itself, it means language is not final
        //Help function
        private bool checkForAnyFinalState()
        {
            int counter = 0;
            foreach (var finalState in finalStates)
            {
                counter = 0;
                foreach (var symbol in alphabet)
                {
                    if (!delta.ContainsKey((finalState, symbol)))
                    {
                        counter++;
                    }
                }
                if (counter == alphabet.Count)
                {
                    return true;
                }
            }
            return false;
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

            if (convert.checkForAnyFinalState())
            {
                return false;
            }

            if (convert.finalStates.Count == 0 || convert.finalStates.Count == convert.states.Count)
            {
                return false;
            }

            if (!isLanguageFinalHelpFunc() /*|| !secondCheckIfLanguageIsFinal()*/)
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
                if (automatGraph.dfsForLongestWord(finalState, this))
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
            return true;
        }

        //NOT SURE ABOUT THIS FUNC, TAKE A LOOK LATER
        /*private bool secondCheckIfLanguageIsFinal()
        {
            int counter = 0;
            foreach (var finalState in finalStates)
            {
                foreach (var symbol in alphabet)
                {
                    if (!delta.ContainsKey((finalState, symbol)))
                    {
                        counter++;
                    }
                }
            }
            if (counter == alphabet.Count)
            {
                return false;
            }
            else
            {
                return true;
            }
        }*/

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
                    if (delta.ContainsKey((temp, symbol)) && delta[(temp, symbol)] != temp)
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

        //Self explanatory function name
        //Same logic implemented as we've practiced live
        public Automat convertENKAtoDKA()
        {
            Automat DKA = new();
            //ADDED
            //Ads dead state, easier to convert
            //Otherwise, a lot of errors
            //Dead state doesn't affect anything in here!
            //this.addDeadState();
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
        //Returns infinity if there is auto-transition in any state between start state and final state, no matter how many final states are there
        //If there is a cycle from final state onwards, return infinity
        //Else it finds longest word
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
            if (!isLanguageFinalHelpFunc() /*|| !secondCheckIfLanguageIsFinal()*/)
            {
                Console.WriteLine("It's infinity, even thought here we don't have infinity, I'll just print out the biggest possible number");
                return int.MaxValue;
            }

            //Make standard graph
            possibleDKA.makeGraphFromAutomataWithAllConnections(automatGraph);

            //If there is a cycle, longest word is inifity
            foreach (var finalState in possibleDKA.finalStates)
            {
                if (automatGraph.dfsForLongestWord(finalState, this))
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

        //Some getters
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
                convertFirst.addDeadState();
            }
            else
            {
                this.addDeadState();
            }
            if (other.checkIfIsENKA())
            {
                convertSecond = other.convertENKAtoDKA();
                convertSecond.addDeadState();
            }
            else
            {
                other.addDeadState();
            }

            //convertFirst.printStatesAndAlphabet();
            //convertSecond.printStatesAndAlphabet();

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
                    //Console.WriteLine(first.delta[(state, symbol)] + " " + second.delta[(state, symbol)]);
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
            while (automatGraph.minimiseAutomataHelper(this)) { }

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
                }
            }

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
                        fullMinimization.Add(statesToMinimize.ElementAt(i));
                    }
                    else
                    {
                        string temp2 = "";
                        for (int g = 0; g < temp.Length - 1; g++)
                        {
                            temp2 += temp[g];
                        }
                        fullMinimization.Add(temp2);
                    }
                }
            }

            //We got our states which we can fully minimize
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

            makeMinimisedAutomataWithoutSeparators(minimized);

            //minimized.delta = newDelta;

            return minimized;
        }

        private void makeMinimisedAutomataWithoutSeparators(Automat minimized)
        {
            HashSet<string> newStates = new();
            HashSet<string> newFinalSt = new();
            Dictionary<(string, char), string> newDelta = new();
            foreach (var state in minimized.states)
            {
                bool isFinalState = false;
                bool isStartState = false;
                string temp = "";
                if (state.Contains(':'))
                {
                    string[] splitStates = state.Split(':');
                    foreach (var s in splitStates)
                    {
                        if (StartState.Contains(s))
                        {
                            isStartState = true;
                        }
                        if (finalStates.Contains(s))
                        {
                            isFinalState = true;
                        }
                        if (s != "")
                            temp += s;
                    }
                    newStates.Add(temp);
                    if (isFinalState)
                    {
                        newFinalSt.Add(temp);
                        isFinalState = false;
                    }
                    if (isStartState)
                    {
                        minimized.StartState = temp;
                    }
                }
                else
                {
                    newStates.Add(state);
                    if (minimized.finalStates.Contains(state))
                    {
                        newFinalSt.Add(state);
                    }
                }
            }

            foreach (var symbol in minimized.alphabet)
            {
                foreach (var state in minimized.states)
                {
                    if (minimized.delta.ContainsKey((state, symbol)))
                    {
                        string[] splitFirst = state.Split(':');
                        string stateFoundFrom = findStateForMinimisation(newStates, splitFirst[0]);
                        string stateTo = minimized.delta[(state, symbol)];
                        string[] splitSecond = stateTo.Split(':');
                        string stateFoundTo = findStateForMinimisation(newStates, splitSecond[0]);
                        newDelta[(stateFoundFrom, symbol)] = stateFoundTo;
                    }
                }
            }

            minimized.states.Clear();
            minimized.finalStates.Clear();
            minimized.delta.Clear();

            foreach (var elem in newStates)
            {
                minimized.states.Add(elem);
            }

            foreach (var elem in newFinalSt)
            {
                minimized.finalStates.Add(elem);
            }

            foreach (var state in minimized.states)
            {
                foreach (var symbol in minimized.alphabet)
                {
                    if (newDelta.ContainsKey((state, symbol)))
                    {
                        minimized.delta[(state, symbol)] = newDelta[(state, symbol)];
                    }
                }
            }
        }

        private static string findStateForMinimisation(HashSet<string> newStates, string state)
        {
            for(int i = 0; i < newStates.Count; i++)
            {
                if(newStates.ElementAt(i).Contains(state))
                {
                    return newStates.ElementAt(i);
                }
            }
            return null;
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

            //Print out reachable states, test
            SortedSet<string> nodesVisited = automatGraph.dfs(StartState);

            return nodesVisited;
        }

        //Help functions for regular expressions
        private static int checkHowManyBrackets(string input, char symbol)
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

        private static bool checkIfRegularExpressionIsCorrect(string input)
        {
            HashSet<char> alphabet = makeAlphabet(input);
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

        private static bool checkPositionOfBrackets(string input)
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

        private static bool checkIfStartsWithPlus(string input)
        {
            return input[0] == '+';
        }

        private static bool checkIfPlusIsBeforeClosedBracket(string input)
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

        private static bool checkIfEndsWithPlus(string input)
        {
            return (input[input.Length - 1] == '+');
        }

        private static bool checkIfTwoPlusesAreNextToEachOther(string input)
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

        //Function to find union (a+b) for example
        //Done by definition
        public Automat findUnionBetweenTwoLanguages(Automat other)
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

        //Make "alphabet" based on regular expression
        private static HashSet<char> makeAlphabet(string regexp)
        {
            HashSet<char> alphabet = new();

            foreach (var symbol in regexp)
            {
                if (symbol != '(' && symbol != ')' && symbol != '*' && symbol != '+' && symbol != '|')
                {
                    alphabet.Add(symbol);
                }
            }

            return alphabet;
        }

        //Help function for transforming regex to automata
        //Infix to postfix transition helper
        private static int getInputElementPriority(char element)
        {
            if (element == ')')
            {
                return 1;
            }
            if (element == '(')
            {
                return 6;
            }
            if (element == '|')
            {
                return 4;
            }
            if (element == '+')
            {
                return 3;
            }
            return -1;
        }

        //Same as for the last function
        private static int getStackElementPriority(char element)
        {
            if (element == '(')
            {
                return 0;
            }
            if (element == '+')
            {
                return 3;
            }
            if (element == '|')
            {
                return 4;
            }

            return -1;
        }

        //Converting from infix to postfix because it's easier for us
        //There is an evaluate postfix function that checks if regular expression is correct and evaluates our expression
        private static string convertInfixToPostfix(string expression)
        {
            string converted = "";
            Stack<string> stack = new Stack<string>();
            int rank = 0;
            //char first = expression[0];
            int i = 0;
            HashSet<char> alphabet = makeAlphabet(expression);

            while (i < expression.Length)
            {
                //Console.Write(expression[i]);
                if (alphabet.Contains(expression[i]) || expression[i] == '*')
                {
                    converted += expression[i];
                    rank = rank + 1;
                    if (expression[i] == '*')
                    {
                        rank = rank - 1;
                    }
                }
                else
                {
                    while (stack.Count != 0 && getInputElementPriority(expression[i]) <= getStackElementPriority(char.Parse(stack.Peek())))
                    {
                        string element = stack.Pop();
                        converted += element;
                        if (element != ")" && element != "(")
                        {
                            rank -= 1;
                        }
                        if (rank < 1)
                        {
                            throw new Exception("RANK < 1!");
                        }
                    }
                    if (expression[i] != ')')
                    {
                        stack.Push(expression[i].ToString());
                    }
                    else
                    {
                        string element = stack.Pop();
                    }
                }
                i++;
            }
            while (stack.Count != 0)
            {
                string elem = stack.Pop();
                if (elem != "")
                {
                    converted += elem;
                    rank -= 1;
                }
            }
            if (rank != 1)
            {
                throw new Exception("Not correct!");
            }

            return converted;
        }

        //Help for user, so he can put for example ab and I add (a|b)
        //Could be deadly for me, because I might have forgotten some condition in if statement
        private static string addConcatenationSigns(string regularExpression, HashSet<char> alphabet)
        {
            string newString = "";
            bool first = true;
            int counter = 0;
            for (int i = 0; i < regularExpression.Length; i++)
            {
                if (i + 1 < regularExpression.Length &&
                    ((alphabet.Contains(regularExpression[i]) && alphabet.Contains(regularExpression[i + 1])) ||
                    (alphabet.Contains(regularExpression[i]) && regularExpression[i + 1] == '(') ||
                    (regularExpression[i] == '*' && alphabet.Contains(regularExpression[i + 1])) ||
                    (regularExpression[i] == ')' && alphabet.Contains(regularExpression[i + 1])) ||
                    (regularExpression[i] == '*' && regularExpression[i + 1] == '(')))
                {
                    if (first)
                    {
                        newString = regularExpression.Insert(i + 1, "|");
                        first = false;
                        counter++;
                    }
                    else
                    {
                        newString = newString.Insert(i + 1 + counter, "|");
                        counter++;
                    }
                }
            }
            return newString;
        }

        //Function that does the transformation
        //Same as the function that we've done on another subject, SPA -> Evaluate postfix algorithm
        //Corrected in that sense that we make automatas and push them on stack
        //And as operations come, we pop from stack, either one operand if it's unary operator, or two if it's binary operator
        //Operations are done and pushed back to stack
        public static Automat makeAutomataFromRegularExpression(string regularExpression)
        {
            Automat result = new();
            HashSet<char> alphabet = makeAlphabet(regularExpression);
            string regExp = addConcatenationSigns(regularExpression, alphabet);
            regularExpression = regExp;
            //Console.WriteLine(regularExpression);
            string convert = convertInfixToPostfix(regularExpression);
            Stack<Automat> automataStack = new();
            int i = 0;
            while (i < convert.Length)
            {
                char element = convert[i];
                if (alphabet.Contains(element))
                {
                    Automat temp = new();
                    temp.StartState = "q0" + temp.id;
                    string newState = "q1" + temp.id;
                    temp.states.Add(temp.StartState);
                    temp.states.Add(newState);
                    temp.finalStates.Add(newState);
                    temp.alphabet.Add('E');
                    foreach (var symbol in alphabet)
                    {
                        temp.alphabet.Add(symbol);
                    }
                    temp.ESwitching(temp.StartState, element, newState);
                    automataStack.Push(temp);
                }
                else if (element == '*')
                {
                    Automat temp = new();
                    Automat operand = automataStack.Pop();
                    temp = operand.applyKleeneStar();
                    automataStack.Push(temp);
                }
                else if (element == '|' || element == '+')
                {
                    Automat operand2 = automataStack.Pop();
                    Automat operand1 = automataStack.Pop();
                    Automat temp = new();

                    if (element == '|')
                    {
                        temp.StartState = operand1.StartState;
                        foreach (var finalState in operand2.finalStates)
                        {
                            temp.finalStates.Add(finalState);
                        }
                        foreach (var state in operand1.states)
                        {
                            temp.states.Add(state);
                        }
                        foreach (var state in operand2.states)
                        {
                            temp.states.Add(state);
                        }
                        foreach (var symbol in operand1.alphabet)
                        {
                            temp.alphabet.Add(symbol);
                        }
                        foreach (var symbol in operand1.alphabet)
                        {
                            foreach (var state in operand1.states)
                            {
                                if (operand1.deltaForEpsilon.ContainsKey((state, symbol)))
                                {
                                    foreach (var s in operand1.deltaForEpsilon[(state, symbol)])
                                        temp.ESwitching(state, symbol, s);
                                }
                            }
                        }
                        foreach (var symbol in operand2.alphabet)
                        {
                            foreach (var state in operand2.states)
                            {
                                if (operand2.deltaForEpsilon.ContainsKey((state, symbol)))
                                {
                                    foreach (var s in operand2.deltaForEpsilon[(state, symbol)])
                                        temp.ESwitching(state, symbol, s);
                                }
                            }
                        }
                        foreach (var finalState in operand1.finalStates)
                        {
                            temp.ESwitching(finalState, 'E', operand2.StartState);
                        }
                    }
                    else
                    {
                        temp = operand1.findUnionBetweenTwoLanguages(operand2);
                    }
                    automataStack.Push(temp);
                }
                i++;
            }

            result = automataStack.Pop();
            if (automataStack.Count == 0)
            {
                //result.printStatesAndAlphabet();
                return result;
            }
            else
            {
                throw new Exception("Expression not correct, returning null!");
            }
        }

        //Help for minimization and stuff
        //Add dead state so minimization doesn't cause any errors
        private void addDeadState()
        {
            string deadState = "DEADSTATE" + id;
            bool first = true;
            bool isDeadStateThere = false;
            for (int i = 0; i < states.Count; i++)
            {
                foreach (var symbol in alphabet)
                {
                    if (!checkIfIsENKA())
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
                        if (!deltaForEpsilon.ContainsKey((states.ElementAt(i), symbol)))
                        {
                            if (first)
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
            if (isDeadStateThere)
            {
                foreach (var symbol in alphabet)
                {
                    if (!checkIfIsENKA())
                    {
                        delta[(deadState, symbol)] = deadState;
                    }
                    else
                    {
                        if (symbol != 'E')
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
        /*public bool checkIfTwoRegularExpressionsAreTheSame(string regexp1, string regexp2)
        {
            Automat convertFirstRegularExpression = makeAutomataFromRegularExpression(regexp1);
            Automat convertSecondRegularExpression = makeAutomataFromRegularExpression(regexp2);

            if (convertFirstRegularExpression.compareTwoAutomatas(convertSecondRegularExpression))
            {
                return true;
            }
            else
            {
                return false;
            }
        }*/

        //Help for finding a cycle in any state
        //Why? Helps us for finding longest word
        //If a state between start and corresponding final state contains auto-transiton somewhere, longest word is infinity!
        public bool checkIfStateHasCycle(string state)
        {
            foreach (var symbol in alphabet)
            {
                if (delta.ContainsKey((state, symbol)) && delta[(state, symbol)] == state)
                {
                    return true;
                }
            }
            return false;
        }

        //Additionaly added getters and setters, so other classes don't have direct access to attributes
        public HashSet<string> getFinalStates()
        {
            return finalStates;
        }

        public void setFinalState(string state)
        {
            if(states.Contains(state))
            {
                finalStates.Add(state);
            }
            else
            {
                throw new StateNotPresentException();
            }
        }

        public Dictionary<(string, char), string> getDelta()
        {
            return delta;
        }

        public string getElementBasedOnDelta(string state, char symbol)
        {
            return delta[(state, symbol)];
        }

        public void setStartState(string state)
        {
            StartState = state;
        }

        public void setSymbolInAlphabet(char symbol)
        {
            if(symbol == ':')
            {
                throw new IncorrectAlphabetSymbolException();
            }
            alphabet.Add(symbol);
        }

        public void setAlphabet(HashSet<char> alphabet)
        {
            foreach(var symbol in alphabet)
            {
                this.alphabet.Add(symbol);
            }
        }

        public HashSet<string> getStates()
        {
            return states;
        } 

        public void addState(string state)
        {
            states.Add(state);
        }

        public void addTransition(string state, char symbol, string nextState)
        {
            if(!states.Contains(state))
            {
                throw new StateNotPresentException();
            }
            if (!alphabet.Contains(symbol))
            {
                throw new AlphabetNotContainsException();
            }
            delta[(state, symbol)] = nextState;
        }

    }

}
