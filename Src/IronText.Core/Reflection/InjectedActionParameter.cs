using System;

namespace IronText.Reflection
{
    /// <summary>
    /// Injected action parameter will deviler value from the nearest source.
    /// </summary>
    /// <remarks>
    /// The most relevant concept to this class is *inherited attribute*. However inherited
    /// attribute is applied to symbols and parser states while 'injected parameter' is more
    /// friendly in a sense that it is close to the actual user requirement.
    /// For instance, if user wants to access current local scope of some language in some action,
    /// he just needs to add injected parameter of type 'Environment' which will be resolved 
    /// in runtime to the nearest defined scope.
    /// </remarks>
    [Serializable]
    public class InjectedActionParameter : IProductionComponent
    {
        /// <summary>
        /// Creates instance of the <see cref="InjectedActionParameter"/>
        /// </summary>
        public InjectedActionParameter(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.Name = name;
        }

        public object Identity  { get { return this; } }

        public string Name      { get; private set; }

        public int    InputSize { get { return 0; } }

        public void FillInput(Symbol[] input, int startIndex) { }

        public IProductionComponent[] ChildComponents { get { return new IProductionComponent[0]; } }
    }
}
