using Express.Net.Reference.Assemblies;

namespace Express.Net.Emit
{
    public static class TargetFrameworks
    {
        private static readonly TargetFramework _netCore50 = TargetFramework.BuildTargetFramework(
            "net5.0",
            "Microsoft.NETCore.App",
            "5.0.0",
            Net50.All);

        private static readonly TargetFramework _aspNetCore50 = TargetFramework.BuildTargetFramework(
            "net5.0",
            "Microsoft.AspNetCore.App",
            "5.0.0",
            AspNet50.All);

        private static readonly TargetFramework _expressNet = TargetFramework.BuildTargetFramework(
            "net5.0",
            "Microsoft.AspNetCore.App",
            "5.0.0",
            BaseLibrary.All);

        public static TargetFramework NetCore50 => _netCore50;

        public static TargetFramework AspNetCore50 => _aspNetCore50;

        public static TargetFramework ExpressNet => _expressNet;
    }
}
