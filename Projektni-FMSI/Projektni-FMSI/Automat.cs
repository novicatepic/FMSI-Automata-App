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
        private List<string>[] listsOfStrings;
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
                            //if(this.delta.TryGetValue())
                            //result.delta[(state, symbol)] = this.delta[(state, symbol)];
                            if(symbol != 'E')
                            {
                                result.addTransitionENKA(state, symbol, delta[(state, symbol)]);
                            }                         
                        }
                        else
                        {
                            //result.delta[(state, symbol)] = other.StartState;
                            result.addTransitionENKA(state, 'E', other.StartState);
                        }
                    }
                }
                foreach(var state in other.states)
                {
                    foreach(var symbol in other.alphabet)
                    {
                        //result.delta[(state, symbol)] = other.delta[(state, symbol)];
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
            //result.deltaForEpsilon[("NSS", 'E')].Add(result.finalStates.ElementAt(0));
            //result.deltaForEpsilon[("NSS", 'E')].Add(this.StartState);


            
            foreach(var finalState in this.finalStates)
            {
                result.addTransitionENKA(finalState, 'E', "NFS");
                result.addTransitionENKA(finalState, 'E', StartState);

                //result.deltaForEpsilon[(finalState, 'E')].Add("NFS");
                //result.deltaForEpsilon[(finalState, 'E')].Add(this.StartState);
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

        private bool AcceptsDKA(string input)
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
                                listsOfStrings = new List<string>[states.Count];
                                for(int i = 0; i < states.Count; i++)
                                {
                                    listsOfStrings[i] = new List<string>();
                                }
                                counter++;
                            }
                            addTransitionENKA(state, symbol, nextState);
                        }
                    }
                } while (inputHelp != "--exit");            
            }
        }

        //static int i = 0;
        private void addTransitionENKA(string currentState, char symbol, string nextState)
        {
            if (counter == 0)
            {
                listsOfStrings = new List<string>[states.Count];
                for (int j = 0; j < states.Count; j++)
                {
                    listsOfStrings[j] = new List<string>();
                }
                counter++;
            }

            int i;
            string[] array = states.ToArray<string>();
            for(i = 0; i < states.Count; i++)
            {
                if(currentState == array[i]) {
                    deltaForEpsilon[(currentState, symbol)] = listsOfStrings[i];
                    break;
                }
            }

            if(symbol == 'E')
            {
               // if(!deltaForEpsilon.ContainsKey((currentState, symbol)))
                    //deltaForEpsilon[(currentState, symbol)] = listsOfStrings[i];
                if(!deltaForEpsilon[(currentState, symbol)].Contains(nextState))
                    deltaForEpsilon[(currentState, symbol)].Add(nextState);
            }
            else
            {
                delta[(currentState, symbol)] = nextState;
            }
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
            Console.WriteLine("E STUFF: ");
            for(int i = 0; i < states.Count; i++)
            {
                foreach(var s in listsOfStrings[i])
                {
                    Console.Write(s + " ");
                }
                Console.WriteLine();
            }
        }

        private int getNumberOfStates()
        {
            int counter = 0;
            foreach(var state in states)
            {
                counter++;
            }
            return counter;
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
                        result = false;
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

            for(int a = 0; a < states.Count; a++, Console.WriteLine())
            {
                for(int b = 0; b < states.Count; b++)
                {
                    Console.Write(ms[a, b] + " ");
                }
            }

        }

    }

}
