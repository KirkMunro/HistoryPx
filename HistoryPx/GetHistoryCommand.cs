using System.Management.Automation;

namespace HistoryPx
{
    [Cmdlet(
        VerbsCommon.Get,
        "History",
        HelpUri = "http://go.microsoft.com/fwlink/?LinkID=113317"
    )]
    [OutputType("Microsoft.PowerShell.Commands.HistoryInfo#Extended")]
    public class GetHistoryCommand : Microsoft.PowerShell.Commands.GetHistoryCommand
    {
        protected SteppablePipeline steppablePipeline = null;

        protected override void BeginProcessing()
        {
            // Define the steppable pipeline that we want to do the work
            PowerShell ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
            ps.AddCommand(@"Microsoft.PowerShell.Core\Get-History", false);
            foreach (string parameterName in MyInvocation.BoundParameters.Keys)
            {
                ps.AddParameter(parameterName, MyInvocation.BoundParameters[parameterName]);
            }

            // Invoke the steppable pipeline
            steppablePipeline = ps.GetSteppablePipeline(this);
            steppablePipeline.Begin(false);
        }

        protected override void ProcessRecord()
        {
            // Process the steppable pipeline
            if (steppablePipeline != null)
            {
                foreach (PSObject item in steppablePipeline.Process())
                {
                    // Pass the object through the ExtendedHistoryManager to add extended history
                    // information if it is a HistoryInfo object, and write the resulting object
                    // out to the pipeline
                    WriteObject(ExtendedHistoryManager.ExtendHistoryInfoObject(item));
                }
            }
        }

        protected override void EndProcessing()
        {
            // End the processing of the steppable pipeline
            if (steppablePipeline != null)
            {
                foreach (PSObject item in steppablePipeline.End())
                {
                    // We never get history information here, so we can just write whatever we get
                    // out to the pipeline
                    WriteObject(item);
                }
            }
        }
    }
}
