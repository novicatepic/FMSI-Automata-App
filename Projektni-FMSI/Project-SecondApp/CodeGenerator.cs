using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Project_SecondApp
{
    public class CodeGenerator
    {
        public CodeGenerator()
        {

        }

        public void generateCode()
        {
            var path = Path.Combine("./" + "generator" + ".txt");
            if(!File.Exists(path))
            {
                //File.Create(path);
                //TextWriter writeFile = new StreamWriter(path);
                StringBuilder resultString = new StringBuilder();
                appendSpecificationClass(resultString);
                appendGenerationClass(resultString);
                using(StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine(resultString);
                }
                /*string str = resultString.ToString();
                string[] strArray = str.Split('\n');
                File.WriteAllLinesAsync(path, strArray);*/
                //File.WriteAllLinesAsync(path, resultString.ToString().ToCharArray());
                //File.WriteAllLinesAsync(path, )
                //writeFile.Write(resultString);
                //writeFile.Close();
            }
            else
            {
                Console.WriteLine("File already exists, code generated!");
            }

        }

        private static void appendGenerationClass(StringBuilder resultString)
        {
            resultString.Append("public class GeneratedAutomata\n{");
            resultString.Append("private string switchFirstState(Specification spec0, Specification spec1, char symbol)\n{\n");
            resultString.Append("string currentState = null;\n" +
                "switch(symbol)\n{\n" +
                "case 'a':\n" +
                "currentState = \"q0\";\n" +
                "spec0.doActionsForState(currentState);\n" +
                "break;\n" +
                "case 'b':\n" +
                "currentState = \"q1\";\n" +
                "spec1.doActionsForState(currentState);\n" +
                "break;\n" +
                "default:\n" +
                "throw new Exception();\n" +
                "}\n" +
                "return currentState;\n}\n");

            resultString.Append("private string switchSecondState(Specification spec0, Specification spec1, char symbol)\n{\n");
            resultString.Append("string currentState = null;\n" +
                "switch(symbol)\n{\n" +
                "case 'a':\n" +
                "currentState = \"q1\";\n" +
                "spec1.doActionsForState(currentState);\n" +
                "break;\n" +
                "case 'b':\n" +
                "currentState = \"q0\";\n" +
                "spec0.doActionsForState(currentState);\n" +
                "break;\n" +
                "default:\n" +
                "throw new Exception();\n" +
                "}\n" +
                "return currentState;\n}\n");

            resultString.Append("public void chainNStuff(Specification input, Specification output, Specification spec0, Specification spec1, HashSet<char> alphabet)\n{\n");
            resultString.Append("string initState = \"q0\";\n" +
                "foreach(var symbol in alphabet)\n{\n" +
                " if(initState == \"q0\")\n{\n" +
                "output.doActionsForState(initState);\n" +
                "initState = switchFirstState(spec0, spec1, symbol);\n" +
                "input.doActionsForState(initState);\n" +
                "}\n" +
                "else if(initState == \"q1\")\n{\n" +
                "output.doActionsForState(initState);\n" +
                "initState = switchSecondState(spec0, spec1, symbol);\n" +
                "input.doActionsForState(initState);\n}\n" +
                "else\n{\n" +
                "throw new Exception();\n" +
                "}\n}\n}\n}\n");
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
                "\n{\naction.Invoke(state);\n}\n}\n}");
        }
    }
}
