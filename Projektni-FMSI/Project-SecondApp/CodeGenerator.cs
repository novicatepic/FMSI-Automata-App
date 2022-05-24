using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Projektni_FMSI;

namespace Project_SecondApp
{
    //Tried out if code works
    //Used string builder to make it
    //And write it to a file
    public class CodeGenerator
    {
        Automat toGenerate;

        public CodeGenerator(Automat a)
        {
            toGenerate = a;
        }

        public void generate()
        {
            var path = Path.Combine("./" + "gen" + ".txt");
            StringBuilder resultString = new StringBuilder();
            appendSpecificationClass(resultString);
            generateClass(resultString);
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(resultString);
            }
        }

        private void generateClass(StringBuilder resultString)
        {
            if (toGenerate.checkIfIsENKA())
            {
                throw new Exception("Only DKA supported!");
            }
            toGenerate.addDeadState();

            HashSet<string> listOfSpecFuncs = new();
            HashSet<string> listOfTemp2s = new();
            resultString.Append("\npublic class GeneratedAutomata\n{");
            foreach (var state in toGenerate.getStates())
            {
                string temp2 = "(";
                int counter = 0;

                foreach (var symbol in toGenerate.getAlphabet())
                {
                    string innerTemp = "spec" + counter++;
                    listOfSpecFuncs.Add(innerTemp);
                    temp2 += "Specification " + innerTemp + ", ";
                    string tmp = "Specification " + innerTemp + ", ";
                    listOfTemp2s.Add(tmp);
                }
                temp2 += "char symbol)\n{\n";
                string temp = "private string switch" + state + "State" + temp2;
                resultString.Append(temp);
                resultString.Append("string currentState = null;\n");
                resultString.Append("switch(symbol)\n{\n");
                counter = 0;
                foreach (var symbol in toGenerate.getAlphabet())
                {
                    if (toGenerate.getDelta().ContainsKey((state, symbol)))
                    {
                        string tmp = "case " + "'" + symbol + "':\n";
                        resultString.Append(tmp);
                        tmp = "";
                        tmp = "currentState = " + "\"" + toGenerate.getDelta()[(state, symbol)] + "\";\n";
                        resultString.Append(tmp);
                        tmp = "";
                        resultString.Append(listOfSpecFuncs.ElementAt(counter++) + ".doActionsForState(currentState);\n");
                        resultString.Append("break;\n");
                    }
                    /*else
                    {
                        counter++;
                    }*/
                }
                resultString.Append("default:\nthrow new Exception();\n}");
                resultString.Append("return currentState;\n}\n\n\n");
            }


            resultString.Append("public void chainNStuff(Specification input, Specification output, ");
            foreach (var element in listOfSpecFuncs)
            {
                resultString.Append("Specification ");
                resultString.Append(element + ", ");
            }
            resultString.Append("HashSet<char> alphabet)\n{\n");
            resultString.Append("string initState = " + "\"" + toGenerate.getStartState() + "\";\n");
            resultString.Append("foreach(var symbol in alphabet)\n{\n");
            foreach (var state in toGenerate.getStates())
            {
                resultString.Append("if(initState == " + "\"" + state + "\")\n{\n");
                resultString.Append("output.doActionsForState(initState);\n");
                resultString.Append("initState = switch" + state + "State(");
                foreach (var elem in listOfSpecFuncs)
                {
                    resultString.Append(elem + ", ");
                }
                resultString.Append("symbol);\n");
                resultString.Append("input.doActionsForState(initState);\n}\n");
            }
            resultString.Append("}\n}\n}\n");

        }

        private static void appendSpecificationClass(StringBuilder resultString)
        {
            resultString.Append("using System;\n");
            resultString.Append("using System.Collections.Generic;\n\n");
            resultString.Append("public class Specification\n{\n");
            resultString.Append(" private HashSet<Action<string>> actions = new HashSet<Action<string>>();\n" +
                "public void addAction(Action<string> action)\n{\n" +
                "actions.Add(action);\n}");
            resultString.Append("public void removeAction(Action<string> action)\n{\n" +
                "actions.Remove(action);\n}\n");
            resultString.Append("public void deleteAllActions()\n{\n" +
                "actions.Clear();\n}");
            resultString.Append("public void doActionsForState(string state)\n{\n" +
                "foreach(var action in actions)\n" +
                "\n{\naction.Invoke(state);\n}\n}\n}\n\n\n\n");
        }
    }
}
