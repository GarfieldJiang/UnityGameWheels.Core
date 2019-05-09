using COL.UnityGameWheels.Core.Asset;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class AssetIndexTests
    {
        private void TestAssetIndex(Func<AssetIndexBase> createAssetIndex)
        {
            const string platform = "Android";
            const string bundleVersion = "1.0.5";
            const int internalAssetVersion = 123;
            var assetIndex = createAssetIndex();

            if (assetIndex is AssetIndexAugmented assetIndexAugmented)
            {
                assetIndexAugmented.Platform = platform;
                assetIndexAugmented.BundleVersion = bundleVersion;
                assetIndexAugmented.InternalAssetVersion = internalAssetVersion;
            }

            var fakeAssetInfo = new AssetInfo();
            fakeAssetInfo.Path = "abc";
            fakeAssetInfo.ResourcePath = "123";
            fakeAssetInfo.DependencyAssetPaths.Add("xyz1");
            fakeAssetInfo.DependencyAssetPaths.Add("xyz2");
            assetIndex.AssetInfos.Add(fakeAssetInfo.Path, fakeAssetInfo);

            var fakeResourceInfo = new ResourceInfo();
            fakeResourceInfo.Path = "xyz1";
            fakeResourceInfo.Size = 1000L;
            fakeResourceInfo.Crc32 = 456;
            fakeResourceInfo.Hash = "ThisIsAFakeHash";

            var fakeResourceBasicInfo = new ResourceBasicInfo();
            fakeResourceBasicInfo.Path = "xyz1";
            fakeResourceBasicInfo.GroupId = 1700;

            var memoryStream = new MemoryStream();
            using (var bw = new BinaryWriter(memoryStream))
            {
                assetIndex.ToBinary(bw);
            }

            memoryStream = new MemoryStream(memoryStream.ToArray());
            var anotherAssetIndex = createAssetIndex();
            using (var br = new BinaryReader(memoryStream))
            {
                anotherAssetIndex.FromBinary(br);
            }

            AssertAssetIndicesAreEqual(assetIndex, anotherAssetIndex);

            memoryStream.Close();
        }

        private bool ResourceInfosAreEqual(ResourceInfo x, ResourceInfo y)
        {
            return x.Path == y.Path && x.Crc32 == y.Crc32 && x.Size == y.Size && x.Hash == y.Hash;
        }

        private void AssertAssetIndicesAreEqual(AssetIndexBase x, AssetIndexBase y)
        {
            Assert.True(x.GetType() == y.GetType() &&
                        (
                            !(x is AssetIndexAugmented x1 && y is AssetIndexAugmented y1) ||
                            x1.Platform == y1.Platform &&
                            x1.BundleVersion == y1.BundleVersion &&
                            x1.InternalAssetVersion == y1.InternalAssetVersion
                        )
            );

            Assert.AreEqual(x.ResourceInfos.Count, y.ResourceInfos.Count);

            var resourceInfoKeys = x.ResourceInfos.Keys.ToList();
            foreach (var key in resourceInfoKeys)
            {
                var resourceInfoX = x.ResourceInfos[key];
                var resourceInfoY = y.ResourceInfos[key];
                Assert.True(ResourceInfosAreEqual(resourceInfoX, resourceInfoY));
            }

            Assert.AreEqual(x.AssetInfos.Count, y.AssetInfos.Count);
            var assetInfoKeys = x.AssetInfos.Keys.ToList();
            foreach (var key in assetInfoKeys)
            {
                var assetInfoX = x.AssetInfos[key];
                var assetInfoY = y.AssetInfos[key];
                Assert.AreEqual(assetInfoX.Path, assetInfoY.Path);
                Assert.AreEqual(assetInfoX.ResourcePath, assetInfoY.ResourcePath);
                Assert.True(assetInfoX.DependencyAssetPaths.SetEquals(assetInfoY.DependencyAssetPaths));
            }

            Assert.AreEqual(x.ResourceBasicInfos.Count, y.ResourceBasicInfos.Count);
            foreach (var key in x.ResourceBasicInfos.Keys)
            {
                var infoX = x.ResourceBasicInfos[key];
                var infoY = y.ResourceBasicInfos[key];
                Assert.AreEqual(infoX.Path, infoY.Path);
                Assert.AreEqual(infoX.GroupId, infoY.GroupId);
            }
        }

        [Test]
        public void TestAssetIndexForInstaller()
        {
            TestAssetIndex(() => new AssetIndexForInstaller());
        }

        [Test]
        public void TestAssetIndexForReadWrite()
        {
            TestAssetIndex(() => new AssetIndexForReadWrite());
        }

        [Test]
        public void TestAssetIndexRemote()
        {
            TestAssetIndex(() => new AssetIndexForRemote());
        }
    }
}