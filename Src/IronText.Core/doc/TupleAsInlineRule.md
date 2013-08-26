Tuple as Inline Rule
--------------------


.Net Tuple<...> class can be used to define one-rule non-terms without explicitly defining corresponding class.

Example:

[Operative("make-dict")]
public void MakeDict(List<Tuple<Sym, Num>> pairs) { ... }

Will match following inputs:
	(make-dict Jhon 33 Mary 25 John 50)
	(make-dict)

This input will cause error because there is odd argument count in operative:
	(make-dict Jhon 33 Mary 25 John)
