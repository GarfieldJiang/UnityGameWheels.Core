using System;

namespace COL.UnityGameWheels.Core.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class GuardTests
    {
        private class ExceptionNormal : Exception
        {
            public ExceptionNormal()
            {
            }
        }

        private class ExceptionWithoutDefaultConstructor : Exception
        {
            public ExceptionWithoutDefaultConstructor(string message)
            {
            }
        }

        [Test]
        public void TestRequireSuccess()
        {
            Guard.RequireTrue<ExceptionNormal>(true);
            Guard.RequireFalse<ExceptionNormal>(false);
        }

        [Test]
        public void TestRequireFailure()
        {
            bool thrown;

            thrown = false;
            try
            {
                Guard.RequireTrue<ExceptionNormal>(false, "The message");
            }
            catch (Exception e)
            {
                Assert.True(e.GetType() == typeof(ExceptionNormal));
                Assert.AreEqual("The message", e.Message);
                Assert.IsNull(e.InnerException);
                thrown = true;
            }

            Assert.True(thrown);

            thrown = false;
            try
            {
                Guard.RequireFalse<ExceptionNormal>(true, "The message", new ExceptionWithoutDefaultConstructor(string.Empty));
            }
            catch (Exception e)
            {
                Assert.True(e.GetType() == typeof(ExceptionNormal));
                Assert.AreEqual("The message", e.Message);
                Assert.NotNull(e.InnerException);
                Assert.True(e.InnerException.GetType() == typeof(ExceptionWithoutDefaultConstructor));
                thrown = true;
            }

            Assert.True(thrown);
        }
    }
}