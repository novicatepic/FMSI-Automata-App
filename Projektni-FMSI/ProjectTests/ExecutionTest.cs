using NUnit.Framework;
using Projektni_FMSI;
using Projektni_FMSI.Exceptions;

namespace ProjectTests
{
    public class ExecutionTests
    {
        private Automat a;
        private Automat b;

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
            a.addTransition("q1", '1', "q0");

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
        }

        [Test]
        public void TestDKA_DoesItAcceptWord()
        {
            Assert.IsTrue(a.AcceptsDKA("000100"));
        }

        [Test]
        public void TestDKA_DontAcceptWord()
        {
            Assert.IsFalse(a.AcceptsDKA("001100"));
        }

        [Test]
        public void TestDKA_IsExceptionThrown()
        {

            try
            {
                a.AcceptsDKA("ad");
                Assert.Fail();
            }
            catch (AlphabetNotContainsException)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void TestENKA_DoesItAcceptWord()
        {
            Assert.IsTrue(b.acceptsENKA("aaba"));
        }

        [Test]
        public void TestENKA_DontAcceptWord()
        {
            Assert.IsFalse(b.acceptsENKA("aaaa"));
        }

        [Test]
        public void TestENKA_IsAlphabetExceptionThrown()
        {
            try
            {
                b.acceptsENKA("101");
                Assert.Fail();
            }
            catch(AlphabetNotContainsException)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void TestENKA_IsKeyNotFoundExceptionThrown()
        {
            try
            {
                b.ESwitching("q0", 'g', "q1");
                Assert.Fail();
            }
            catch(NoKeyFoundException)
            {
                Assert.Pass();
            }
        }

    }
}