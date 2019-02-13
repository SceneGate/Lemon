// ContainerManager.cs
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
    using System;
    using Lemon.Containers.Converters;
    using Lemon.Containers.Formats;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;

    /// <summary>
    /// Manage containers for 3DS formats.
    /// </summary>
    public static class ContainerManager
    {
        /// <summary>
        /// Unpack a node with a binary format representing a .3ds file.
        /// </summary>
        /// <param name="gameNode">Node to unpack with .3ds format.</param>
        public static void Unpack3DSNode(Node gameNode)
        {
            if (!(gameNode.Format is BinaryFormat))
                throw new ArgumentException("Invalid node format", nameof(gameNode));

            // First transform the binary format into NCSD (CCI/CSU/NAND).
            gameNode.Transform<Ncsd>();

            // All the partition have NCCH format.
            foreach (var partition in gameNode.Children) {
                partition.Transform<Ncch>();
            }

            // Unpack each partition until we have the actual file system.
            gameNode.Children["program"].Children["rom"]
                .Transform<BinaryIvfc2NodeContainer, BinaryFormat, NodeContainerFormat>();
        }
    }
}
