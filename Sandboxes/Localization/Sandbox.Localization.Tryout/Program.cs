using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using Umbraco.Foundation.Localization;
using Umbraco.Foundation.Localization.Processing;
using Umbraco.Foundation.Localization.Maintenance;
using Umbraco.Foundation.Localization.Parsing;
using Umbraco.Foundation.Localization.Maintenance.Extraction;
using System.Text.RegularExpressions;

namespace Sandbox.Localization.Tryout
{
    class Program
    {

        static EvaluationContext CreateContext(string ns, LanguageInfo lang, Dictionary<string, object> values)
        {
            return new EvaluationContext
            {
                Namespace = ns,
                Language = lang,
                Parameters = (DicitionaryParameterSet) values,
                StringEncoder = (x)=>x
            };
        }


        static void Main(string[] args)
        {

            //TestSwitchCaseCondition("@Test", 1);


            TestSwitchCaseCondition("% 10 = 2 or (1 and < 1)", 1);
            TestSwitchCaseCondition("% 10 = 2 or (1 and < 1)", 12);
            TestSwitchCaseCondition("% 10 = 2 or 5, 6, 7, 8, 9", 7);
            TestSwitchCaseCondition("% 10 = 2 or 5, 6, 7, 8, 9", 15);


            TestSwitchCaseCondition(" +1 % 10 = 1", 10);

            TestSwitchCaseCondition(" + 1 % 10 = 1", 10);

            TestSwitchCaseCondition("%10=1", 1);
            TestSwitchCaseCondition("%10=1", 10);
            TestSwitchCaseCondition("%10=1", 25);
            TestSwitchCaseCondition("%10=1", 11);            

            


            var dialect = new DefaultDialect();
            var manager = new DummyManager();


            var test = "Hej {P1: Format spec} <F1:   {Moo:tahr}> {Abe:kat} #Fisso{1:  Foo 10 #Nested{Boo:Hawrh!} | 10} #Moo{{ab}} #Foo{|?Gok}";

            var p = new DefaultExpressionParser();            
            var e = p.Parse(test, null);
            e.Accept(new Printer());

            var eval = new PatternEvaluator(e);            
            Console.Out.WriteLine();
            Console.Out.WriteLine(e.ToString());            

            Console.Out.WriteLine("Custom expression part");
            var customTest = "Hej {Name}";
            e = p.Parse(customTest, null);
            e.Parts.Add(new TestPart());
            e.Accept(new PatternDecorator(manager, dialect));

            eval = new PatternEvaluator(e);
            

            e.Accept(new Printer());
            Console.Out.WriteLine();
            Console.Out.WriteLine(eval.Evaluate(
                CreateContext(null, manager.CurrentLanguage, new Dictionary<string, object> { { "Name", "John" } })));
            Console.Out.WriteLine();
            Console.Out.WriteLine(e.ToString());

            var ptest = @"The values are {Name} and <Bold: <Bold:{Test}> <Bold:{Test}>><Bold: {Number:N2} (roman: {Number:roman})>. #Number{1: Singularis: 1 ({#}) | Pluralis\: n ({#})}.
            
                Boolean Expression: #Number{2 or 5: Yes | No}

                ModuloTest: #Number{% 2 = 0: Yes | No}
            
                {Name} has {Name.Length:N2} characters

                Let's enumerate 
                Enum test 1: #EnumTest1{0:{#}|, {#}|-1:"" and {#}""}
                Enum test 2: #EnumTest2{0:{#}|, {#}|-1:"" and {#}""}";

            Console.Out.WriteLine();

            var evaluator = dialect.GetEvaluator(ptest, manager);
            Console.Out.WriteLine(evaluator.Evaluate(
                CreateContext(null, manager.CurrentLanguage, new Dictionary<string, object> {
                {"Name", "John Doe"},
                {"Number", 5},
                {"Bold", "<b>{#}</b>"},
                {"EnumTest1", new string[] {"A", "B", "C", "D"}},
                {"EnumTest2", new string[] {"A"}},
                {"EnumTest3", new string[] {"A", "B"}},
                {"Test", true}})));


            var texts = new List<LocalizedText>
            {
                new LocalizedText { Key="LookupCondition", Pattern = "% 10 <= 2", Language = "en-US", PatternDialect = "Text"},

                new LocalizedText { Key="Plural1", Pattern = "1", Language = "en-US", PatternDialect = "Text"},
                new LocalizedText { Key="Plural2", Pattern = "<20", Language = "en-US", PatternDialect = "Text"},                

                new LocalizedText { Key="ShortPlural", Pattern = "The number is #Plural(Number){Not plural|Plural|More plural}", Language = "en-US"},

                new LocalizedText { Key="PluralFun", Pattern = "The number is #Number{@LookupCondition: Plural|Not plural}", Language = "en-US"},

                new LocalizedText { Key="Arithmetic", Pattern = "Test #Number{% 10 = 1: Module | + 1 * 2 > 10: Combined}", Language = "en-US"},

                new LocalizedText { Key = "Text one", Pattern = "Test one", Language = "en-US" },                
                new LocalizedText { Key = "Text two", Pattern = "Hello {UserName}", Language = "en-US" },
                new LocalizedText { Key = "Text two", Pattern = "Hej {UserName}", Language = "da-DK" },
                new LocalizedText { Key = "Enum", Pattern = @"#0{0: {#Index:roman}: {#}|, {#Index:roman}\: {#}|-1: "" {1} {#Index:roman}\: {#}""}", Language="en-US"},

                new LocalizedText { Key = "Reffed", Pattern = "Test {0} String literal {1}", Language="en-US"},
                new LocalizedText { Key = "Ref test 1", Pattern = "Ref 1: {@Reffed} and Another namespace {@Another__Reffed}",  Language="en-US" },
                new LocalizedText { Key = "Ref test 2", Pattern = @"Ref 2: {@Reffed(UserName, ""Moo"")}",  Language="en-US" },
                //This one makes sense, and shows why pattern references are neat
                new LocalizedText { Key = "Ref test 3", Pattern = @"You have selected {@Enum(List, 'and')}",  Language="en-US" },

                new LocalizedText { Key = "Ref test 3", Namespace="Another", Pattern = "Test one {@Reffed} and {@__Reffed}", Language = "en-US" },

                new LocalizedText { Key = "Reffed", Namespace="Another", Pattern = "I'm another", Language = "en-US" },

                new LocalizedText { Key = "FormattedSwitch",  Pattern = "Two decimals #Count:N2{1: One - ({#}) | More - {#}}", Language = "en-US" },
            };

            var xml = new XmlTextSource();
            xml.Put(texts.Select(x => new LocalizedTextState { Text = x, Status = LocalizedTextStatus.Unchanged }), TextMergeOptions.KeepUnused);

            xml.Document.Save("Texts.xml");


            xml = new XmlTextSource();
            xml.Document = XDocument.Load("Texts.xml");


            manager.Texts.Sources.Add(new PrioritizedTextSource(xml));


            Console.Out.WriteLine(manager.Get("LookupCondition"));
            Console.Out.WriteLine(manager.Get("PluralFun", new { Number = 1 }));

            Console.Out.WriteLine("Shorthand:");
            Console.Out.WriteLine(manager.Get("ShortPlural", new { Number = 30 }));

            Console.Out.WriteLine(manager.Get("Text two", new { UserName = "Bob" }));

            Console.Out.WriteLine("Debug:");
            Console.Out.WriteLine(manager.Get("Text two", new { UserName = "Bob" }, debug: true));
            Console.Out.WriteLine();

            Console.Out.WriteLine(manager.Get("FormattedSwitch", new { Count = 7 }));

            Console.Out.WriteLine(manager.Get("Ref test 1", new { UserName = "Bob" }));
            Console.Out.WriteLine(manager.Get("Ref test 2", new { UserName = "Bob" }));

            Console.Out.WriteLine(manager.Get("Ref test 3", new { List = new[] { "Item 1", "Item 2", "Item 7", "Item 17" } }));
            Console.Out.WriteLine(manager.Get("Ref test 3", new { UserName = "Bob" }, ns: "Another"));

            string a = "Key"; /* @L10n @da-DK Foo @en-US Bar */
            string b = "Key 2"; // @L10n @da-DK Foo 2
            string.Format("Key 3" /* @L10n @da-DK Foo 3 */);
            

            

            string g = "Key"; /* @L10n @da-DK */


            string m = "Key" /* @L10n @en-US Goo */;

            //var extractor = new CStyleLanguageTextExtractor();
            //extractor.SourceFiles =
            //    new SourceFileList(@"C:\Users\niels.kuhnel\Stuff\Umbraco\Umbraco 5\I18n\i18n\Sandboxes\Localization\Sandbox.Localization.Tryout",
            //        new[] { "cs" }, new[] { "obj" }).GetFiles();

            //xml.Document = null;
            //xml.Put(extractor.Get().Select(x => new LocalizedTextState { Text = x }), TextMergeOptions.KeepUnused);
            //xml.Document.Save("Extracted.xml");


            Console.Out.WriteLine("Arithmetic");
            Console.Out.WriteLine(manager.Get("Arithmetic", new { Number = 11 }));

            Console.In.Read();
        }


        static bool TestSwitchCaseCondition(string spelling, double value)
        {
            var dialect = new DefaultDialect();
            var manager = new DummyManager();

            var sc = dialect.GetSwitchConditionEvaluator(new Expression
            {
                Parts =
                    new List<ExpressionPart> { new Text { Spelling = spelling } }
            }, manager);

            var success = sc.Evaluate(ParameterValue.Wrap(value), CreateContext(null, new LanguageInfo{Key="en-US"}, null));
            Console.Out.WriteLine(value + " matches " + spelling + ": " + success);

            return success;
        }

        class TestPart : CustomExpressionPart
        {
            public string Text = "I'm a test part";
            public string DecoratedText;

            public override void Decorate(PatternDialect dialect, TextManager manager)
            {
                DecoratedText = "Decorated: " + Text;
            }

            public override void Evaluate(EvaluationContext context, System.IO.TextWriter writer)
            {
                writer.Write(DecoratedText);
            }

            public override string ToString()
            {
                return "Custom: " + Text;
            }
        }

        class DummyManager : TextManager
        {
            public LanguageInfo CurrentLanguage { get; set; }
            
           

            public DummyManager()
            {
                CurrentLanguage = new LanguageInfo { Key = "en-US", Culture = new CultureInfo("en-US") };
            }

            public override LanguageInfo GetCurrentLanguage()
            {
                return CurrentLanguage;
            }
        }


        class Printer : IPatternVisitor<object>
        {
            int indent = 0;
            void Print(string text)
            {
                for (int i = 0; i < indent; i++) Console.Out.Write("    ");
                Console.Out.WriteLine(text);
            }

            public void Visit(Expression expression, object state)
            {
                foreach (var part in expression.Parts) part.Accept(this);
            }

            public void Visit(Text text, object state)
            {
                Print("Text = '" + text.Spelling + "'");
            }

            public void Visit(ParameterSpec spec, object state)
            {
                Print("PSpec = " + spec.ParameterName + "  :  " + spec.ParameterFormat);
            }

            public void Visit(Switch sw, object state)
            {
                Print("Switch = " + sw.ParameterName);

                ++indent;
                if (sw.NullExpression != null)
                {
                    Print("Null");
                    ++indent; sw.NullExpression.Accept(this); --indent;
                }
                foreach (var cs in sw.Cases)
                {
                    cs.Accept(this);
                }
                --indent;
            }

            public void Visit(SwitchCase sc, object state)
            {
                Print("SwitchCase");
                ++indent;
                if (sc.Condition != null)
                {
                    Print("Condition");
                    ++indent;
                    sc.Condition.Accept(this);
                    --indent;
                }

                Print("Expression");
                ++indent;
                sc.Expression.Accept(this);
                --indent;
                --indent;
            }

            public void Visit(FormatGroup group, object state)
            {
                Print("FormatGroup: " + group.ParameterName);
                ++indent;
                if (group.Expression != null)
                {
                    group.Expression.Accept(this);
                }
                --indent;
            }

            public void Visit(CustomExpressionPart part, object state)
            {
                Print(part.ToString());
            }

            public virtual object CreateInitialState()
            {
                return null;
            }
        }

    }
}
