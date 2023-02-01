// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Security.Cryptography;
using System.Text;

namespace Vlingo.Xoom.UUID;

/// <summary>
/// Name based UUID generator according to RFC4122.
/// This is capable of generating Version-3 (with MD5 hashing)
/// and Version-5 (with SHA-1 hashing) variants of RFC4122 UUID.
/// </summary>
public sealed class NameBasedGenerator : IDisposable
{
    private static readonly Guid[] NameSpaceGuids = {
        Guid.Empty,
        Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8"), // DNS
        Guid.Parse("6ba7b811-9dad-11d1-80b4-00c04fd430c8"), // URL
        Guid.Parse("6ba7b812-9dad-11d1-80b4-00c04fd430c8"), // IOD
        Guid.Parse("6ba7b814-9dad-11d1-80b4-00c04fd430c8") // X500
    };

    private HashAlgorithm? _hashAlgorithm;
    private readonly UUIDVersion _version;

    /// <summary>
    /// Creates an instance of name based RFC4122 UUID generator. 
    /// Either Version-3 (MD5 hashing) or Version-5 (SHA-1 hashing) variant is instantiated based on  <paramref name="hashType"/>.
    /// </summary>
    /// <param name="hashType">Hashing algorithm type be used (MD5 or SHA-1)</param>
    public NameBasedGenerator(HashType hashType)
    {
        _hashAlgorithm = hashType == HashType.Md5 ? (HashAlgorithm)MD5.Create() : SHA1.Create();
        _version = hashType == HashType.Md5 ? UUIDVersion.NameBasedWithMd5 : UUIDVersion.NamedBasedWithSha1;
    }

    /// <summary>
    /// Creates an instance of name based RFC4122 UUID Version-3 generator, using MD5 hashing.
    /// </summary>
    public NameBasedGenerator()
        : this(HashType.Md5)
    {
    }

    /// <summary>
    /// Generates RFC4122 name based UUID without any namespace for the given <paramref name="name"/>
    /// </summary>
    /// <param name="name">The name to use when generating UUID</param>
    /// <returns>RFC4122 UUID generated using the <paramref name="name"/></returns>
    public Guid GenerateGuid(string name) => GenerateGuid(UUIDNameSpace.None, name);

    /// <summary>
    /// Generates RFC4122 name based UUID with the given <paramref name="nameSpace"/> and <paramref name="name"/>
    /// </summary>
    /// <param name="nameSpace">RFC4122 suggested standard namespace for the UUID, or None</param>
    /// <param name="name">The name to use when generating UUID</param>
    /// <returns>RFC4122 UUID generated using the <paramref name="nameSpace"/> and the <paramref name="name"/></returns>
    public Guid GenerateGuid(UUIDNameSpace nameSpace, string name) => GenerateGuid(NameSpaceGuids[(int)nameSpace], name);

    /// <summary>
    /// Generates RFC4122 name based UUID with the given <paramref name="customNamespaceGuid"/> and <paramref name="name"/>
    /// </summary>
    /// <param name="customNamespaceGuid">The custom namespace (as GUID) to use when generating UUID</param>
    /// <param name="name">The name to use when generating UUID</param>
    /// <returns>RFC4122 UUID generated using the <paramref name="customNamespaceGuid"/> and the <paramref name="name"/></returns>
    public Guid GenerateGuid(Guid customNamespaceGuid, string name)
    {
        var nsBytes = Guid.Empty == customNamespaceGuid ? new byte[0] : customNamespaceGuid.ToActuallyOrderedBytes();
        var nameBytes = Encoding.UTF8.GetBytes(name);
        var data = new byte[nsBytes.Length + nameBytes.Length];
        if(nsBytes.Length > 0)
        {
            Array.Copy(nsBytes, data, nsBytes.Length);
        }
        Array.Copy(nameBytes, 0, data, nsBytes.Length, nameBytes.Length);

        var result = _hashAlgorithm!
            .ComputeHash(data)
            .TrimTo16Bytes()
            .AddVariantMarker()
            .AddVersionMarker(_version);

        return result.ToGuidFromActuallyOrderedBytes();
    }

    public void Dispose()
    {
        if (_hashAlgorithm != null)
        {
            _hashAlgorithm.Dispose();
            _hashAlgorithm = null;
        }
    }

    ~NameBasedGenerator()
    {
        _hashAlgorithm?.Dispose();
    }
}