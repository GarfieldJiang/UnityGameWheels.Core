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

            var fakeAssetInfo = new AssetInfo {Path = "abc", ResourcePath = "123"};
            fakeAssetInfo.DependencyAssetPaths.Add("xyz1");
            fakeAssetInfo.DependencyAssetPaths.Add("xyz2");
            assetIndex.AssetInfos.Add(fakeAssetInfo.Path, fakeAssetInfo);

            var fakeResourceInfo = new ResourceInfo {Path = "123", Size = 1000L, Crc32 = 456, Hash = "ThisIsAFakeHash"};
            var fakeResourceBasicInfo = new ResourceBasicInfo {Path = "123", GroupId = 1700};
            var fakeResourceInfo1 = new ResourceInfo {Path = "xyz1", Size = 2000L, Crc32 = 4568, Hash = "ThisIsAFakeHash1"};
            var fakeResourceBasicInfo1 = new ResourceBasicInfo {Path = "xyz1", GroupId = 1500};
            var fakeResourceInfo2 = new ResourceInfo {Path = "xyz2", Size = 1500L, Crc32 = 4569, Hash = "ThisIsAFakeHash2"};
            var fakeResourceBasicInfo2 = new ResourceBasicInfo {Path = "xyz2", GroupId = 1800};

            assetIndex.ResourceInfos.Add(fakeResourceInfo.Path, fakeResourceInfo);
            assetIndex.ResourceInfos.Add(fakeResourceInfo1.Path, fakeResourceInfo1);
            assetIndex.ResourceInfos.Add(fakeResourceInfo2.Path, fakeResourceInfo2);

            assetIndex.ResourceBasicInfos.Add(fakeResourceBasicInfo.Path, fakeResourceBasicInfo);
            assetIndex.ResourceBasicInfos.Add(fakeResourceBasicInfo1.Path, fakeResourceBasicInfo1);
            assetIndex.ResourceBasicInfos.Add(fakeResourceBasicInfo2.Path, fakeResourceBasicInfo2);

            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                using (var bw = new BinaryWriter(memoryStream))
                {
                    new AssetIndexSerializerV2().ToBinary(bw, assetIndex);
                }

                bytes = memoryStream.ToArray();
            }


            var anotherAssetIndex = createAssetIndex();
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var br = new BinaryReader(memoryStream))
                {
                    new AssetIndexSerializerV2().FromBinary(br, anotherAssetIndex);
                }
            }

            AssertAssetIndicesAreEqual(assetIndex, anotherAssetIndex);

            using (var memoryStream = new MemoryStream())
            {
                using (var bw = new BinaryWriter(memoryStream))
                {
                    new AssetIndexSerializer().ToBinary(bw, assetIndex);
                }

                bytes = memoryStream.ToArray();
            }

            anotherAssetIndex.Clear();
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var br = new BinaryReader(memoryStream))
                {
                    new AssetIndexSerializer().FromBinary(br, anotherAssetIndex);
                }
            }

            AssertAssetIndicesAreEqual(assetIndex, anotherAssetIndex);
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