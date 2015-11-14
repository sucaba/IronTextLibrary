namespace IronText.Runtime
{
    public interface ILanguageLoader
    {
        ILanguageRuntime Load(ILanguageSource languageName);
    }
}
