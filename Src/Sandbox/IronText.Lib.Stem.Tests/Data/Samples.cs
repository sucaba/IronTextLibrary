using System.IO;

namespace IronText.Tests.Data
{
    /// <summary>
    /// Data samples
    /// </summary>
    public static class DataSamples
    {
        public const string ProjectDir = @"IronText.Tests";
        public const string DataDir = @"Data";
        public static readonly string SampleFileName = Path.Combine(DataDir, @"SampleInputTextFile.txt");
        public static readonly string CompileSample0FilePath = Path.Combine(DataDir, @"Sample0.il");
        public static readonly string CompileSample1FilePath = Path.Combine(DataDir, @"Sample1.il");
        public static readonly string CompileSample2FilePath = Path.Combine(DataDir, @"Sample2.il");
        public static readonly string CompileSample3FilePath = Path.Combine(DataDir, @"Sample3.il");
        public static readonly string CompileSample4FilePath = Path.Combine(DataDir, @"Sample4.il");
        public static readonly string CompileSwitchBugFilePath = Path.Combine(DataDir, @"SwitchBug.il");
    }
}
