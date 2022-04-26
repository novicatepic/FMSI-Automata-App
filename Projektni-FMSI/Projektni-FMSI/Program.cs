using System;
using Projektni_FMSI;

Automat a = new Automat();
a.makeAutomata();
//a.callGraph();
Automat res = a.convertENKAtoDKA();
res.printStates();
if(res.AcceptsDKA("a"))
{
    Console.WriteLine("Word accepted!");
}
//Automat b = new();
//b.makeAutomata();
//a.printStates();
//a.callGraph();
//Automat go = a.connectLanguages(b);
//go.printStates();
//Automat b = new Automat();
//b.makeAutomata();
//Automat res = a.connectLanguages(b);
