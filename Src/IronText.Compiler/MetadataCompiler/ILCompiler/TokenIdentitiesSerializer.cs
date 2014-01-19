using System;
using System.Collections.Generic;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.Ctem;
using IronText.Lib.IL;

namespace IronText.MetadataCompiler
{
    class TokenIdentitiesSerializer
    {
        private readonly ITokenRefResolver tokenRefResolver;

        public TokenIdentitiesSerializer(ITokenRefResolver tokenRefResolver)
        {
            this.tokenRefResolver = tokenRefResolver;
        }

        public EmitSyntax Build(EmitSyntax emit)
        {
            var result = emit.Locals.Generate().GetRef();

            emit
                .Local(result.Def, emit.Types.Import(LanguageBase.Fields.tokenKeyToId.FieldType))
                .Newobj(() => new Dictionary<object,int>())
                .Stloc(result);

            foreach (var pair in EnumerateTokenKeyToId())
            {
                emit.Ldloc(result);

                if (pair.Item1 is string)
                {
                    emit.Ldstr(new QStr((string)pair.Item1));
                }
                else if (pair.Item1 is Type)
                {
                    emit
                        .Ldtoken(emit.Types.Import((Type)pair.Item1))
                        .Call((RuntimeTypeHandle h) => Type.GetTypeFromHandle(h));
                }
                else
                {
                    throw new InvalidOperationException("Internal error: Unsupported token key type");
                }

                emit
                    .Ldc_I4(pair.Item2)
                    .Call((Dictionary<object, int> self, object _key, int _id) => self.Add(_key, _id))
                    ;
            }

            return emit.Ldloc(result);
        }

        private IEnumerable<Tuple<object,int>> EnumerateTokenKeyToId()
        {
            foreach (var def in tokenRefResolver.Definitions)
            {
                foreach (var literal in def.Literals)
                {
                    yield return Tuple.Create((object)literal, def.Id);
                }

                if (def.TokenType != null)
                {
                    yield return Tuple.Create((object)def.TokenType, def.Id);
                }
            }
        }

        private void LdTid(EmitSyntax emit, CilSymbolRef tid)
        {
            if (tid.TokenType != null)
            {
                emit
                    .Ldtoken(emit.Types.Import(tid.TokenType))
                    .Call((RuntimeTypeHandle h) => Type.GetTypeFromHandle(h))
                    ;
            }
            else
            {
                emit.Ldnull();
            }

            if (tid.LiteralText == null)
            {
                emit.Ldnull();
            }
            else
            {
                emit.Ldstr(new QStr(tid.LiteralText));
            }

            emit
                .Newobj(() => new CilSymbolRef(default(Type), default(string)));
        }
    }
}
