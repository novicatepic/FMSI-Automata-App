using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    public class LexicalAnalysis
    {
        private string[] stringToAnalayse;

        public LexicalAnalysis(string[] analyseString)
        {
            stringToAnalayse = analyseString;
        }

        //Prints out found tokens
        //And errors
        //Probably shouldn't print out tokens found, just for fun
        public bool lexicalAnalysisForRegularExpression(string[] regularExpression)
        {
            bool errorFound = false;
            LexerForRegularExpression lexer = new(regularExpression);
            bool temp = true;

            while(temp)
            {
                TokenRegEx token = lexer.Next();
                if (token == null)
                {
                    errorFound = true;
                    Console.WriteLine();
                    Console.WriteLine("Error at line: " + (lexer.getSourcePosition() - 1));
                    Console.WriteLine();
                }
                else if (token.Type.Equals("EOF"))
                {
                    temp = false;
                }
                else
                {
                    Console.Write("Token recognized at line :" + (lexer.getSourcePosition() - 1));
                    token.printToken();
                    Console.WriteLine();
                }
            }

            return errorFound;
        }

        //Same as for last function, only this applies to automatas
        public bool lexicalAnalysisForAutomata(string[] automataExpression)
        {
            bool errorFound = false;
            LexerForAutomata lexer = new(automataExpression);
            bool temp = true;

            while(temp)
            {
                TokenAutomata token = lexer.Next();
                if(token == null)
                {
                    errorFound = true;
                    Console.WriteLine("Error at line: " + (lexer.getSourcePosition() - 1));
                }
                else if(token.Type.Equals("EOF"))
                {
                    temp = false;
                }
                else
                {
                    Console.Write("Token recognized at line :" + (lexer.getSourcePosition() - 1));
                    token.printToken();
                    Console.WriteLine();
                }
            }
            return errorFound;
        }
    }
}
