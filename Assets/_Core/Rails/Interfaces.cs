using System;
using UnityEngine;

namespace Faust.Rails
{
    public interface IHookInstance
    {
        string HookID { get; }
        
        // This is called by Agent B's HookLifecycleManager when the contract is forged
        void Enable();
        
        // This is called when the contract is reforged or F12 is pressed
        void Disable();
    }

    public interface ISimulationAPI
    {
        void ExecuteSkill(in AbilityContext context);
        void SpawnProjectile(in AbilityContext context, Vector3 position, Vector3 direction);
    }

    public interface IDemoAPI
    {
        void ResetAll();
    }

    public interface IContractRuntime
    {
        void ApplyContract(ContractModel model);
    }
    
    // For logging raw AI output
    public interface ILogSink
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}
