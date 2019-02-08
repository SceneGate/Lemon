// NcsdConverterTests.cs
//
// Copyright (c) 2019 SceneGate
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
namespace Lemon.IntegrationTests.Containers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Lemon.Containers;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    [TestFixtureSource(typeof(TestData), nameof(TestData.NcsdParams))]
    public class NcsdConverterTests
    {
        readonly string ncsdPath;
        readonly string jsonPath;

        Node actualNode;
        Ncsd actual;
        NcsdTestInfo expected;

        public NcsdConverterTests(string ncsdPath, string jsonPath)
        {
            this.ncsdPath = ncsdPath;
            this.jsonPath = jsonPath;
        }

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            if (!File.Exists(ncsdPath))
                Assert.Ignore($"NCSD file doesn't exist: {ncsdPath}");
            if (!File.Exists(jsonPath))
                Assert.Ignore($"JSON file doesn't exist: {jsonPath}");

            actualNode = NodeFactory.FromFile(ncsdPath);
            Assert.That(() => actualNode.Transform<Ncsd>(), Throws.Nothing);
            actual = actualNode.GetFormatAs<Ncsd>();

            string json = File.ReadAllText(jsonPath);
            expected = JsonConvert.DeserializeObject<NcsdTestInfo>(json);
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
