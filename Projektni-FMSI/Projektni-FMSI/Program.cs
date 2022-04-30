using System;
using Projektni_FMSI;

Automat a = new Automat();
Automat b = new();
Automat g = new();
Automat f = new();
Automat.chainOperations();

g.StartState = "p1";
g.states.Add("p1");
g.states.Add("p2");
g.alphabet.Add('a');
g.alphabet.Add('b');
g.delta[("p1", 'a')] = "p2";
g.delta[("p1", 'b')] = "p1";
g.delta[("p2", 'a')] = "p2";
g.delta[("p2", 'b')] = "p2";
g.finalStates.Add("p2");
//a.makeAutomata();
//g.makeAutomata();
a.StartState = "q1";
//a.states.Add("q0");
a.states.Add("q1");
a.states.Add("q2");
/*a.states.Add("q3");
a.states.Add("q4");
a.states.Add("q5");
a.states.Add("q6");
a.states.Add("q7");
a.states.Add("q8");
a.states.Add("q9");
a.states.Add("q10");*/

//a.alphabet.Add('E');
a.alphabet.Add('a');
a.alphabet.Add('b');
a.finalStates.Add("q2");
//a.finalStates.Add("q7");
a.delta[("q1", 'a')] = "q2";
a.delta[("q1", 'b')] = "q1";
//a.delta[("q1", '1')] = "q4";
//a.delta[("q1", '0')] = "q3";

a.delta[("q2", 'a')] = "q2";
a.delta[("q2", 'b')] = "q2";
//a.chainOperations();
/*a.delta[("q3", '0')] = "q7";
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
a.delta[("q9", '1')] = "q9";*/
//a.delta[("q10", '0')] = "q0";
//a.delta[("q10", '1')] = "q6";

//b = a.applyKleeneStar();
/*b = a.connectLanguages(g);
b.printStatesAndAlphabet();
//b.printStatesAndAlphabet();
Automat c = b.convertENKAtoDKA();
c.printStatesAndAlphabet();*/
//c.printStatesAndAlphabet();

//b = a.minimiseAutomata();

/*if(c.AcceptsDKA("aaba"))
{
    Console.WriteLine("Accepted!");
}*/

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
