
namespace IronText.Runtime
{
    public sealed class Interpreter<TDefinition> : Interpreter
        where TDefinition : class
    {
        public Interpreter(TDefinition context)
            : base(context, Language.Get(typeof(TDefinition)))
        {
        }

        public Interpreter()
            : base(Language.Get(typeof(TDefinition)))
        {
        }

        public new TDefinition Context
        {
            get { return (TDefinition)base.Context;  }
            set { base.Context = value;  }
        }
    }
}
