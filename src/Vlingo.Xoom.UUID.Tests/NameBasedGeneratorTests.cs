// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Xunit;

namespace Vlingo.Xoom.UUID.Tests;

public class NameBasedGeneratorTests
{
    [Theory]
    [InlineData(HashType.Md5)]
    [InlineData(HashType.Sha1)]
    public void GeneratedUUID_ShouldHaveProperVersion(HashType hashType)
    {
        var name = Guid.NewGuid().ToString();
        var nameSpace = Guid.NewGuid();

        var expectedVersion = hashType == HashType.Md5 ? 0x30 : 0x50;

        using (var generator = new NameBasedGenerator(hashType))
        {
            var guid = generator.GenerateGuid(nameSpace, name);

            var array = guid.ToActuallyOrderedBytes();

            Assert.Equal(expectedVersion, array[6] & 0xf0);
        }
    }

    [Theory]
    [InlineData(HashType.Md5, UUIDNameSpace.None)]
    [InlineData(HashType.Md5, UUIDNameSpace.Dns)]
    [InlineData(HashType.Md5, UUIDNameSpace.Oid)]
    [InlineData(HashType.Md5, UUIDNameSpace.Url)]
    [InlineData(HashType.Md5, UUIDNameSpace.X500)]
    [InlineData(HashType.Sha1, UUIDNameSpace.None)]
    [InlineData(HashType.Sha1, UUIDNameSpace.Dns)]
    [InlineData(HashType.Sha1, UUIDNameSpace.Oid)]
    [InlineData(HashType.Sha1, UUIDNameSpace.Url)]
    [InlineData(HashType.Sha1, UUIDNameSpace.X500)]
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
    [InlineData(HashType.Md5, UUIDNameSpace.None)]
    [InlineData(HashType.Md5, UUIDNameSpace.Dns)]
    [InlineData(HashType.Md5, UUIDNameSpace.Oid)]
    [InlineData(HashType.Md5, UUIDNameSpace.Url)]
    [InlineData(HashType.Md5, UUIDNameSpace.X500)]
    [InlineData(HashType.Sha1, UUIDNameSpace.None)]
    [InlineData(HashType.Sha1, UUIDNameSpace.Dns)]
    [InlineData(HashType.Sha1, UUIDNameSpace.Oid)]
    [InlineData(HashType.Sha1, UUIDNameSpace.Url)]
    [InlineData(HashType.Sha1, UUIDNameSpace.X500)]
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
    [InlineData(HashType.Md5, UUIDNameSpace.Dns, UUIDNameSpace.None)]
    [InlineData(HashType.Md5, UUIDNameSpace.None, UUIDNameSpace.Oid)]
    [InlineData(HashType.Md5, UUIDNameSpace.Oid, UUIDNameSpace.Url)]
    [InlineData(HashType.Md5, UUIDNameSpace.Url, UUIDNameSpace.X500)]
    [InlineData(HashType.Sha1, UUIDNameSpace.Dns, UUIDNameSpace.None)]
    [InlineData(HashType.Sha1, UUIDNameSpace.None, UUIDNameSpace.Oid)]
    [InlineData(HashType.Sha1, UUIDNameSpace.Oid, UUIDNameSpace.Url)]
    [InlineData(HashType.Sha1, UUIDNameSpace.Url, UUIDNameSpace.X500)]
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
    [InlineData(HashType.Md5)]
    [InlineData(HashType.Sha1)]
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

    [Fact]
    public void UUIDGenerated_ShouldGenerateProperNamebasedGuid_ForCustomNamespaceAndName()
    {
        // bugfix for issue: https://github.com/vlingo-net/vlingo-net-uuid/issues/7

        var uuidNamespace = Guid.Parse("a4405a8d-8bb2-467a-bbc3-961ab93bb538");
        var name = "9912310000";

        var generator = new NameBasedGenerator(HashType.Sha1);
        var uuidV5 = generator.GenerateGuid(uuidNamespace, name);

        Assert.Equal("a045c4bc-d81c-5fc4-88bd-313db5b2d1fc", uuidV5.ToString());
    }
}