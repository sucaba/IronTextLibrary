
namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new MyArchiver();
            context.Interpret(string.Join(" ", args));
        }
    }
}
