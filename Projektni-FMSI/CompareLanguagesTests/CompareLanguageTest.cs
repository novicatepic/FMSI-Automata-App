using NUnit.Framework;
using Projektni_FMSI;

namespace CompareLanguagesTests
{
    public class Tests
    {
        Automat c;
        Automat b;
        string regexp1 = "(a+b)*c";
        string regexp2 = "(b+a)*c";
        string regexp3 = "a+(a*bc)a";
        [SetUp]
        public void Setup()
        {
            c = new();
            b = new();

            //YOUR EXAMPLE WITH SNJ, ZNJ ETC.
            c.setStartState("q5");
            c.addState("q5");
            c.addState("q7");
            c.addState("q1");
            c.addState("q2");
            c.addState("q50");
            c.addState("q728");
            c.setSymbolInAlphabet('a');
            c.setSymbolInAlphabet('b');
            c.setFinalState("q1");
            c.setFinalState("q728");
            c.setFinalState("q50");
            c.addTransition("q5", 'a', "q7");
            c.addTransition("q5", 'b', "q1");
            c.addTransition("q7", 'a', "q2");
            c.addTransition("q7", 'b', "q7");
            c.addTransition("q1", 'a', "q7");
            c.addTransition("q1", 'b', "q50");
            c.addTransition("q2", 'a', "q50");
            c.addTransition("q2", 'b', "q728");
            c.addTransition("q50", 'a', "q728");
            c.addTransition("q50", 'b', "q7");
            c.addTransition("q728", 'a', "q7");
            c.addTransition("q728", 'b', "q1");
            c.addTransition("q728", 'b', "q1");

            b.setStartState("f8");
            b.addState("f8");
            b.addState("c");
            b.addState("z");
            b.addState("znj");
            b.addState("se");
            b.addState("s9");
            b.setSymbolInAlphabet('a');
            b.setSymbolInAlphabet('b');
            b.setFinalState("s9");
            b.setFinalState("se");
            b.setFinalState("z");
            b.addTransition("s9", 'a', "znj");
            b.addTransition("s9", 'b', "se");
            b.addTransition("f8", 'a', "znj");
            b.addTransition("f8", 'b', "s9");
            b.addTransition("se", 'a', "z");
            b.addTransition("se", 'b', "znj");
            b.addTransition("c", 'a', "se");
            b.addTransition("c", 'b', "z");
            b.addTransition("z", 'a', "znj");
            b.addTransition("z", 'b', "s9");
            b.addTransition("znj", 'a', "c");
            b.addTransition("znj", 'b', "znj");
        }


        //Everything is done behind close curtains
        //E-NKA or regexp are converted to DKA, deadstates are added if needed, we minimise everything, and then we compare
        //Why dead states? Errors otherwise, and dead states don't make much more difference
        [Test]
        public void Test_TestIfTwoAutomatasAreTheSame()
        {
            Assert.IsTrue(b.compareTwoAutomatas(c));
        }

        [Test]
        public void Test_TestIfTwoRegularExpressionsAreTheSame()
        {
            Automat convert1 = Automat.makeAutomataFromRegularExpression(regexp1);
            Automat convert2 = Automat.makeAutomataFromRegularExpression(regexp2);
            Automat convert1toDKA;
            Automat convert2toDKA;
            if(convert1.checkIfIsENKA())
            {
                convert1toDKA = convert1.convertENKAtoDKA();
            }
            else
            {
                convert1toDKA = convert1;
            }

            if (convert2.checkIfIsENKA())
            {
                convert2toDKA = convert2.convertENKAtoDKA();
            }
            else
            {
                convert2toDKA = convert2;
            }

            Assert.IsTrue(convert1toDKA.compareTwoAutomatas(convert2toDKA));
        }

        [Test]
        public void Test_TestIfTwoRegularExpressionsAreTheSame2()
        {
            Automat convert1 = Automat.makeAutomataFromRegularExpression(regexp1);
            Automat convert2 = Automat.makeAutomataFromRegularExpression(regexp3);
            Automat convert1toDKA;
            Automat convert2toDKA;
            if (convert1.checkIfIsENKA())
            {
                convert1toDKA = convert1.convertENKAtoDKA();
            }
            else
            {
                convert1toDKA = convert1;
            }

            if (convert2.checkIfIsENKA())
            {
                convert2toDKA = convert2.convertENKAtoDKA();
            }
            else
            {
                convert2toDKA = convert2;
            }

            Assert.IsFalse(convert1toDKA.compareTwoAutomatas(convert2toDKA));
        }

        [Test]
        public void Test_TestAutomataAndRegex()
        {
            Automat convert1 = Automat.makeAutomataFromRegularExpression(regexp1);
            Automat convert1toDKA;
            if (convert1.checkIfIsENKA())
            {
                convert1toDKA = convert1.convertENKAtoDKA();
            }
            else
            {
                convert1toDKA = convert1;
            }

            Assert.IsFalse(convert1toDKA.compareTwoAutomatas(b));
        }
    }
}