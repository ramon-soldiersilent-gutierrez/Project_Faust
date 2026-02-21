using UnityEngine;

namespace Faust.Rails
{
    // STUBS so Agent C worktree compiles in isolation.
    // In final merge, these will be overwritten by Agent A and Agent B's real implementations.

    public class SimulationManager : MonoBehaviour
    {
        public static SimulationManager Instance { get; private set; }
        private void Awake() { Instance = this; }

        public void ClearAll() {}
    }

    public class HookLifecycleManager : MonoBehaviour
    {
        public static HookLifecycleManager Instance { get; private set; }
        private void Awake() { Instance = this; }

        public void ClearAllHooks() {}
    }

    public static class PlayerContext
    {
        public static void Reset() {}
    }
}
