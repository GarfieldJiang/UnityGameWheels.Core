namespace COL.UnityGameWheels.Core.Tests
{
    using NUnit.Framework;
    using System.IO;
    using System.Text;
    using Crc32 = Algorithm.Crc32;

    [TestFixture]
    public class Crc32Tests
    {
        [Test]
        public void Crc32ForEmptySequenseIs0()
        {
            var actual = Crc32.Sum(new byte[0]);
            Assert.That(actual, Is.EqualTo(0));
        }

        [Test]
        public void Normal()
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual(0x83DCEFB7, Crc32.Sum("1"));
                Assert.AreEqual(0xCBF43926, Crc32.Sum("123456789"));
                Assert.AreEqual(2768625435u, Crc32.Sum(new byte[] {1}));
                Assert.AreEqual(622876539u, Crc32.Sum(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}));
            });
        }

        [Test]
        public void DifferentInputForms()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 10000; i++)
            {
                sb.Append(i.ToString());
            }

            var text = sb.ToString();
            var byString = Crc32.Sum(text, Encoding.UTF8);

            var bytes = Encoding.UTF8.GetBytes(text);
            var byByteArray = Crc32.Sum(bytes);
            Assert.AreEqual(byString, byByteArray);

            uint byStream;
            using (var ms = new MemoryStream(bytes))
            {
                byStream = Crc32.Sum(ms);
            }

            Assert.AreEqual(byString, byStream);
        }
    }
}