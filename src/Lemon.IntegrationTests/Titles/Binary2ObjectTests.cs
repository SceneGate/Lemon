// Copyright (c) 2020 SceneGate

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
    using FluentAssertions;
    using NUnit.Framework;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public abstract class Binary2ObjectTests<T>
        where T : IFormat
    {
        int initialStreams;
        BinaryFormat original;
        T expected;
        IConverter<BinaryFormat, T> objectConverter;
        IConverter<T, BinaryFormat> binaryConverter;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            expected = GetObject();
            objectConverter = GetToObjectConverter();
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
            // By opening and disposing in each we prevent other tests failing
            // because the file is still open.
            initialStreams = DataStream.ActiveStreams;
            original = GetBinary();
        }

        [Test]
        public void TransformToObject()
        {
            int numStreams = DataStream.ActiveStreams;

            T actual = objectConverter.Convert(original);
            AssertObjects(actual, expected);

            // Check everything is virtual node
            Assert.That(DataStream.ActiveStreams, Is.EqualTo(numStreams));
        }

        [Test]
        public void TransformBothWays()
        {
            if (binaryConverter == null) {
                Assert.Ignore();
            }

            T actual = objectConverter.Convert(original);
            using var actualBinary = binaryConverter.Convert(actual);

            Assert.That(original.Stream.Compare(actualBinary.Stream), Is.True, "Binaries are not identical");
        }

        protected virtual void AssertObjects(T actual, T expected)
        {
            actual.Should().BeEquivalentTo(expected);
        }

        protected abstract BinaryFormat GetBinary();

        protected abstract T GetObject();

        protected abstract IConverter<BinaryFormat, T> GetToObjectConverter();

        protected abstract IConverter<T, BinaryFormat> GetToBinaryConverter();
    }
}
