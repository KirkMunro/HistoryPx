using System;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace HistoryPx
{
    internal static class PowerShellInternals
    {
        internal static SteppablePipeline GetSteppablePipeline(this PowerShell ps, PSCmdlet psCmdlet)
        {
            // The PowerShell language itself supports getting steppable pipelines from a script block,
            // however this support inside of compiled cmdlets is hidden behind internal methods. Since
            // proxying commands allows for powerful extension support in PowerShell, and since cmdlets
            // perform much better than their PowerShell function equivalent, I decided to expose the
            // internal method that is required via an extension method on a PowerShell object. This
            // extension method can be included in any project where it is needed.

            // Look up the GetSteppablePipeline internal method
            MethodInfo getSteppablePipelineMethod = typeof(PowerShell).GetMethod("GetSteppablePipeline", BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (getSteppablePipelineMethod == null)
            {
                MethodAccessException exception = new MethodAccessException("Failed to find internal GetSteppablePipeline method.");
                ErrorRecord errorRecord = new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.ResourceUnavailable, ps);
                psCmdlet.ThrowTerminatingError(errorRecord);
            }

            // Return the steppable pipeline
            return (SteppablePipeline)getSteppablePipelineMethod.Invoke(ps, null);
        }

        internal static ScriptBlockAst GetCurrentPipelineAst(this Runspace runspace)
        {
            // Use private field data to identify the current pipeline string and generate
            // the AST for that script block (I would much prefer use public APIs to get
            // this information, however the PowerShell SDK does not seem to expose this to
            // me anywhere -- InvocationInfo, the call stack, etc. do not provide this info
            // in every situation, such as when performing a simple assignment like $x = 42).

            // Use internal fields to drill into the current pipeline to get the pipeline string
            // (stored in runspace.pipelineThread.workItem._target._historyString)
            FieldInfo pipelineThreadField = runspace.GetType().GetField("pipelineThread", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pipelineThreadField != null)
            {
                var pipelineThread = pipelineThreadField.GetValue(runspace);
                if (pipelineThread != null)
                {
                    FieldInfo workItemField = pipelineThread.GetType().GetField("workItem", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (workItemField != null)
                    {
                        var workItem = workItemField.GetValue(pipelineThread);
                        if (workItem != null)
                        {
                            FieldInfo _targetField = workItem.GetType().GetField("_target", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (_targetField != null)
                            {
                                var target = _targetField.GetValue(workItem);
                                if (target != null)
                                {
                                    FieldInfo _historyStringField = target.GetType().BaseType.GetField("_historyString", BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (_historyStringField != null)
                                    {
                                        string pipelineString = (string)_historyStringField.GetValue(target);
                                        if (pipelineString != null)
                                        {
                                            try
                                            {
                                                // If we found the pipeline string, generate and return the ScriptBlockAst
                                                return ScriptBlock.Create(pipelineString).Ast as ScriptBlockAst;
                                            }
                                            catch
                                            {
                                                // If we failed to generate the ScriptBlockAst object, then the pipeline does
                                                // not parse at all; return null in this case and the caller will handle it
                                                // properly by keeping the last $__ value
                                                return null;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
