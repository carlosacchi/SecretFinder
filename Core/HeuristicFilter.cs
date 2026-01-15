using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretsFinder.Core
{
    public static class HeuristicFilter
    {
        // Minimal English-centric common words; can be extended. Kept short for runtime cost.
        private static readonly HashSet<string> CommonWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "password", "passwd", "pass", "secret", "admin", "user", "login", "test",
            "example", "sample", "default", "changeme", "welcome", "guest", "root"
        };

        // Basic entropy check: threshold tuned to keep random-looking strings and drop simple words.
        public static bool LooksRandomEnough(string value, double threshold = 3.3)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            // Very short values are likely not secrets.
            if (value.Length < 6)
                return false;

            double entropy = CalculateShannonEntropy(value);
            return entropy >= threshold;
        }

        public static bool IsLikelyCommonWord(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            string trimmed = value.Trim('"', '\'', '`');

            // If the whole value is a common word, drop it.
            if (CommonWords.Contains(trimmed))
                return true;

            // If value is purely alphabetic and all lower, likely a normal word.
            if (trimmed.All(char.IsLetter) && trimmed.ToLowerInvariant() == trimmed)
                return true;

            return false;
        }

        private static double CalculateShannonEntropy(string s)
        {
            var counts = new Dictionary<char, int>();
            foreach (char c in s)
            {
                if (!counts.ContainsKey(c)) counts[c] = 0;
                counts[c]++;
            }

            double entropy = 0.0;
            double len = s.Length;
            foreach (var kv in counts)
            {
                double p = kv.Value / len;
                entropy -= p * Math.Log(p, 2);
            }

            return entropy;
        }
    }
}
