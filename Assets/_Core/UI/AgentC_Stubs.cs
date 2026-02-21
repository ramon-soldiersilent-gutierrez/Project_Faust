using UnityEngine;

namespace Faust.Rails
{
    // STUBS so Agent C worktree compiles in isolation.
    // In final merge, these will be overwritten by Agent B's real implementations.

    public class HookLifecycleManager : MonoBehaviour
    {
        public static HookLifecycleManager Instance { get; private set; }
        private void Awake() { Instance = this; }

        public void ClearAllHooks() {}
    }
}
