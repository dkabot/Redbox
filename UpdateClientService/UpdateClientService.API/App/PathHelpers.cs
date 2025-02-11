// This was added because we are using .NET Framework 4.7

using System;
using System.IO;
using System.Text;

namespace UpdateClientService.API.App
{
    public static class PathHelpers
    {
        public static string GetRelativePath(string relativeTo, string path)
        {
            var relativeToSegments = relativeTo.Split(Path.DirectorySeparatorChar);
            var pathSegments = path.Split(Path.DirectorySeparatorChar);

            var commonPrefixLength = 0;
            var minLength = Math.Min(relativeToSegments.Length, pathSegments.Length);

            while (commonPrefixLength < minLength &&
                   string.Equals(relativeToSegments[commonPrefixLength], pathSegments[commonPrefixLength],
                       StringComparison.OrdinalIgnoreCase))
                commonPrefixLength++;

            if (commonPrefixLength == 0) return path;

            var relativePath = new StringBuilder();

            for (var i = commonPrefixLength; i < relativeToSegments.Length; i++)
            {
                if (relativePath.Length > 0) relativePath.Append(Path.DirectorySeparatorChar);
                relativePath.Append("..");
            }

            for (var i = commonPrefixLength; i < pathSegments.Length; i++)
            {
                if (relativePath.Length > 0) relativePath.Append(Path.DirectorySeparatorChar);
                relativePath.Append(pathSegments[i]);
            }

            return relativePath.ToString();
        }
    }
}