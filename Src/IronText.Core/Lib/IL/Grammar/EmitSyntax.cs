using System;
using System.Linq.Expressions;
using IronText.Framework;
using IronText.Lib.Ctem;
using IronText.Lib.Shared;
using IronText.Misc;
using SR = System.Reflection;

namespace IronText.Lib.IL
{
    [Demand]
    [StaticContext(typeof(Builtins))]
    [StaticContext(typeof(Numbers))]
    public interface EmitSyntax
    {
        [SubContext]
        OnDemandNs<Labels> Labels { get; }

        [SubContext]
        DefFirstNs<Locals> Locals { get; }

        [SubContext]
        DefFirstNs<Args> Args { get; }

        [SubContext]
        IResolutionScopeNs ResolutionScopeNs { get; }

        [SubContext]
        ITypeNs Types { get; }

        [SubContext]
        IMethodNs Methods { get; }

        [Produce("}")]
        ClassSyntax EndBody();

        [Produce(null, ":")]
        EmitSyntax Label(Def<Labels> label);

        [Produce(".override", "method")]
        EmitSyntax Override(Ref<Methods> method);

        [Produce(".locals", null, null)]
        EmitSyntax Local(Def<Locals> name, Ref<Types> type);

        [Produce("br.s", null)]
        EmitSyntax Br_S(Ref<Labels> label);

        [Produce("break")]
        EmitSyntax Break();

        [Produce("brtrue.s", null)]
        EmitSyntax Brtrue_S(Ref<Labels> label);

        [Produce("brtrue", null)]
        EmitSyntax Brtrue(Ref<Labels> label);

        [Produce("brfalse.s", null)]
        EmitSyntax Brfalse_S(Ref<Labels> label);

        [Produce("brfalse", null)]
        EmitSyntax Brfalse(Ref<Labels> label);

        [Produce("br", null)]
        EmitSyntax Br(Ref<Labels> label);

        [Produce("bne_un", null)]
        EmitSyntax Bne_Un(Ref<Labels> label);

        [Produce("beq", null)]
        EmitSyntax Beq(Ref<Labels> label);

        [Produce("bgt", null)]
        EmitSyntax Bgt(Ref<Labels> label);

        [Produce("blt", null)]
        EmitSyntax Blt(Ref<Labels> label);

        [Produce("bge", null)]
        EmitSyntax Bge(Ref<Labels> label);

        [Produce("ble", null)]
        EmitSyntax Ble(Ref<Labels> label);

        [Produce("jmp", null)]
        EmitSyntax Jmp(Ref<Methods> method);

        [Produce("switch", "(", null, ")")]
        EmitSyntax Switch(params Ref<Labels>[] labels);

        [Produce("call", null)]
        EmitSyntax Call(Ref<Methods> method);

        [Produce("callvirt", null)]
        EmitSyntax Callvirt(Ref<Methods> method);

        [Produce(".entrypoint")]
        EmitSyntax EntryPoint();

        [Produce("ldarg", null)]
        EmitSyntax Ldarg(Ref<Args> arg);

        [Produce("ldarga", null)]
        EmitSyntax Ldarga(Ref<Args> arg);

        [Produce("ldarg", null)]
        EmitSyntax Ldarg(int argIndex);
        
        [Produce("ldarga", null)]
        EmitSyntax Ldarga(int argIndex);

        [Produce("starg", null)]
        EmitSyntax Starg(int argIndex);

        [Produce("ldc.i4", null)]
        EmitSyntax Ldc_I4(int constant);

        [Produce("ldc.i4.0")]
        EmitSyntax Ldc_I4_0();

        [Produce("ldc.i4.1")]
        EmitSyntax Ldc_I4_1();

        [Produce("ldind.i4")]
        EmitSyntax Ldind_I4();
        
        [Produce("ldloc", null)]
        EmitSyntax Ldloc(Ref<Locals> id);

        [Produce("ldloca", null)]
        EmitSyntax Ldloca(Ref<Locals> id);

        EmitSyntax Ldfld(SR.FieldInfo fieldInfo);

        EmitSyntax Ldsfld(SR.FieldInfo fieldInfo);

        EmitSyntax Stfld(SR.FieldInfo fieldInfo);

        EmitSyntax Stsfld(SR.FieldInfo fieldInfo);

        [Produce("ldloc.0")]
        EmitSyntax Ldloc0();

        [Produce("ldelem", null)]
        EmitSyntax Ldelem(Ref<Types> type);

        [Produce("ldelem.ref")]
        EmitSyntax Ldelem_Ref();

        [Produce("ldelem.i4")]
        EmitSyntax Ldelem_I4();

        [Produce("ldelem.u2")]
        EmitSyntax Ldelem_U2();

        [Produce("ldelema", null)]
        EmitSyntax Ldelema(Ref<Types> type);

        [Produce("ldlen")]
        EmitSyntax Ldlen();

        [Produce("ldnull")]
        EmitSyntax Ldnull();

        [Produce("ldstr", null)]
        EmitSyntax Ldstr(QStr str);

        [Produce("throw")]
        EmitSyntax Throw();

        [Produce("mul")]
        EmitSyntax Mul();

        [Produce("newarr", null)]
        EmitSyntax Newarr(TypeSpec elementType);

        [Produce("newobj", null)]
        EmitSyntax Newobj(Ref<Methods> constructor);

        [Produce("initobj", null)]
        EmitSyntax Initobj(Ref<Types> valueType);

        [Produce("nop")]
        EmitSyntax Nop();

        [Produce("dup")]
        EmitSyntax Dup();

        [Produce("pop")]
        EmitSyntax Pop();

        [Produce("ret")]
        EmitSyntax Ret();

        [Produce("stelem.ref")]
        EmitSyntax Stelem_Ref();

        [Produce("stelem.i4")]
        EmitSyntax Stelem_I4();

        [Produce("stelem", null)]
        EmitSyntax Stelem(Ref<Types> type);

        [Produce("stind.i4")]
        EmitSyntax Stind_I4();

        [Produce("stind.ref")]
        EmitSyntax Stind_Ref();

        [Produce("stloc", null)]
        EmitSyntax Stloc(Ref<Locals> id);

        [Produce("stloc.0")]
        EmitSyntax Stloc0();

        [Produce("add")]
        EmitSyntax Add();

        [Produce("sub")]
        EmitSyntax Sub();

        [Produce("box", null)]
        EmitSyntax Box(Ref<Types> type);

        [Produce("unbox.any", null)]
        EmitSyntax Unbox_Any(Ref<Types> type);

        EmitSyntax Ldtoken(SR.FieldInfo token);

        [Produce("ldtoken", null)]
        EmitSyntax Ldtoken(Ref<Types> token);

        [Produce("ldftn", null)]
        EmitSyntax Ldftn(Ref<Methods> method);
    }

    public static class Emit
    {
        public static CilDocumentSyntax Do(this CilDocumentSyntax @this, Pipe<CilDocumentSyntax> action)
        {
            return action(@this);
        }

        public static ClassSyntax Do(this ClassSyntax @this, Pipe<ClassSyntax> action)
        {
            return action(@this);
        }

        public static WantArgsBase Do(this WantArgsBase @this, Pipe<WantArgsBase> action)
        {
            return action(@this);
        }

        public static WantArgsSigBase Do(this WantArgsSigBase @this, Pipe<WantArgsSigBase> action)
        {
            return action(@this);
        }

        public static EmitSyntax Do(this EmitSyntax emit, Pipe<EmitSyntax> builder)
        {
            builder(emit);
            return emit;
        }

        public static EmitSyntax Ldfld<T,R>(this EmitSyntax emit, Expression<Func<T,R>> expr)
        {
            emit.Ldfld(ExpressionUtils.GetField(expr));
            return emit;
        }

        public static EmitSyntax Stfld<T,R>(this EmitSyntax emit, Expression<Func<T,R>> expr)
        {
            emit.Stfld(ExpressionUtils.GetField(expr));
            return emit;
        }

        public static EmitSyntax Ldprop<T,R>(this EmitSyntax emit, Expression<Func<T,R>> expr)
        {
            var property = ExpressionUtils.GetProperty(expr);
            return emit.Call(property.GetGetMethod());
        }

        public static EmitSyntax Stprop<T,R>(this EmitSyntax emit, Expression<Func<T,R>> expr)
        {
            var property = ExpressionUtils.GetProperty(expr);
            return emit.Call(property.GetSetMethod());
        }

        public static EmitSyntax Call(this EmitSyntax emit, Expression<Action> callExpr)
        {
            return emit .CallFromLambda(callExpr);
        }

        public static EmitSyntax Call<T>(this EmitSyntax emit, Expression<Action<T>> callExpr)
        {
            return emit .CallFromLambda(callExpr);
        }

        public static EmitSyntax Call<T1,T2>(this EmitSyntax emit, Expression<Action<T1,T2>> callExpr)
        {
            return emit .CallFromLambda(callExpr);
        }

        public static EmitSyntax Call<T1,T2,T3>(this EmitSyntax emit, Expression<Action<T1,T2,T3>> callExpr)
        {
            return emit .CallFromLambda(callExpr);
        }

        public static EmitSyntax Call<R>(this EmitSyntax emit, Expression<Func<R>> callExpr)
        {
            return emit .CallFromLambda(callExpr);
        }

        public static EmitSyntax Call<T1,R>(this EmitSyntax emit, Expression<Func<T1,R>> callExpr)
        {
            return emit .CallFromLambda(callExpr);
        }

        public static EmitSyntax Call<T1,T2,R>(this EmitSyntax emit, Expression<Func<T1,T2,R>> callExpr)
        {
            return emit .CallFromLambda(callExpr);
        }

        public static EmitSyntax Call<T1,T2,T3,R>(this EmitSyntax emit, Expression<Func<T1,T2,T3,R>> callExpr)
        {
            return emit .CallFromLambda(callExpr);
        }

        /// <summary>
        /// Create new object using default constructor of the specified type
        /// </summary>
        /// <param name="emit"></param>
        /// <param name="typeRef"></param>
        /// <returns></returns>
        public static EmitSyntax Newobj(this EmitSyntax emit, Ref<Types> typeRef)
        {
            return emit.Newobj(
                     emit.Methods.Method(
                        _ => _.StartSignature
                            .Instance
                            .Returning(emit.Types.Void)
                            .DecaringType(typeRef)
                            .Named(".ctor")
                            .BeginArgs()
                            .EndArgs()));
        }

        public static EmitSyntax Newobj(this EmitSyntax emit, Expression<Action> callExpr)
        {
            return emit .NewobjFromLambda(callExpr);
        }

        public static EmitSyntax Newobj<T>(this EmitSyntax emit, Expression<Action<T>> callExpr)
        {
            return emit .NewobjFromLambda(callExpr);
        }

        public static EmitSyntax Newobj<T1,T2>(this EmitSyntax emit, Expression<Action<T1,T2>> callExpr)
        {
            return emit .NewobjFromLambda(callExpr);
        }

        public static EmitSyntax Newobj<T1,T2,T3>(this EmitSyntax emit, Expression<Action<T1,T2,T3>> callExpr)
        {
            return emit .NewobjFromLambda(callExpr);
        }

        public static EmitSyntax CallFromLambda(this EmitSyntax emit, LambdaExpression callExpr)
        {
            var method = ExpressionUtils.GetMethodFromLambda(callExpr);
            return emit.Call(method);
        }

        public static EmitSyntax NewobjFromLambda(this EmitSyntax emit, LambdaExpression callExpr)
        {
            var ctor = ExpressionUtils.GetConstructorFromLambda(callExpr);
            return emit.Newobj(ctor);
        }

        public static EmitSyntax NewDelegate(this EmitSyntax emit, Type delegateType)
        {
            if (!typeof(Delegate).IsAssignableFrom(delegateType))
            {
                throw new ArgumentException("Expected delegate type.", "delegateType");
            }

            var ctor = delegateType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
            return emit .Newobj(ctor);
        }

        public static EmitSyntax Newobj(this EmitSyntax emit, SR.ConstructorInfo constructor)
        {
            var ctorRef = emit.Methods.Import(constructor);
            return emit.Newobj(ctorRef);
        }

        public static EmitSyntax Call(this EmitSyntax emit, SR.MethodInfo method)
        {
            var methodRef = emit.Methods.Import(method);
            if (method.IsStatic)
            {
                return emit.Call(methodRef);
            }
            else
            {
                return emit.Callvirt(methodRef);
            }
        }

        public static EmitSyntax Print(this EmitSyntax emit)
        {
            return emit .Call((object x) => Console.Write(x));
        }

        public static EmitSyntax PrintLn(this EmitSyntax emit)
        {
            return emit.Call((object x) => Console.WriteLine(x));
        }

        public static EmitSyntax PrintIntLn(this EmitSyntax emit)
        {
            return emit.Call((int x) => Console.WriteLine(x));
        }

        public static EmitSyntax PrintStrLn(this EmitSyntax emit)
        {
            return emit.Call((string x) => Console.WriteLine(x));
        }

        public static EmitSyntax Swap(this EmitSyntax emit, Def<Locals> x, Def<Locals> y, Def<Locals> tmp)
        {
            return 
                emit
                .Ldloc(x.GetRef())
                .Stloc(tmp.GetRef())
                .Ldloc(y.GetRef())
                .Stloc(x.GetRef())
                .Ldloc(tmp.GetRef())
                .Stloc(y.GetRef());
        }

        public static EmitSyntax LdMethodDelegate(
            this EmitSyntax emit,
            Ref<Types> declaringType,
            string methodName,
            Type delegateType)
        {
            var method = delegateType.GetMethod("Invoke");
            Pipe<WantArgsSigBase> argBuilder = 
                args =>
                {
                    foreach (var param in method.GetParameters())
                    {
                        var argType = emit.Types.Import(param.ParameterType);
                        args = args.Argument(argType, param.Name);
                    }

                    return args;
                };

            return emit.LdMethodDelegate(
                    scope => 
                    {
                        return scope.StartSignature
                            .Returning(emit.Types.Import(method.ReturnType))
                            .DecaringType(declaringType)
                            .Named(methodName)
                            .BeginArgs()
                                .Do(argBuilder)
                            .EndArgs()
                            ;
                    },
                    delegateType);
        }

        public static EmitSyntax Local(this EmitSyntax emit, Def<Locals> name, Type type)
        {
            return emit.Local(name, emit.Types.Import(type));
        }

        public static EmitSyntax Initobj(this EmitSyntax emit, Type valueType)
        {
            return emit.Initobj(emit.Types.Import(valueType));
        }

        public static EmitSyntax Newarr(this EmitSyntax emit, Type elementType)
        {
            return emit.Newarr(emit.Types.Import(elementType));
        }

        private static EmitSyntax LdMethodDelegate(
            this EmitSyntax emit,
            Pipe<IMethodNs,DoneMethodSig> methodSig,
            Type delegateType)
        {
            return emit
                .Ldnull()
                .Ldftn(emit.Methods.Method(methodSig))
                .NewDelegate(delegateType);
        }
    }
}
