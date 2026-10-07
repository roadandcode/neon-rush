using UnityEngine;

namespace RoadAndCode.NeonRush.Effects.Logic
{
    internal interface IEffectsView
    {
        /// <summary>A burst where the runner hit something.</summary>
        void PlayImpact(Vector3 position);

        /// <summary>A small burst where a pickup was collected.</summary>
        void PlayPickup(Vector3 position);
    }
}
