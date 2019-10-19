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
namespace Lemon.IntegrationTests.Containers
{
    using System;
    using System.IO;
    using Lemon.Containers.Converters;
    using Lemon.Logging;
    using NUnit.Framework;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    [TestFixtureSource(typeof(TestData), nameof(TestData.IvfcParams))]
    public class IvfcConverterTests
    {
        readonly string yamlPath;
        readonly string binaryPath;
        readonly int offset;
        readonly int size;

        CaptureLogger logger;
        Node node;

        public IvfcConverterTests(string yamlPath, string binaryPath, int offset, int size)
        {
            this.yamlPath = yamlPath;
            this.binaryPath = binaryPath;
            this.offset = offset;
            this.size = size;
        }

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            if (!File.Exists(binaryPath))
                Assert.Ignore($"Binary file doesn't exist: {binaryPath}");
            if (!File.Exists(yamlPath))
                Assert.Ignore($"YAML file doesn't exist: {yamlPath}");

            logger = new CaptureLogger();
            LogProvider.SetCurrentLogProvider(logger);
        }

        [SetUp]
        public void SetUp()
        {
            logger.Clear();

            Console.WriteLine(Path.GetFileName(binaryPath));
            var stream = DataStreamFactory.FromFile(binaryPath, FileOpenMode.Read, offset, size);
            node = new Node("rom", new BinaryFormat(stream));
        }

        [TearDown]
        public void TearDownFixture()
        {
            node?.Dispose();
        }

        [Test]
        public void TransformToContainer()
        {
            try {
                Assert.That(
                    () => node.TransformWith<BinaryIvfc2NodeContainer>(),
                    Throws.Nothing);
                Assert.That(logger.IsEmpty, Is.True);

                node.Dispose();
                Assert.That(DataStream.ActiveStreams, Is.EqualTo(0));
            }
            catch {
            }
        }

        [Test]
        public void ValidateNodes()
        {
            string yaml = File.ReadAllText(yamlPath);
            NodeContainerInfo expected = new DeserializerBuilder()
                .WithNamingConvention(new UnderscoredNamingConvention())
                .Build()
                .Deserialize<NodeContainerInfo>(yaml);

            node.TransformWith<BinaryIvfc2NodeContainer>();
            CheckNode(expected, node);
        }

        [Test]
        public void TransformBothWays()
        {
            BinaryFormat expected = null;
            NodeContainerFormat content = null;
            BinaryFormat actual = null;
            try {
                expected = node.GetFormatAs<BinaryFormat>();
                content = (NodeContainerFormat)ConvertFormat
                    .With<BinaryIvfc2NodeContainer>(expected);

                Assert.That(
                    () => actual = (BinaryFormat)ConvertFormat
                        .With<NodeContainer2BinaryIvfc>(content),
                    Throws.Nothing);
                Assert.That(logger.IsEmpty, Is.True);

                Assert.That(expected.Stream.Compare(actual.Stream), Is.True);
            } finally {
                expected?.Dispose();
                content?.Dispose();
                actual?.Dispose();
                node.Dispose();
                Assert.That(DataStream.ActiveStreams, Is.EqualTo(0));
            }
        }

        [Test]
        public void TransformBotnWaysInChildren()
        {
            DataStream expected = null;
            DataStream actual = null;
            try {
                expected = new DataStream(node.Stream, 0, node.Stream.Length);

                Node parent = new Node("program");
                parent.Add(node);

                actual = parent.Children["rom"]
                    .TransformWith<BinaryIvfc2NodeContainer>()
                    .TransformWith<NodeContainer2BinaryIvfc>()
                    .Stream;

                Assert.That(logger.IsEmpty, Is.True);
                Assert.That(expected.Compare(actual), Is.True);
            } finally {
                expected.Dispose();
                node.Dispose();
                Assert.That(DataStream.ActiveStreams, Is.EqualTo(0));
            }
        }

        public void CheckNode(NodeContainerInfo expected, Node actual)
        {
            Assert.That(
                actual.Name,
                Is.EqualTo(expected.Name),
                actual.Path);

            Assert.That(
                actual.Format.GetType().FullName,
                Is.EqualTo(expected.FormatType),
                actual.Path);

            if (actual.Stream != null) {
                Assert.That(
                    actual.Stream.Offset,
                    Is.EqualTo(expected.StreamOffset),
                    actual.Path);
                Assert.That(
                    actual.Stream.Length,
                    Is.EqualTo(expected.StreamLength),
                    actual.Path);
            }

            if (expected.CheckChildren) {
                Assert.That(
                    expected.Children?.Count ?? 0,
                    Is.EqualTo(actual.Children.Count),
                    actual.Path);

                for (int i = 0; i < actual.Children.Count; i++) {
                    CheckNode(expected.Children[i], actual.Children[i]);
                }
            }
        }
    }
}
