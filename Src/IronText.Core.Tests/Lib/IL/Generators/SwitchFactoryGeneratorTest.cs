using System;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.MetadataCompiler;
using IronText.Framework;
using Moq;
using NUnit.Framework;

namespace IronText.Tests.Lib.IL.Generators
{
    [TestFixture]
    public class SwitchFactoryGeneratorTest
    {
        private readonly TokenRefResolver resolver = new TokenRefResolver();

        [Test]
        public void Test()
        {
            var switchRules = new []
            {
                CreateSwitchRule(typeof(TestReceiver<int>), 2),
                CreateSwitchRule(typeof(TestReceiver<double>), 4)
            };

            var target = new SwitchFactoryGenerator(switchRules, resolver);

            var factory = new CachedMethod<SwitchFactory>(
                GetType().Name + ".Assembly0",
                (emit, args) =>
                {
                    var contextResolver = new ContextResolverCode(
                                            emit,
                                            il => { throw new NotImplementedException(); },
                                            null,
                                            typeof(object));

                    target.Build(
                        emit,
                        contextResolver,
                        il => il.Ldarg(1),
                        il => il.Ldarg(2),
                        il => il.Ldarg(3));

                    return emit;
                }
                ).Delegate;

            IReceiver<Msg> exit = null;
            
            var languageMock = new Mock<ILanguage>();

            Assert.IsInstanceOf(typeof(TestReceiver<int>), factory(null, 2, exit, languageMock.Object));
            Assert.IsInstanceOf(typeof(TestReceiver<double>), factory(null, 4, exit, languageMock.Object));

            Assert.IsNull(factory(null, 1, exit, languageMock.Object));
            Assert.IsNull(factory(null, 3, exit, languageMock.Object));
            Assert.IsNull(factory(null, 5, exit, languageMock.Object));
            Assert.IsNull(factory(null, 6, exit, languageMock.Object));
        }

        private SwitchRule CreateSwitchRule(Type type, int id)
        {
            var tid = TokenRef.Typed(type);
            resolver.SetId(tid, id);

            return new SwitchRule
            {
                Tid = tid,
                ActionBuilder = code =>
                    code
                        .LdExitReceiver()
                        .LdLanguage()
                        .Emit(il => il
                            .Newobj(
                                type.GetConstructor(
                                    new[] { typeof(IReceiver<Msg>), typeof(ILanguage) }))
                            .Ret()),
            }
            ;
        }
    }

    public class TestReceiver<T> : IReceiver<Msg>
    {
        public readonly IReceiver<Msg> Exit;
        public readonly ILanguage      Language;

        public TestReceiver(IReceiver<Msg> exit, ILanguage lang)
        {
            this.Exit = exit;
            this.Language = lang;
        }

        public IReceiver<Msg> Next(Msg item)
        {
            throw new NotImplementedException();
        }

        public IReceiver<Msg> Done()
        {
            throw new NotImplementedException();
        }
    }
}
