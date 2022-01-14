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
namespace SceneGate.Lemon.IntegrationTests.Titles
{
    using System.IO;
    using System.Text.RegularExpressions;
    using FluentAssertions;
    using FluentAssertions.Equivalency;
    using NUnit.Framework;
    using SceneGate.Lemon.Titles;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    [TestFixtureSource(typeof(TestData), nameof(TestData.TmdParams))]
    public class Binary2TitleMetadataTests : Binary2ObjectTests<TitleMetadata>
    {
        readonly string yamlPath;
        readonly string binaryPath;
        readonly int offset;
        readonly int size;

        public Binary2TitleMetadataTests(string yamlPath, string binaryPath, int offset, int size)
        {
            this.yamlPath = yamlPath;
            this.binaryPath = binaryPath;
            this.offset = offset;
            this.size = size;

            TestDataBase.IgnoreIfFileDoesNotExist(binaryPath);
            TestDataBase.IgnoreIfFileDoesNotExist(yamlPath);
        }

        protected override void AssertObjects(TitleMetadata actual, TitleMetadata expected)
        {
            actual.Should().BeEquivalentTo(
                expected,
                opts => opts.Excluding(t => t.Signature)
                    .Excluding(t => t.Hash)
                    .Excluding((IMemberInfo i) => i.RemovingIndexes() == "InfoRecords.Hash")
                    .Excluding((IMemberInfo i) => i.RemovingIndexes() == "Chunks.Hash"));
        }

        protected override BinaryFormat GetBinary()
        {
            TestContext.WriteLine($"{nameof(Binary2TitleMetadataTests)}: {Path.GetFileName(binaryPath)}");
            var stream = DataStreamFactory.FromFile(binaryPath, FileOpenMode.Read, offset, size);
            return new BinaryFormat(stream);
        }

        protected override TitleMetadata GetObject()
        {
            string yaml = File.ReadAllText(yamlPath);
            return new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build()
                .Deserialize<TitleMetadata>(yaml);
        }

        protected override IConverter<BinaryFormat, TitleMetadata> GetToObjectConverter()
        {
            return new Binary2TitleMetadata();
        }

        protected override IConverter<TitleMetadata, BinaryFormat> GetToBinaryConverter()
        {
            return new TitleMetadata2Binary();
        }
    }
}
