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
    using NUnit.Framework;
    using SceneGate.Lemon.Logging;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    public abstract class Binary2ContainerTests
    {
        readonly CaptureLogger logger;
        int initialStreams;
        BinaryFormat original;
        NodeContainerInfo containerInfo;
        IConverter<BinaryFormat, NodeContainerFormat> containerConverter;
        IConverter<NodeContainerFormat, BinaryFormat> binaryConverter;

        protected Binary2ContainerTests()
        {
            logger = new CaptureLogger();
            LogProvider.SetCurrentLogProvider(logger);
        }

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            containerInfo = GetContainerInfo();
            containerConverter = GetToContainerConverter();
            binaryConverter = GetToBinaryConverter();
        }

        [TearDown]
        public void TearDown()
        {
            original?.Dispose();

            // Make sure we didn't leave anything without dispose.
            Assert.That(DataStream.ActiveStreams, Is.EqualTo(initialStreams));
        }

        [SetUp]
        public void SetUp()
        {
            logger.Clear();

            // By opening and disposing in each we prevent other tests failing
            // because the file is still open.
            initialStreams = DataStream.ActiveStreams;
            original = GetBinary();
        }

        [Test]
        public void TransformToContainer()
        {
            // Check nodes are expected
            using var nodes = containerConverter.Convert(original);
            CheckNode(containerInfo, nodes.Root);

            // Check everything is virtual node (only the binary stream)
            Assert.That(DataStream.ActiveStreams, Is.EqualTo(initialStreams + 1));
            Assert.That(logger.IsEmpty, Is.True);
        }

        [Test]
        public void TransformBothWays()
        {
            if (binaryConverter == null) {
                Assert.Ignore();
            }

            using var nodes = containerConverter.Convert(original);
            using var actualBinary = binaryConverter.Convert(nodes);

            Assert.That(original.Stream.Compare(actualBinary.Stream), Is.True, "Streams are not identical");
        }

        protected abstract BinaryFormat GetBinary();

        protected abstract NodeContainerInfo GetContainerInfo();

        protected abstract IConverter<BinaryFormat, NodeContainerFormat> GetToContainerConverter();

        protected abstract IConverter<NodeContainerFormat, BinaryFormat> GetToBinaryConverter();

        private static void CheckNode(NodeContainerInfo info, Node node)
        {
            TestContext.WriteLine(node.Path);
            Assert.That(node.Name, Is.EqualTo(info.Name));
            Assert.That(node.Format?.GetType().FullName, Is.EqualTo(info.FormatType));

            if (info.Tags != null) {
                // YAML deserializer always gets the value as a string
                foreach (var entry in info.Tags) {
                    Assert.That(node.Tags.ContainsKey(entry.Key), Is.True);
                    Assert.That(node.Tags[entry.Key].ToString(), Is.EqualTo(entry.Value));
                }
            }

            if (info.StreamLength > 0) {
                Assert.That(node.Stream.Offset, Is.EqualTo(info.StreamOffset));
                Assert.That(node.Stream.Length, Is.EqualTo(info.StreamLength));
            }

            if (info.CheckChildren) {
                int expectedCount = info.Children?.Count ?? 0;
                Assert.That(expectedCount, Is.EqualTo(node.Children.Count));

                for (int i = 0; i < expectedCount; i++) {
                    CheckNode(info.Children[i], node.Children[i]);
                }
            }
        }
    }
}
