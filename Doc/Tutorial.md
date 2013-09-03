Iron Text Library in 10 minutes
===============================

Tutorial demonstrates step-by-step creation of the custom config file parser.


Step 1: Boilerplate code
------------------------

Following parser accepts "foo" *literal* text and fails with any other input.
Note: *Literal* means fixed text like a keyword or delimiter.

```csharp
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
        // Similar to [Scan("'foo'")]
        [Literal("foo")]
        public string FooKeyword(string text) { return text; }
    }
}
```

Step 2: Parsing integer values
------------------------------

Add integer value scan-rule istead of scanning dummy literal.


```csharp
...
var context = new MyLang();
Language.Parse(context, "12345");
...

[Language]
public class MyLang
{
    [ParseResult]
    public int Result { get; set; } 

    // Scans one-or-more decimal digit
    [Scan("digit+")]
    public int Integer(string text) { return int.Parse(text); }
}
```

Step 3: Integer List 
--------------------

Let's do more advanced parsing of the non-empty list of space-separted integers.
Not really a config file parser but gives basic intuition behind the 
parsing logic.

Rules for parsing non-empty list are defined using generic parse methods.


```csharp

...
var context = new MyLang();
Language.Parse(context, "12 3 456");
...

[Language]
public class MyLang
{
    [ParseResult]
    public List<int> Result { get; set; } 

    // BNF: 
    //  list ::= INT
    [Parse]
    public List<T> SingleItem<T>(T x) { return new List<T> { x }; }

    // BNF:  
    //  list ::= list INT
    [Parse]
    public List<T> List<T>(List<T> items, T newItem) { items.Add(newItem); return items; }

    [Scan("digit+")]
    public int Scan(string text) { return int.Parse(text); }

    // Skip whitespaces and tabs. 
    // 'void' result in method signature defines *no-result* scan-rule 
    // which is still invoked but produces no token to be consumed by 
    // the parser.
    [Scan("blank+")]
    public void Blank() { }
}
```

Step 4: Named integer values
----------------------------

ParseAttribute can specify literal-mask which is
a list of string values.  They can be either
- nulls (method argument placeholders) 

or 
- constant strings (literals).

Trailing null items in literal-mask are optional.

Note: 
Empty string-literals are not allowed. 
They will cause build-time errors.


```csharp
...
Language.Parse(context, "Width=12 Heigth=300 TabSize=4");
...

[Language]
public class MyLang
{
    [ParseResult]
    public Dictionary<string,int> Result { get; set; } 

    // BNF: 
    //  dict ::= /*empty*/
    [Parse]
    public Dictionary<string,int> Empty() { return new Dictionary<string,int>(); }

    // BNF: 
    //  dict ::= dict string '=' INT
    [Parse(null, null, "=", null)]
    public Dictionary<string,int> Pair(
            Dictionary<string,int> items,
            string name,
            int value) 
    {
        items.Add(name, value)
        return items;
    }

    // Scan config-option identifier
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

Let's add string and double types support to MyConfig.

```csharp
...
var config = new MyConfig();
Language.Parse(config, @"Width=12 Heigth=300 Title=""Hello world"" Pi=3.14");
...

[Language]
public class MyConfig
{
    [ParseResult]
    public Dictionary<string,object> Result { get; set; } 

    [Parse]
    public Dictionary<string,object> Empty() { return new Dictionary<string,object>(); }

    [Parse(null, null, "=", null)]
    public Dictionary<string,object> Pair(
            Dictionary<string,object> items,
            string name,
            Variant variant) 
    {
        items.Add(name, variant.Value)
        return items;
    }

    [Scan("('_' | alpha) alnum*")]
    public string Name(string text) { return text; }

    [Scan("quot ~quot* quot")]
    public Variant QuotedString(string text)
    { 
        return new Variant(text.Substring(1, text.Length - 2));
    }

    [Scan("digit+ '.' digit* | '.' digit+")]
    public Variant Double(string text) { return new Variant(double.Parse(text)); }

    [Scan("digit+")]
    public Variant Integer(string text) { return new Variant(int.Parse(text)); }

    [Scan("blank+")]
    public void Blank() { }
}

// Quoted string token
public class QStr
{
    public SQStr(string text) { this.Text = text; }

    public string Text { get; private set; }
}

// Wraps any config value
public class Variant
{
    public Variant(object value) { this.Value = value; }

    public Value { get; private set; }
}

```

Step 6: Reuse vocabulary of lexical elements
--------------------------------------------

Add reusable lexical elements for a C-like language from the CtemScanner vocabulary:
- quoted strings
- numbers
- single-line comments
- multi-line comments
- blanks
- new lines


```csharp
...
var config = new MyConfig();
Language.Parse(config, 
    @"
    // Window parameters:
    Width  = 12
    Heigth = 300
    Title  = ""Hello world""

    /* Some additional constants */
    Pi     = 3.14
    ");
...

using IronText.Framework;
using IronText.Lib.Ctem;

[Language]
public class MyConfig
{
    // Initialize instance of CtemScanner sub-context
    public MyConfig() { Scanner = new CtemScanner(); }

    [ParseResult]
    public Dictionary<string,object> Result { get; set; } 

    // Use relevant scan and parse rules from this vocabulary during
    // the build and use Scanner-property value as a runtime-context 
    // for these rules.
    [SubContext]
    public CtemScanner Scanner { get; private set; }

    [Parse]
    public Dictionary<string,object> Empty() { return new Dictionary<string,object>(); }

    [Parse(null, null, "=", null)]
    public Dictionary<string,object> AddPair<T>(
            Dictionary<string,object> items,
            string name,
            Variant variant) 
    {
        items.Add(name, variant.Value)
        return items;
    }

    [Parse]
    public Variant QuotedStringValue(QStr str) { return new Variant(str.Text); }

    [Parse]
    public Variant NumberValue(Num num)
    {
        return new Variant(
            num.Contains('.') 
             ? double.Parse(num.Text) 
             : int.Parse(num.Text));
    }
}

// Wraps any config value
public class Variant
{
    public Variant(object value) { this.Value = value; }

    public Value { get; private set; }
}

```

Step 7: Error handling
----------------------

Use Interpreter class to gain more control over the parsing process.


```csharp
...

const string configPath = @".\local.cfg";

var config = new MyConfig();
using (var interp = new Interpreter<MyConfig>(config))
// Take real config file for parsing
using (var source = new StreamReader(configPath))
{
    // Print all parsing errors instead of throwing exception
    // on a first one:
    interp.LogKind = LoggingKind.ConsoleOut;

    bool ok = interp.Parse(source, configPath); // configPath is used only for error reporting

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
