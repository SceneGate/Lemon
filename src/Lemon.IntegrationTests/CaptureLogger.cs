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
namespace SceneGate.Lemon.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SceneGate.Lemon.Logging;

    public class CaptureLogger : ILogProvider
    {
        readonly Dictionary<LogLevel, List<string>> logs;

        public CaptureLogger()
        {
            logs = new Dictionary<LogLevel, List<string>>();

            IsEmpty = true;
            foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>()) {
                logs[level] = new List<string>();
            }
        }

        public bool IsEmpty {
            get;
            private set;
        }

        public IReadOnlyList<string> TraceLogs => logs[LogLevel.Trace];

        public IReadOnlyList<string> DebugLogs => logs[LogLevel.Debug];

        public IReadOnlyList<string> InfoLogs => logs[LogLevel.Info];

        public IReadOnlyList<string> WarningLogs => logs[LogLevel.Warn];

        public IReadOnlyList<string> ErrorLogs => logs[LogLevel.Error];

        public IReadOnlyList<string> FatalLogs => logs[LogLevel.Fatal];

        public void Clear()
        {
            foreach (LogLevel level in Enum.GetValues(typeof(LogLevel))) {
                logs[level].Clear();
            }
        }

        public Logger GetLogger(string name)
        {
            return Logger;
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }

        bool Logger(LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters)
        {
            if (messageFunc == null) {
                return true;
            }

            IsEmpty = false;

            string msg = messageFunc();
            logs[logLevel].Add(msg);
            Console.WriteLine($"{logLevel}: {msg}");

            return true;
        }
    }
}
