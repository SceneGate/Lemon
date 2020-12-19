// Copyright (c) 2019 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace SceneGate.Lemon.IntegrationTests.Containers
{
    using System.IO;
    using NUnit.Framework;
    using SceneGate.Lemon.Containers.Formats;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    [TestFixtureSource(typeof(TestData), nameof(TestData.NcsdParams))]
    public class NcsdConverterTests
    {
        readonly string ncsdPath;
        readonly string yamlPath;

        Node actualNode;
        Ncsd actual;
        NcsdTestInfo expected;

        public NcsdConverterTests(string ncsdPath, string yamlPath)
        {
            this.ncsdPath = ncsdPath;
            this.yamlPath = yamlPath;
        }

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            TestDataBase.IgnoreIfFileDoesNotExist(ncsdPath);
            TestDataBase.IgnoreIfFileDoesNotExist(yamlPath);

            actualNode = NodeFactory.FromFile(ncsdPath);
            Assert.That(() => actualNode.TransformTo<Ncsd>(), Throws.Nothing);
            actual = actualNode.GetFormatAs<Ncsd>();

            string yaml = File.ReadAllText(yamlPath);
            expected = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build()
                .Deserialize<NcsdTestInfo>(yaml);
        }

        [OneTimeTearDown]
        public void TearDownFixture()
        {
            actualNode?.Dispose();
            Assert.That(DataStream.ActiveStreams, Is.EqualTo(0));
        }

        [Test]
        public void ValidateHeader()
        {
            var header = actual.Header;
            Assert.That(header.Signature, Has.Length.EqualTo(expected.SignatureLength));
            Assert.That(header.Size, Is.EqualTo(expected.Size));
            Assert.That(header.MediaId, Is.EqualTo(expected.MediaId));
            Assert.That(header.FirmwaresType, Is.EquivalentTo(expected.FirmwaresType));
            Assert.That(header.CryptType, Is.EquivalentTo(expected.CryptType));
        }

        [Test]
        public void ValidatePartitions()
        {
            Assert.That(
                actual.Root.Children,
                Has.Count.EqualTo(expected.AvailablePartitions.Length));

            for (int i = 0; i < actual.Root.Children.Count; i++) {
                var child = actual.Root.Children[i];
                Assert.That(child.Name, Is.EqualTo(expected.AvailablePartitions[i]));
                Assert.That(child.Stream.Offset, Is.EqualTo(expected.PartitionsOffset[i]));
                Assert.That(child.Stream.Length, Is.EqualTo(expected.PartitionsSize[i]));
            }
        }
    }
}
