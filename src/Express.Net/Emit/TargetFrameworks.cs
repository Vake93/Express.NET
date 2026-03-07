using Express.Net.Reference.Assemblies;

namespace Express.Net.Emit
{
    public static class TargetFrameworks
    {
        private static readonly TargetFramework _netCore10 = TargetFramework.BuildTargetFramework(
            "net10.0",
            "Microsoft.NETCore.App",
            "10.0.0",
            Net10.All);

        private static readonly TargetFramework _aspNetCore10 = TargetFramework.BuildTargetFramework(
            "net10.0",
            "Microsoft.AspNetCore.App",
            "10.0.0",
            AspNet10.All);

        private static readonly TargetFramework _expressNet = TargetFramework.BuildTargetFramework(
            "net10.0",
            "Microsoft.AspNetCore.App",
            "10.0.0",
            BaseLibrary.All);

        public static TargetFramework NetCore10 => _netCore10;

        public static TargetFramework AspNetCore10 => _aspNetCore10;

        public static TargetFramework ExpressNet => _expressNet;
    }
}
