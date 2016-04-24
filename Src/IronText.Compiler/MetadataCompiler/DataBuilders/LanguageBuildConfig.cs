namespace IronText.MetadataCompiler
{
    class LanguageBuildConfig
    {
        public LanguageBuildConfig(bool isBootstrap)
        {
            this.IsBootstrap = isBootstrap;
        }

        public bool IsBootstrap { get; }
    }
}
