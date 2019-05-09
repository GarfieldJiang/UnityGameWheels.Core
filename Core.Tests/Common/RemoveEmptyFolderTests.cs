using NUnit.Framework;
using System.IO;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    internal class RemoveEmptyFolderTests
    {
        private DirectoryInfo m_Root = null;

        [SetUp]
        public void SetUp()
        {
            var rootPath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "root");
            m_Root = new DirectoryInfo(rootPath);
            m_Root.Create();
        }

        [TearDown]
        public void TearDown()
        {
            m_Root.Delete(true);
        }

        [Test]
        public void Normal()
        {
            var a = m_Root.CreateSubdirectory("a");
            var b = m_Root.CreateSubdirectory("b");
            var c = b.CreateSubdirectory("c");
            var d = m_Root.CreateSubdirectory("d");
            var e = d.CreateSubdirectory("e");
            var fileA = new FileInfo(Path.Combine(d.FullName, "a"));
            fileA.Create().Close();
            var f = m_Root.CreateSubdirectory("f");
            var g = f.CreateSubdirectory("g");
            var fileB = new FileInfo(Path.Combine(g.FullName, "b"));
            fileB.Create().Close();

            Utility.IO.DeleteEmptyFolders(m_Root);

            Assert.IsTrue(m_Root.Exists);
            Assert.IsFalse(a.Exists);
            Assert.IsFalse(b.Exists);
            Assert.IsFalse(c.Exists);
            Assert.IsTrue(d.Exists);
            Assert.IsFalse(e.Exists);
            Assert.IsTrue(f.Exists);
            Assert.IsTrue(g.Exists);
            Assert.IsTrue(fileA.Exists);
            Assert.IsTrue(fileB.Exists);
        }
    }
}
