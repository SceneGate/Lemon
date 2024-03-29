﻿// Copyright (c) 2019 SceneGate

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
namespace SceneGate.Lemon.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;

    public static class TestDataBase
    {
        public static string RootFromOutputPath {
            get {
                string envVar = Environment.GetEnvironmentVariable("YARHL_TEST_DIR");
                if (!string.IsNullOrEmpty(envVar))
                    return envVar;

                string programDir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(
                    programDir,
                    "..", // framework
                    "..", // configuration
                    "..", // bin
                    "Resources");
                return Path.GetFullPath(path);
            }
        }

        public static void IgnoreIfFileDoesNotExist(string file)
        {
            if (!File.Exists(file)) {
                TestContext.Progress.WriteLine(
                    "[{0}] Missing resource file: {1}",
                    TestContext.CurrentContext.Test.ClassName,
                    file);
                Assert.Ignore();
            }
        }

        public static IEnumerable<string> ReadTestListFile(string filePath)
        {
            if (!File.Exists(filePath)) {
                return Array.Empty<string>();
            }

            return File.ReadAllLines(filePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim())
                .Where(line => !line.StartsWith('#'));
        }
    }
}
