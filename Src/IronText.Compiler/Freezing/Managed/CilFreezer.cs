using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Runtime;

namespace IronText.Freezing.Managed
{
    public class CilFreezer<TContext> : IDisposable
        where TContext : class
    {
        public Pipe<TContext> Compile(string input)
        {
            SppfNode root = BuildTree(input);

            var cachedMethod = new CachedMethod<Pipe<TContext>>("temp", (emit, args)=> Build(emit, args, root));
            return cachedMethod.Delegate;
        }

        private EmitSyntax Build(EmitSyntax emit, Ref<Args>[] args, SppfNode root)
        {
            int index = 0;
            return BuildNodeInvocation(ref index, emit, args, root) .Ret();
        }

        private EmitSyntax BuildNodeInvocation(ref int startArgIndex, EmitSyntax emit, Ref<Args>[] args, SppfNode node)
        {
            if (node.IsTerminal)
            {
                return emit.Ldarg(args[startArgIndex++]);
            }

            int count = node.Children.Length;
            for (int i = 0; i != count; ++i)
            {
                emit = BuildNodeInvocation(ref startArgIndex, emit, args, node.Children[i]);
            }

            return emit;
        }

        ~CilFreezer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private SppfNode BuildTree(string input)
        {
            using (var interp = new Interpreter<TContext>())
            {
                return interp.BuildTree(input);
            }
        }

        private void Dispose(bool disposing)
        {
        }
    }
}
