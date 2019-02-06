// FileSystemType.cs
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
namespace Lemon.Containers
{
    /// <summary>
    /// File system type.
    /// </summary>
    public enum FileSystemType
    {
        /// <summary>
        /// Regular partition.
        /// </summary>
        None = 0,

        /// <summary>
        /// DS(i) firmware partition.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Firmware partition.
        /// </summary>
        Firmware = 3,

        /// <summary>
        /// GBA Firmware partition.
        /// </summary>
        AgbFirmware = 4,
    }
}
