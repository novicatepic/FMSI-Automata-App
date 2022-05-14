using NUnit.Framework;
using Projektni_FMSI;

namespace MinimiseDKATest
{
    public class Tests
    {
        Automat a;
        Automat b;
        Automat c;

        [SetUp]
        public void Setup()
        {
            a = new();
            a.setStartState("q0");
            a.addState("q0");
            a.addState("q1");
            a.setSymbolInAlphabet('0');
            a.setSymbolInAlphabet('1');
            a.setFinalState("q1");
            a.addTransition("q0", '0', "q0");
            a.addTransition("q0", '1', "q1");
            a.addTransition("q1", '0', "q1");
            a.addTransition("q1", '1', "q1");

            //AUTOMATA FROM PROFESSOR'S PRESENTATION
            b = new();
            b.setStartState("q0");
            b.addState("q0");
            b.addState("q1");
            b.addState("q2");
            b.addState("q3");
            b.addState("q4");
            b.setSymbolInAlphabet('a');
            b.setSymbolInAlphabet('b');
            b.setFinalState("q3");
            b.addTransition("q0", 'a', "q1");
            b.addTransition("q0", 'b', "q4");
            b.addTransition("q1", 'a', "q1");
            b.addTransition("q1", 'b', "q2");
            b.addTransition("q2", 'a', "q1");
            b.addTransition("q2", 'b', "q3");
            b.addTransition("q3", 'a', "q1");
            b.addTransition("q3", 'b', "q4");
            b.addTransition("q4", 'a', "q1");
            b.addTransition("q4", 'b', "q4");

            //YOUR EXAMPLE
            c = new();
            c.setStartState("q0");
            c.addState("q0");
            c.addState("q1");
            c.addState("q2");
            c.addState("q3");
            c.addState("q4");
            c.addState("q5");
            c.addState("q6");
            c.addState("q7");
            c.addState("q8");
            c.addState("q9");
            c.setSymbolInAlphabet('a');
            c.setSymbolInAlphabet('b');
            c.setFinalState("q9");
            c.addTransition("q0", 'a', "q1");
            c.addTransition("q0", 'b', "q2");
            c.addTransition("q1", 'a', "q3");
            c.addTransition("q1", 'b', "q4");
            c.addTransition("q2", 'a', "q5");
            c.addTransition("q2", 'b', "q6");
            c.addTransition("q3", 'a', "q7");
            c.addTransition("q3", 'b', "q4");
            c.addTransition("q4", 'a', "q7");
            c.addTransition("q4", 'b', "q3");
            c.addTransition("q5", 'a', "q8");
            c.addTransition("q5", 'b', "q6");
            c.addTransition("q6", 'a', "q8");
            c.addTransition("q6", 'b', "q5");
            c.addTransition("q7", 'a', "q9");
            c.addTransition("q7", 'b', "q7");
            c.addTransition("q8", 'a', "q9");
            c.addTransition("q8", 'b', "q8");
            c.addTransition("q9", 'a', "q9");
            c.addTransition("q9", 'b', "q9");
        }

        [Test]
        public void Test_AlreadyMinimised()
        {
            Automat minimise = a.minimiseAutomata();
            Assert.IsTrue(minimise.getStates().Count == 2);
        }

        [Test]
        public void Test_MinimiseWithTwoStates()
        {
            Automat minimise = b.minimiseAutomata();
            Assert.IsTrue(minimise.getStates().Count == 4);
        }

        [Test]
        public void Test_MinimiseWithTwoStatesDoesAcceptWord()
        {
            Automat minimise = b.minimiseAutomata();
            Assert.IsTrue(minimise.AcceptsDKA("aaabb"));
        }

        [Test]
        public void Test_MinimiseWithMultipleStatesMinimized()
        {
            Automat minimise = c.minimiseAutomata();
            Assert.IsTrue(minimise.getStates().Count == 5);
        }

        [Test]
        public void Test_MinimisedWithMultipleStatesDoesAcceptWord()
        {
            Automat minimise = c.minimiseAutomata();
            Assert.IsTrue(minimise.AcceptsDKA("bbbabbaaa"));
        }
    }
}