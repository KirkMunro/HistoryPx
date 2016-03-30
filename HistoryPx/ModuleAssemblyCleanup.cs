using System.Management.Automation;

namespace HistoryPx
{
    // The inheritance from IModuleAssemblyCleanup can be applied once PowerShell 5.0 is
    // the minimum requirement for this module; in the meantime, use the OnImport method
    // of the ModuleAssemblyInitializer to set up the OnRemove handler for the module such
    // that it invokes this handler
    public class ModuleAssemblyCleanup // : IModuleAssemblyCleanup
    {
        public void OnRemove(PSModuleInfo psModuleInfo)
        {
            // Let the extended history manager do its cleanup
            ExtendedHistoryManager.OnRemoveModule();
        }
    }
}
