using System;
using Projektni_FMSI;
using System.Collections.Generic;
using System.IO;

class FirstApp
{
    //MAIN FUNCTION FOR FIRST APP
    //OPTIONS OFFERED FOR USER
    //HE CHOOSES AND PROGRAM STARTS
    static void Main(string[] args)
    {
        Console.WriteLine("===================================");
        Console.WriteLine("===================================");
        Console.WriteLine("WELCOME: ");
        Console.WriteLine("===================================");
        Console.WriteLine("===================================");
        Console.WriteLine("You have these options: ");
        Console.WriteLine("1-Execute automata\n2-Construct union\n3-Construct intersection\n4-Construct difference\n5-Connect two languages\n6-Apply Kleene star" +
                            "\n7-Chain operations\n8-Find shortest word\n9-Find longest word\n10-Check out if language is final\n11-Minimise DKA\n" +
                            "12-Transform ENKA to DKA\n13-Check if two languages are the same\n14-Transform regular expression to automata" +
                            "\n15-Load Specification\n16-Construct complement ");
        Console.WriteLine("--exit to finish!");       
        string input = "";
        Automat a = null;
        a = new();
        Automat b = null;
        b = new();
        Console.WriteLine("Enter your option: ");
        do
        {
            input = Console.ReadLine();
            if (input == "1")
            {
                Console.WriteLine("Construct your automata: ");
                a.makeAutomata();
                a.runAutomata();
            }
            else if (input == "2")
            {
                Console.WriteLine("Construct your language: ");
                Automat.makeLanguagesForUnionIntersectionDifference(ref a);
                Console.WriteLine("Construct your second language: ");
                Automat.makeLanguagesForUnionIntersectionDifference(ref b);
                Automat res = a.findUnionDKA(b);
                Console.WriteLine("Result: ");
                res.printStatesAndAlphabet();
            }
            else if (input == "3")
            {
                Console.WriteLine("Construct your language: ");
                Automat.makeLanguagesForUnionIntersectionDifference(ref a);
                Console.WriteLine("Construct your second language: ");
                Automat.makeLanguagesForUnionIntersectionDifference(ref b);
                Automat res = a.findIntersection(b);
                Console.WriteLine("Result: ");
                res.printStatesAndAlphabet();
            }
            else if (input == "4")
            {
                Console.WriteLine("Construct your language: ");
                Automat.makeLanguagesForUnionIntersectionDifference(ref a);
                Console.WriteLine("Construct your second language: ");
                Automat.makeLanguagesForUnionIntersectionDifference(ref b);
                Automat res = a.findDifference(b);
                Console.WriteLine("Result: ");
                res.printStatesAndAlphabet();
            }
            else if (input == "5")
            {
                Console.WriteLine("Construct your first language: ");
                Automat.makeLanguagesForUnionIntersectionDifference(ref a);
                Console.WriteLine("Construct your second language: ");
                Automat.makeLanguagesForUnionIntersectionDifference(ref b);
                Automat res = a.connectLanguages(b);
                Console.WriteLine("Result: ");
                res.printStatesAndAlphabet();
            }
            else if (input == "6")
            {
                Console.WriteLine("Construct your language: ");
                Automat.makeLanguagesForUnionIntersectionDifference(ref a);
                Automat res = a.applyKleeneStar();
                Console.WriteLine("Result: ");
                res.printStatesAndAlphabet();
            }
            else if (input == "7")
            {
                Console.WriteLine("Chaining has started: ");
                Automat.chainOperations();
            }
            else if (input == "8")
            {
                Console.WriteLine("Construct your language: ");
                Automat.makeLanguagesForUnionIntersectionDifference(ref a);
                Console.WriteLine("Shortest word (if it exists) is: " + a.findShortestPath());
            }
            else if (input == "9")
            {
                Console.WriteLine("Construct your language: ");
                Automat.makeLanguagesForUnionIntersectionDifference(ref a);
                //a.printStatesAndAlphabet();
                Console.WriteLine("Longest word (if it exists) is: " + a.findLongestPath());
            }
            else if (input == "10")
            {
                Console.WriteLine("Construct your language: ");
                Automat.makeLanguagesForUnionIntersectionDifference(ref a); 
                if (a.isLanguageFinal())
                {
                    Console.WriteLine("Language IS final!");
                }
                else
                {
                    Console.WriteLine("Language is NOT final!");
                }
            }
            else if (input == "11")
            {
                Console.WriteLine("Construct your automata: ");
                a.makeAutomata();
                Automat res;
                if(a.checkIfIsENKA())
                {
                    Console.WriteLine("You've entered E-NKA, we'll convert it to DKA and then minimise it...");
                    a = a.convertENKAtoDKA();
                    res = a.minimiseAutomata();
                    Console.WriteLine("Minimised automata: ");
                    res.printStatesAndAlphabet();
                }
                else
                {
                    res = a.minimiseAutomata();
                    Console.WriteLine("Minimised automata: ");
                    res.printStatesAndAlphabet();
                }
            }
            else if (input == "12")
            {
                Console.WriteLine("Construct your automata: ");
                a.makeAutomata();
                if (a.checkIfIsENKA())
                {
                    Automat rez = a.convertENKAtoDKA();
                    rez.printStatesAndAlphabet();
                }
                else
                {
                    Console.WriteLine("You've entered DKA anyways, why would I convert it?");
                }
            }
            else if (input == "13")
            {
                Automat.makeLanguagesForUnionIntersectionDifference(ref a);
                Automat.makeLanguagesForUnionIntersectionDifference(ref b);
                if (a.compareTwoAutomatas(b))
                {
                    Console.WriteLine("Automatas are the same!");
                }
                else
                {
                    Console.WriteLine("Automatas are not the same!");
                }
            }
            else if (input == "14")
            {
                string regexp;
                Console.WriteLine("Enter your regular expression: ");
                regexp = Console.ReadLine();
                Console.WriteLine("Converting...");
                Automat convertedAutomata = Automat.makeAutomataFromRegularExpression(regexp);
                convertedAutomata.printStatesAndAlphabet();
            }
            else if (input == "15")
            {
                AutomataSpecification.enterSpecification(args);
            }
            else if(input == "16")
            {
                Console.WriteLine("Construct your language: ");
                Automat.makeLanguagesForUnionIntersectionDifference(ref a);
                Automat res = a.constructComplement();
                Console.WriteLine("Result: ");
                res.printStatesAndAlphabet();
            }
            else if (input == "--exit")
            {

            }
            else
            {
                Console.WriteLine("Wrong option, try again!");
            }
        } while (input != "--exit");
    }
}
