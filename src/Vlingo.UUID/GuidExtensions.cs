// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.UUID
{
    public static class GuidExtensions
    {
        /// <summary>
        /// Converts the Guid into the byte order as they appear in Guid string
        /// </summary>
        /// <remarks>
        /// Reference: https://docs.microsoft.com/en-us/dotnet/api/system.guid.tobytearray?view=netcore-3.1#remarks
        /// </remarks>
        /// <param name="guid"></param>
        /// <returns>Array of bytes</returns>
        public static byte[] ToActuallyOrderedBytes(this Guid guid)
        {
            var array = guid.ToByteArray();
            ChangeGuidByteOrders(array);
            return array;
        }

        /// <summary>
        /// Converts the byte array so that it appears in the Guid string in the same order
        /// Caution: mutates the actual array to save memory
        /// </summary>
        /// <remarks>
        /// Reference: https://docs.microsoft.com/en-us/dotnet/api/system.guid.tobytearray?view=netcore-3.1#remarks
        /// </remarks>
        /// <param name="array"></param>
        /// <returns>A Guid</returns>
        public static Guid ToGuidFromActuallyOrderedBytes(this byte[] array)
        {
            ChangeGuidByteOrders(array);
            return new Guid(array);
        }

        /// <summary>
        /// Swaps bytes in positions as:
        /// 0 <-> 3, 1 <-> 2, 4 <-> 5, 6 <-> 7
        /// </summary>
        /// <param name="array"></param>
        private static void ChangeGuidByteOrders(byte[] array)
        {
            var temp = array[0];
            array[0] = array[3];
            array[3] = temp;

            temp = array[1];
            array[1] = array[2];
            array[2] = temp;

            temp = array[4];
            array[4] = array[5];
            array[5] = temp;

            temp = array[6];
            array[6] = array[7];
            array[7] = temp;
        }
        
        private static bool GetBit(byte b) => (b & 1) != 0;
    }
}