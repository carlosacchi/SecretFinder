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

        public static bool ShouldDropHighEntropy(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            string trimmed = value.Trim('"', '\'', '`');

            // Very short strings are noise.
            if (trimmed.Length < 20)
                return true;

            // Obvious config/paths or common tokens.
            if (ContainsCommonTokens(trimmed))
                return true;

            if (LooksLikePathOrField(trimmed))
                return true;

            var classCount = CountCharClasses(trimmed);
            if (classCount < 2)
                return true;

            // Raise entropy bar for high-entropy detections.
            if (!LooksRandomEnough(trimmed, threshold: 3.8))
                return true;

            return false;
        }

        private static int CountCharClasses(string s)
        {
            bool lower = false, upper = false, digit = false, symbol = false;
            foreach (char c in s)
            {
                if (char.IsLower(c)) lower = true;
                else if (char.IsUpper(c)) upper = true;
                else if (char.IsDigit(c)) digit = true;
                else symbol = true;
            }
            int count = 0;
            if (lower) count++;
            if (upper) count++;
            if (digit) count++;
            if (symbol) count++;
            return count;
        }

        private static bool ContainsCommonTokens(string s)
        {
            // Lowercased check for common configuration/infra words that create noise.
            string lower = s.ToLowerInvariant();
            string[] noiseTokens =
            {
                "subscription", "resource", "group", "account", "contentlength", "properties",
                "centralindia", "administrator", "deployment", "storage", "blob", "keyvault",
                "rg-", "tf-", "wireguard", "github", "azure", "container"
            };
            foreach (var t in noiseTokens)
            {
                if (lower.Contains(t))
                    return true;
            }
            return false;
        }

        private static bool LooksLikePathOrField(string s)
        {
            // Paths or field-like tokens should not be treated as secrets.
            if (s.Contains("/") || s.Contains("\\"))
                return true;

            // key.subkey or colon-separated fields
            if (s.Contains(":") || s.Contains("."))
            {
                int digits = s.Count(char.IsDigit);
                int uppers = s.Count(char.IsUpper);
                if (digits < 3 && uppers < 2)
                    return true;
            }

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
