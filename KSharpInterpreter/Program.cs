using System;
using System.Collections;
using System.Collections.Generic;

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
    class MainClass {
        public static void Main (string[] args) {
            Console.WriteLine ("Write your code here! Press 'enter' + 'esc' when you are ready to compile");
            Syntax Kyntax = new Syntax ();
            ConsoleInterface Kinterface = new ConsoleInterface ();
            SimpleParse KimpleKarse = new SimpleParse ();
            List<string> enteredString = new List<string> ();

            //now we must parse into tokens
            //enteredString = Kinterface.TakeInput ();
            //string[] split = KimpleKarse.splitIntoIndiv (enteredString[0]);
            //Console.WriteLine (string.Join (", ", split));
        }
    }

    class ConsoleInterface {
        public List<string> TakeInput () {
            List<string> enteredString = new List<string> ();
            while (Console.ReadKey (true).Key != ConsoleKey.Enter) {
                if (Console.ReadKey (true).Key == ConsoleKey.Escape) {
                    continue;
                }
                enteredString.Add (Console.ReadLine ());
            }
            foreach (string ES in enteredString) {
                Console.WriteLine (ES);
            }
            return enteredString;
        }
    }
    class SimpleParse {
        char[] delimiters = new char[4] { ' ', '(', ')', ';' };
        public string[] splitIntoIndiv (string oneLine) {
            if (oneLine.Length == 0) {
                //alert error, empty input
            }
            return oneLine.Split (delimiters);
        }
    }
    class Lexer {

    }
    class Syntax {

    }
}