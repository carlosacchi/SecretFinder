using System.Collections.Generic;
using System.Drawing;
using Kbg.NppPluginNET.PluginInfrastructure;
using SecretsFinder.Utils;

namespace SecretsFinder.Core
{
    public class IndicatorManager
    {
        private int _indicatorId = -1;
        private bool _isAllocated = false;

        public bool IsInitialized => _isAllocated && _indicatorId >= 0;
        public int IndicatorId => _indicatorId;

        public bool Initialize()
        {
            if (_isAllocated)
                return true;

            // Try to allocate from Notepad++ (requires NPP 8.5.6+)
            if (Npp.notepad.AllocateIndicators(1, out int[] indicators))
            {
                _indicatorId = indicators[0];
                _isAllocated = true;
                return true;
            }

            // Fallback: use indicator 9 (commonly available in user range 9-20)
            _indicatorId = 9;
            _isAllocated = true;
            return true;
        }

        public void SetupIndicatorStyle(Color highlightColor)
        {
            if (!IsInitialized)
                return;

            Npp.editor.SetIndicatorCurrent(_indicatorId);
            Npp.editor.IndicSetStyle(_indicatorId, IndicatorStyle.ROUNDBOX);
            Npp.editor.IndicSetFore(_indicatorId, new Colour(
                highlightColor.R, highlightColor.G, highlightColor.B));
            Npp.editor.IndicSetAlpha(_indicatorId, (Alpha)100);
            Npp.editor.IndicSetOutlineAlpha(_indicatorId, Alpha.OPAQUE);
            Npp.editor.IndicSetUnder(_indicatorId, true);
        }

        public void HighlightRange(int start, int length)
        {
            if (!IsInitialized || length <= 0)
                return;

            Npp.editor.SetIndicatorCurrent(_indicatorId);
            Npp.editor.IndicatorFillRange(start, length);
        }

        public void ClearAllHighlights()
        {
            if (!IsInitialized)
                return;

            Npp.editor.SetIndicatorCurrent(_indicatorId);
            if (Npp.editor.TryGetLengthAsInt(out int length) && length > 0)
            {
                Npp.editor.IndicatorClearRange(0, length);
            }
        }

        public void HighlightMatches(IEnumerable<SecretMatch> matches)
        {
            ClearAllHighlights();

            if (matches == null)
                return;

            foreach (var match in matches)
            {
                if (!match.IsDismissed)
                {
                    HighlightRange(match.StartPosition, match.Length);
                }
            }
        }

        public void HighlightMatchesForCurrentFile(IEnumerable<SecretMatch> matches, string currentFilePath)
        {
            ClearAllHighlights();

            if (matches == null || string.IsNullOrEmpty(currentFilePath))
                return;

            foreach (var match in matches)
            {
                if (!match.IsDismissed &&
                    string.Equals(match.FilePath, currentFilePath, System.StringComparison.OrdinalIgnoreCase))
                {
                    HighlightRange(match.StartPosition, match.Length);
                }
            }
        }
    }
}
