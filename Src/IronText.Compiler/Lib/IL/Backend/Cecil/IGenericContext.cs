using Mono.Cecil;

namespace IronText.Lib.IL.Backend.Cecil
{
	public interface IGenericContext
    {
		IGenericParameterProvider Type { get; }
		IGenericParameterProvider Method { get; }
	}
}
