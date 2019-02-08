// NcsdTestInfo.cs
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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class NcsdTestInfo
    {
        public int SignatureLength {
            get;
            set;
        }

        public long Size {
            get;
            set;
        }

        public ulong MediaId {
            get;
            set;
        }

        public byte[] CryptType {
            get;
            set;
        }

        public uint[] PartitionsOffset {
            get;
            set;
        }

        public uint[] PartitionsSize {
            get;
            set;
        }

        public string[] AvailablePartitions {
            get;
            set;
        }
    }
}
