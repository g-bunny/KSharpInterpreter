using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KSharpInterpreter {
    //sample input to interpret:
    //fn AddIfNotSame(num a, num b){
    //  if(equals(a,b)){
    //      return minus(a,b)
    //  }
    //  return plus(a,b)
    //num a = AddIfNotSame(3,4)
    //num b = AddIfNotSame(4,5)
    //string c = "Hello World"
    //return plus(a,b)
    //esc
    public enum TokenType {
        Statement = 0, //functionDefinition, if, return
        Expression, //literal (number, string), function call, built-in-function
        BuiltInFunction, //plus, minus, equals
        CustomFunction, //keyword "fn" 
        Delimiter, //LParen, RParen, dot, comma, semicolon
        Assigner,
        Empty,
        Undefined
    }
    class MainClass {
        public static void Main (string[] args) {

            Dictionary<string, string> StringsInMemory = new Dictionary<string, string> ();
            Dictionary<string, float> NumsInMemory = new Dictionary<string, float> ();
            Console.WriteLine ("Write your code here! Press 'enter' + 'esc' when you are ready to compile");
            BuiltInFunctions BuiltInKunctions = new BuiltInFunctions ();
            ConsoleInterface Kinterface = new ConsoleInterface ();
            SimpleParse KimpleKarse = new SimpleParse ();
            Lexer KLexer = new Lexer ();
            List<string> enteredString = new List<string> ();
            //now we must parse into tokens
            enteredString = Kinterface.TakeInput ();
            List<string[]> allTokens = KimpleKarse.splitMultipleLines (enteredString);
            List<Token> myTokens = new List<Token> ();
            AST myAST = new AST ();
            List<AST> myASTs = new List<AST> ();
            for (int i = 0; i < allTokens.Count; i++) {
                for (int j = 0; j < allTokens[i].Length; j++) {
                    Token t = KLexer.Tokenize (allTokens[i][j]);
                    if (t.KTokenType != TokenType.Empty) {
                        myTokens.Add (t);
                        Console.WriteLine ("Token #: " + myTokens.Count + " value: " + t.value + " detail: " + t.TokenDetail);
                    }
                }
            }
            myASTs = (myAST.IdentifyRoot (myTokens));
            Console.Write ("myASTs count: " + myASTs.Count);
            foreach (AST ast in myASTs) {
                ast.WalkTree (ast);
            }
            // Console.Write (myAST.root.myToken.value);

            // while (myAST.root != null) {
            //     //go down to the lowest root
            //     Console.Write ("root: " + myAST.root.myToken.value + " left: " + myAST.root.left.myToken.value + " right: " + myAST.root.right.myToken.value);
            //     myAST.root = myAST.root.left;
            // }
            //Console.Write (myAST.root.myToken.value);
            //bool thinggy = myAST.WalkTree ();
        }
    }

    class ConsoleInterface {
        public List<string> TakeInput () {
            List<string> enteredString = new List<string> ();
            while (Console.ReadKey (true).Key != ConsoleKey.Escape) {
                enteredString.Add (Console.ReadLine ());
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
            } else if (val == "=") {
                myTok.KTokenType = TokenType.Assigner;
                myTok.TokenDetail = "equalsAssigner";
            } else if (float.TryParse (val, out f)) {
                myTok.KTokenType = TokenType.Expression;
                myTok.TokenDetail = "Number";
            } else {
                myTok.TokenDetail = "Unknown";
            }
            return myTok;
        }
    }

    public class Token {
        public TokenType KTokenType;
        public string TokenDetail;
        public int lineId;
        public int horizId;
        public float numericalVal;
        public string value;
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

    public class ASTNode {
        public int id;
        public Token myToken;
        public ASTNode left;
        public ASTNode right;
        public ASTNode (int id, Token myToken) {
            this.id = id;
            this.myToken = myToken;
            left = null;
            right = null;
        }
    }
    public class AST {
        public ASTNode root = null;
        public AST () {

        }
        public void WalkTree (AST myAST) { //inorder traversal
            ASTNode prevRoot = myAST.root;
            ASTNode currentRoot = prevRoot;
            while (currentRoot.left != null) {
                //go down to the lowest root
                Console.Write ("root: " + myAST.root.myToken.value);
                if (currentRoot.left != null) Console.Write (" left: " + currentRoot.left.myToken.value);
                if (currentRoot.right != null) {
                    Console.Write (" right: " + currentRoot.right.myToken.value);
                    // if (currentRoot.right.myToken.value == "return") {
                    //     EvaluateTree (currentRoot.right, currentRoot.right.left.myToken.value);
                    //     Console.WriteLine (currentRoot.right.left.myToken.value);
                    //     return;
                    // }
                }
                // if (currentRoot.right != null && currentRoot.right.myToken.value == "return") {
                //     EvaluateTree (currentRoot.right, currentRoot.right.left.myToken.value);
                //     Console.WriteLine (currentRoot.right.left.myToken.value);
                //     return;
                // }
                prevRoot = currentRoot;
                currentRoot = currentRoot.left;
            }
        }
        public List<AST> IdentifyRoot (List<Token> oneLine) {
            List<AST> myASTs = new List<AST> ();
            for (int i = 0; i < oneLine.Count; i++) {
                ASTNode rootNode = new ASTNode (i, oneLine[i]);
                if (oneLine[i].KTokenType == TokenType.Assigner && oneLine[i].TokenDetail == "equalsAssigner") {
                    AssignmentTree AssignmentAST = new AssignmentTree (rootNode);
                    myASTs.Add (AssignmentAST.ConstructAnAssignment (oneLine, rootNode));
                } else if (oneLine[i].KTokenType == TokenType.BuiltInFunction) {
                    BinaryOp BinaryAST = new BinaryOp (rootNode);
                    myASTs.Add (BinaryAST.ConstructBinaryOperationTree (oneLine, rootNode));
                } else if (oneLine[i].KTokenType == TokenType.Statement && oneLine[i].TokenDetail == "if") { } else if (oneLine[i].value == "return") {
                    ReturnStatement returnStatement = new ReturnStatement ();
                    myASTs.Add (returnStatement.EvaluateTree (rootNode, oneLine[i + 1].value));
                }
            }
            return myASTs;
        }
    }

    public class BinaryOp : AST {
        public BinaryOp (ASTNode op) {
            this.root = op;
            //op becomes root 
        }
        public AST ConstructBinaryOperationTree (List<Token> oneLine, ASTNode myRoot) {
            AST myTree = new AST ();
            for (int i = 0; i < oneLine.Count; i++) {
                if (oneLine[i].KTokenType == TokenType.BuiltInFunction) {
                    myTree.root = new ASTNode (i, oneLine[i]);
                    break;
                }
            }
            for (int i = 0; i < oneLine.Count; i++) {
                ASTNode myNode = new ASTNode (i, oneLine[i]);
                //populate the children... the way the grammar is structured, it expects a '(', first param, second param, ')'
                if (i == myTree.root.id + 2) {
                    myTree.root.left = myNode;
                } else if (i == myTree.root.id + 4) {
                    myTree.root.right = myNode;
                }
            }
            return myTree;
        }
    }

    public class AssignmentTree : AST {
        public AssignmentTree (ASTNode op) {
            this.root = op;
        }
        public AST ConstructAnAssignment (List<Token> oneLine, ASTNode myRoot) {
            AST myTree = new AST ();
            myTree.root = myRoot;
            myTree.root.left = new ASTNode (myTree.root.id - 1, oneLine[myTree.root.id - 1]);
            myTree.root.right = new ASTNode (myTree.root.id + 1, oneLine[myTree.root.id + 1]);

            if (oneLine[myTree.root.id - 2].TokenDetail == "num") {
                myTree.root.right.myToken.numericalVal = float.Parse (myTree.root.right.myToken.value);
            } else if (oneLine[myTree.root.id - 2].TokenDetail == "string") {
                myTree.root.right.myToken.value = myTree.root.right.myToken.value.Trim ('"');
            }

            //if first index is “num”, the following index is variable name. check that the same name does not exist. this index is left node
            //the index after equals is right node. store as decimal type
            //if first index is “string”, 
            //}
            return myTree;
        }
        public Tuple<string, string> AddStringToMemory (AssignmentTree AT) {
            return Tuple.Create (AT.root.left.myToken.value, AT.root.right.myToken.value);
        }

        public Tuple<string, float> AddNumberToMemory (AssignmentTree AT) {
            return Tuple.Create (AT.root.left.myToken.value, AT.root.right.myToken.numericalVal);
        }
        //evaluate a fully formed assignment tree:

    }

    public class Conditional : AST {
        public Conditional (AST ast, ASTNode left, ASTNode op, ASTNode right) {
            //op becomes root .. which is "if" in this case
            AST ConstructConditional (Token[] oneLine) {
                AST myTree = new AST ();
                for (int i = 0; i < oneLine.Length; i++) {
                    if (oneLine[i].KTokenType == TokenType.Statement && oneLine[i].TokenDetail == "if") {
                        myTree.root = new ASTNode (i, oneLine[i]);
                        break;
                    }
                }
                for (int i = 0; i < oneLine.Length; i++) {
                    ASTNode myNode = new ASTNode (i, oneLine[i]);

                }
                return myTree;
            }
        }
    }
    public class ReturnStatement : AST {
        public AST EvaluateTree (ASTNode node, string returnVal) {
            AST returnAst = new AST ();
            returnAst.root = node;
            node.left = new ASTNode (-1, new Token (returnVal));
            return returnAst;
        }
    }

}