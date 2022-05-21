using System;
using System.Collections.Generic;

namespace Project_SecondApp
{
    /*public class Specification
    {
        private HashSet<Action<string>> actions = new HashSet<Action<string>>();
        public void addAction(Action<string> action)
        {
            actions.Add(action);
        }
        public void removeAction(Action<string> action)
        {
            actions.Remove(action);
        }
        public void deleteAllActions()
        {
            actions.Clear();
        }
        public void doActionsForState(string state)
        {
            foreach(var action in actions)
            {
                action.Invoke(state);
            }
        }


    }

    public class GeneratedAutomata
    {
        private string switchFirstState(Specification spec0, Specification spec1, char symbol)
        {
            string currentState = null;
            switch(symbol)
            {
                case 'a':
                    currentState = "q0";
                    spec0.doActionsForState(currentState);
                    break;
                case 'b':
                    currentState = "q1";
                    spec1.doActionsForState(currentState);
                    break;
                default:
                    throw new Exception();
            }
            return currentState;
        }

        private string switchSecondState(Specification spec0, Specification spec1, char symbol)
        {
            string currentState = null;
            switch(symbol)
            {
                case 'a':
                    currentState = "q1";
                    spec1.doActionsForState(currentState);
                    break;
                case 'b':
                    currentState = "q0";
                    spec0.doActionsForState(currentState);
                    break;
                default:
                    throw new Exception();
            }
            return currentState;
        }

        public void chainNStuff(Specification input, Specification output, Specification spec0, Specification spec1, HashSet<char> alphabet)
        {
            string initState = "q0";
            foreach(var symbol in alphabet)
            {
                if(initState == "q0")
                {
                    output.doActionsForState(initState);
                    initState = switchFirstState(spec0, spec1, symbol);
                    input.doActionsForState(initState);
                }
                else if(initState == "q1")
                {
                    output.doActionsForState(initState);
                    initState = switchSecondState(spec0, spec1, symbol);
                    input.doActionsForState(initState);
                }
                else
                {
                    throw new Exception();
                }
            }
        }

    }*/


    //Just calling func
    class ProgramSecond
    {
        static void Main(string[] args)
        {
            CodeGenerator generator = new();
            generator.generateCode();
        }
    }
}
