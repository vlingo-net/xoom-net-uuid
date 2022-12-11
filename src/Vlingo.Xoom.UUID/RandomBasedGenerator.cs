// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Security.Cryptography;

namespace Vlingo.Xoom.UUID;

/// <summary>
/// Random number based UUID generator according to RFC4122 (version-4)
/// </summary>
public class RandomBasedGenerator
{
    private readonly RandomNumberGenerator _generator;

    /// <summary>
    /// Creates an instance of random number based UUID generator according to RFC4122 (version-4) using the provided random number generator.
    /// </summary>
    /// <param name="generator">The random number generator.</param>
    public RandomBasedGenerator(RandomNumberGenerator generator) => _generator = generator;

    /// <summary>
    /// Creates an instance of random number based UUID generator according to RFC4122 (version-4). 
    /// It uses <see cref="RandomNumberGenerator"/> as the random number generator.
    /// </summary>
    public RandomBasedGenerator() : this(RandomNumberGenerator.Create())
    {
    }

    /// <summary>
    /// Generates a RFC4122 random number based UUID (version-4)
    /// </summary>
    /// <returns></returns>
    public Guid GenerateGuid()
    {
        var data = new byte[16];
        _generator.GetBytes(data);

        data.AddVariantMarker().AddVersionMarker(UUIDVersion.Random);

        return data.ToGuidFromActuallyOrderedBytes();
    }
}