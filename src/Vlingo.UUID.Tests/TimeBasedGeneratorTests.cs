// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.UUID.Tests
{
    public class TimeBasedGeneratorTests
    {
        [Theory]
        [InlineData(UUIDGenerationMode.FasterGeneration)]
        [InlineData(UUIDGenerationMode.WithUniquenessGuarantee)]
        public void GeneratedUUID_ShouldHaveProperVersion(UUIDGenerationMode mode)
        {
            var generator = new TimeBasedGenerator();
            var expectedVersion = 0x10;

            var guid = generator.GenerateGuid(mode);
            var array = guid.ToByteArray();

            Assert.Equal(expectedVersion, array[7] & 0xf0);
        }
    }
}
