using System.Management.Automation;

namespace HistoryPx
{
    [Cmdlet(
        VerbsCommon.Clear,
        "History",
        DefaultParameterSetName = "IDParameter"
    )]
    [OutputType(typeof(void))]
    public class ClearHistoryCommand : Microsoft.PowerShell.Commands.ClearHistoryCommand
    {
        protected override void EndProcessing()
        {
            // Finish processing the end block for the ClearHistory command
            base.EndProcessing();

            // Synchronize the extended history table with the history table
            ExtendedHistoryManager.SyncWithHistoryTable();
        }
    }
}