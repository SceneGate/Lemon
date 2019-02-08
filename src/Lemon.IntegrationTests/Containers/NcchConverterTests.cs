// NcchConverterTests.cs
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
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    [TestFixtureSource(typeof(TestData), nameof(TestData.NcchParams))]
    public class NcchConverterTests
    {
        readonly string jsonPath;
        readonly string binaryPath;
        readonly int offset;
        readonly int size;

        Node actualNode;
        Ncch actual;
        // NcsdTestInfo expected;

        public NcchConverterTests(string jsonPath, string binaryPath, int offset, int size)
        {
            this.jsonPath = jsonPath;
            this.binaryPath = binaryPath;
            this.offset = offset;
            this.size = size;
        }

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            if (!File.Exists(binaryPath))
                Assert.Ignore("Binary file doesn't exist");
            // if (!File.Exists(jsonPath))
                // Assert.Ignore("JSON file doesn't exist");

            using (var stream = new DataStream(binaryPath, offset, size, FileOpenMode.Read)) {
                actualNode = new Node("actual", new BinaryFormat(stream));
            }

            Assert.That(() => actualNode.Transform<Ncch>(), Throws.Nothing);
            actual = actualNode.GetFormatAs<Ncch>();

            // string json = File.ReadAllText(jsonPath);
            // expected = JsonConvert.DeserializeObject<NcsdTestInfo>(json);
        }

        [OneTimeTearDown]
        public void TearDownFixture()
        {
            actualNode.Dispose();
            Assert.That(DataStream.ActiveStreams, Is.EqualTo(0));
        }

        [Test]
        public void ValidateHeader()
        {
            Assert.Pass();
        }
    }
}
