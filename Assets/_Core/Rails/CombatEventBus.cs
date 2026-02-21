using System;
using UnityEngine;

namespace Faust.Rails
{
    public static class CombatEventBus
    {
        public static Action<CastInfo> OnCast;
        public static Action<HitInfo> OnHit;
        public static Action<float> OnPlayerDamaged; // Amount damaged
        public static Action<float> OnEnemyKilled; // XP Value
    }

    public struct CastInfo
    {
        public Vector3 Position;
        public Vector3 Direction;
        public AbilityContext Context;
    }

    public struct HitInfo
    {
        public Transform Target;
        public Transform Instigator;
        public AbilityContext Context;
    }
}
