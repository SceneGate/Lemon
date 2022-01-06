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
    using SceneGate.Lemon.Containers.Converters;
    using SceneGate.Lemon.Containers.Formats;
    using SceneGate.Lemon.Logging;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    [TestFixtureSource(typeof(TestData), nameof(TestData.NcchParams))]
    public class NcchConverterTests
    {
        readonly CaptureLogger logger;
        readonly string yamlPath;
        readonly string binaryPath;
        readonly int offset;
        readonly int size;

        int initialStreams;
        BinaryFormat original;
        IConverter<BinaryFormat, Ncch> containerConverter;
        IConverter<Ncch, BinaryFormat> binaryConverter;

        NcchTestInfo expected;

        public NcchConverterTests(string yamlPath, string binaryPath, int offset, int size)
        {
            logger = new CaptureLogger();
            LogProvider.SetCurrentLogProvider(logger);

            this.yamlPath = yamlPath;
            this.binaryPath = binaryPath;
            this.offset = offset;
            this.size = size;

            TestDataBase.IgnoreIfFileDoesNotExist(binaryPath);
            TestDataBase.IgnoreIfFileDoesNotExist(yamlPath);
        }

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            containerConverter = new Binary2Ncch();
            binaryConverter = new Ncch2Binary();

            string yaml = File.ReadAllText(yamlPath);
            expected = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build()
                .Deserialize<NcchTestInfo>(yaml);
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
            original = GetBinary(offset, size);
        }

        [Test]
        public void TransformThreeWays()
        {
            if (binaryConverter == null) {
                Assert.Ignore();
            }

            // Convert the binary to NCCH, and check the original header and regions are expected
            using var actual = containerConverter.Convert(original);
            ValidateHeader(actual);
            ValidateRegions(actual, true);

            // Convert the new NCCH to binary (and vice-versa), and check the header and regions lengths are expected
            using var generatedBinary = binaryConverter.Convert(actual);
            using var generatedNcch = containerConverter.Convert(generatedBinary);
            ValidateHeader(generatedNcch);
            ValidateRegions(generatedNcch, false);
        }

        protected BinaryFormat GetBinary(int offset, int size)
        {
            TestContext.WriteLine(Path.GetFileName(binaryPath));
            var stream = DataStreamFactory.FromFile(binaryPath, FileOpenMode.Read, offset, size);
            return new BinaryFormat(stream);
        }

        protected void ValidateHeader(Ncch actual)
        {
            var header = actual.Header;
            Assert.That(header.Signature, Has.Length.EqualTo(expected.SignatureLength));
        }

        protected void ValidateRegions(Ncch actual, bool checkOffset)
        {
            Assert.That(
                actual.Root.Children,
                Has.Count.EqualTo(expected.AvailableRegions.Length));

            for (int i = 0; i < actual.Root.Children.Count; i++) {
                var child = actual.Root.Children[i];
                Assert.That(child.Name, Is.EqualTo(expected.AvailableRegions[i]));
                if (checkOffset) {
                    Assert.That(child.Stream.Offset, Is.EqualTo(offset + expected.RegionsOffset[i]));
                }

                Assert.That(child.Stream.Length, Is.EqualTo(expected.RegionsSize[i]));
            }
        }
    }
}
