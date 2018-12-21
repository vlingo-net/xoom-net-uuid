// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Xunit;

namespace Vlingo.UUID.Tests
{
    public class NameBasedGeneratorTests
    {
        [Theory]
        [InlineData(HashType.MD5)]
        [InlineData(HashType.SHA1)]
        public void GeneratedUUID_ShouldHaveProperVersion(HashType hashType)
        {
            var name = Guid.NewGuid().ToString();
            var nameSpace = Guid.NewGuid();

            var expectedVersion = hashType == HashType.MD5 ? 0x30 : 0x50;

            using (var generator = new NameBasedGenerator(hashType))
            {
                var guid = generator.GenerateGuid(nameSpace, name);

                var array = guid.ToByteArray();

                Assert.Equal(expectedVersion, array[7] & 0xf0);
            }
        }

        [Theory]
        [InlineData(HashType.MD5, UUIDNameSpace.None)]
        [InlineData(HashType.MD5, UUIDNameSpace.DNS)]
        [InlineData(HashType.MD5, UUIDNameSpace.OID)]
        [InlineData(HashType.MD5, UUIDNameSpace.URL)]
        [InlineData(HashType.MD5, UUIDNameSpace.X500)]
        [InlineData(HashType.SHA1, UUIDNameSpace.None)]
        [InlineData(HashType.SHA1, UUIDNameSpace.DNS)]
        [InlineData(HashType.SHA1, UUIDNameSpace.OID)]
        [InlineData(HashType.SHA1, UUIDNameSpace.URL)]
        [InlineData(HashType.SHA1, UUIDNameSpace.X500)]
        public void UUIDGenerated_InSameNameAndNameSpace_InDifferentTimes_ShouldBeSame(HashType hashType, UUIDNameSpace nameSpace)
        {
            var name = Guid.NewGuid().ToString();
            using(var generator = new NameBasedGenerator(hashType))
            {
                var first = generator.GenerateGuid(nameSpace, name);
                var second = generator.GenerateGuid(nameSpace, name);

                Assert.Equal(first, second);
            }
        }

        [Theory]
        [InlineData(HashType.MD5, UUIDNameSpace.None)]
        [InlineData(HashType.MD5, UUIDNameSpace.DNS)]
        [InlineData(HashType.MD5, UUIDNameSpace.OID)]
        [InlineData(HashType.MD5, UUIDNameSpace.URL)]
        [InlineData(HashType.MD5, UUIDNameSpace.X500)]
        [InlineData(HashType.SHA1, UUIDNameSpace.None)]
        [InlineData(HashType.SHA1, UUIDNameSpace.DNS)]
        [InlineData(HashType.SHA1, UUIDNameSpace.OID)]
        [InlineData(HashType.SHA1, UUIDNameSpace.URL)]
        [InlineData(HashType.SHA1, UUIDNameSpace.X500)]
        public void UUIDGenerated_InSameNameSpace_WithDifferentNames_ShouldBeDifferent(HashType hashType, UUIDNameSpace nameSpace)
        {
            var firstName = Guid.NewGuid().ToString();
            var secondName = Guid.NewGuid().ToString();
            using (var generator = new NameBasedGenerator(hashType))
            {
                var first = generator.GenerateGuid(nameSpace, firstName);
                var second = generator.GenerateGuid(nameSpace, secondName);

                Assert.NotEqual(first, second);
            }
        }

        [Theory]
        [InlineData(HashType.MD5, UUIDNameSpace.DNS, UUIDNameSpace.None)]
        [InlineData(HashType.MD5, UUIDNameSpace.None, UUIDNameSpace.OID)]
        [InlineData(HashType.MD5, UUIDNameSpace.OID, UUIDNameSpace.URL)]
        [InlineData(HashType.MD5, UUIDNameSpace.URL, UUIDNameSpace.X500)]
        [InlineData(HashType.SHA1, UUIDNameSpace.DNS, UUIDNameSpace.None)]
        [InlineData(HashType.SHA1, UUIDNameSpace.None, UUIDNameSpace.OID)]
        [InlineData(HashType.SHA1, UUIDNameSpace.OID, UUIDNameSpace.URL)]
        [InlineData(HashType.SHA1, UUIDNameSpace.URL, UUIDNameSpace.X500)]
        public void UUIDGenerated_InWithSameName_InDifferentStandardNameSpace_ShouldBeDifferent(
            HashType hashType, 
            UUIDNameSpace firstNs, 
            UUIDNameSpace secondNs)
        {
            var name = Guid.NewGuid().ToString();
            using (var generator = new NameBasedGenerator(hashType))
            {
                var first = generator.GenerateGuid(firstNs, name);
                var second = generator.GenerateGuid(secondNs, name);

                Assert.NotEqual(first, second);
            }
        }

        [Theory]
        [InlineData(HashType.MD5)]
        [InlineData(HashType.SHA1)]
        public void UUIDGenerated_InWithSameName_InDifferentCustomNameSpace_ShouldBeDifferent(HashType hashType)
        {
            var name = Guid.NewGuid().ToString();
            var firstNs = Guid.NewGuid();
            var secondNs = Guid.NewGuid();

            using (var generator = new NameBasedGenerator(hashType))
            {
                var first = generator.GenerateGuid(firstNs, name);
                var second = generator.GenerateGuid(secondNs, name);

                Assert.NotEqual(first, second);
            }
        }
    }
}
