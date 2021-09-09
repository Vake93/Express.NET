﻿using System.Reflection;

namespace Express.Net.Reference.Assemblies;

internal static class ResourceLoader
{
    public static byte[] GetOrCreateResource(ref byte[]? resource, string name)
    {
        if (resource == null)
        {
            resource = GetResourceBlob(name);
        }

        return resource;
    }

    private static Stream GetResourceStream(string name)
    {
        var assembly = typeof(ResourceLoader).GetTypeInfo().Assembly;
        var stream = assembly.GetManifestResourceStream(name);

        if (stream == null)
        {
            throw new InvalidOperationException($"Resource '{name}' not found in {assembly.FullName}.");
        }

        return stream;
    }

    private static byte[] GetResourceBlob(string name)
    {
        using var stream = GetResourceStream(name);
        var bytes = new byte[stream.Length];

        stream.Read(bytes);

        return bytes;
    }
}
