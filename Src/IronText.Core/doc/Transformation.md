Using token types without invoking rules
----------------------------------------

After terminal types are defined it is not necessary to
create all instances of these types and invoke all 
rules to have benefit from parsing.

class PlusExprImpl : PlusExpr
{
	public override void A(Expr expr0, PlusKwd plus, Expr expr1)
	{
	}
}

var transformer = new Transformer(typeof(CalcExpr));
transformer.On<PlusExpr>(new PlusExprImpl());

Text to Text transformation
---------------------------