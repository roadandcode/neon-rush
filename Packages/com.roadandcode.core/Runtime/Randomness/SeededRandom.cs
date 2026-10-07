using System;

namespace RoadAndCode.Core.Randomness
{
    /// <summary>Random numbers behind an interface, so logic that uses them can be tested and replayed.</summary>
    public interface IRandom
    {
        /// <summary>A value in [0, 1).</summary>
        float NextFloat();

        /// <summary>A value in [minInclusive, maxExclusive).</summary>
        int NextInt(int minInclusive, int maxExclusive);
    }

    /// <summary>
    /// Xorshift32. The same seed gives the same sequence on every platform and runtime,
    /// which <see cref="System.Random"/> and <c>UnityEngine.Random</c> don't promise.
    /// </summary>
    public sealed class SeededRandom : IRandom
    {
        private const uint FallbackSeed = 0x9E3779B9;

        private uint _state;

        public SeededRandom(int seed)
        {
            Reseed(seed);
        }

        public void Reseed(int seed)
        {
            // Xorshift never leaves zero, so zero can't be a starting state.
            _state = seed == 0 ? FallbackSeed : unchecked((uint)seed);
        }

        public float NextFloat()
        {
            // 24 bits is everything a float mantissa can hold exactly.
            return (Next() >> 8) * (1f / (1 << 24));
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "Range must contain at least one value.");
            }

            uint span = (uint)((long)maxExclusive - minInclusive);
            return (int)(minInclusive + Next() % span);
        }

        private uint Next()
        {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }
    }
}
