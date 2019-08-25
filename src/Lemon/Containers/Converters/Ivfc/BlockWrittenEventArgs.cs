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
namespace Lemon.Containers.Converters.Ivfc
{
    /// <summary>
    /// Event argument for written blocks of data.
    /// </summary>
    internal class BlockWrittenEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="BlockWrittenEventArgs" /> class.
        /// </summary>
        /// <param name="hash">The hash to pass in the argument.</param>
        public BlockWrittenEventArgs(byte[] hash)
        {
            Hash = hash;
        }

        /// <summary>
        /// Gets the hash of the written block.
        /// </summary>
        public byte[] Hash { get; }
    }
}
