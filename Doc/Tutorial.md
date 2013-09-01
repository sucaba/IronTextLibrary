IronText in 10 minutes
======================

Tutorial demonstrates step-by-step creation of the custom config file parser.


Step 1: Boilerplate code
------------------------

Following parser accepts "foo" *literal* text and fails with any other input.
Note: *Literal* means means constant text like keyword or delimiter.

```
using IronText.Framework;

public class Program
{
    public static void Main()
    {
        var context = new MyLang();
        try
        {
            Language.Parse(context, "foo");
            Console.WriteLine(context.Result);

            // Should print 'foo'
        }
        catch (SyntaxException e)
        {
            Console.WriteLine("error ({0}): {1}", e.HLocation.FirstColumn, e.Message);
        }
    }

    [Language]
    public class MyLang
    {
        // In case of success setter will be invoked with a final result of parsing.
        // Getter is optional.
        [ParseResult]
        public string Result { get; set; } 

        // Scans 'foo' text and sends value to the parser
        [Scan("'foo'")]
        public string FooKeyword(string text) { return text; }
    }
}
```

Step 2: Parsing integer values
------------------------------

Following creates parser which can parse integer values.


```
using IronText.Framework;

public class Program
{
    public static void Main()
    {
        var context = new MyLang();
        try
        {
            Language.Parse(context, "12345");
            Console.WriteLine(context.Result);

            // Should produce 12345 output
        }
        catch (SyntaxException e)
        {
            Console.WriteLine("error ({0}): {1}", e.HLocation.FirstColumn, e.Message);
        }
    }

    [Language]
    public class MyLang
    {
        [ParseResult]
        public int Result { get; set; } 

        [Scan("digit+")]
        public int Integer(string text) { return int.Parse(text); }
    }
}
```

Step 3: Comma-separted list 
---------------------------

```
...
Language.Parse(context, "12, 3  ,  456");
...

[Language]
public class MyLang
{
    [ParseResult]
    public List<int> Result { get; set; } 

    // BNF: list ::= /*empty*/
    [Parse]
    public List<int> Empty() { return new List<int>(); }

    // ParseAttribute specifies literal-mask which contains
    // null items in place of argumenst and text for literals.
    // Trailing null items are optional.
    // BNF:  list ::= list ',' INT
    [Parse]
    public List<int> List(List<int> items, int newItem) { items.Add(newItem); return items; }

    [Scan("digit+")]
    public int Scan(string text) { return int.Parse(text); }

    [Scan("blank+")]
    public void Blank() { }
}
```

Step 4: Named integer values
----------------------------

```
...
Language.Parse(context, "Width=12; Heigth=300; TabSize=4;");
...

[Language]
public class MyLang
{
    [ParseResult]
    public Dictionary<string,int> Result { get; set; } 

    [Parse]
    public Dictionary<string,int> Empty() 
    { 
        return new Dictionary<string,int>();
    }

    [Parse(null, null, "=", null, ";")]
    public Dictionary<string,int> Pair(
            Dictionary<string,int> items,
            string name,
            int value) 
    {
        items.Add(name, value)
        return items;
    }

    [Scan("('_' | alpha) alnum*")]
    public string Name(string text) { return text; }

    [Scan("digit+")]
    public int Integer(string text) { return int.Parse(text); }

    [Scan("blank+")]
    public void Blank() { }
}
```

Step 5: Adding more value types
-------------------------------

```
...
var config = new MyConfig();
Language.Parse(config, "Width=12; Heigth=300; Title="Hello world"; Pi=3.14;");
...

[Language]
public class MyConfig
{
    [ParseResult]
    public Dictionary<string,object> Result { get; set; } 

    [Parse]
    public Dictionary<string,object> Empty() { return new Dictionary<string,object>(); }

    [Parse(null, null, "=", null, ";")]
    public Dictionary<string,object> Pair(
            Dictionary<string,object> items,
            string name,
            object value) 
    {
        items.Add(name, value)
        return items;
    }

    [Scan("('_' | alpha) alnum*")]
    public string Name(string text) { return text; }

    [Scan("quot ~quot* quot")]
    public object QuotedString(string text)
    { 
        return new SQStr(text.Substring(1, text.Length - 2)); 
    }

    [Scan("digit+ '.' digit* | '.' digit+")]
    public object Double(string text) { return double.Parse(text); }

    [Scan("digit+")]
    public object Integer(string text) { return int.Parse(text); }

    [Scan("blank+")]
    public void Blank() { }
}

// Single quote token
public class SQStr
{
    public SQStr(string text) { this.Text = text; }

    public string Text { get; private set; }
}

```

Step 6: Reuse Ctem elements
---------------------------


```
...
var config = new MyConfig();
Language.Parse(config, 
    @"
    Width  = 12;
    Heigth = 300;
    Title  = "Hello world";
    Pi     = 3.14;
    ");
...

using IronText.Framework;
using IronText.Lib.Ctem;

[Language]
public class MyConfig
{
    // Initialize instance of Ctem sub-context
    public MyConfig() { Scanner = new CtemScanner(); }

    [ParseResult]
    public Dictionary<string,object> Result { get; set; } 

    // Adds reusable lexical elements for a C-like language:
    //   - quoted strings
    //   - numbers
    //   - single-line comments
    //   - multi-line comments
    //   - blanks
    //   - new lines
    [SubContext]
    public CtemScanner Scanner { get; private set; }

    [Parse]
    public Dictionary<string,object> Empty() { return new Dictionary<string,object>(); }

    [Parse(null, null, "=", null, ";")]
    public Dictionary<string,object> AddPair(
            Dictionary<string,object> items,
            string name,
            object value) 
    {
        items.Add(name, value)
        return items;
    }

    [Parse]
    public object QuotedStringValue(QStr str) { return str.Text; }

    [Parse]
    public object NumberValue(Num num)
    {
        return num.Contains('.') 
             ? double.Parse(num.Text) 
             : int.Parse(num.Text);
    }
}

```

Step 6: Error handling
----------------------


```
...

var config = new MyConfig();
using (var interp = new Interpreter<MyConfig>())
{
    // Print all parsing errors instead of throwing exception
    // on a first one:
    interp.LogKind = LoggingKind.ConsoleOut;

    bool ok = interp.Parse(
        config, 
        @"
        Width  = 12;
        Heigth = 300;
        Title  = "Hello world";
        Pi     = 3.14;
        ");

    if (!ok)
    {
        Console.WriteLine(
            "========== Config reader: {0} errors {1} warnings ========",
            interp.ErrorCount,
            interp.WarningCount);
    }
}
...

```
