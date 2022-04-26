using System;
using Projektni_FMSI;

Automat a = new Automat();
a.makeAutomata();
//Automat res = a.convertENKAtoDKA();
//res.printStates();
a.printENKAEverything();
//a.callGraph();
Automat res = a.convertENKAtoDKA();
res.printStates();
if(res.AcceptsDKA("abbbba"))
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
