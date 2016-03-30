using System.Management.Automation;

namespace HistoryPx
{
    public sealed class ModuleAssemblyInitializer : IModuleAssemblyInitializer
    {
        public void OnImport()
        {
            // Initialize the extended history table for the current runspace
            ExtendedHistoryManager.OnImportModule();
        }
    }
}