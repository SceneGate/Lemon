// TestData.cs
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
    using System.Collections;
    using System.IO;
    using NUnit.Framework;

    public class TestData
    {
        public static string ResourcePath {
            get {
                var programDir = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(
                    programDir,
                    "..", "..", "..", "..", "..",
                    "test_resources",
                    "containers");
            }
        }

        public static IEnumerable CciParams {
            get {
                yield return new TestFixtureData("aofm");
            }
        }
    }
}
