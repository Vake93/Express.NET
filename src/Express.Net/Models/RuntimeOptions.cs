using System.Collections.Generic;

namespace Express.Net.Models
{
    public record RuntimeOptions(string Tfm, Framework Framework, Dictionary<string, object> ConfigProperties);

    public record Framework(string Name, string Version);
}
