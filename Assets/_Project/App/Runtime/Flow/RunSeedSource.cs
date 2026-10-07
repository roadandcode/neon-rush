using System;

namespace RoadAndCode.NeonRush.App.Flow
{
    /// <summary>Chooses the seed for each run. Swapping this is how a daily or shared-seed mode gets added.</summary>
    internal interface IRunSeedSource
    {
        int NextSeed();
    }

    /// <summary>A different seed every run.</summary>
    internal sealed class ClockSeedSource : IRunSeedSource
    {
        private const int Scramble = 397;

        private int _runs;

        public int NextSeed()
        {
            // The counter keeps two runs started within the same tick from sharing a seed.
            return unchecked((Environment.TickCount * Scramble) ^ ++_runs);
        }
    }
}
