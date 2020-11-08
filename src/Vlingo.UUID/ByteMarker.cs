// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.UUID
{
    internal static class ByteMarker
    {
        private const int VariantIndexPosition = 8;
        private const int VariantMask = 0x3f;
        private const int VariantBits = 0x80;
        private const int VersionIndexPosition = 6;
        private const int VersionMask = 0x0f;

        /// <summary>
        /// Sets the first two bits of the 8th (0-based index) byte to binary '10'
        /// </summary>
        /// <param name="array">The array to modify in place</param>
        /// <returns>The modified <paramref name="array"/></returns>
        public static byte[] AddVariantMarker(this byte[] array)
        {
            array[VariantIndexPosition] &= VariantMask;
            array[VariantIndexPosition] |= VariantBits;
            return array;
        }

        /// <summary>
        /// Sets the 4 most significant bits of 7th (0-based index) byte to the version number
        /// </summary>
        /// <param name="array">The array to modify in place</param>
        /// <param name="version">The UUID version</param>
        /// <returns>The modified <paramref name="array"/></returns>
        public static byte[] AddVersionMarker(this byte[] array, UUIDVersion version)
        {
            var versionBits = (byte)version;
            array[VersionIndexPosition] &= VersionMask;
            array[VersionIndexPosition] |= versionBits;
            return array;
        }

        public static byte[] TrimTo16Bytes(this byte[] array)
        {
            var result = new byte[16];
            Array.Copy(array, result, 16);
            return result;
        }
    }
}
