using System;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Text.RegularExpressions;
using Microsoft.PowerShell.Commands;

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
                    // If the item we receive is not a history info object, pass it through as is
                    if (!(item.BaseObject is HistoryInfo))
                    {
                        WriteObject(item);
                        continue;
                    }

                    // Look up the extended history information
                    HistoryInfo hi = (HistoryInfo)item.BaseObject;
                    ExtendedHistoryInfo ehi = ExtendedHistoryTable.Item(hi.Id);

                    // Add a custom type name to identify the extended object
                    item.TypeNames.Insert(0,"Microsoft.PowerShell.Commands.HistoryInfo#Extended");

                    // Add a duration property to the extended history information object
                    item.Members.Add(new PSNoteProperty("Duration", hi.EndExecutionTime != null ? hi.EndExecutionTime - hi.StartExecutionTime : (object)null));

                    // Add a success property to the extended history information object
                    bool success = false;
                    if (hi.ExecutionStatus != PipelineState.Failed)
                    {
                        if (ehi != null)
                        {
                            success = (bool)ehi.CommandSuccessful;
                        }
                        else
                        {
                            success = hi.ExecutionStatus == PipelineState.Completed;
                        }
                    }
                    item.Members.Add(new PSNoteProperty("Success", success));

                    // Add an output property to the extended history information object
                    item.Members.Add(new PSNoteProperty("Output", ehi != null ? ehi.Output : null));

                    // Add an outputcount property to the extended history information object
                    item.Members.Add(new PSNoteProperty("OutputCount", ehi != null && ehi.OutputCount > 0 ? ehi.OutputCount : (object)null));

                    // Add an error property to the extended history information object
                    item.Members.Add(new PSNoteProperty("Error", ehi != null ? ehi.Error : null));

                    // Add an errorcount property to the extended history information object
                    item.Members.Add(new PSNoteProperty("ErrorCount", ehi != null ? ehi.Error.Length : (object)null));

                    // Now write out the extended history information object to the pipeline
                    WriteObject(item);
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
                    WriteObject(item);
                }
            }
        }
    }
}
