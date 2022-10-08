# Project-FormalneMetode

The idea of this application is to make a library for working with regular languages. Regular languages are represented with finite automatas (DFA and Epsilon NFA). Functionalities that are implemented are:
-	Running finite automatas that are created from the user of the library
-	Constructing union, intersection, difference, connection and complement between languages and applying Kleene star on language, with chaining operations enabled
-	Finding shortest and longest word in a language, as well as checking if a language is final or not
-	Minimising number of states for DFA
-	Transforming Epsilon NFA into DFA, as well as transforming regular expressions into finite automata
-	Comparing representations of regular languages, including regular expressions, where the parameter of comparison is equality of languages

Second application required in this specification is the one that allows user to check if specified strings belong to the represented language. For each regular language, lexical analysis is required to check whether there are lexical errors, and if there are lexical errors, user should know how many of them are there in concrete specification.

Make an application that will generate programming code which will simulate state machine based on specification of DFA. For each formed state, users of generated code can specify reaction on events. Events are a term that considers either entering or exiting a state. This application is supposed to allow users of generated code to specify reaction on event for each symbol in the alphabet. Chaining of the operations is required, so we can form chaining of reactions based on some event.
Besides these requirements, code is commented, and asymptotic complexity is documented.

These applications are made in C#, and this is a console application.
If you want to check out specification in Serbian language, read FMSI 2021-22 Projektni zadatak.pdf.
