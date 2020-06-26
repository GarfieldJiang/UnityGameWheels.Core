using System;
using NUnit.Framework;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class VersionTests
    {
        [Test]
        public void TestCompare()
        {
            Assert.Less(new Version("1.2.3"), new Version("1.3"));
            Assert.Less(new Version("2.199"), new Version("3"));
            Assert.AreEqual(new Version("1.0.0"), new Version("1"));
            Assert.AreEqual(new Version("1.0.0"), new Version("1.0"));
        }

        [Test]
        public void TestParse()
        {
            var v = new Version("3.5.12324");
            Assert.AreEqual(v.Major, 3);
            Assert.AreEqual(v.Minor, 5);
            Assert.AreEqual(v.Patch, 12324);
        }

        [Test]
        public void TestParseFailure()
        {
            Assert.Throws<ArgumentException>(() => { new Version("-1.2.3"); });

            Assert.Throws<ArgumentException>(() => { new Version("xu+-"); });
        }

        [Test]
        public void TestToString()
        {
            var text = "1000.0.1111";
            var v = new Version(text);
            Assert.AreEqual(text, v.ToString());
            Assert.AreEqual(text, v.ToString(1));
            Assert.AreEqual(text, v.ToString(2));
            Assert.AreEqual(text, v.ToString(3));
            Assert.Throws<ArgumentOutOfRangeException>(() => { v.ToString(0); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { v.ToString(4); });

            text = "1000.1111.0";
            v = new Version(text);
            Assert.AreEqual("1000.1111", v.ToString(1));
            Assert.AreEqual("1000.1111", v.ToString(2));
            Assert.AreEqual(text, v.ToString(3));

            text = "3.0.0";
            v = new Version(text);
            Assert.AreEqual("3", v.ToString(1));
            Assert.AreEqual("3.0", v.ToString(2));
            Assert.AreEqual(text, v.ToString(3));
        }
    }
}