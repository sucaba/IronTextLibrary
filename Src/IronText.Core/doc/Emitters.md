Emitters. Languages with side effects
-------------------------------------

BNF grammar decomposes language structure in a functional way such
that each non-term token consists with lowlevel tokens.
While it is good way to represent language, sometimes it is not 
good enough way to represent "language builder" interface.
Some language builders have side-effects and do not support 
functional way of composing grammar expressions (token instances).
Example of such language-builder is System.Reflection.Emit.ILGenerator
class which does not accumulate list of instructions (at least publicly).
Instead it has Emit(...) methods which cause side effects to this class
by writing instructions to some internal storage.

Wasp prposes following mapping solution to this problem:

[LanguageModule(typeof(ILProgram))]
public interface IIL
{
	public ILProgram Prog(ListStub<IIL> instructions)

	// Emit rules

	[Operative("stloc")]
	IIL StLoc(Idn name);

	[Operative("ldloc")]
	IIL LdLoc(Idn name);
	...
}

Which means that language module type is used as a token type.
In this case it is similar to the Haskell's 'World' type.

Nice feature of this approach is that it is heandy to write sequences of emit-calls (idea borrowed from the BLToolkit):

IIL emit = ...;

emit.StLoc(...)
    .LdLoc(...)
	.StLoc(...);
