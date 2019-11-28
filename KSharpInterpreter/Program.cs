using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KSharpInterpreter {
    /* An interpreter for K#, a simple programming language built with C#

    Not whitespace sensitive, and no semicolons needed
    Included variable types: num, string

    Keywords: return, if

    Currently supported functions:
    plus(num a, num b)
    minus(num a, num b)
    equals(num a, num b)
    equals(string a, string b)

    Works as a console application. Type the code into the console. To exit code entering mode & start interpreting, press esc. */
    public enum TokenType {
        Statement = 0, //functionDefinition, if, return
        Expression, //literal (number, string), function call, built-in-function
        BuiltInFunction, //plus, minus, equals
        CustomFunction, //keyword "fn" 
        Delimiter, //LParen, RParen, dot, comma, semicolon
        Assigner,
        NumType,
        StringType,
        NumType, //literal
        StringType, //literal
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
                        Console.WriteLine ("Token #: " + myTokens.Count + " value: " + t.value + " tokenType: " + t.KTokenType);
                    }
                }
            }
            myASTs = (myAST.IdentifyRoot (myTokens));
            Console.Write ("myASTs count: " + myASTs.Count);
            foreach (AST ast in myASTs) {
                //ast.WalkTree (ast);
                //ast.inorder (ast);
                Console.WriteLine ("returning: " + ast.EvaluateResult (ast, NumsInMemory, StringsInMemory));
            }
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
                //discard this
                myTok.KTokenType = TokenType.Empty;
            } else if (val == "fn") {
                myTok.KTokenType = TokenType.CustomFunction;
            } else if (val == "if") {
                myTok.KTokenType = TokenType.Statement;
            } else if (val == "return") {
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
            } else if (val == ")") {
                myTok.KTokenType = TokenType.Delimiter;
            } else if (val == "{") {
                //start of a function inscription
                myTok.KTokenType = TokenType.Delimiter;
            } else if (val == "}") {
                //end of a function inscription
                myTok.KTokenType = TokenType.Delimiter;
            } else if (val == ",") {
                //this will separate parameters in functions
                myTok.KTokenType = TokenType.Delimiter;
            } else if (val == "\"") {
                //this makes a custom variable into a string, but MUST be closed by another "
                myTok.KTokenType = TokenType.StringType;
            } else if (val == "=") {
                myTok.KTokenType = TokenType.Assigner;
            } else if (float.TryParse (val, out f)) {
                myTok.KTokenType = TokenType.NumType;
            } else { }
            return myTok;
        }
    }

    public class Token {
        public TokenType KTokenType;
        public float numericalVal;
        public string value;
        public Token (string value) {
            this.value = value;
            this.KTokenType = TokenType.Undefined;
        }
    }
    public class BuiltInFunctions {
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
        public enum TreeType {
            Undefined = 0,
            AssignmentNum,
            AssignmentString,
            BinaryOperation,
            Conditional,
            Return
        }
        public TreeType ASTtype;
        public ASTNode root = null;
        public BuiltInFunctions builtFunc = new BuiltInFunctions ();
        // public ASTNode WalkDownToEndOfBranch (AST myAST, Dictionary<string, float> num, Dictionary<string, string> str) {
        //     ASTNode prevRoot = myAST.root;
        //     ASTNode currentRoot = myAST.root;
        //     while (currentRoot.left != null) {
        //         prevRoot = currentRoot;
        //         currentRoot = currentRoot.left;
        //     }
        //     if (currentRoot != myAST.root) {
        //         prevRoot.myToken.value = EvaluateResult (myAST, num, str);
        //     }
        //     return currentRoot;
        // }
        // public ASTNode WalkTreeBubbleUp (AST myAST) { //inorder traversal
        //     ASTNode prevRoot = myAST.root;
        //     ASTNode currentRoot = myAST.root;
        //     while (currentRoot.left != null) {
        //         currentRoot = currentRoot.left;
        //         //go down to the lowest root
        //         Console.Write ("root: " + myAST.root.myToken.value);
        //         if (currentRoot.left != null) Console.Write (" left: " + currentRoot.left.myToken.value);
        //         if (currentRoot.right != null) {
        //             Console.Write (" right: " + currentRoot.right.myToken.value);
        //             // if (currentRoot.right.myToken.value == "return") {
        //             //     EvaluateTree (currentRoot.right, currentRoot.right.left.myToken.value);
        //             //     Console.WriteLine (currentRoot.right.left.myToken.value);
        //             //     return;
        //             // }
        //         }
        //         // if (currentRoot.right != null && currentRoot.right.myToken.value == "return") {
        //         //     EvaluateTree (currentRoot.right, currentRoot.right.left.myToken.value);
        //         //     Console.WriteLine (currentRoot.right.left.myToken.value);
        //         //     return;
        //         // }
        //         prevRoot = currentRoot;
        //         currentRoot = currentRoot.left;
        //     }
        // }
        public string EvaluateResult (AST ast, Dictionary<string, float> numMem, Dictionary<string, string> stringMem) {
            string result = "";
            if (ast.ASTtype == AST.TreeType.Undefined) {
                if (numMem.ContainsKey (ast.AddNumberToMemory ().Item1)) {
                    if (ast.root.myToken.KTokenType == TokenType.Assigner) {
                        float f;
                        if (float.TryParse (ast.root.right.myToken.value, out f)) {
                            ast.root.right.myToken.numericalVal = float.Parse (ast.root.right.myToken.value);
                        }
                        numMem[ast.AddNumberToMemory ().Item1] = ast.AddNumberToMemory ().Item2;
                    }
                }
            }
            if (ast.ASTtype == AST.TreeType.AssignmentNum) {
                numMem.Add (ast.AddNumberToMemory ().Item1, ast.AddNumberToMemory ().Item2);
            } else if (ast.ASTtype == AST.TreeType.AssignmentString) {
                if (stringMem.ContainsKey (ast.AddStringToMemory ().Item1)) {
                    stringMem[ast.AddStringToMemory ().Item1] = ast.AddStringToMemory ().Item2;
                } else {
                    stringMem.Add (ast.AddStringToMemory ().Item1, ast.AddStringToMemory ().Item2);
                }
            } else if (ast.ASTtype == AST.TreeType.BinaryOperation) {
                result = ast.BinaryOperation (ast).ToString ();

            } else if (ast.ASTtype == AST.TreeType.Return) {
                if (ast.root.left.myToken.KTokenType == TokenType.BuiltInFunction) {
                    result = ast.BinaryOperation (ast).ToString ();
                } else if (ast.root.left.myToken.KTokenType == TokenType.NumType) {
                    result = ast.root.left.myToken.value;
                }
                if (numMem.ContainsKey (ast.root.left.myToken.value)) {
                    result = numMem[ast.root.left.myToken.value].ToString ();
                } else if (stringMem.ContainsKey (ast.root.left.myToken.value)) {
                    result = stringMem[ast.root.left.myToken.value];
                }
            }
            return result;
        }
        public List<AST> IdentifyRoot (List<Token> oneLine) {
            List<AST> myASTs = new List<AST> ();
            for (int i = 0; i < oneLine.Count; i++) {
                ASTNode rootNode = new ASTNode (i, oneLine[i]);
                if (oneLine[i].KTokenType == TokenType.Assigner && oneLine[i].value == "=") {
                    AssignmentTree AssignmentAST = new AssignmentTree ();
                    myASTs.Add (AssignmentAST.ConstructAnAssignment (oneLine, rootNode));
                } else if (oneLine[i].KTokenType == TokenType.BuiltInFunction) {
                    BinaryOp BinaryAST = new BinaryOp ();
                    myASTs.Add (BinaryAST.ConstructBinaryOperationTree (oneLine, rootNode));
                } else if (oneLine[i].KTokenType == TokenType.Statement && oneLine[i].value == "if") {
                    Conditional cond = new Conditional ();
                    myASTs.Add (cond.ConstructConditional (oneLine));
                } else if (oneLine[i].value == "return") {
                    ReturnStatement returnStatement = new ReturnStatement ();
                    myASTs.Add (returnStatement.EvaluateTree (rootNode, oneLine[i + 1].value));
                }
            }
            return myASTs;
        }
        public Tuple<string, string> AddStringToMemory () {
            return Tuple.Create (root.left.myToken.value, root.right.myToken.value);
        }
        public Tuple<string, float> AddNumberToMemory () {
            return Tuple.Create (root.left.myToken.value, root.right.myToken.numericalVal);
        }

        public float BinaryOperation (AST ast) { //built in function methods "plus" & "minus"
            float result = -1;
            if (ast.root.myToken.value == "plus") {
                result = builtFunc.plus (ast.root.left.myToken.numericalVal, ast.root.right.myToken.numericalVal);
            } else if (ast.root.myToken.value == "minus") {
                result = builtFunc.minus (ast.root.left.myToken.numericalVal, ast.root.right.myToken.numericalVal);
            }
            return result;
        }
    }

    public class BinaryOp : AST {
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
            myTree.ASTtype = TreeType.BinaryOperation;
            return myTree;
        }
    }

    public class AssignmentTree : AST {
        public AST ConstructAnAssignment (List<Token> oneLine, ASTNode myRoot) {
            AST myTree = new AST ();
            myTree.root = myRoot;
            myTree.root.left = new ASTNode (myTree.root.id - 1, oneLine[myTree.root.id - 1]);
            myTree.root.right = new ASTNode (myTree.root.id + 1, oneLine[myTree.root.id + 1]);

            if (oneLine[myTree.root.id - 2].value == "num") {
                myTree.root.right.myToken.numericalVal = float.Parse (myTree.root.right.myToken.value);
                myTree.ASTtype = TreeType.AssignmentNum;

            } else if (oneLine[myTree.root.id - 2].value == "string") {
                myTree.root.right.myToken.value = myTree.root.right.myToken.value.Trim ('"');
                myTree.ASTtype = TreeType.AssignmentString;

            }
            //if first index is “num”, the following index is variable name. check that the same name does not exist. this index is left node
            //the index after equals is right node. store as decimal type
            //if first index is “string”, 
            //}
            return myTree;
        }
    }

    public class Conditional : AST {
        public AST ConstructConditional (List<Token> oneLine) {
            AST myTree = new AST ();
            for (int i = 0; i < oneLine.Count; i++) {
                if (oneLine[i].KTokenType == TokenType.Statement && oneLine[i].value == "if") {
                    myTree.root = new ASTNode (i, oneLine[i]);
                    break;
                }
            }
            for (int i = 0; i < oneLine.Count; i++) {
                ASTNode myNode = new ASTNode (i, oneLine[i]);

            }
            myTree.ASTtype = TreeType.Conditional;
            return myTree;
        }
        // }
    }
    public class ReturnStatement : AST {
        public AST EvaluateTree (ASTNode node, string returnVal) {
            AST myTree = new AST ();
            myTree.root = node;
            node.left = new ASTNode (-1, new Token (returnVal));
            myTree.ASTtype = TreeType.Return;
            return myTree;
        }
    }
}