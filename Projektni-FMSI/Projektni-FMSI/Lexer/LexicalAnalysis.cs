using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektni_FMSI
{
    public class LexicalAnalysis
    {
        private string[] stringToAnalayse;

        public LexicalAnalysis(string[] analyseString)
        {
            stringToAnalayse = analyseString;
        }

        public List<int> lexicalAnalysisForRegularExpression(string[] regularExpression)
        {
            List<int> errorsFound = new();
            LexerForRegularExpression lexer = new(regularExpression);

            while(lexer.Next().Type != "EOF")
            {
                if(lexer.Next() == null)
                {
                    errorsFound.Add(lexer.getSourcePosition());
                }
            }

            return errorsFound;
        }
    }
}
