using IronText.Framework;
using IronText.Lib.IL;
using IronText.Reflection.Managed;
using System;

namespace IronText.MetadataCompiler
{
    class InlinedProductionCode : IActionCode
    {
        public IActionCode Emit(Pipe<EmitSyntax> emit)
        {
            throw new System.NotImplementedException();
        }

        public IActionCode LdSemantic(string name)
        {
            throw new System.NotImplementedException();
        }

        public IActionCode LdActionArgument(int index)
        {
            throw new System.NotImplementedException();
        }

        public IActionCode LdActionArgument(int index, Type argType)
        {
            throw new System.NotImplementedException();
        }

        public IActionCode LdMergerOldValue()
        {
            throw new System.NotImplementedException();
        }

        public IActionCode LdMergerNewValue()
        {
            throw new System.NotImplementedException();
        }

        public IActionCode LdMatcherTokenString()
        {
            throw new System.NotImplementedException();
        }

        public IActionCode ReturnFromAction()
        {
            throw new System.NotImplementedException();
        }

        public IActionCode SkipAction()
        {
            throw new System.NotImplementedException();
        }
    }
}
