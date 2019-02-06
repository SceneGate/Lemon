// CciConverterTests.cs
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

    [TestFixtureSource(typeof(TestData), "CciParams")]
    public class CciConverterTests
    {
        readonly Ncsd actual;
        readonly CciTestInfo expected;

        public CciConverterTests(string name)
        {
            string cciPath = Path.Combine(TestData.ResourcePath, name + ".3ds");
            string jsonPath = Path.Combine(TestData.ResourcePath, name + ".json");

            if (!File.Exists(cciPath))
                Assert.Ignore("CCI file doesn't exist");
            if (!File.Exists(jsonPath))
                Assert.Ignore("JSON file doesn't exist");

            var cciNode = NodeFactory.FromFile(cciPath);
            Assert.That(() => cciNode.Transform<Ncsd>(), Throws.Nothing);
            actual = cciNode.GetFormatAs<Ncsd>();

            string json = File.ReadAllText(jsonPath);
            expected = JsonConvert.DeserializeObject<CciTestInfo>(json);
        }

        [Test]
        public void ValidateCciHeaderValues()
        {
            var header = actual.Header;
            Assert.That(header.Signature, Has.Length.EqualTo(expected.SignatureLength));
            Assert.That(header.Size, Is.EqualTo(expected.Size));
            Assert.That(header.MediaId, Is.EqualTo(expected.MediaId));
            Assert.That(header.CryptType, Is.EquivalentTo(expected.CryptType));
        }
    }
}
