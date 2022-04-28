using System;
using Projektni_FMSI;

Automat a = new Automat();
Automat b = new();
a.makeAutomata();
a.minimiseAutomata();
if (a.AcceptsDKA("abbbb"))
{
    Console.WriteLine("true");
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
