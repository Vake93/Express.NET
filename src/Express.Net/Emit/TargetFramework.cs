using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;

namespace Express.Net.Emit
{
    public class TargetFramework : IEnumerable<MetadataReference>
    {
        private readonly IEnumerable<MetadataReference> _references;

        private TargetFramework(string tfm, string name, string version, IEnumerable<MetadataReference> references)
        {
            _references = references;
            Version = version;
            Name = name;
            Tfm = tfm;
        }

        public string Tfm { get; init; }

        public string Name { get; init; }

        public string Version { get; init; }

        public IEnumerator<MetadataReference> GetEnumerator() => _references.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _references.GetEnumerator();

        internal static TargetFramework BuildTargetFramework(string tfm, string name, string version, IEnumerable<MetadataReference> references)
            => new (tfm, name, version, references);
    }
}
