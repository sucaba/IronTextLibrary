using IronText.Framework;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class StartRulesTest
    {
        const string Prefix = "prefix";
        const string Suffix = "suffix";

        [Test]
        public void Test()
        {
            var lang = Language.Get(typeof(MyParser));

            foreach (var value in new[] {0, 1, 2, 3, 4})
            {
                using (var interp = new Interpreter<MyParser>())
                {
                    string input = value.ToString();
                    if (value == 2 || value == 4)
                    {
                        input = Prefix + input + Suffix;
                    }

                    interp.Parse(input);
                    Assert.AreEqual(value, interp.Context.StartChoice);
                }
            }
        }

        [Language]
        public class MyParser
        {
            public int StartChoice;

            // Property setter
            [ParseResult]
            public Choice0 Start0 
            { 
                set { StartChoice = (int)value; } 
            }

            // Regular method
            [Parse]
            public void Start1(Choice1 choice) { StartChoice = (int)choice; }

            // With keyword mask
            [Parse(Prefix, null, Suffix)]
            public void Start2(Choice2 choice) { StartChoice = (int)choice; }

            // Property setter
            public Choice3 Start3 
            { 
                [Parse] 
                set { StartChoice = (int)value; } 
            }

            // Property setter with keyword mask
            public Choice4 Start4
            { 
                [Parse(Prefix, null, Suffix)]
                set { StartChoice = (int)value; } 
            }

            [Scan("'0'")]
            public Choice0 Term0() { return Choice0.Value; }

            [Scan("'1'")]
            public Choice1 Term1() { return Choice1.Value; }

            [Scan("'2'")]
            public Choice2 Term2() { return Choice2.Value; }

            [Scan("'3'")]
            public Choice3 Term3() { return Choice3.Value; }

            [Scan("'4'")]
            public Choice4 Term4() { return Choice4.Value; }
        }

        public enum Choice0 { Value = 0 };
        public enum Choice1 { Value = 1 };
        public enum Choice2 { Value = 2 };
        public enum Choice3 { Value = 3 };
        public enum Choice4 { Value = 4 };
    }
}
