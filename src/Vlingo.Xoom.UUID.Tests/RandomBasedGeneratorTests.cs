// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Xoom.UUID.Tests
{
    public class RandomBasedGeneratorTests
    {
        [Fact]
        public void GeneratedUUID_ShouldHaveProperVersion()
        {
            var generator = new RandomBasedGenerator();
            var expectedVersion = 0x40;

            var guid = generator.GenerateGuid();
            var array = guid.ToActuallyOrderedBytes();

            Assert.Equal(expectedVersion, array[6] & 0xf0);
        }
    }
}
