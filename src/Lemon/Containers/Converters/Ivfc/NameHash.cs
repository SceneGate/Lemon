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
    /// Generate hashes for name and directory lookup.
    /// </summary>
    internal static class NameHash
    {
        /// <summary>
        /// Calculates the size of tokens for the hash table.
        /// </summary>
        /// <param name="numEntries">The number of entries to hash.</param>
        /// <returns>The number of tokens of the hash table.</returns>
        public static int CalculateTableLength(int numEntries)
        {
            if (numEntries < 3) {
                return 3;
            } else if (numEntries < 19) {
                // This will return 15 that it is not prime, but whatever Nin.
                return numEntries | 1;
            }

            // Find the first prime number with a simple algorithm.
            // Really lazy algorithm eh, Nin.
            bool IsMoreOrLessPrime(int x) =>
                (x % 2 != 0) && (x % 3 != 0) && (x % 5 != 0) && (x % 7 != 0) &&
                (x % 11 != 0) && (x % 13 != 0) && (x % 17 != 0);

            int count = numEntries;
            while (!IsMoreOrLessPrime(count))
            {
                count++;
            }

            return count;
        }

        /// <summary>
        /// Calculates the hash for an entry.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <param name="name">The name of the entry.</param>
        /// <returns>The calculated hash.</returns>
        public static uint CalculateHash(uint seed, byte[] name)
        {
            uint hash = seed ^ 123456789;
            for (int i = 0; i < name.Length; i += 2) {
                hash = (hash >> 5) | (hash << 27);
                hash ^= (ushort)(name[i] | (name[i + 1] << 8));
            }

            return hash;
        }
    }
}