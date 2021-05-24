using System;
using System.Collections.Generic;
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
            Assert.True(container.Make<IServiceA>().IsInited);
            Assert.True(serviceB.IsInited);
            serviceB.Execute();
            Assert.True(serviceB.IsExecuted);
            Assert.True(!serviceB.IsShut);
            container.Dispose();
            Assert.True(serviceB.IsShut);
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
                Assert.False(serviceB.IsInited);
                Assert.IsNull(serviceB.ServiceA);
            }
        }

        [Test]
        public void TestPropertyInjection()
        {
            using (var container = new Container())
            {
                container.BindSingleton<IServiceA, ServiceA>(new PropertyInjection
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

        private interface IServiceA : ILifeCycle
        {
            int IntProperty { get; set; }

            bool IsExecuted { get; }

            bool IsInited { get; }

            bool IsShut { get; }

            void Execute();
        }

        private class ServiceA : IServiceA
        {
            public int IntProperty { get; set; }

            public void OnInit()
            {
                Assert.True(!IsInited);
                IsInited = true;
            }

            public void OnShutdown()
            {
                Assert.True(!IsShut);
                IsShut = true;
            }

            public bool IsExecuted { get; private set; }
            public bool IsInited { get; private set; }
            public bool IsShut { get; private set; }

            public void Execute()
            {
                Assert.True(IsInited);
                IsExecuted = true;
            }
        }

        private class ServiceB : ILifeCycle
        {
            [Inject]
            public IServiceA ServiceA { get; set; }

            public bool IsExecuted { get; private set; }
            public bool IsInited { get; private set; }
            public bool IsShut { get; private set; }

            public void OnInit()
            {
                Assert.True(!IsInited);
                Assert.True(ServiceA.IsInited);
                IsInited = true;
            }

            public void OnShutdown()
            {
                Assert.True(!IsShut);
                Assert.True(!ServiceA.IsShut);
                IsShut = true;
            }

            public void Execute()
            {
                Assert.True(IsInited);
                ServiceA.Execute();
                IsExecuted = true;
            }
        }

        private class ServiceC : ServiceB
        {
        }

        private class ServiceD : ILifeCycle
        {
            [Inject]
            public IServiceA ServiceA { get; set; }

            [Inject]
            public ServiceB ServiceB { get; set; }

            [Inject]
            public ServiceC ServiceC { get; set; }

            public void OnInit()
            {
                Assert.True(!Inited);
                Inited = true;
            }

            public void OnShutdown()
            {
                Assert.True(!Shutdown);
                Assert.True(!ServiceA.IsShut);
                Assert.True(!ServiceB.IsShut);
                Assert.True(!ServiceC.IsShut);
                Shutdown = true;
            }

            public bool Executed { get; private set; }
            public bool Inited { get; private set; }
            public bool Shutdown { get; private set; }

            public void Execute()
            {
                Assert.True(Inited);
                Assert.True(ServiceA.IsInited);
                Assert.True(!ServiceA.IsExecuted);
                Assert.True(ServiceB.IsInited);
                Assert.True(!ServiceB.IsExecuted);
                Assert.True(ServiceC.IsInited);
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