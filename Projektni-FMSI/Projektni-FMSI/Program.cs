using System;
using Projektni_FMSI;

Automat a = new Automat();
a.makeAutomata();
Automat go = a.applyKleeneStar();
go.printStates();
//Automat b = new Automat();
//b.makeAutomata();
//Automat res = a.connectLanguages(b);
