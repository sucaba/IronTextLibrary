public class Start  { }
public class Stop   { }
public class Pause  { }
public class Resume { }

[Language]
public interface StopWatch
{
    [Outcome]
    decimal Time { get; set; }

    [Produce]
    Stopped Create(); 

    [Match("space+")]
    void Skip();
}

[Demand]
public interface Final
{
    [Produce]
    decimal GetCurrent();
}

[Demand]
public interface Stopped : Final
{
    [Produce]
    Counting Start(Start msg);
}

[Demand]
public interface Counting : Final
{
    [Produce]
    Paused Pause(Pause msg);

    [Produce]
    Stopped Stop(Stop msg);
}

[Demand]
public interface Paused : Final
{
    [Produce]
    Counting Resume(Resume msg);
}

StopWatch context = ...;
var interp = new Interpreter<StopWatch>(context);
interp.Parse(new [] { new Start(), new Pause(), new Resume(), new Stop() });
Console.WriteLine($"Time = {context.Time}");

// or equivalently without parsing:

context.Time = context
    .Create()
    .Start(new Start())
    .Pause(new Pause())
    .Resume(new Resume())
    .Stop(new Stop())
    .GetCurrent();
