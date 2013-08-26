
Ctx<T> can be defined not as a class but as delegate, which simplifies syntax:

public delegate T Ctx<T>();

Current code :

[Pattern]
public void Ref(Ctx<Scope> scope, Idn variable) 
{ 
	Value = (double)scope.Instance.GetLocal(variable.Text);
}

will become:

[Pattern]
public void Ref(Ctx<Scope> scope, Idn variable) 
{ 
	Value = (double)scope().GetLocal(variable.Text);
}

Or even simpler, instead of passing scope we can work with 
all delegates instead of introducing special Ctx<> token:

public delegate T RefLocal<T>(string name);

[Pattern]
public void Ref(RefLocal<double> refLocal, Idn variable) 
{ 
	Value = refLocal(variable.Text);
}