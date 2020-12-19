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
    using System.Collections;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;

    public static class TestData
    {
        public static IEnumerable TmdParams {
            get => GetSubstreamAndInfoCollection("tmd.txt");
        }

        public static string ResourceDirectory {
            get => Path.Combine(TestDataBase.RootFromOutputPath, "titles");
        }

        private static IEnumerable GetSubstreamAndInfoCollection(string listName)
        {
            return TestDataBase.ReadTestListFile(Path.Combine(ResourceDirectory, listName))
                    .Select(line => line.Split(','))
                    .Select(data => new TestFixtureData(
                        Path.Combine(ResourceDirectory, data[0]),
                        Path.Combine(ResourceDirectory, data[1]),
                        int.Parse(data[2]),
                        int.Parse(data[3])));
        }
    }
}
