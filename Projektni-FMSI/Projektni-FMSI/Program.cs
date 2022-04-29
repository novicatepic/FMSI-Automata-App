using System;
using Projektni_FMSI;

Automat a = new Automat();
Automat b = new();
a.StartState = "q0";
a.states.Add("q0");
a.states.Add("q1");
a.states.Add("q2");
a.states.Add("q3");
a.states.Add("q4");
a.states.Add("q5");
a.states.Add("q6");
a.states.Add("q7");
a.states.Add("q8");
a.states.Add("q9");
//a.states.Add("q10");
a.alphabet.Add('0');
a.alphabet.Add('1');
a.finalStates.Add("q9");
//a.finalStates.Add("q7");
a.delta[("q0", '0')] = "q1";
a.delta[("q0", '1')] = "q2";
a.delta[("q1", '1')] = "q4";
a.delta[("q1", '0')] = "q3";

a.delta[("q2", '0')] = "q5";
a.delta[("q2", '1')] = "q6";
a.delta[("q3", '0')] = "q7";
a.delta[("q3", '1')] = "q4";
a.delta[("q4", '0')] = "q7";
a.delta[("q4", '1')] = "q3";
a.delta[("q5", '0')] = "q8";
a.delta[("q5", '1')] = "q6";

a.delta[("q6", '0')] = "q8";
a.delta[("q6", '1')] = "q5";
a.delta[("q7", '0')] = "q9";
a.delta[("q7", '1')] = "q7";

a.delta[("q8", '0')] = "q9";
a.delta[("q8", '1')] = "q8";
a.delta[("q9", '0')] = "q9";
a.delta[("q9", '1')] = "q9";
//a.delta[("q10", '0')] = "q0";
//a.delta[("q10", '1')] = "q6";

b = a.minimiseAutomata();

if(b.AcceptsDKA("000001"))
{
    Console.WriteLine("Accepted!");
}

//b.makeAutomata();
//b.makeAutomata();
//a.compareTwoAutomatas(b);
//Automat res = a.convertENKAtoDKA();
//res.printStates();
//a.callGraph();
//Automat b = new();
//b.makeAutomata();
//a.printStates();
//a.callGraph();
//Automat go = a.connectLanguages(b);
//go.printStates();
//Automat b = new Automat();
//b.makeAutomata();
//Automat res = a.connectLanguages(b);
