using System;
using System.Collections.Generic;
using IronText.Framework;
using IronText.Reflection;

namespace Calculator
{
    [Language]
    [Precedence("=", 0, Associativity.None)]
    [Precedence("-", 1)]
    [Precedence("+", 1)]
    [Precedence("*", 2)]
    [Precedence("/", 2)]
    [Precedence("%", 2)]
    [Precedence("^", 10, Associativity.Right)]
    // [DescribeParserStateMachine("Calculator.info")]
    // [ParserGraph("Calculator.gv")]
    public class Calculator
    {
        const string _ = null;

        public readonly Dictionary<string,double> Variables = new Dictionary<string,double>();

        [Outcome]
        public double Result { get; set; }

        public bool Done { get; set; }

        [Produce]
        public double Number(Const<double> c) { return c == null ? 0 : c.Value; }

        [Produce]
        public double VarRef(string name) { return Variables[name]; }

        [Produce(_, "+", _)]
        public double Plus(double x, double y) { return  x + y; }
        
        [Produce(_, "-", _)]
        public double Minus(double x, double y) { return  x - y; }

        [Produce(_, "*", _)]
        public double Prod(double x, double y) { return  x * y; }

        [Produce(_, "/", _)]
        public double Div(double x, double y) { return  x / y; }

        [Produce(_, "%", _)]
        public double Mod(double x, double y) { return  x % y; }

        [Produce(_, "^", _)]
        public double Pow(double x, double y) { return  Math.Pow(x, y); }

        [Produce("sqrt", "(", _, ")")]
        public double Sqrt(double x) { return Math.Sqrt(x); }

        [Produce("sin", "(", _, ")")]
        public double Sin(double x) { return Math.Sin(x); }

        [Produce("cos", "(", _, ")")]
        public double Cos(double x) { return Math.Cos(x); }

        [Produce(_, "=", _)]
        public double Let(string var, double rexpr) { Variables[var] = rexpr; return 0; }

        [Produce("print", "(", _, ")")]
        public double Print(double expr) { Console.WriteLine(expr); return 0; }

        [Produce("exit")]
        [Produce("quit")]
        public void Exit() { Done = true; }

        [Match("blank+")]
        public void Space() { }

        [Match("alpha alnum*")]
        public string Identifier(string name) { return name; }

        [Match("digit+ ('.' digit*)?")]
        public Const<double> Number(string text)
        { 
            return new Const<double>(double.Parse(text));
        }

        public class Const<T> 
        {
            public readonly T Value;

            public Const(T value) { Value = value; }

            public override string ToString()
            {
                return Value.ToString();
            }
        }
    }
}
