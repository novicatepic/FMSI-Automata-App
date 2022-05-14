using NUnit.Framework;
using Projektni_FMSI;

namespace TransformENKAtoDKATest
{
    public class Tests
    {
        Automat b;
        Automat c;
        string regex = "a+(a*b)+aabb";
        string regex2 = "a*(a+(a*b)c)";

        [SetUp]
        public void Setup()
        {
            b = new();
            b.setStartState("q1");
            b.addState("q1");
            b.addState("q2");
            b.addState("q3");
            b.setSymbolInAlphabet('E');
            b.setSymbolInAlphabet('a');
            b.setSymbolInAlphabet('b');
            b.setFinalState("q2");
            b.ESwitching("q1", 'a', "q2");
            b.ESwitching("q1", 'b', "q2");
            b.ESwitching("q1", 'b', "q3");
            b.ESwitching("q2", 'a', "q1");
            b.ESwitching("q2", 'b', "q2");
            b.ESwitching("q3", 'b', "q2");
            b.ESwitching("q3", 'a', "q2");
            b.ESwitching("q3", 'E', "q1");

            //YOUR EXAMPLE
            c = new();
            c.setStartState("q0");
            c.addState("q0");
            c.addState("q1");
            c.addState("q2");
            c.addState("q3");
            c.addState("q4");
            c.addState("q5");
            c.setSymbolInAlphabet('E');
            c.setSymbolInAlphabet('a');
            c.setSymbolInAlphabet('b');
            c.setFinalState("q5");
            c.ESwitching("q0", 'a', "q1");
            c.ESwitching("q0", 'b', "q0");
            c.ESwitching("q1", 'E', "q2");
            c.ESwitching("q1", 'E', "q4");
            c.ESwitching("q1", 'b', "q1");
            c.ESwitching("q2", 'a', "q2");
            c.ESwitching("q2", 'b', "q5");
            c.ESwitching("q3", 'a', "q4");
            c.ESwitching("q4", 'b', "q3");
            c.ESwitching("q4", 'a', "q5");
        }

        [Test]
        public void Test_IsConversionCorrect()
        {
            Automat convert = b.convertENKAtoDKA();
            Assert.IsTrue(convert.AcceptsDKA("abbbb"));
            Assert.IsTrue(convert.AcceptsDKA("baa"));
            Assert.IsFalse(convert.AcceptsDKA("aaaa"));
        }

        [Test]
        public void Test_IsConversionCorrectSecondTest()
        {
            Automat convert = c.convertENKAtoDKA();
            Assert.IsTrue(convert.AcceptsDKA("aa"));
            Assert.IsTrue(convert.AcceptsDKA("abab"));
            Assert.IsFalse(convert.AcceptsDKA("ababa"));
        }

        [Test]
        public void Test_TransformRegExToAutomata()
        {
            Automat convert = Automat.makeAutomataFromRegularExpression(regex);
            Automat convert2 = Automat.makeAutomataFromRegularExpression(regex2);
            Assert.IsTrue(convert.acceptsENKA("a"));
            Assert.IsTrue(convert.acceptsENKA("aaaaaaaaaaaab"));
            Assert.IsTrue(convert.acceptsENKA("aabb"));
            Assert.IsFalse(convert.acceptsENKA("aa"));
            Assert.IsTrue(convert2.acceptsENKA("bc"));
            Assert.IsFalse(convert2.acceptsENKA("b"));
            Assert.IsTrue(convert2.acceptsENKA("aaaaaa"));
            Assert.IsTrue(convert2.acceptsENKA("aaaaaabc"));
            Assert.IsFalse(convert2.acceptsENKA("aaaaaaaaaab"));
        }
    }
}