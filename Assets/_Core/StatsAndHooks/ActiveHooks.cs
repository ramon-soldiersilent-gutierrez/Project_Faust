using System;
using UnityEngine;
using Faust.Rails;

namespace Faust.StatsAndHooks
{
    // --- Boons ---

    public class DamageSpikeHook : IHookInstance
    {
        public string HookID => "Boon_DamageSpike";
        
        public void Enable()
        {
            CombatEventBus.OnHit += HandleHit;
        }

        public void Disable()
        {
            CombatEventBus.OnHit -= HandleHit;
        }

        private void HandleHit(HitInfo info)
        {
            // 10% chance to deal massive bonus damage. 
            // Since we don't have direct access to reduce enemy health from here, 
            // we will simulate the spike by printing it or relying on the agent A to apply hit info.
            // Ideally Agent A reads Context from HitInfo. 
            // If it's a spike, we just log it for now to visually prove the hook executed.
            if (UnityEngine.Random.value < 0.1f)
            {
                Debug.Log($"<color=green>DAMAGE SPIKE!</color> Dealt massive damage to {info.Target?.name}!");
            }
        }
    }

    public class MachineGunHook : IHookInstance
    {
        public string HookID => "Boon_MachineGun";
        
        // This boon relies heavily on StatPackage (Attack Speed +), 
        // but as a hook, we could auto-fire an extra weak projectile.
        public void Enable()
        {
            CombatEventBus.OnCast += HandleCast;
        }

        public void Disable()
        {
            CombatEventBus.OnCast -= HandleCast;
        }

        private void HandleCast(CastInfo info)
        {
            // 20% chance to immediately fire an extra shot
            if (UnityEngine.Random.value < 0.2f)
            {
                Debug.Log("<color=green>MACHINE GUN BONUS FIRE!</color>");
                // Note: Getting ISimulationAPI would require dependency injection or a service locator.
                // Assuming we can find it via FindObjectOfType or similar if it's a MonoBehaviour.
                var simApi = UnityEngine.Object.FindAnyObjectByType<MonoBehaviour>()?.GetComponent<ISimulationAPI>();
                if (simApi != null)
                {
                    simApi.SpawnProjectile(info.Context, info.Position, info.Direction);
                }
            }
        }
    }

    public class MulticastHook : IHookInstance
    {
        public string HookID => "Boon_Multicast";
        
        public void Enable()
        {
            CombatEventBus.OnCast += HandleCast;
        }

        public void Disable()
        {
            CombatEventBus.OnCast -= HandleCast;
        }

        private void HandleCast(CastInfo info)
        {
            var simApi = UnityEngine.Object.FindAnyObjectByType<MonoBehaviour>()?.GetComponent<ISimulationAPI>();
            if (simApi != null)
            {
                // Fire two additional projectiles at slight angles
                Vector3 leftDir = Quaternion.Euler(0, -15, 0) * info.Direction;
                Vector3 rightDir = Quaternion.Euler(0, 15, 0) * info.Direction;

                simApi.SpawnProjectile(info.Context, info.Position, leftDir);
                simApi.SpawnProjectile(info.Context, info.Position, rightDir);
            }
        }
    }

    public class VampiricHook : IHookInstance
    {
        public string HookID => "Boon_Vampiric";
        
        public void Enable()
        {
            CombatEventBus.OnHit += HandleHit;
        }

        public void Disable()
        {
            CombatEventBus.OnHit -= HandleHit;
        }

        private void HandleHit(HitInfo info)
        {
            if (Faust.Simulation.PlayerController.Instance != null && info.Instigator == Faust.Simulation.PlayerController.Instance.transform)
            {
                // Heal 5% of max health on hit
                float healAmount = Faust.Simulation.PlayerController.Instance.MaxHealth * 0.05f;
                Faust.Simulation.PlayerController.Instance.Heal(healAmount);
                Debug.Log($"<color=green>VAMPIRIC HEAL:</color> +{healAmount} HP");
            }
        }
    }

    // --- Curses ---

    public class TeleportOnHitHook : IHookInstance
    {
        public string HookID => "Curse_TeleportOnHit";
        
        public void Enable()
        {
            CombatEventBus.OnPlayerDamaged += HandleDamaged;
        }

        public void Disable()
        {
            CombatEventBus.OnPlayerDamaged -= HandleDamaged;
        }

        private void HandleDamaged(float amount)
        {
            if (Faust.Simulation.PlayerController.Instance != null)
            {
                // Teleport randomly within 5 units
                Vector2 randPos = UnityEngine.Random.insideUnitCircle * 5f;
                Vector3 newPos = Faust.Simulation.PlayerController.Instance.transform.position + new Vector3(randPos.x, 0, randPos.y);
                Faust.Simulation.PlayerController.Instance.transform.position = newPos;
                Debug.Log("<color=red>CURSE: TELEPORTED ON HIT!</color>");
            }
        }
    }

    public class GlassCannonHook : IHookInstance
    {
        public string HookID => "Curse_GlassCannon";
        
        public void Enable()
        {
            CombatEventBus.OnPlayerDamaged += HandleDamaged;
        }

        public void Disable()
        {
            CombatEventBus.OnPlayerDamaged -= HandleDamaged;
        }

        private void HandleDamaged(float amount)
        {
            if (Faust.Simulation.PlayerController.Instance != null)
            {
                // Take an extra 50% damage
                float extraDamage = amount * 0.5f;
                Faust.Simulation.PlayerController.Instance.TakeDamage(extraDamage);
                Debug.Log($"<color=red>CURSE: GLASS CANNON EXTRA DAMAGE:</color> {extraDamage}");
            }
        }
    }

    public class SelfDamageHook : IHookInstance
    {
        public string HookID => "Curse_SelfDamage";
        
        public void Enable()
        {
            CombatEventBus.OnCast += HandleCast;
        }

        public void Disable()
        {
            CombatEventBus.OnCast -= HandleCast;
        }

        private void HandleCast(CastInfo info)
        {
            if (Faust.Simulation.PlayerController.Instance != null)
            {
                // Deal 5 damage to self on cast
                Faust.Simulation.PlayerController.Instance.TakeDamage(5f);
                Debug.Log("<color=red>CURSE: TOOK 5 DMG ON CAST!</color>");
            }
        }
    }

    public class RootedHook : IHookInstance
    {
        public string HookID => "Curse_Rooted";
        
        public void Enable()
        {
            CombatEventBus.OnPlayerDamaged += HandleDamaged;
        }

        public void Disable()
        {
            CombatEventBus.OnPlayerDamaged -= HandleDamaged;
        }

        private void HandleDamaged(float amount)
        {
            if (Faust.Simulation.PlayerController.Instance != null)
            {
                // Logically set a rooted flag. The PlayerController will handle it.
                Faust.Simulation.PlayerController.Instance.IsRooted = true;
                Debug.Log("<color=red>CURSE: ROOTED!</color>");
            }
        }
    }
}
