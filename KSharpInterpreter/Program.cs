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
        Expression, //function call
        BuiltInFunction, //plus, minus, equals
        CustomFunction, //keyword "fn" 
        Delimiter, //LParen, RParen, comma
        Assigner, // =
        NumType, //literal
        StringType, //literal
        Variable,
        Empty,
        Undefined
    }
    class MainClass {
        public static void Main (string[] args) {

            Dictionary<string, string> StringsInMemory = new Dictionary<string, string> ();
            Dictionary<string, float> NumsInMemory = new Dictionary<string, float> ();
            Console.WriteLine ("Write your code here! Press 'enter' + 'esc' when you are ready to compile");
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
                        Console.WriteLine ("Token #: " + myTokens.Count + " value: " + t.value + " tokenType: " + t.KTokenType + " numVal: " + t.numericalVal);
                    }
                }
            }
            myASTs = myAST.BuildIndivTreesFromRoot (myTokens);
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
                //Console.WriteLine ("\"" + string.Join ("\",\"", allTokenValues[i]));
            }
            return allTokenValues;
        }
    }
    class Lexer {
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
            } else if (val == "{") { //start of a function inscription
                myTok.KTokenType = TokenType.Delimiter;
            } else if (val == "}") { //end of a function inscription
                myTok.KTokenType = TokenType.Delimiter;
            } else if (val == ",") { //this will separate parameters in functions
                myTok.KTokenType = TokenType.Delimiter;
            } else if (val == "\"") { //this makes a custom variable into a string, but MUST be closed by another "
                myTok.KTokenType = TokenType.StringType;
            } else if (val == "=") {
                myTok.KTokenType = TokenType.Assigner;
            } else if (float.TryParse (val, out f)) {
                myTok.KTokenType = TokenType.NumType;
                myTok.numericalVal = float.Parse (val);
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
        // public List<ASTNode> optionalMiddleNodes;
        public ASTNode right;
        // public ASTNode parent;
        // public bool visited;
        public ASTNode (int id, Token myToken) {
            this.id = id;
            this.myToken = myToken;
            left = null;
            // optionalMiddleNodes = null;
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
            switch (ast.ASTtype) {
                case AST.TreeType.Undefined:
                    if (ast.root.myToken.KTokenType == TokenType.Assigner) {
                        if (stringMem.ContainsKey (ast.AddNumberToMemory ().Item1)) {
                            stringMem[ast.AddStringToMemory ().Item1] = ast.AddStringToMemory ().Item2;
                        } else if (numMem.ContainsKey (ast.AddNumberToMemory ().Item1)) {
                            numMem[ast.AddNumberToMemory ().Item1] = ast.AddNumberToMemory ().Item2; //override existing num in memory
                        }
                    }
                    break;
                case AST.TreeType.AssignmentNum:
                    numMem.Add (ast.AddNumberToMemory ().Item1, ast.AddNumberToMemory ().Item2);
                    break;
                case AST.TreeType.AssignmentString:
                    stringMem.Add (ast.AddStringToMemory ().Item1, ast.AddStringToMemory ().Item2);
                    break;
                case AST.TreeType.BinaryOperation:
                    if (ast.root.myToken.value == "equals") {
                        if (ast.BinaryComparison (ast)) {
                            result = "true";
                        } else {
                            result = "false";
                        }
                    } else {
                        result = ast.BinaryOperation (ast).ToString ();
                    }
                    break;
                case AST.TreeType.Return:
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
                    break;
            }
            return result;
        }
        public virtual void TraverseCompoundTree (AST myAST) {
            if (myAST.root == null) {
                return;
            }
            Stack<ASTNode> nodeStack = new Stack<ASTNode> ();
            ASTNode current = myAST.root;
            // traverse the tree  
            while (current != null || nodeStack.Count > 0) {
                while (current != null) {
                    nodeStack.Push (current);
                    current = current.left;
                }
                current = nodeStack.Pop ();
                Console.WriteLine ("traversing: " + current.myToken.value + ", ");
                current = current.right;
            }
        }
        public List<AST> BuildIndivTreesFromRoot (List<Token> tokens) {
            List<AST> myASTs = new List<AST> ();
            for (int i = 0; i < tokens.Count; i++) {
                ASTNode rootNode = new ASTNode (i, tokens[i]);
                if (tokens[i].KTokenType == TokenType.Assigner && tokens[i].value == "=") {
                    AssignmentTree AssignmentAST = new AssignmentTree ();
                    myASTs.Add (AssignmentAST.ConstructAnAssignment (tokens, rootNode));
                } else if (tokens[i].KTokenType == TokenType.BuiltInFunction) {
                    BinaryOp BinaryAST = new BinaryOp ();
                    myASTs.Add (BinaryAST.ConstructBinaryOperationTree (tokens, rootNode));
                } else if (tokens[i].KTokenType == TokenType.Statement && tokens[i].value == "if") {
                    Conditional cond = new Conditional ();
                    myASTs.Add (cond.ConstructConditional (tokens));
                } else if (tokens[i].value == "return") {
                    ReturnStatement returnStatement = new ReturnStatement ();
                    myASTs.Add (returnStatement.EvaluateTree (rootNode, tokens[i + 1].value));
                }
            }
            return myASTs;
        }
        //assignment methods 
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
        public bool BinaryComparison (AST ast) { //built in function "equals"
            bool result = false;
            if (ast.root.myToken.value == "equals") {
                if (ast.root.left.myToken.KTokenType != ast.root.right.myToken.KTokenType) {
                    return result; //eventually throw type incompatibility error
                }
                if (ast.root.left.myToken.KTokenType == TokenType.NumType) {
                    result = builtFunc.equals (ast.root.left.myToken.numericalVal, ast.root.right.myToken.numericalVal);
                } else {
                    result = builtFunc.equals (ast.root.left.myToken.value, ast.root.right.myToken.value);
                }
            }
            return result;
        }
    }
    public class BinaryOp : AST {
        public AST ConstructBinaryOperationTree (List<Token> tokens, ASTNode myRoot) {
            AST myTree = new AST ();
            for (int i = 0; i < tokens.Count; i++) {
                if (tokens[i].KTokenType == TokenType.BuiltInFunction) {
                    myTree.root = new ASTNode (i, tokens[i]);
                    break;
                }
            }
            myTree.root.left = new ASTNode (myTree.root.id + 2, tokens[myTree.root.id + 2]);
            myTree.root.right = new ASTNode (myTree.root.id + 4, tokens[myTree.root.id + 4]);
            myTree.ASTtype = TreeType.BinaryOperation;
            return myTree;
        }
    }
    public class AssignmentTree : AST {
        public AST ConstructAnAssignment (List<Token> tokens, ASTNode myRoot) {
            AST myTree = new AST ();
            myTree.root = myRoot;
            myTree.root.left = new ASTNode (myTree.root.id - 1, tokens[myTree.root.id - 1]);
            myTree.root.right = new ASTNode (myTree.root.id + 1, tokens[myTree.root.id + 1]);

            if (tokens[myTree.root.id - 2].value == "num") {
                myTree.ASTtype = TreeType.AssignmentNum;
            } else if (tokens[myTree.root.id - 2].value == "string") {
                myTree.root.right.myToken.value = myTree.root.right.myToken.value.Trim ('"');
                myTree.ASTtype = TreeType.AssignmentString;
            }
            return myTree;
        }
    }
    public class Conditional : AST {
        public AST ConstructConditional (List<Token> tokens) {
            AST myTree = new AST ();
            for (int i = 0; i < tokens.Count; i++) {
                if (tokens[i].KTokenType == TokenType.Statement && tokens[i].value == "if") {
                    myTree.root = new ASTNode (i, tokens[i]);
                    break;
                }
            }
            for (int i = 0; i < tokens.Count; i++) {
                ASTNode myNode = new ASTNode (i, tokens[i]);
            }
            myTree.ASTtype = TreeType.Conditional;
            return myTree;
        }
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