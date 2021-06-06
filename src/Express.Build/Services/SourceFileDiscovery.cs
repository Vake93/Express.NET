﻿using System.IO;
using System.Linq;

namespace Express.Net.Build.Services
{
    internal static class SourceFileDiscovery
    {
        public static string GetProjectFileInDirectory(string directory)
        {
            return Directory.EnumerateFiles(directory, "*.enproj").FirstOrDefault() ?? string.Empty;
        }

        public static string[] GetSourceFilesInDirectory(string directory)
        {
            return Directory.EnumerateFiles(directory, "*.en").ToArray();
        }
    }
}
