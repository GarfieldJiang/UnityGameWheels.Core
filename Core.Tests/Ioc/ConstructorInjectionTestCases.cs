using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Constraints;

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

        [Test]
        public void TestLifeCycle()
        {
            var container = new Container();
            var statuses = new Dictionary<Type, LifeCycle.LifeCycleStatus>
            {
                { typeof(LifeCycle.ServiceA), LifeCycle.LifeCycleStatus.None },
                { typeof(LifeCycle.ServiceB), LifeCycle.LifeCycleStatus.None },
                { typeof(LifeCycle.ServiceC), LifeCycle.LifeCycleStatus.None },
                { typeof(LifeCycle.ServiceD), LifeCycle.LifeCycleStatus.None },
            };

            var instances = new Dictionary<Type, object>
            {
                { typeof(LifeCycle.ServiceA), null },
                { typeof(LifeCycle.ServiceB), null },
                { typeof(LifeCycle.ServiceC), null },
                { typeof(LifeCycle.ServiceD), null },
            };

            void SetStatus(Type type, LifeCycle.LifeCycleStatus status)
            {
                statuses[type] = status;
            }

            container.BindSingleton<LifeCycle.ServiceA>()
                .OnInstanceCreated(serviceInstance => { SetStatus(serviceInstance.GetType(), LifeCycle.LifeCycleStatus.InstanceCreated); })
                .OnPreDispose(serviceInstance =>
                {
                    Assert.AreEqual(LifeCycle.LifeCycleStatus.Disposed, statuses[typeof(LifeCycle.ServiceB)]);
                    Assert.AreEqual(LifeCycle.LifeCycleStatus.Disposed, statuses[typeof(LifeCycle.ServiceD)]);
                    SetStatus(serviceInstance.GetType(), LifeCycle.LifeCycleStatus.PreDispose);
                })
                .OnDisposed(() => { SetStatus(typeof(LifeCycle.ServiceA), LifeCycle.LifeCycleStatus.Disposed); });
            container.BindSingleton<LifeCycle.ServiceB>()
                .OnInstanceCreated(serviceInstance => { SetStatus(serviceInstance.GetType(), LifeCycle.LifeCycleStatus.InstanceCreated); })
                .OnPreDispose(serviceInstance => { SetStatus(serviceInstance.GetType(), LifeCycle.LifeCycleStatus.PreDispose); })
                .OnDisposed(() =>
                {
                    Assert.AreEqual(LifeCycle.LifeCycleStatus.InstanceCreated, statuses[typeof(LifeCycle.ServiceA)]);
                    SetStatus(typeof(LifeCycle.ServiceB), LifeCycle.LifeCycleStatus.Disposed);
                });
            var bindingDataC = container.Bind<LifeCycle.ServiceC>()
                .OnInstanceCreated(serviceInstance => { SetStatus(serviceInstance.GetType(), LifeCycle.LifeCycleStatus.InstanceCreated); });
            Assert.Throws<InvalidOperationException>(() => { bindingDataC.OnPreDispose(o => { }); });
            Assert.Throws<InvalidOperationException>(() => { bindingDataC.OnDisposed(() => { }); });

            container.BindSingleton<LifeCycle.ServiceD>()
                .OnInstanceCreated(serviceInstance => { SetStatus(serviceInstance.GetType(), LifeCycle.LifeCycleStatus.InstanceCreated); })
                .OnPreDispose(serviceInstance => { SetStatus(serviceInstance.GetType(), LifeCycle.LifeCycleStatus.PreDispose); })
                .OnDisposed(() => { SetStatus(typeof(LifeCycle.ServiceD), LifeCycle.LifeCycleStatus.Disposed); });


            var d = container.Make<LifeCycle.ServiceD>();
            var c = container.Make<LifeCycle.ServiceC>();
            var b = container.Make<LifeCycle.ServiceB>();
            var a = container.Make<LifeCycle.ServiceA>();
            foreach (var instance in new object[] { a, b, c, d })
            {
                instances[instance.GetType()] = instance;
            }

            // Instances have been created.
            foreach (var kv in statuses)
            {
                Assert.AreEqual(LifeCycle.LifeCycleStatus.InstanceCreated, kv.Value);
            }

            container.Dispose();
            Assert.AreEqual(LifeCycle.LifeCycleStatus.Disposed, statuses[typeof(LifeCycle.ServiceA)]);
            Assert.True(a.Disposed);
            Assert.AreEqual(LifeCycle.LifeCycleStatus.Disposed, statuses[typeof(LifeCycle.ServiceB)]);
            Assert.True(b.Disposed);
            // ServiceC is not singleton, so it won't be disposed by the container.
            Assert.AreEqual(LifeCycle.LifeCycleStatus.InstanceCreated, statuses[typeof(LifeCycle.ServiceC)]);
            Assert.False(c.Disposed);
            Assert.AreEqual(LifeCycle.LifeCycleStatus.Disposed, statuses[typeof(LifeCycle.ServiceD)]);
            Assert.True(d.Disposed);
        }

        private static class LifeCycle
        {
            public enum LifeCycleStatus
            {
                None,
                InstanceCreated,
                PreDispose,
                Disposed,
            }

            public class ServiceA : IDisposable
            {
                public bool Disposed { get; private set; }

                public ServiceA()
                {
                }

                public void Dispose()
                {
                    Disposed = true;
                }
            }

            public class ServiceB : IDisposable
            {
                public bool Disposed { get; private set; }

                public ServiceB(ServiceA a)
                {
                    Assert.True(a != null && !a.Disposed);
                }

                public void Dispose()
                {
                    Disposed = true;
                }
            }

            public class ServiceC : IDisposable
            {
                public bool Disposed { get; private set; }

                public ServiceC(ServiceA a)
                {
                    Assert.True(a != null && !a.Disposed);
                }

                public void Dispose()
                {
                    Disposed = true;
                }
            }

            public class ServiceD : IDisposable
            {
                public bool Disposed { get; private set; }

                public ServiceD(ServiceB b)
                {
                    Assert.True(b != null && !b.Disposed);
                }

                public void Dispose()
                {
                    Disposed = true;
                }
            }
        }
    }
}