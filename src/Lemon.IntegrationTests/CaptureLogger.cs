// Copyright (c) 2019 SceneGate Team
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
    using System.Linq;
    using Lemon.Logging;

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
