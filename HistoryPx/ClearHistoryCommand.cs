using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Xml;
using Microsoft.PowerShell.Commands;


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

            // Get the remaining history ids so that we can clean up the ExtendedHistoryTable
            PowerShell ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
            ps.AddScript(string.Format("@(Get-History -Count {0}).foreach('Id')", ExtendedHistoryTable.MaximumEntryCount), false);
            Collection<PSObject> results = ps.Invoke();

            // Send any errors we received to the error stream
            if (ps.HadErrors)
            {
                foreach (ErrorRecord error in ps.Streams.Error)
                {
                    WriteError(error);
                }
            }

            // Clean up the ExtendedHistoryTable
            ExtendedHistoryTable.RemoveAllExcept(results);
        }
    }
}