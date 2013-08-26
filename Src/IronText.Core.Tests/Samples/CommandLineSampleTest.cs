using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IronText.Framework;
using IronText.Lib;
using NUnit.Framework;

namespace IronText.Tests.Samples
{
    [TestFixture]
    public class CommandLineSampleTest
    {
        [Test]
        public void Test()
        {
            var context = new MyArchiver();
            context.Interpret(@"add NightlyBuild.zip --compress 5 --format zip --sfx doc\readme.txt src\prog.cpp");
            context.Interpret(@"test NightlyBuild.zip");
            context.Interpret(@"extract NightlyBuild.zip --output d:\MyWorkingDir");
        }

        [Language]
        [StaticContext(typeof(Builtins))]
        public class MyArchiver
        {
            private int           CompressLevel;
            private string        ArchiveFormat;
            private bool          IsSfx;
            private DirectoryInfo OutputDir;

            public void Interpret(string commandLine)
            {
                Language.Parse(this, commandLine);
            }

            [Parse("add")]
            public void Add(FileInfo archive, List<Option> options, FileInfo[] files) 
            {
                Debug.WriteLine("compressing{1} archive '{0}' ...", archive, IsSfx ? " self-extracting" : "" );
                foreach (var file in files)
                {
                    Debug.WriteLine(" adding file '{0}' ...", file);
                }

                Debug.WriteLine("done!");
            }

            [Parse("test")]
            public void Test(FileInfo archive) 
            {
                Debug.WriteLine("testing archive '{0}' ...", archive);
                Debug.WriteLine("done!");
            }

            [Parse("extract")]
            public void Extract(FileInfo archive, List<Option> options) 
            {
                Debug.WriteLine("extracting archive '{0}' to '{1}' ...", archive, OutputDir);
                Debug.WriteLine("done!");
            }

            [Parse("--compress")]
            [Parse("-c")]
            public Option Compress(int level) { this.CompressLevel = level; return null; }

            [Parse("--format")]
            [Parse("-f")]
            public Option Format(string format) { this.ArchiveFormat = format; return null; }
            
            [Parse("--sfx")]
            public Option Sfx() { this.IsSfx = true; return null; }

            [Parse("--output")]
            [Parse("-o")]
            public Option OutputDirectory(DirectoryInfo dir) { this.OutputDir = dir; return null; }

            [Parse]
            public FileInfo FileInfo(string word) { return new FileInfo(word); }

            [Parse]
            public DirectoryInfo DirectoryInfo(string word) { return new DirectoryInfo(word); }

            [Scan("digit+")]
            public int Integer(string text) { return int.Parse(text); }

            [Scan("quot ~quot* quot")]
            public string QuotedWord(char[] buffer, int start, int length) 
            {
                string text = new string(buffer, start + 1, length - 2);
                return text;
            }

            [Scan("~(blank | quot | '-') ~(blank | quot)*")]
            public string Word(string name) { return name; }

            [Scan("blank+")]
            public void Blank() { }
        }

        public interface Option {}
        public interface ArchiveFormat {}
    }
}
