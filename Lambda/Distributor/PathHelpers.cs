using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Distributor
{
    public static class PathHelpers
    {
        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (!Path.IsPathRooted(toPath))
                return toPath;
            var fromPathArray = GetPathArray(Path.GetFullPath(fromPath));
            var toPathArray = GetPathArray(Path.GetFullPath(toPath));
            var resultBuilder = new StringBuilder();

            var firstDifferentIndex = GetFirstDifferentIndex(fromPathArray, toPathArray);
            MoveFromPathUp(fromPathArray, firstDifferentIndex, resultBuilder);
            MoveToPathDown(toPathArray, firstDifferentIndex, resultBuilder);
            return ExcludeTrailingSlash(resultBuilder.ToString());
        }

        private static string[] GetPathArray(string path)
        {
            return path.Split(Path.DirectorySeparatorChar);
        }

        public static string ExcludeFirstSlash(string source)
        {
            if (!string.IsNullOrEmpty(source) && (source[0] == Path.DirectorySeparatorChar || source[0] == Path.AltDirectorySeparatorChar))
                return source.Substring(1);
            return source;
        }

        public static string ExcludeTrailingSlash(string source)
        {
            return source != null && (source.EndsWith(Path.DirectorySeparatorChar.ToString()))
                       ? source.Substring(0, source.Length - 1)
                       : source;
        }

        public static string IncludeTrailingDirectorySlash(string source)
        {
            if (source.EndsWith(Path.DirectorySeparatorChar.ToString()))
                return source;
            if (source.EndsWith("/"))
                return source.Substring(0, source.Length - 1) + Path.DirectorySeparatorChar;
            return source + Path.DirectorySeparatorChar;
        }

        private static void MoveToPathDown(string[] toPathArray, int startIndex, StringBuilder resultBuilder)
        {
            for (var i = startIndex; i < toPathArray.Length; i++)
                resultBuilder.AppendFormat("{0}{1}", toPathArray[i], Path.DirectorySeparatorChar);
        }

        private static void MoveFromPathUp(ICollection<string> fromPathArray, int startIndex,
                                           StringBuilder resultBuilder)
        {
            for (var i = startIndex; i < fromPathArray.Count - 1; i++)
                resultBuilder.Append(string.Format("..{0}", Path.DirectorySeparatorChar));
        }

        private static int GetFirstDifferentIndex(string[] fromPathArray, string[] toPathArray)
        {
            var result = 0;
            while (result < fromPathArray.Length && result < toPathArray.Length &&
                   StringComparer.OrdinalIgnoreCase.Compare(fromPathArray[result], toPathArray[result]) == 0)
                result++;
            return result;
        }

        public static string RemoveProtocol(string path)
        {
            return String.IsNullOrEmpty(path) ? path : protocolRegex.Replace(path, "$2");
        }

        public static string AssumeHttpProtocol(string url)
        {
            return (string.IsNullOrEmpty(url) || protocolRegex.Match(url).Success) ? url : "http://" + url;
        }

        private static readonly Regex protocolRegex = new Regex(@"^(\w+://)(.*)", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex remotePath = new Regex(@"^\\\\([^\\]+)\\(\w+)\$\\(.+)$", RegexOptions.Singleline | RegexOptions.Compiled);

        public static bool TryLocalizeRemotePath(string path, out string remoteMachine, out string localizedPath)
        {
            var match = remotePath.Match(path);
            if (!match.Success)
            {
                localizedPath = path;
                remoteMachine = null;
                return false;
            }
            remoteMachine = match.Groups[1].Value;
            localizedPath = match.Groups[2].Value + @":\" + match.Groups[3].Value;
            return true;
        }

        public static string LocalizeRemotePath(string path)
        {
            string remoteMachine;
            string localizedPath;
            return TryLocalizeRemotePath(path, out remoteMachine, out localizedPath) ? localizedPath : path;
        }

        public static string GetRemoteMachineOrNull(string path)
        {
            string remoteMachine;
            string localizedPath;
            return TryLocalizeRemotePath(path, out remoteMachine, out localizedPath) ? remoteMachine : null;
        }

        public static string GetTopDirectory(string path)
        {
            return path.Split(Path.DirectorySeparatorChar).First();
        }

        public static string Normalize(string path)
        {
            return ExcludeTrailingSlash(Path.GetFullPath(path));
        }
    }
}