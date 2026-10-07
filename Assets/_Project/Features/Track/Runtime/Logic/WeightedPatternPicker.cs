using System.Collections.Generic;
using RoadAndCode.Core.Diagnostics;
using RoadAndCode.Core.Randomness;

namespace RoadAndCode.NeonRush.Track.Logic
{
    /// <summary>
    /// Picks among the patterns unlocked at the current tier, by weight, and avoids playing the
    /// same pattern twice in a row when there is a choice.
    /// </summary>
    internal sealed class WeightedPatternPicker : IPatternPicker
    {
        private readonly IReadOnlyList<ITrackPattern> _patterns;
        private readonly IRandom _random;
        private ITrackPattern _previous;

        public WeightedPatternPicker(IReadOnlyList<ITrackPattern> patterns, IRandom random)
        {
            _patterns = Guard.NotNull(patterns, nameof(patterns));
            _random = Guard.NotNull(random, nameof(random));

            Guard.Require(_patterns.Count > 0, "The track needs at least one pattern.");
            bool hasOpener = false;
            for (int i = 0; i < _patterns.Count; i++)
            {
                Guard.Require(_patterns[i] != null && _patterns[i].RowCount > 0, "Every pattern needs at least one row.");
                hasOpener |= _patterns[i].MinTier == 0;
            }

            Guard.Require(hasOpener, "At least one pattern must be available at tier zero.");
        }

        public void Reset() => _previous = null;

        public ITrackPattern Next(int tier)
        {
            float total = TotalWeight(tier, _previous);
            var skip = _previous;
            if (total <= 0f)
            {
                // The previous pattern is the only eligible one, so a repeat is unavoidable.
                skip = null;
                total = TotalWeight(tier, null);
            }

            float roll = _random.NextFloat() * total;
            ITrackPattern chosen = null;
            for (int i = 0; i < _patterns.Count; i++)
            {
                var pattern = _patterns[i];
                if (!IsEligible(pattern, tier, skip)) continue;

                chosen = pattern;
                roll -= pattern.Weight;
                if (roll < 0f) break;
            }

            _previous = chosen;
            return chosen;
        }

        private float TotalWeight(int tier, ITrackPattern skip)
        {
            float total = 0f;
            for (int i = 0; i < _patterns.Count; i++)
            {
                if (IsEligible(_patterns[i], tier, skip)) total += _patterns[i].Weight;
            }

            return total;
        }

        private static bool IsEligible(ITrackPattern pattern, int tier, ITrackPattern skip)
        {
            return pattern.MinTier <= tier && !ReferenceEquals(pattern, skip);
        }
    }
}
