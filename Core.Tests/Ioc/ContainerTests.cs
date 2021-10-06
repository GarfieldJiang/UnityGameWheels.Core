using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using NUnit.Framework;

namespace COL.UnityGameWheels.Core.Ioc.Test
{
    [TestFixture]
    public class ContainerTests
    {
        [Test]
        public void TestBasic()
        {
            var container = new Container();
            container.BindSingleton<IServiceA, ServiceA>();
            container.BindSingleton<ServiceB>();
            Assert.Throws<InvalidOperationException>(() => { container.BindSingleton(typeof(ServiceB), typeof(ServiceB)); });
            var serviceB = container.Make<ServiceB>();
            var serviceA = container.Make<IServiceA>();
            Assert.AreSame(serviceA, container.Make(typeof(IServiceA)));
            serviceB.Execute();
            Assert.True(serviceB.IsExecuted);
            Assert.True(!serviceB.IsShut);
            container.Dispose();
            Assert.True(serviceB.IsShut);
        }

        [Test]
        public void TestLifeStyle()
        {
            var container = new Container();
            container.BindSingleton<IServiceA, ServiceA>();
            container.Bind<ServiceB>();
            container.BindSingleton<ServiceC>();
            container.Bind<ServiceD>();
            var d1 = container.Make<ServiceD>();
            var d2 = container.Make<ServiceD>();
            Assert.AreNotSame(d1, d2);
            Assert.AreSame(d1.ServiceC, d2.ServiceC);
            Assert.AreNotSame(d1.ServiceB, d2.ServiceB);
            Assert.AreSame(d1.ServiceA, d2.ServiceA);
            container.Dispose();
            foreach (var d in new[] { d1, d2 })
            {
                Assert.True(d.ServiceA.IsShut);
                Assert.True(d.ServiceC.IsShut);
                Assert.True(!d.ServiceB.IsShut);
                Assert.True(!d.IsShut);
            }
        }

        [Test]
        public void TestDiamondDependency()
        {
            var container = new Container();
            container.BindSingleton<IServiceA, ServiceA>();
            container.BindSingleton<ServiceB>();
            container.BindSingleton<ServiceC, ServiceC>();
            container.BindSingleton<ServiceD>();
            var serviceD = container.Make<ServiceD>();
            serviceD.Execute();
            var serviceA = (IServiceA)container.Make(typeof(IServiceA));
            container.Dispose();
            Assert.True(serviceA.IsShut);
        }

        [Test]
        public void TestBindingData()
        {
            using (var container = new Container())
            {
                var bindingData = container.BindSingleton<IServiceA, ServiceA>();
                Assert.True(container.IsBound<IServiceA>());
                Assert.True(container.TypeIsBound(typeof(IServiceA)));
                var bindingData2 = container.GetBindingData(typeof(IServiceA));
                Assert.AreSame(bindingData, bindingData2);
            }
        }


        [Test]
        public void TestBindInstance()
        {
            using (var container = new Container())
            {
                container.BindSingleton<IServiceA, ServiceA>();
                container.BindInstance(new ServiceB());
                var serviceB = container.Make<ServiceB>();

                // Life cycle is not managed, so no auto wiring or auto initialization.
                Assert.IsNull(serviceB.ServiceA);
            }
        }

        [Test]
        public void TestPropertyInjection()
        {
            using (var container = new Container())
            {
                container.BindSingleton<IServiceA, ServiceA>().AddPropertyInjections(new PropertyInjection
                {
                    PropertyName = "IntProperty",
                    Value = 125,
                });

                Assert.AreEqual(125, container.Make<IServiceA>().IntProperty);
            }
        }

        [Test]
        public void TestGenericUnsupported()
        {
            using (var container = new Container())
            {
                Assert.Throws<ArgumentException>(() => { container.BindSingleton(typeof(IGenericServiceA<>), typeof(GenericServiceA<>)); });
                Assert.Throws<ArgumentException>(() => { container.BindSingleton(typeof(GenericServiceA<>), typeof(GenericServiceA<>)); });
                container.BindSingleton<IGenericServiceA<int>, GenericServiceA<int>>();
                Assert.AreEqual(typeof(GenericServiceA<int>), container.Make<IGenericServiceA<int>>().GetType());
            }
        }

        [Test]
        public void TestBindBeforeMake()
        {
            using (var container = new Container())
            {
                container.BindSingleton<ServiceA>();
                container.Make<ServiceA>();
                Assert.Throws<InvalidOperationException>(() => { container.BindSingleton<ServiceB>(); });
            }
        }

        private interface IServiceA
        {
            int IntProperty { get; set; }

            bool IsExecuted { get; }

            bool IsShut { get; }

            void Execute();
        }

        private class ServiceA : IServiceA, IDisposable
        {
            public int IntProperty { get; set; }

            public void Dispose()
            {
                Assert.True(!IsShut);
                IsShut = true;
            }

            public bool IsExecuted { get; private set; }
            public bool IsShut { get; private set; }

            public void Execute()
            {
                IsExecuted = true;
            }
        }

        private class ServiceB : IDisposable
        {
            [Inject]
            public IServiceA ServiceA { get; set; }

            public bool IsExecuted { get; private set; }
            public bool IsShut { get; private set; }

            public void Dispose()
            {
                Assert.True(!IsShut);
                Assert.True(!ServiceA.IsShut);
                IsShut = true;
            }

            public void Execute()
            {
                ServiceA.Execute();
                IsExecuted = true;
            }
        }

        private class ServiceC : ServiceB
        {
        }

        private class ServiceD : IDisposable
        {
            [Inject]
            public IServiceA ServiceA { get; set; }

            [Inject]
            public ServiceB ServiceB { get; set; }

            [Inject]
            public ServiceC ServiceC { get; set; }

            public void Dispose()
            {
                Assert.True(!IsShut);
                Assert.True(!ServiceA.IsShut);
                Assert.True(!ServiceB.IsShut);
                Assert.True(!ServiceC.IsShut);
                IsShut = true;
            }

            public bool Executed { get; private set; }
            public bool IsShut { get; private set; }

            public void Execute()
            {
                Assert.True(!ServiceA.IsExecuted);
                Assert.True(!ServiceB.IsExecuted);
                Assert.True(!ServiceC.IsExecuted);
                ServiceB.Execute();
                ServiceC.Execute();
                Executed = true;
            }
        }

        private interface IGenericServiceA<T>
        {
        }

        private class GenericServiceA<T> : IGenericServiceA<T>
        {
        }
    }
}