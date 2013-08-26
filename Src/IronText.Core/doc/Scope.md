Managing Scope
--------------

	class CalcExpr
	{
        [SContext]
        public Scope Scope;

        [SName("let")]
        public void ClassicalLet(
            PushScopeFrame _1, 
            Opn opn, Sym var, CalcExpr expr, Cls cls,
            PushVar       _2, 
            List<CalcExpr> body, 
            PopScopeFrame _3)
        {
            Value = NoValue;
        }

        [SName("let")]
        public void SingleLineLet(Sym var, CalcExpr expr, PushVar _1)
        {
            Value = NoValue;
        }

        [SName("begin")]
        public void Begin(PushScopeFrame _1, List<CalcExpr> body, PopScopeFrame _2)
        {
            Value = body.Last().Value;
        }
	}

    public class Scope
    {
        public void PushVar(Sym var, CalcExpr value) { throw new NotImplementedException(); }

        public void PopVar() { throw new NotImplementedException();  }

        public void PushFrame() { throw new NotImplementedException(); }

        public void PopFrame() { throw new NotImplementedException();  }

        public CalcExpr GetVar(Sym value) { throw new NotImplementedException();  }
    }

    public struct PushVar
    {
        public PushVar(Scope scope, Sym var, CalcExpr expr) { scope.PushVar(var, expr); }
    }

    public struct PopVar
    {
        public PopVar(Scope scope) { scope.PopVar(); }
    }

    public struct PushScopeFrame
    {
        public PushScopeFrame(Scope scope) { scope.PushFrame(); }
    }

    public struct PopScopeFrame
    {
        public PopScopeFrame(Scope scope) { scope.PopFrame(); }
    }