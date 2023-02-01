// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Xoom.UUID.Tests;

public class TimeBasedGeneratorTests
{
    [Theory]
    [InlineData(GuidGenerationMode.FasterGeneration)]
    [InlineData(GuidGenerationMode.WithUniquenessGuarantee)]
    public void GeneratedUUID_ShouldHaveProperVersion(GuidGenerationMode mode)
    {
        var generator = new TimeBasedGenerator();
        var expectedVersion = 0x10;

        var guid = generator.GenerateGuid(mode);
        var array = guid.ToActuallyOrderedBytes();

        Assert.Equal(expectedVersion, array[6] & 0xf0);
    }
}