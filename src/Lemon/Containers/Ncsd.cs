// Ncsd.cs
//
// Author:
//      Benito Palacios Sánchez (aka pleonex) <benito356@gmail.com>
//
// Copyright (c) 2019 Benito Palacios Sánchez
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
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;

    /// <summary>
    /// Nintendo CTR SD.
    /// This is the format for the CCI, NAND and CSU specializations.
    /// It can contains upto 8 containers / nodes.
    /// </summary>
    public class Ncsd : NodeContainerFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ncsd"/> class.
        /// </summary>
        public Ncsd()
        {
            Header = new NcsdHeader();
        }

        /// <summary>
        /// Gets the maximum number of partitions on a Ncsd format.
        /// </summary>
        /// <value>The maximum number of partitions.</value>
        public static int NumPartitions {
            get { return 8; }
        }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>The header.</value>
        public NcsdHeader Header {
            get;
            set;
        }
    }
}
