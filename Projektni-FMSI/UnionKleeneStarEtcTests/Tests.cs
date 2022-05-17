using NUnit.Framework;
using Projektni_FMSI;

namespace UnionKleeneStarEtcTests
{
    public class Tests
    {
        Automat a;
        Automat b;
        string regexp1 = "a*b";
        string regexp2 = "aab*a";

        [SetUp]
        public void Setup()
        {
            a = new();
            b = new();

            a.setStartState("q0");
            a.addState("q0");
            a.addState("q1");
            a.setSymbolInAlphabet('a');
            a.setSymbolInAlphabet('b');
            a.setFinalState("q1");
            a.addTransition("q0", 'a', "q0");
            a.addTransition("q0", 'b', "q1");
            a.addTransition("q1", 'a', "q1");
            a.addTransition("q1", 'b', "q1");

            b.setStartState("p0");
            b.addState("p0");
            b.addState("p1");
            b.setSymbolInAlphabet('a');
            b.setSymbolInAlphabet('b');
            b.setFinalState("p1");
            b.addTransition("p0", 'b', "p0");
            b.addTransition("p0", 'a', "p1");
            b.addTransition("p1", 'b', "p1");
            b.addTransition("p1", 'a', "p1");
        }

        //Couldn't test union, intersection etc.
        //Because inside those functions I create a second language and then I proceed to do necessary stuff


        [Test]
        public void Test_KleeneStar()
        {
            Automat res = a.applyKleeneStar();
            //Multiple repetitions tested
            Assert.IsTrue(res.acceptsENKA("bbbb"));
            //a stays in first automata start state, cannot go to the final state
            Assert.IsFalse(res.acceptsENKA("aaaa"));
            //Added two new states, definition for Kleene star
            Assert.IsTrue(res.getStates().Count == 4);
        }

        [Test] 
        public void Test_Union()
        {
            Automat res = a.findUnionBetweenTwoLanguages(b);
            Assert.IsTrue(res.acceptsENKA("abbb"));
            Assert.IsTrue(res.acceptsENKA("baaa"));
        }

        [Test]
        public void Test_OtherUnionDKA()
        {
            Automat res = a.findUnionDKA(b);
            //Assert.IsTrue(res.getStates().Count == 4);
            Assert.IsTrue(res.AcceptsDKA("aba"));
        }

        [Test]
        public void Test_IntersectionDKA()
        {
            Automat res = a.findIntersection(b);
            Assert.IsTrue(res.AcceptsDKA("ba"));
            Assert.IsFalse(res.AcceptsDKA("bb"));
        }

        [Test]
        public void Test_LanguageConnectionDKA()
        {
            Automat res = a.connectLanguages(b);
            Assert.IsTrue(res.acceptsENKA("aba"));
            Assert.IsFalse(res.acceptsENKA("ab"));
            //if(res.acceptsENKA("aba"))
        }

        [Test]
        public void Test_LanguageConnectionRegEx()
        {
            //Simulating what's being done behind closed curtains!
            Automat a1 = Automat.makeAutomataFromRegularExpression(regexp1);
            Automat a2 = Automat.makeAutomataFromRegularExpression(regexp2);
            Automat res = a1.connectLanguages(a2);
            Assert.IsTrue(res.acceptsENKA("baaa"));
            Assert.IsFalse(res.acceptsENKA("baa"));
            Assert.IsFalse(res.acceptsENKA("aaaaaabbb"));
            Assert.IsTrue(res.acceptsENKA("aaaaabaabbbbbba"));
        }

        [Test]
        public void Test_LanguageUnionRegEx()
        {
            Automat a1 = Automat.makeAutomataFromRegularExpression(regexp1);
            Automat a2 = Automat.makeAutomataFromRegularExpression(regexp2);
            Automat res = a1.findUnionBetweenTwoLanguages(a2);
            Assert.IsTrue(res.acceptsENKA("aaaab"));
            Assert.IsFalse(res.acceptsENKA("aaaabbb"));
            Assert.IsTrue(res.acceptsENKA("aabba"));
            Assert.IsFalse(res.acceptsENKA("aa"));
        }
    }
}