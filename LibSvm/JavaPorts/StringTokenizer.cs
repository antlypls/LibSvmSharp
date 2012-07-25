using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm.JavaPorts
{
    class StringTokenizer
    {
        private readonly IList<string> _tokens;
        private int currentToken = -1;
        private static readonly char[] DefaultDelimeters = new[] {' ', '\t', '\n', '\r', '\f',};

        public StringTokenizer(string untokenizedString) : this(untokenizedString, DefaultDelimeters)
        {
        }

        public StringTokenizer(string untokenizedString, char[] delimiters)
        {
            _tokens = untokenizedString.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        }

        public string NextToken()
        {
            currentToken++;
            return _tokens[currentToken];
        }

        public int CountTokens()
        {
            return _tokens.Count - currentToken;
        }
    }
}
