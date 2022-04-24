using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    public class Automat
    {
        private Dictionary<(string, char), string> delta = new();
        private HashSet<string> finalStates = new();
        private string StartState { get; set; }
        private HashSet<char> alphabet = new();
        private HashSet<string> states = new();

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
                        Console.WriteLine("Accepted!");
                    }
                    else
                    {
                        Console.WriteLine("Not accepted!");
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

        private void addTransition(string currentState, char symbol, string nextState)
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

        /*public Automat convertDKAtoENKA()
        {
            Automat result = new Automat();
            foreach(var state in states)
            {

            }

            return result;
        }*/

        /*private HashSet<string> returnEClosure(string state)
        {
            HashSet<string> result = new();
            int size = states.Count;
            String[] array = states.ToArray();
            int[] visit = new int[size];

            for(int i = 0; i < states.Count; i++)
            {
                visit[i] = 0;
            }



            return result;
        } */

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
                        addTransition(state, symbol, nextState);
                    }
                } while (inputHelp != "--exit");            
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

    }




}
