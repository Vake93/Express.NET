using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Express.Reference.Assemblies
{
    internal static class ResourceLoader
    {
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

            using var memoryStream = new MemoryStream(bytes);
            stream.CopyTo(memoryStream);

            return bytes;
        }

        public static byte[] GetOrCreateResource(ref byte[]? resource, string name)
        {
            if (resource == null)
            {
                resource = GetResourceBlob(name);
            }

            return resource;
        }

        public static string GetOrCreateResource(ref string resource, string name)
        {
            if (resource == null)
            {
                var stream = GetResourceStream(name);
                var streamReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

                resource = streamReader.ReadToEnd();
            }

            return resource;
        }
    }
}
