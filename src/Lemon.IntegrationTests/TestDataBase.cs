// TestDataBase.cs
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
namespace Lemon.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public abstract class TestDataBase
    {
        protected static string RootFromOutputPath {
            get {
                string programDir = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(
                    programDir,
                    "..", "..", "..", "..", "..");
            }
        }

        protected static IEnumerable<string> ReadTestListFile(string filePath)
        {
            return File.ReadAllLines(filePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim())
                .Where(line => !line.StartsWith("#"));
        }
    }
}
