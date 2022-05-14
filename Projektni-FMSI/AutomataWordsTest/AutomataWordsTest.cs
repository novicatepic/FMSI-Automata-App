using NUnit.Framework;
using Projektni_FMSI;

namespace LongestShortestIsLanguageFinalTests
{
    public class Tests
    {
        Automat a;
        string regexp = "ab*ca*";

        [SetUp]
        public void Setup()
        {
            a = new();
            a.setStartState("q0");
            a.addState("q0");
            a.addState("q1");
            a.addState("q2");
            a.addState("q3");
            a.setSymbolInAlphabet('0');
            a.setSymbolInAlphabet('1');
            a.setFinalState("q1");
            a.setFinalState("q3");
            a.addTransition("q0", '0', "q3");
            a.addTransition("q0", '1', "q2");
            a.addTransition("q2", '0', "q1");
            a.addTransition("q2", '1', "q3");
        }

        [Test]
        public void Test_ShortestWordForAutomata()
        {
            Assert.AreEqual(1, a.findShortestPath());
        }

        [Test]
        public void Test_ShortestWordForRegExp()
        {
            //This is what's done behind closed curtains in reality!
            Automat b = Automat.makeAutomataFromRegularExpression(regexp);
            b = b.convertENKAtoDKA();
            Assert.AreEqual(2, b.findShortestPath());
        }
        
        [Test]
        public void Test_LongestWordForAutomata()
        {
            Assert.AreEqual(2, a.findLongestPath());
        }

        [Test]
        public void Test_LongestWordForRegExpInfinity()
        {
            //This is what's done behind closed curtains in reality!
            Automat b = Automat.makeAutomataFromRegularExpression(regexp);
            b = b.convertENKAtoDKA();
            Assert.AreEqual(int.MaxValue, b.findLongestPath());
        }

        [Test]
        public void Test_IsLanguageFinalAutomata()
        {
            //Returns false because when we get to the final state, we have nowhere else to go to!
            Assert.AreEqual(false, a.isLanguageFinal());
        }

        [Test]
        public void Test_IsLanguageFinalAutomataTrue()
        {
            Automat c = a;
            c.addState("DEADSTATE");
            a.addTransition("q1", '0', "DEADSTATE");
            a.addTransition("q1", '1', "DEADSTATE");
            a.addTransition("q3", '0', "DEADSTATE");
            a.addTransition("q3", '1', "DEADSTATE");
            Assert.AreEqual(true, a.isLanguageFinal());
        }

        [Test]
        public void Test_IsLanguageFinalRegExp()
        {
            //This is what's done behind closed curtains in reality!
            Automat b = Automat.makeAutomataFromRegularExpression(regexp);
            b = b.convertENKAtoDKA();
            Assert.AreEqual(false, b.isLanguageFinal());
        }

    }
}