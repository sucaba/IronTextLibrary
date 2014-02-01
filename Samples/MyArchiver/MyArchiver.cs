using System;
using System.Collections.Generic;
using System.IO;
using IronText.Framework;
using IronText.Lib;
using IronText.Runtime;

namespace Samples
{
    [Language]
    [StaticContext(typeof(Builtins))]
    public class MyArchiver
    {
        private int CompressLevel;
        private string ArchiveFormat;
        private bool IsSfx;
        private DirectoryInfo OutputDir;

        public void Interpret(string commandLine)
        {
            Language.Parse(this, commandLine);
        }

        [Produce("add")]
        public void Add(FileInfo archive, List<Option> options, FileInfo[] files)
        {
            Console.WriteLine("compressing{1} archive '{0}' ...", archive, IsSfx ? " self-extracting" : "");
            foreach (var file in files)
            {
                Console.WriteLine(" adding file '{0}' ...", file);
            }

            Console.WriteLine("done!");
        }

        [Produce("test")]
        public void Test(FileInfo archive)
        {
            Console.WriteLine("testing archive '{0}' ...", archive);
            Console.WriteLine("done!");
        }

        [Produce("extract")]
        public void Extract(FileInfo archive, List<Option> options)
        {
            Console.WriteLine("extracting archive '{0}' to '{1}' ...", archive, OutputDir);
            Console.WriteLine("done!");
        }

        [Produce("--compress")]
        [Produce("-c")]
        public Option Compress(int level) { this.CompressLevel = level; return null; }

        [Produce("--format")]
        [Produce("-f")]
        public Option Format(string format) { this.ArchiveFormat = format; return null; }

        [Produce("--sfx")]
        public Option Sfx() { this.IsSfx = true; return null; }

        [Produce("--output")]
        [Produce("-o")]
        public Option OutputDirectory(DirectoryInfo dir) { this.OutputDir = dir; return null; }

        [Produce]
        public FileInfo FileInfo(string word) { return new FileInfo(word); }

        [Produce]
        public DirectoryInfo DirectoryInfo(string word) { return new DirectoryInfo(word); }

        [Match("digit+")]
        public int Integer(string text) { return int.Parse(text); }

        [Match("quot ~quot* quot")]
        public string QuotedWord(char[] buffer, int start, int length)
        {
            string text = new string(buffer, start + 1, length - 2);
            return text;
        }

        [Match("~(blank | quot | '-') ~(blank | quot)*")]
        public string Word(string name) { return name; }

        [Match("blank+")]
        public void Blank() { }
    }

    public interface Option { }
}
