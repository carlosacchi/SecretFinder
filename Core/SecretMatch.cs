using System;
using System.IO;

namespace SecretsFinder.Core
{
    public class SecretMatch
    {
        public string FilePath { get; set; }
        public string FileName => string.IsNullOrEmpty(FilePath) ? "Unknown" : Path.GetFileName(FilePath);

        public string DisplayPath
        {
            get
            {
                if (string.IsNullOrEmpty(FilePath)) return FileName;
                try
                {
                    return Path.GetFullPath(FilePath);
                }
                catch
                {
                    return FilePath;
                }
            }
        }
        public int LineNumber { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public int Length => EndPosition - StartPosition;
        public string MatchedValue { get; set; }
        public string ObscuredValue => ObscureValue(MatchedValue);
        public SecretPattern Pattern { get; set; }
        public DateTime DetectedAt { get; set; }
        public bool IsDismissed { get; set; }

        public SecretMatch()
        {
            DetectedAt = DateTime.Now;
            IsDismissed = false;
        }

        public SecretMatch(string filePath, int lineNumber, int startPos, int endPos, string value, SecretPattern pattern)
        {
            FilePath = filePath;
            LineNumber = lineNumber;
            StartPosition = startPos;
            EndPosition = endPos;
            MatchedValue = value;
            Pattern = pattern;
            DetectedAt = DateTime.Now;
            IsDismissed = false;
        }

        private static string ObscureValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "****";

            if (value.Length <= 8)
                return new string('*', value.Length);

            int showChars = Math.Min(4, value.Length / 4);
            string start = value.Substring(0, showChars);
            string end = value.Substring(value.Length - showChars);
            int hiddenCount = value.Length - (showChars * 2);
            return $"{start}{new string('*', hiddenCount)}{end}";
        }

        public override string ToString()
        {
            return $"[{Pattern?.Name ?? "Unknown"}] {FileName}:{LineNumber} - {ObscuredValue}";
        }
    }
}
