namespace IronText.Framework
{
    public delegate T Pipe<T>(T input);

    public delegate R Pipe<T,R>(T input);
}
