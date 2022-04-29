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
a.alphabet.Add('a');
a.alphabet.Add('b');
a.finalStates.Add("q5");
a.delta[("q0", 'a')] = "q4";
a.delta[("q0", 'b')] = "q1";
a.delta[("q1", 'a')] = "q3";
a.delta[("q1", 'b')] = "q2";
a.delta[("q2", 'a')] = "q3";
a.delta[("q2", 'b')] = "q2";
a.delta[("q3", 'a')] = "q2";
a.delta[("q3", 'b')] = "q3";
a.delta[("q4", 'a')] = "q0";
a.delta[("q4", 'b')] = "q5";
a.delta[("q5", 'a')] = "q2";
a.delta[("q5", 'b')] = "q3";
b = a.minimiseAutomata();
if(b.AcceptsDKA("baaaaaaababa"))
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
