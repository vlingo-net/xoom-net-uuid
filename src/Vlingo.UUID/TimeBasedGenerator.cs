// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Threading;

namespace Vlingo.UUID
{
    /// <summary>
    /// Time based UUID (Version-1) generator according to RFC4122.
    /// </summary>
    public sealed class TimeBasedGenerator
    {
        private static readonly RandomNumberGenerator RandomGenerator = new RNGCryptoServiceProvider();
        private static readonly DateTimeOffset ClockStart = new DateTimeOffset(1582, 10, 15, 0, 0, 0, TimeSpan.Zero);

        private readonly byte[] _macAddressBytes;
        private readonly ReaderWriterLock _rwLock;
        private readonly object _mutex;

        private DateTimeOffset _lastClockSyncedAt;
        private byte[] _currentClockSequenceBytes;

        /// <summary>
        /// Creates an instance of RFC4122 time based UUID generator, using the IEEE 802 MAC address (6 bytes) provided as node. 
        /// </summary>
        /// <param name="macAddressBytes">6 bytes IEEE 802 MAC address to use as node</param>
        public TimeBasedGenerator(byte[] macAddressBytes)
        {
            _macAddressBytes = macAddressBytes;
            _lastClockSyncedAt = DateTimeOffset.UtcNow;
            _rwLock = new ReaderWriterLock();
            _mutex = new object();
            _currentClockSequenceBytes = GetRandomBytes(2, RandomGenerator);
        }

        /// <summary>
        /// Creates an instance of RFC4122 time based UUID generator, 
        /// using the first IEEE 802 MAC address as node from available network workinterfaces on the machine. 
        /// If none is available, a pseudo-random number generator is used.
        /// </summary>
        public TimeBasedGenerator()
            : this(GetIEEE802MACAddressBytes() ?? GetRandomBytes(6, RandomGenerator))
        {
        }

        /// <summary>
        /// Generates RFC4122 time based UUID based on the <paramref name="dateTimeOffset"/> provided.
        /// </summary>
        /// <param name="dateTimeOffset">The DateTimeOffset to generate UUID on.</param>
        /// <returns></returns>
        public Guid GenerateGuid(DateTimeOffset dateTimeOffset)
            => GenerateGuid(dateTimeOffset, GetClockSequenceData(dateTimeOffset.ToUniversalTime().Ticks), _macAddressBytes);

        /// <summary>
        /// Generates RFC4122 time based UUID based on the <paramref name="dateTime"/> provided. 
        /// The <paramref name="dateTime"/> is converted into UTC DateTimeOffset before use.
        /// </summary>
        /// <param name="dateTime">The DateTime to generate UUID on.</param>
        /// <returns></returns>
        public Guid GenerateGuid(DateTime dateTime)
            => GenerateGuid(new DateTimeOffset(dateTime.ToUniversalTime(), TimeSpan.Zero));

        /// <summary>
        /// Generates a RFC4122 time based UUID in the give <paramref name="mode"/>.
        /// </summary>
        /// <param name="mode">Use <c>UUIDGenerationMode.FasterGeneration</c> for faster UUID generation, without synchronizing the system clock. 
        /// Use <c>UUIDGenerationMode.WithUniquenessGuarantee</c> to synchronize system clock. Later approach may be slower than the former one.</param>
        /// <returns></returns>
        public Guid GenerateGuid(GuidGenerationMode mode)
        {
            if (mode == GuidGenerationMode.FasterGeneration)
            {
                var clockSequenceData = ReadClockSequenceBytes();
                return GenerateGuid(DateTimeOffset.UtcNow, clockSequenceData, _macAddressBytes);
            }

            var now = DateTimeOffset.UtcNow;
            if (now <= _lastClockSyncedAt)
            {
                lock (_mutex)
                {
                    if (now <= _lastClockSyncedAt)
                    {
                        UpdateClockSequenceBytes();
                        _lastClockSyncedAt = now;
                    }
                }
            }
            return GenerateGuid(now, ReadClockSequenceBytes(), _macAddressBytes);
        }

        /// <summary>
        /// Generates a RFC4122 time based UUID in <c>UUIDGenerationMode.FasterGeneration</c> mode.
        /// </summary>
        /// <returns></returns>
        public Guid GenerateGuid() => GenerateGuid(GuidGenerationMode.FasterGeneration);

        private static Guid GenerateGuid(DateTimeOffset dateTime, byte[] clockSequenceData, byte[] macAddressBytes)
        {
            if(macAddressBytes == null)
            {
                throw new ArgumentNullException(nameof(macAddressBytes));
            }

            if(macAddressBytes.Length != 6)
            {
                throw new ArgumentException($"{nameof(macAddressBytes)} must have 6 bytes.");
            }

            if(clockSequenceData == null)
            {
                throw new ArgumentNullException(nameof(clockSequenceData));
            }

            if(clockSequenceData.Length != 2)
            {
                throw new ArgumentException($"{nameof(clockSequenceData)} must have 2 bytes.");
            }

            var ticksSinceStart = (dateTime - ClockStart).Ticks;
            var timestampBytes = BitConverter.GetBytes(ticksSinceStart);

            var data = new byte[16];

            /*
            - Set the time_low field equal to the least significant 32 bits (bits zero through 31) of the timestamp in the same order of significance.
            - Set the time_mid field equal to bits 32 through 47 from the timestamp in the same order of significance.
            - Set the 12 least significant bits (bits zero through 11) of the time_hi_and_version field equal to bits 48 through 59 from the timestamp in the same order of significance. 
             */
            Array.Copy(timestampBytes, 0, data, 0, Math.Min(timestampBytes.Length, 8));


            /*
            - Set the clock_seq_low field to the eight least significant bits (bits zero through 7) of the clock sequence in the same order of significance.
            - Set the 6 least significant bits (bits zero through 5) of the clock_seq_hi_and_reserved field to the 6 most significant bits (bits 8 through 13) of the clock sequence in the same order of significance.
             */
            Array.Copy(clockSequenceData, 0, data, 8, clockSequenceData.Length);


            /*
            - Set the node field to the 48-bit IEEE address in the same order of significance as the address.
             */
            Array.Copy(macAddressBytes, 0, data, 10, macAddressBytes.Length);


            data.AddVariantMarker().AddVersionMarker(UUIDVersion.TimeBased);

            return new Guid(data);
        }

        private static byte[] GetClockSequenceData(long ticks)
        {
            var data = BitConverter.GetBytes(ticks);
            switch (data.Length)
            {
                case 0: return new byte[] { 0, 0 };
                case 1: return new byte[] { 0, data[0] };
                default: return new[] { data[0], data[1] };
            }
        }

        private byte[] ReadClockSequenceBytes()
        {
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return new[] { _currentClockSequenceBytes[0], _currentClockSequenceBytes[1] };
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        private void UpdateClockSequenceBytes()
        {
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                _currentClockSequenceBytes = GetRandomBytes(2, RandomGenerator);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        private static byte[] GetRandomBytes(int length, RandomNumberGenerator randomGenerator)
        {
            var data = new byte[length];
            randomGenerator.GetBytes(data);
            return data;
        }

        // ReSharper disable once InconsistentNaming
        private static byte[]? GetIEEE802MACAddressBytes()
        {
            try
            {
                foreach (var @interface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (@interface.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    {
                        continue;
                    }

                    var physicalAddress = @interface.GetPhysicalAddress();

                    if (string.IsNullOrEmpty(physicalAddress.ToString()))
                    {
                        continue;
                    }

                    return physicalAddress.GetAddressBytes();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }
    }
}
