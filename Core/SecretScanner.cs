using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SecretsFinder.Core
{
    public class SecretScanner
    {
        private readonly List<SecretPattern> _patterns;

        public SecretScanner(IEnumerable<SecretPattern> patterns)
        {
            _patterns = patterns.Where(p => p.IsEnabled).ToList();

            foreach (var pattern in _patterns)
            {
                if (pattern.CompiledRegex == null)
                {
                    try
                    {
                        pattern.CompiledRegex = new Regex(pattern.Pattern,
                            RegexOptions.Compiled | RegexOptions.Multiline);
                    }
                    catch (ArgumentException)
                    {
                        pattern.IsEnabled = false;
                    }
                }
            }
        }

        public List<SecretMatch> ScanText(string text, string filePath)
        {
            var matches = new List<SecretMatch>();

            if (string.IsNullOrEmpty(text))
                return matches;

            foreach (var pattern in _patterns.Where(p => p.IsEnabled && p.CompiledRegex != null))
            {
                try
                {
                    var regexMatches = pattern.CompiledRegex.Matches(text);
                    foreach (Match match in regexMatches)
                    {
                        int lineNumber = GetLineNumber(text, match.Index);

                        var secretMatch = new SecretMatch(
                            filePath,
                            lineNumber,
                            match.Index,
                            match.Index + match.Length,
                            match.Value,
                            pattern
                        );

                        matches.Add(secretMatch);
                    }
                }
                catch (RegexMatchTimeoutException)
                {
                    // Skip patterns that timeout
                }
            }

            return matches.OrderBy(m => m.StartPosition).ToList();
        }

        public List<SecretMatch> ScanFile(string filePath)
        {
            if (!File.Exists(filePath))
                return new List<SecretMatch>();

            try
            {
                string content = File.ReadAllText(filePath);
                return ScanText(content, filePath);
            }
            catch (Exception)
            {
                return new List<SecretMatch>();
            }
        }

        public List<SecretMatch> ScanDirectory(string directoryPath, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            var allMatches = new List<SecretMatch>();

            if (!Directory.Exists(directoryPath))
                return allMatches;

            try
            {
                var files = Directory.GetFiles(directoryPath, searchPattern, searchOption);
                foreach (var file in files)
                {
                    try
                    {
                        var matches = ScanFile(file);
                        allMatches.AddRange(matches);
                    }
                    catch
                    {
                        // Skip files that can't be read
                    }
                }
            }
            catch
            {
                // Skip if directory can't be enumerated
            }

            return allMatches;
        }

        private int GetLineNumber(string text, int position)
        {
            int line = 1;
            for (int i = 0; i < position && i < text.Length; i++)
            {
                if (text[i] == '\n')
                    line++;
            }
            return line;
        }

        public static int GetColumnNumber(string text, int position)
        {
            int column = 1;
            for (int i = position - 1; i >= 0 && text[i] != '\n'; i--)
            {
                column++;
            }
            return column;
        }
    }
}
