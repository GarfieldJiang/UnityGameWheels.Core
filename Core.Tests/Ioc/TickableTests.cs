using NUnit.Framework;

namespace COL.UnityGameWheels.Core.Ioc.Test
{
    [TestFixture]
    public class TickableTests
    {
        private class NonTickableService : ILifeCycle
        {
            public int ExecuteCount;
            public bool IsInited;
            public bool IsShut;

            public void Execute()
            {
                ++ExecuteCount;
            }

            public void OnInit()
            {
                IsInited = true;
            }

            public void OnShutdown()
            {
                IsShut = true;
            }

            public bool CanSafelyShutDown => true;
        }

        private class TickableServiceDependingOnNonTickableService : ITickable
        {
            public int TickCount;

            [Inject]
            public NonTickableService NonTickableService { get; set; }

            public void OnUpdate(TimeStruct timeStruct)
            {
                ++TickCount;
                NonTickableService.Execute();
            }
        }

        private class TickableServiceWithLifeCycle : ILifeCycle, ITickable
        {
            public const int TickCountBeforeCanSafelyShutdown = 10;

            public int TickCount;
            public bool IsInited;
            public bool IsShut;

            [Inject]
            public NonTickableService NonTickableService { get; set; }

            public void OnInit()
            {
                IsInited = true;
            }

            public void OnShutdown()
            {
                IsShut = true;
            }

            public bool CanSafelyShutDown => TickCount > TickCountBeforeCanSafelyShutdown;

            public void OnUpdate(TimeStruct timeStruct)
            {
                ++TickCount;
                NonTickableService.Execute();
            }
        }

        [Test]
        public void TestBasic()
        {
            ITickableContainer container = new TickableContainer();
            container.BindSingleton<TickableServiceWithLifeCycle>();
            container.BindSingleton<TickableServiceDependingOnNonTickableService>();
            container.BindSingleton<NonTickableService>();
            var tickableServiceWithLifeCycle = (TickableServiceWithLifeCycle)container.Make(typeof(TickableServiceWithLifeCycle));
            Assert.True(tickableServiceWithLifeCycle.IsInited);
            Assert.AreEqual(0, tickableServiceWithLifeCycle.TickCount);
            Assert.AreEqual(0, container.Make<TickableServiceDependingOnNonTickableService>().TickCount);
            int tickCount = 0;
            for (; tickCount < TickableServiceWithLifeCycle.TickCountBeforeCanSafelyShutdown / 2; tickCount++)
            {
                container.OnUpdate(default);
                Assert.False(tickableServiceWithLifeCycle.CanSafelyShutDown);
                Assert.AreEqual(2 * (tickCount + 1), container.Make<NonTickableService>().ExecuteCount);
            }

            var nonTickableService = container.Make<NonTickableService>();
            container.RequestShutdown();

            Assert.False(container.IsShuttingDown);
            Assert.False(container.IsShut);
            Assert.True(container.IsRequestingShutdown);
            Assert.False(nonTickableService.IsShut);
            Assert.False(tickableServiceWithLifeCycle.IsShut);

            container.OnUpdate(default);
            tickCount++;

            // NonTickableService is still used by TickableServiceWithLifeCycle;
            Assert.False(nonTickableService.IsShut);
            Assert.False(tickableServiceWithLifeCycle.IsShut);

            for (; tickCount < TickableServiceWithLifeCycle.TickCountBeforeCanSafelyShutdown; tickCount++)
            {
                container.OnUpdate(default);
                Assert.False(container.IsShuttingDown);
                Assert.False(container.IsShut);
                Assert.False(tickableServiceWithLifeCycle.CanSafelyShutDown);
            }


            container.OnUpdate(default);
            tickCount++;
            Assert.True(nonTickableService.IsShut);
            Assert.True(tickableServiceWithLifeCycle.IsShut);
            Assert.False(container.IsShuttingDown);
            Assert.True(container.IsShut);
        }
    }
}