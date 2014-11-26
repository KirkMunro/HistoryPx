using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace HistoryPx
{
    static internal class RunspaceInternals
    {
        static internal ScriptBlockAst GetCurrentPipelineAst()
        {
            // Use private field data to identify the current pipeline string and generate
            // the AST for that script block (I would much prefer use public APIs to get
            // this information, however the PowerShell SDK does not seem to expose this to
            // me anywhere -- InvocationInfo, the call stack, etc. do not provide this info
            // in every situation (e.g. when performing a simple assignment like $x = 42).
            FieldInfo pipelineThreadField = Runspace.DefaultRunspace.GetType().GetField("pipelineThread", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pipelineThreadField != null)
            {
                var pipelineThread = pipelineThreadField.GetValue(Runspace.DefaultRunspace);
                if (pipelineThread != null)
                {
                    FieldInfo workItemProperty = pipelineThread.GetType().GetField("workItem", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (workItemProperty != null)
                    {
                        var workItem = workItemProperty.GetValue(pipelineThread);
                        if (workItem != null)
                        {
                            FieldInfo targetProperty = workItem.GetType().GetField("_target", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (targetProperty != null)
                            {
                                var target = targetProperty.GetValue(workItem);
                                if (target != null)
                                {
                                    FieldInfo historyStringProperty = target.GetType().BaseType.GetField("_historyString", BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (historyStringProperty != null)
                                    {
                                        string pipelineString = (string)historyStringProperty.GetValue(target);
                                        if (pipelineString != null)
                                        {
                                            return ScriptBlock.Create(pipelineString).Ast as ScriptBlockAst;
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
