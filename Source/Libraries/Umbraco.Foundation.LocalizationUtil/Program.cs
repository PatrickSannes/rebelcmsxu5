using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Foundation.Localization.Maintenance.Extraction;
using CommandLine;
using Umbraco.Foundation.Localization.Maintenance;
using System.Xml.Linq;

namespace Umbraco.Foundation.LocalizationUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            var extractOptions = new ExtractOptions();
            var compareOptions = new CompareOptions();
            var parser = new CommandLineParser();

            if( args.Length > 0 )
            {
                if (args[0] == "extract")
                {
                    if (parser.ParseArguments(args, extractOptions))
                    {
                        extractOptions.RootDirectory = extractOptions.RootDirectory ??
                            Environment.CurrentDirectory;

                        Console.Out.WriteLine("");
                        Console.Out.WriteLine("Extracting entries from:");
                        Console.Out.WriteLine("\"" + extractOptions.RootDirectory + "\"");
                        Console.Out.WriteLine("Extensions: " + string.Join(", ", extractOptions.Extensions));
                        Console.Out.WriteLine("Exclude patterns: " + string.Join(", ", extractOptions.ExcludePatterns));
                        Console.Out.WriteLine();
                        Console.Out.WriteLine("Saving texts to: " + extractOptions.TargetFile);

                        var extractor = new CStyleLanguageTextExtractor();
                        extractor.MarkerLanguage = extractOptions.DefaultLanguage;

                        extractor.SourceFiles = new SourceFileList(
                            extractOptions.RootDirectory,
                            extractOptions.Extensions,
                            extractOptions.ExcludePatterns).GetFiles();

                        var target = new XmlTextSource();
                        target.Put(extractor.Get().Select(x =>
                            new LocalizedTextState { Text = x, Status = LocalizedTextStatus.New }), TextMergeOptions.KeepUnused);

                        target.Document.Save(extractOptions.TargetFile);

                        return;
                    }
                }
                else if (args[0] == "compare")
                {
                    if (parser.ParseArguments(args, compareOptions))
                    {

                        Console.WriteLine();

                        var texts = new TextSourceAggregator();
                        using (texts.BeginUpdate())
                        {
                            foreach (var file in compareOptions.Files)
                            {
                                texts.Sources.Add(new PrioritizedTextSource(new XmlTextSource(XDocument.Load(file)), 1));
                            }
                        }
                        

                        var comparer = new LanguageComparer(texts);
                        var result = comparer.Compare(
                            compareOptions.SourceLanguage, 
                            compareOptions.TargetLanguage);
                        
                        Console.WriteLine(result.Success ?
                            "OK - The text(s) defines the same keys for the languages" :
                            "The two languages differ");
                        Console.Out.WriteLine();                        
                        foreach (var text in result.MissingTexts)
                        {
                            Console.Out.WriteLine("Missing\t" + text.Key);                            
                        }
                        foreach (var text in result.UnmatchedTexts)
                        {
                            Console.Out.WriteLine("Unmatched\t" + text.Key);                            
                        }                        
                        return;
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("Usage: ");
            Console.WriteLine();
            Console.WriteLine("Localization.exe extract");
            WriteHelp(extractOptions);
            Console.WriteLine();
            Console.WriteLine("Localization.exe compare");
            WriteHelp(compareOptions);
            Console.WriteLine();
        }

        static void WriteHelp(object options)
        {
            var flags = new Dictionary<string, string>();
            foreach (var field in options.GetType().GetFields())
            {                
                foreach (OptionAttribute option in field.GetCustomAttributes(typeof(OptionAttribute), true))
                {
                    flags.Add("-" + option.ShortName + " --" + option.LongName,
                        (option.Required ? "" : "(optional) ") + option.HelpText);                    
                }                
            }
            var pad = flags.Max(x => x.Key.Length);
            foreach (var flag in flags)
            {
                Console.Out.WriteLine("{0,-" + pad + "} {1}", flag.Key, flag.Value);
            }
        }

        class CompareOptions
        {
            [OptionArray("f", "files", Required=true, HelpText="Text files")]
            public string[] Files;

            [Option("s", "source", Required=true, HelpText = "Source language")]
            public string SourceLanguage;

            [Option("t", "target", Required = true, HelpText = "Target language")]
            public string TargetLanguage;            
        }

        class ExtractOptions
        {
            [Option("d", "directory", HelpText = "The root directory. Default: Current directory")]
            public string RootDirectory;

            [Option("t", "target", HelpText = "Target file. Default: LocalizationEntries.xml")]
            public string TargetFile = "LocalizationEntries.xml";

            [Option("l", "language", HelpText = "The default language for the parser")]
            public string DefaultLanguage = "";

            [OptionArray("e", "extensions", HelpText="The file extensions to consider. Default: cs")]
            public string[] Extensions = new [] { "cs", "cshtml", "aspx", "js"};

            [OptionArray("x", "exclude", HelpText=@"Exclude patterns by prefix. Default: obj")]
            public string[] ExcludePatterns = new [] {"obj"};            
                
        }
    }
}
