using System;
using UnityEngine;

namespace Faust.Rails
{
    [Serializable]
    public struct StatPackage
    {
        public float BaseValue;
        public float FlatBonus;
        public float IncreasedMultiplier; // Stored as decimal (e.g., 0.5f = +50% Increased)
        public float MoreMultiplier;      // Compounding multiplier (e.g., 1.5f = 50% More). Identity = 1f.

        public StatPackage(float baseValue)
        {
            BaseValue = baseValue;
            FlatBonus = 0f;
            IncreasedMultiplier = 0f;
            MoreMultiplier = 1f;
        }

        // The PoB-style Equation
        public float Resolve() 
        {
            return (BaseValue + FlatBonus) * (1f + IncreasedMultiplier) * MoreMultiplier;
        }

        public void AddFlat(float amount) => FlatBonus += amount;
        public void AddIncreased(float percentageAsDecimal) => IncreasedMultiplier += percentageAsDecimal;
        public void AddMore(float factor) => MoreMultiplier *= factor; 
    }
}
