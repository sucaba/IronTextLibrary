
namespace IronText.Runtime
{
    public interface ILanguageLoader
    {
        ILanguage Load(LanguageName languageName);
    }
}
