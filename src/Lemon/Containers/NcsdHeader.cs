// NcsdHeader.cs
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

    public class NcsdHeader
    {
        public static string MagicId {
            get { return "NCSD"; }
        }

        public static int Unit {
            get { return 0x200; }
        }

        public byte[] Signature {
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

        public NcsdFileSystemType[] FileSystemType {
            get;
            set;
        }

        public byte[] CryptType {
            get;
            set;
        }
    }
}
