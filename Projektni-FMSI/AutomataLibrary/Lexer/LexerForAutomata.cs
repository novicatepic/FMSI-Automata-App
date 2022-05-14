using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    public class LexerForAutomata : ILexer
    {
        //Where we read from
        private readonly string[] source;

        //Help stuff
        private bool isInStates = false;
        private bool isInAlphabet = false;
        private bool isInTransitions = false;
        private bool isInFinalStates = false;
        private readonly string stateString = "AUTOMATA-STATES:";
        private readonly string alphabetString = "ALPHABET:";
        private readonly string transitionString = "DELTA TRANSITIONS:";
        private readonly string finalStateString = "FINAL STATES:";

        public LexerForAutomata(string[] input)
        {
            sourcePosition = 0;
            source = input;
        }

        //Reads tokens
        //States go like this
        //AUTOMATA-STATES: then in each line following this line we put states
        //ALPHABET: after this we put alphabet stuff
        //DELTA TRANSITIONS: after this we put delta transition like this - fromState:symbol:toState
        //FINAL STATES: after this we put our final states
        //Logic pretty much the same as you've showed us
        public override TokenAutomata Next()
        {
            //throw new NotImplementedException();
            if(sourcePosition >= source.Length)
            {
                return new TokenAutomata("EOF", null);
            }
            else if(sourcePosition > source.Length)
            {
                return null;
            }

            while (sourcePosition < source.Length && source[sourcePosition] == "")
            {
                sourcePosition++;
            }

            if(source[sourcePosition].Equals(stateString))
            {
                sourcePosition++;
                isInStates = true;
            }

            if(isInStates)
            {
                if(source[sourcePosition].Equals(alphabetString))
                {
                    sourcePosition++;
                    isInStates = false;
                    isInAlphabet = true;
                }
                else
                {
                    if (TokenAutomata.isStateTokenValid(source[sourcePosition]))
                    {
                        sourcePosition++;
                        return new TokenAutomata("state", source[sourcePosition - 1]);
                    }
                    else
                    {
                        sourcePosition++;
                        return null;
                    }
                }
            }

            if(isInAlphabet)
            {
                if(source[sourcePosition].Equals(transitionString))
                {
                    sourcePosition++;
                    isInAlphabet = false;
                    isInTransitions = true;
                }
                else
                {
                    if (TokenAutomata.isAlphabetTokenValid(source[sourcePosition]))
                    {
                        sourcePosition++;
                        return new TokenAutomata("symbol", source[sourcePosition - 1]);
                    }
                    else
                    {
                        sourcePosition++;
                        return null;
                    }
                }
            }

            if (isInTransitions)
            {
                if (source[sourcePosition].Equals(finalStateString))
                {
                    sourcePosition++;
                    isInTransitions = false;
                    isInFinalStates = true;
                }
                else
                {
                    string[] splitStates = source[sourcePosition].Split(':');
                    if (splitStates.Length != 3)
                    {
                        sourcePosition++;
                        return null;
                    }
                    else
                    {
                        if (TokenAutomata.isStateTokenValid(splitStates[0]) && TokenAutomata.isAlphabetTokenValid(splitStates[1]) && TokenAutomata.isStateTokenValid(splitStates[2]))
                        {
                            sourcePosition++;
                            return new TokenAutomata("delta-transition", source[sourcePosition - 1]);
                        }
                        else
                        {
                            sourcePosition++;
                            return null;
                        }
                    }
                }               
            }

            if (isInFinalStates)
            {
                if(sourcePosition == source.Length)
                {
                    sourcePosition++;
                    isInFinalStates = false;
                }
                else
                {
                    if (TokenAutomata.isStateTokenValid(source[sourcePosition]))
                    {
                        sourcePosition++;
                        return new TokenAutomata("final-state", source[sourcePosition - 1]);
                    }
                    else
                    {
                        sourcePosition++;
                        return null;
                    }
                }
            }

            sourcePosition++;
            return null;
        }
    }
}
