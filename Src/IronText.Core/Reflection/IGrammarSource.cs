
namespace IronText.Reflection
{
    public interface IGrammarSource
    {
        string LanguageName      { get; }

        string FullLanguageName  { get; }

        string Origin            { get; }
    }
}
