// NcsdHeader.cs
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
namespace Lemon.Containers.Formats
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// NCCH header information.
    /// </summary>
    public class NcsdHeader
    {
        /// <summary>
        /// Gets the magic identifier of the format.
        /// </summary>
        /// <value>The magic ID of the format.</value>
        public static string MagicId {
            get { return "NCSD"; }
        }

        /// <summary>
        /// Gets the equivalent in bytes of one unit for the header values.
        /// </summary>
        /// <value>One unit in bytes.</value>
        public static int Unit {
            get { return 0x200; }
        }

        /// <summary>
        /// Gets or sets the RSA-2048 signature of the NCCH header using SHA-256.
        /// </summary>
        /// <value>The signature of the header.</value>
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays",
            Justification="Model or DTO.")]
        public byte[] Signature {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the format.
        /// </summary>
        /// <value>The size of the format in bytes.</value>
        public long Size {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the media ID.
        /// </summary>
        /// <value>The media ID.</value>
        public ulong MediaId {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the firmware for each partition.
        /// </summary>
        /// <value>The type of the partitions firmware.</value>
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays",
            Justification="Model or DTO.")]
        public FirmwareType[] FirmwaresType {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the encryption type of each partition.
        /// </summary>
        /// <value>The encryption type of each partition.</value>
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays",
            Justification="Model or DTO.")]
        public byte[] CryptType {
            get;
            set;
        }
    }
}
