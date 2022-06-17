using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Projektni_FMSI.Exceptions;
using AutomataLibrary.Exceptions;

namespace Projektni_FMSI
{
    public class AutomataSpecification
    {
        //Allows user to input strings for checking from consoles
        private static HashSet<string> getStringsFromUserFromConsole()
        {
            HashSet<string> strings = new();
            Console.WriteLine("Enter strings, when you decide to stop input --exit!");
            string input;
            do
            {
                input = Console.ReadLine();
                if (input != "--exit")
                {
                    if (strings.Contains(input))
                    {
                        Console.WriteLine("You've already added this string, no need to add it again!");
                    }
                    strings.Add(input);
                }
                else { }
            } while (input != "--exit");
            return strings;
        }

        //Allows user to input strings for checking from file, if file exists
        private static HashSet<string> getStringsFromUserFromFile()
        {
            HashSet<string> strings = new();
            Console.WriteLine("Enter file name: ");
            string fileName = Console.ReadLine();
            try
            {
                var path = Path.Combine("./" + fileName + ".txt");
                string[] elements = File.ReadAllLines(path);
                Console.WriteLine("Duplicates will not be added!");
                foreach (var str in elements)
                {
                    strings.Add(str);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Error " + e.Message);
            }
            return strings;
        }

        //Load automata from file, automata is written into the file with predefined standard
        private static Automat loadAutomataSpecificationFromFile()
        {
            //private sealed string AUTOMATA_NAME = "AUTOMATA: "; doesn't work for some reason, wanted to do it this way
            Automat result = new();
            Console.WriteLine("Enter file name: ");
            string fileName = Console.ReadLine();
            try
            {
                var path = Path.Combine("./" + fileName + ".txt");
                string[] elements = File.ReadAllLines(path);
                printSpecification(elements);
                helpMethodForLoadingAutomataSpecificationFromFileOrFromCommandLine(result, elements);
            }
            catch (IOException e)
            {
                Console.WriteLine("Error " + e.Message);
            }
            return result;
        }

        //Prints, easier to stop errors
        private static void printSpecification(string[] regularExpression)
        {
            for (int i = 0; i < regularExpression.Length; i++)
            {
                Console.WriteLine("Line " + i + ": " + regularExpression[i]);
            }
        }

        //Self-explanatory
        private static Automat loadAutomataSpecificationFromCommandLine(string[] args)
        {
            Automat result = new();
            helpMethodForLoadingAutomataSpecificationFromFileOrFromCommandLine(result, args);
            return result;
        }

        //Does lexical analysis
        //If everything is good, go on, load everything supposed to be loaded
        private static void helpMethodForLoadingAutomataSpecificationFromFileOrFromCommandLine(Automat result, string[] elements)
        {
            LexicalAnalysis lexicalAnalysis = new(elements);

            if (lexicalAnalysis.lexicalAnalysisForAutomata(elements))
            {
                throw new LexicalException();
            }

            for (int i = 0; i < elements.Length;)
            {

                if (elements[i] == "AUTOMATA-STATES:")
                {
                    bool firstState = true;
                    while (elements[i] != "ALPHABET:")
                    {
                        i++;
                        if (elements[i] != "ALPHABET:" && elements[i].Length > 0)
                        {
                            if (firstState == true)
                            {
                                result.setStartState(elements[i]);
                                firstState = false;
                            }
                            result.addState(elements[i]);
                        }
                    }
                }
                else if (elements[i] == "ALPHABET:")
                {
                    while (elements[i] != "DELTA TRANSITIONS:")
                    {
                        i++;
                        if (elements[i] != "DELTA TRANSITIONS:" && elements[i].Length > 0)
                        {
                            if(elements[i].Length > 1)
                            {
                                throw new ParsingException();
                            }
                            char symbol = char.Parse(elements[i]);
                            result.setSymbolInAlphabet(symbol);
                        }
                    }
                }
                else if (elements[i] == "DELTA TRANSITIONS:")
                {
                    while (elements[i] != "FINAL STATES:")
                    {
                        i++;
                        if (elements[i] != "FINAL STATES:" && elements[i].Length > 0)
                        {
                            string[] splitDelta = elements[i].Split(':');
                            if(splitDelta.Length != 3)
                            {
                                throw new ParsingException();
                            }
                            if(splitDelta[1].Length > 1)
                            {
                                throw new ParsingException();
                            }
                            char symbol = char.Parse(splitDelta[1]);
                            if(!result.getAlphabet().Contains(symbol))
                            {
                                throw new ParsingException();
                            }
                            if(!result.getStates().Contains(splitDelta[0]) || !result.getStates().Contains(splitDelta[2]))
                            {
                                throw new ParsingException();
                            }
                            if (!result.checkIfIsENKA())
                            {
                                result.setDelta(splitDelta[0], symbol, splitDelta[2]);
                            }
                            else
                            {
                                result.ESwitching(splitDelta[0], symbol, splitDelta[2]);
                            }
                        }
                    }
                }
                else if (elements[i] == "FINAL STATES:")
                {
                    i++;
                    while (i < elements.Length)
                    {
                        if(elements[i].Length != 0)
                        {
                            if(!result.getStates().Contains(elements[i]))
                            {
                                throw new ParsingException();
                            }
                            result.setFinalState(elements[i]);
                            i++;
                        }
                    }
                }
            }
        }

        //Self-explanatory
        private static void loadRegularExpressionFromFile(char flag)
        {
            Console.WriteLine("Enter file name: ");
            string fileName = Console.ReadLine();

            try
            {
                var path = Path.Combine("./" + fileName + ".txt");
                string[] elements = File.ReadAllLines(path);
                printSpecification(elements);
                lexicalAnalysisOfRegularExpressionAndEvaluation(elements, flag);
            }
            catch (IOException e)
            {
                Console.WriteLine("Error " + e.Message);
            }
        }

        //Function that connects all help function and allows user to play with options
        public static void enterSpecification(string[] args)
        {
            Console.WriteLine("Do you want to load specification from file or from console or from command line: ");
            Console.WriteLine("f for file, c for console, cl for command line");
            string inputLoad;
            inputLoad = Console.ReadLine();
            string extraInput;
            if (inputLoad == "f")
            {
                Console.WriteLine("Do you want to load DKA, E-NKA or regular expression (1/2/3): ");
                extraInput = Console.ReadLine();
                if (extraInput == "1")
                {
                    Automat res = loadAutomataSpecificationFromFile();
                    HashSet<string> strings = stringLoaderHelper('f', args);
                    foreach (var str in strings)
                    {
                        doesDKAAccept(res, str);
                    }
                }
                else if (extraInput == "2")
                {
                    Automat res = loadAutomataSpecificationFromFile();
                    HashSet<string> strings = stringLoaderHelper('f', args);
                    foreach (var str in strings)
                    {
                        doesENKAAccept(res, str);
                    }
                }
                else if (extraInput == "3")
                {
                    loadRegularExpressionFromFile('f');
                }
                else
                {
                    Console.WriteLine("Wrong option, sorry!");
                }
            }
            else if (inputLoad == "c")
            {
                Console.WriteLine("Do you want to load DKA, E-NKA or regular expression (1/2/3): ");
                extraInput = Console.ReadLine();
                if (extraInput == "1")
                {
                    string[] loadFromConsole = loadAutomataFromConsole();
                    printSpecification(loadFromConsole);
                    Automat automat = new();
                    HashSet<string> strings = stringLoaderHelper('c', args);
                    helpMethodForLoadingAutomataSpecificationFromFileOrFromCommandLine(automat, loadFromConsole);

                    foreach (var str in strings)
                    {
                        doesDKAAccept(automat, str);
                    }

                }
                else if (extraInput == "2")
                {
                    string[] loadFromConsole = loadAutomataFromConsole();
                    printSpecification(loadFromConsole);
                    Automat automat = new();
                    HashSet<string> strings = stringLoaderHelper('c', args);
                    helpMethodForLoadingAutomataSpecificationFromFileOrFromCommandLine(automat, loadFromConsole);

                    foreach (var str in strings)
                    {
                        doesENKAAccept(automat, str);
                    }
                }
                else if (extraInput == "3")
                {                
                    Console.WriteLine("Enter your regular expression which you want to evaluate: ");
                    Console.WriteLine("If you want to stop, input --exit!");
                    List<string> regularExpressionHelper = new();
                    string userInput;
                    userInput = Console.ReadLine();
                    string[] regularExpression = new string[userInput.Length];
                    for (int i = 0; i < regularExpression.Length; i++)
                    {
                        regularExpression[i] = userInput[i].ToString();
                    }
                    printSpecification(regularExpression);
                    lexicalAnalysisOfRegularExpressionAndEvaluation(regularExpression, 'c');
                }
                else
                {
                    Console.WriteLine("Wrong option, sorry!");
                }
            }
            else if (inputLoad == "cl")
            {
                Console.WriteLine("Do you want to load DKA, E-NKA or regular expression (1/2/3): ");
                extraInput = Console.ReadLine();
                if (extraInput == "1")
                {
                    printSpecification(args);
                    Automat result = loadAutomataSpecificationFromCommandLine(args);
                    HashSet<string> strings = stringLoaderHelper('l', args);
                    foreach (var str in strings)
                    {
                        doesDKAAccept(result, str);
                    }
                }
                else if (extraInput == "2")
                {
                    printSpecification(args);
                    Automat result = loadAutomataSpecificationFromCommandLine(args);
                    HashSet<string> strings = stringLoaderHelper('l', args);
                    foreach (var str in strings)
                    {
                        doesENKAAccept(result, str);
                    }
                }
                else if (extraInput == "3")
                {
                    printSpecification(args);
                    lexicalAnalysisOfRegularExpressionAndEvaluation(args, 'l');
                }
                else
                {
                    Console.WriteLine("Wrong option, sorry!");
                }
            }
            else
            {
                Console.WriteLine("Wrong option, sorry");
            }
        }

        //Self-explanatory
        private static void lexicalAnalysisOfRegularExpressionAndEvaluation(string[] args, char flag)
        {
            LexicalAnalysis lexicalAnalysis = new(args);

            if (lexicalAnalysis.lexicalAnalysisForRegularExpression(args))
            {
                throw new LexicalException();
            }

            string regularExpression = "";
            foreach (var element in args)
            {
                regularExpression += element;
            }
            helpFunctionForRegularExpressions(regularExpression, flag, args);
        }

        //Reduces code repetition
        private static void helpFunctionForRegularExpressions(string regularExpression, char flag, string[] args)
        {
            Automat transform = Automat.makeAutomataFromRegularExpression(regularExpression);
            HashSet<string> load = loadStringsForRegularExpression(flag, args);

            bool isDKA = false;
            if (!transform.checkIfIsENKA())
            {
                isDKA = true;
            }
            foreach (var word in load)
            {
                if (isDKA)
                {
                    doesDKAAccept(transform, word);
                }
                else
                {
                    doesENKAAccept(transform, word);
                }
            }
        }

        //Self-explanatory
        private static void doesENKAAccept(Automat res, string str)
        {
            if (res.acceptsENKA(str))
            {
                Console.WriteLine("Word " + str + " accepted!");
            }
            else
            {
                Console.WriteLine("Word " + str + " not accepted!");
            }
        }

        //Self-explanatory
        private static void doesDKAAccept(Automat res, string str)
        {
            if (res.AcceptsDKA(str))
            {
                Console.WriteLine("Word " + str + " accepted!");
            }
            else
            {
                Console.WriteLine("Word " + str + " not accepted!");
            }
        }

        //Loads strings which are later checked, are they accepted or not
        private static HashSet<string> loadStringsForRegularExpression(char flag, string[] args)
        {
            HashSet<string> loadStrings = new();
            loadStrings = stringLoaderHelper(flag, args);
            return loadStrings;
        }

        //Self explanatory
        private static string[] loadAutomataFromConsole()
        {
            List<string> helpForString = new();

            helpForString.Add("AUTOMATA-STATES:");
            Console.WriteLine("Enter states, if you want to stop, enter --exit!");
            string input;
            do
            {
                Console.WriteLine("Enter state: ");
                input = Console.ReadLine();
                if (input != "--exit")
                {
                    helpForString.Add(input);
                }
            } while (input != "--exit");

            helpForString.Add("ALPHABET:");
            Console.WriteLine("Enter alphabet, if you want to stop, enter --exit!");
            do
            {
                Console.WriteLine("Enter symbol: ");
                input = Console.ReadLine();
                if (input != "--exit")
                {
                    helpForString.Add(input);
                }
            } while (input != "--exit");

            helpForString.Add("DELTA TRANSITIONS:");
            do
            {
                Console.WriteLine("Enter start state: ");
                input = Console.ReadLine();
                if (input != "--exit")
                {
                    string temp = "";
                    temp += input;
                    temp += ":";
                    Console.WriteLine("Enter symbol from alphabet: ");
                    input = Console.ReadLine();
                    temp += input;
                    temp += ":";
                    Console.WriteLine("Enter destination state: ");
                    input = Console.ReadLine();
                    temp += input;
                    helpForString.Add(temp);
                }
            } while (input != "--exit");

            helpForString.Add("FINAL STATES:");
            do
            {
                Console.WriteLine("Enter final state: ");
                input = Console.ReadLine();
                if (input != "--exit")
                {
                    helpForString.Add(input);
                }
            } while (input != "--exit");

            string[] loadedAutomata = new string[helpForString.Count];

            for (int i = 0; i < loadedAutomata.Length; i++)
            {
                loadedAutomata[i] = helpForString.ElementAt(i);
            }

            return loadedAutomata;
        }

        //Important function
        //Allows me to block user if he wants to load both language and strings from command line
        private static HashSet<string> stringLoaderHelper(char flag, string[] args)
        {
            HashSet<string> strings;
            Console.WriteLine("Do you want to load strings from console (c), or from file (f), or from command line(cl): ");
            string whereToLoadFrom = Console.ReadLine();
            strings = new();
            if (whereToLoadFrom == "c")
            {
                strings = getStringsFromUserFromConsole();
            }
            else if (whereToLoadFrom == "f")
            {
                strings = getStringsFromUserFromFile();
            }
            else if (whereToLoadFrom == "cl" && flag != 'l')
            {
                strings = getStringsFromCommandLine(args);
            }
            else
            {
                throw new Exception("You can't read specification and strings from the same location or you messed up input!");
            }

            return strings;
        }

        //Self-explanatory
        private static HashSet<string> getStringsFromCommandLine(string[] args)
        {
            HashSet<string> strings = new();
            foreach (var element in args)
            {
                strings.Add(element);
            }
            return strings;
        }
    }
}
