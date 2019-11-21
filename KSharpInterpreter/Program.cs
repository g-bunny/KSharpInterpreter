using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KSharpInterpreter {
    //sample input to interpret:
    //func AddIfNotSame(num a, num b){
    //  if(equals(a,b)){
    //      return "same"
    //  }
    //  return plus(a,b)
    //var a = AddIfNotSame(3,4)
    //var b = AddIfNotSame(4,5)
    //return plus(a,b)
    //esc
    public enum TokenType {
        Statement = 0, //functionDefinition, if, return
        Expression, //literal (number, string), function call, built-in-function
        BuiltInFunction, //plus, minus, equals
        CustomFunction, //hmm. keyword "func", return type, (must be a literal or void), 
        Delimiter, //LParen, RParen, dot, comma, semicolon
        Empty,
        Undefined
    }
    class MainClass {
        public static void Main (string[] args) {
            Console.WriteLine ("Write your code here! Press 'enter' + 'esc' when you are ready to compile");
            BuiltInFunctions Kyntax = new BuiltInFunctions ();
            ConsoleInterface Kinterface = new ConsoleInterface ();
            SimpleParse KimpleKarse = new SimpleParse ();
            Lexer KLexer = new Lexer ();
            List<string> enteredString = new List<string> ();

            //now we must parse into tokens
            enteredString = Kinterface.TakeInput ();
            List<string[]> allTokens = KimpleKarse.splitMultipleLines (enteredString);
            List<Token> myTokens = new List<Token> ();
            for (int i = 0; i < allTokens.Count; i++) {
                for (int j = 0; j < allTokens[i].Length; j++) {
                    Token t = KLexer.Tokenize (allTokens[i][j]);
                    if (t.KTokenType != TokenType.Empty) {
                        myTokens.Add (t);
                        Console.WriteLine ("Token #: " + myTokens.Count + " value: " + t.value + " detail: " + t.TokenDetail);
                    }
                }
            }
        }
    }

    class ConsoleInterface {
        public List<string> TakeInput () {
            List<string> enteredString = new List<string> ();
            while (Console.ReadKey (true).Key != ConsoleKey.Escape) {
                enteredString.Add (Console.ReadLine ());
            }
            foreach (string ES in enteredString) {
                Console.WriteLine (ES);
            }
            return enteredString;
        }
    }
    class SimpleParse {
        string pattern = @"([=,(){}\s*])";
        public string[] splitIntoIndiv (string oneLine) {
            if (oneLine.Length == 0) {
                Console.WriteLine ("your entry is empty!");
            }
            return Regex.Split (oneLine, pattern);
        }
        public List<string[]> splitMultipleLines (List<string> line) {
            List<string[]> allTokenValues = new List<string[]> ();
            for (int i = 0; i < line.Count; i++) {
                allTokenValues.Add (splitIntoIndiv (line[i]));
                Console.WriteLine ("\"" + string.Join ("\",\"", allTokenValues[i]));
            }
            return allTokenValues;
        }
    }
    class Lexer {
        //List<Token> allTokens = new List<Token> ();
        public Token Tokenize (string val) {
            Token myTok = new Token (val);
            float f;
            if (val == " " || val == "") {
                //destroy
                myTok.KTokenType = TokenType.Empty;
            } else if (val == "fn") {
                myTok.TokenDetail = "function";
                myTok.KTokenType = TokenType.CustomFunction;

            } else if (val == "if") {
                myTok.TokenDetail = "if";
                myTok.KTokenType = TokenType.Statement;
            } else if (val == "return") {
                myTok.TokenDetail = "return";
                myTok.KTokenType = TokenType.Statement;
            } else if (val == "plus") {
                myTok.KTokenType = TokenType.BuiltInFunction;
            } else if (val == "minus") {
                myTok.KTokenType = TokenType.BuiltInFunction;
            } else if (val == "equals") {
                myTok.KTokenType = TokenType.BuiltInFunction;
            } else if (val == "fn") {
                myTok.KTokenType = TokenType.CustomFunction;
            } else if (val == "(") {
                myTok.KTokenType = TokenType.Delimiter;
                myTok.TokenDetail = "LParen";
            } else if (val == ")") {
                myTok.KTokenType = TokenType.Delimiter;
                myTok.TokenDetail = "RParen";
            } else if (val == "{") {
                //start of a function 
                myTok.KTokenType = TokenType.Delimiter;
                myTok.TokenDetail = "LCurly";
            } else if (val == "}") {
                myTok.KTokenType = TokenType.Delimiter;
                myTok.TokenDetail = "RCurly";
            } else if (val == ",") {
                //i guess this will separate parameters in functions
                myTok.KTokenType = TokenType.Delimiter;
                myTok.TokenDetail = "Comma";
            } else if (val == "\"") {
                //this makes a custom variable into a string, but MUST be closed by another "
                myTok.KTokenType = TokenType.Expression;
                myTok.TokenDetail = "String";
            } else if (val == ";") {
                //does anyone need semicolons? debatable
            } else if (float.TryParse (val, out f)) {
                myTok.KTokenType = TokenType.Expression;
                myTok.TokenDetail = "Number";
            } else {
                myTok.TokenDetail = "Unknown";
            }
            return myTok;
        }
    }

    class Token {
        public TokenType KTokenType;
        public string TokenDetail;
        public string value;
        public Token () {

        }
        public Token (string value) {
            this.value = value;
            this.TokenDetail = null;
            this.KTokenType = TokenType.Undefined;
        }
    }
    class BuiltInFunctions {
        public float plus (float a, float b) {
            return a + b;
        }
        public float minus (float a, float b) {
            return a - b;
        }
        public virtual bool equals (float a, float b) {
            const float marginErr = 0.001f;
            if (Math.Abs (a - b) < marginErr) {
                return true;
            }
            return false;
        }
        public virtual bool equals (string a, string b) {
            if (a == b) {
                return true;
            }
            return false;
        }
    }
}