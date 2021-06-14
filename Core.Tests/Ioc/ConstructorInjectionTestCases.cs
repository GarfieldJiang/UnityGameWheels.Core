using System;
using NUnit.Framework;

namespace COL.UnityGameWheels.Core.Ioc.Test
{
    [TestFixture]
    public class ConstructorInjectionTestCases
    {
        [Test]
        public void TestSimpleDefaultConstructor()
        {
            using (var container = new Container())
            {
                container.BindSingleton<SimpleDefaultConstructor.IServiceA, SimpleDefaultConstructor.ServiceA>();
                var instance = container.Make<SimpleDefaultConstructor.IServiceA>();
                Assert.AreEqual(typeof(SimpleDefaultConstructor.ServiceA), instance.GetType());
            }

            using (var container = new Container())
            {
                container.Bind<SimpleDefaultConstructor.IServiceA, SimpleDefaultConstructor.ServiceA>();
                var instance = container.Make<SimpleDefaultConstructor.IServiceA>();
                Assert.AreEqual(typeof(SimpleDefaultConstructor.ServiceA), instance.GetType());
            }
        }

        private static class SimpleDefaultConstructor
        {
            public interface IServiceA
            {
            }

            public class ServiceA : IServiceA
            {
                public ServiceA()
                {
                }
            }
        }

        [Test]
        public void TestConstructorWithPrimitiveTypeParams()
        {
            using (var container = new Container())
            {
                container.Bind<ConstructorWithPrimitiveTypeParams.IServiceA, ConstructorWithPrimitiveTypeParams.ServiceA>();
                Assert.Throws<InvalidOperationException>(() => { container.Make<ConstructorWithPrimitiveTypeParams.IServiceA>(); });
            }
        }

        private static class ConstructorWithPrimitiveTypeParams
        {
            public interface IServiceA
            {
            }

            public class ServiceA : IServiceA
            {
                public ServiceA(int intParam)
                {
                }
            }
        }

        [Test]
        public void TestBasicDependency()
        {
            using (var container = new Container())
            {
                container.BindSingleton<BasicDependency.IServiceA, BasicDependency.ServiceA>();
                container.BindSingleton<BasicDependency.ServiceB>();
                container.BindSingleton<BasicDependency.IServiceC, BasicDependency.ServiceC>();
                container.BindSingleton<BasicDependency.ServiceD>();
                var d = container.Make<BasicDependency.ServiceD>();
                Assert.AreSame(d.A, container.Make<BasicDependency.IServiceA>());
                Assert.AreSame(d.B, container.Make<BasicDependency.ServiceB>());
                Assert.AreSame(d.C, container.Make<BasicDependency.IServiceC>());
            }

            using (var container = new Container())
            {
                container.BindSingleton<BasicDependency.IServiceA, BasicDependency.ServiceA>();
                container.Bind<BasicDependency.ServiceB>();
                container.BindSingleton<BasicDependency.IServiceC, BasicDependency.ServiceC>();
                container.Bind<BasicDependency.ServiceD>();
                var d = container.Make<BasicDependency.ServiceD>();
                Assert.AreSame(d.A, container.Make<BasicDependency.IServiceA>());
                Assert.AreNotSame(d.B, container.Make<BasicDependency.ServiceB>());
                Assert.AreSame(d.C, container.Make<BasicDependency.IServiceC>());
            }
        }

        private static class BasicDependency
        {
            public interface IServiceA
            {
            }

            public class ServiceA : IServiceA
            {
            }

            public class ServiceB
            {
                public IServiceA A { get; private set; }

                public ServiceB(IServiceA a)
                {
                    A = a;
                }
            }

            public interface IServiceC
            {
            }

            public class ServiceC : IServiceC
            {
                public IServiceA A { get; private set; }

                public ServiceC(IServiceA a)
                {
                    A = a;
                }
            }

            public class ServiceD
            {
                public IServiceA A { get; private set; }
                public ServiceB B { get; private set; }

                public IServiceC C { get; private set; }

                public ServiceD(IServiceA a, ServiceB b, IServiceC c)
                {
                    A = a;
                    B = b;
                    C = c;
                }
            }
        }

        [Test]
        public void TestCycleDependency()
        {
            using (var container = new Container())
            {
                container.BindSingleton<CycleDependency.ServiceA>();
                container.BindSingleton<CycleDependency.ServiceB>();
                container.BindSingleton<CycleDependency.ServiceC>();
                container.BindSingleton<CycleDependency.ServiceD>();
                Assert.Throws<InvalidOperationException>(() => { container.Make<CycleDependency.ServiceA>(); });
            }
        }

        private static class CycleDependency
        {
            public class ServiceA
            {
                public ServiceA(ServiceB b)
                {
                }
            }

            public class ServiceB
            {
                public ServiceB(ServiceC c)
                {
                }
            }

            public class ServiceC
            {
                public ServiceC(ServiceD d)
                {
                }
            }

            public class ServiceD
            {
                public ServiceD(ServiceA a, ServiceB b)
                {
                }
            }
        }
    }
}