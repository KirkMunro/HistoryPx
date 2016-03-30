using Microsoft.PowerShell.Commands;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace HistoryPx
{
    internal static class PowerShellInternals
    {
        static BindingFlags publicOrPrivateInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        internal static SteppablePipeline GetSteppablePipeline(this PowerShell ps, PSCmdlet psCmdlet)
        {
            // The PowerShell language itself supports getting steppable pipelines from a script block,
            // however this support inside of compiled cmdlets is hidden behind internal methods. Since
            // proxying commands allows for powerful extension support in PowerShell, and since cmdlets
            // perform much better than their PowerShell function equivalent, I decided to expose the
            // internal method that is required via an extension method on a PowerShell object. This
            // extension method can be included in any project where it is needed.

            // Look up the GetSteppablePipeline internal method
            MethodInfo getSteppablePipelineMethod = typeof(PowerShell).GetMethod("GetSteppablePipeline", publicOrPrivateInstance, null, Type.EmptyTypes, null);
            if (getSteppablePipelineMethod == null)
            {
                MethodAccessException exception = new MethodAccessException("Failed to find GetSteppablePipeline method.");
                ErrorRecord errorRecord = new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.ResourceUnavailable, ps);
                psCmdlet.ThrowTerminatingError(errorRecord);
            }

            // Return the steppable pipeline
            return (SteppablePipeline)getSteppablePipelineMethod.Invoke(ps, null);
        }

        internal static SessionState GetSessionState(this Runspace runspace)
        {
            // In PowerShell, we can get the session state at any time by retrieving the
            // SessionState property on the $ExecutionContext variable. In .NET, that variable
            // and the SessionState property are not readily available, so we need to use
            // reflection to dig it out unless we're willing to compromise on performance
            // and risk breakpoint triggers by invoking PowerShell to get the variable value.
            Assembly smaAssembly = typeof(PowerShell).Assembly;
            Type localRunspaceType = smaAssembly.GetType("System.Management.Automation.Runspaces.LocalRunspace");
            if ((localRunspaceType == null) || !localRunspaceType.IsInstanceOfType(runspace))
            {
                return null;
            }

            var localRunspace = Convert.ChangeType(runspace, localRunspaceType);
            var executionContext = localRunspaceType.GetProperty("GetExecutionContext", publicOrPrivateInstance)
                                                   ?.GetValue(localRunspace);
            if (executionContext == null)
            {
                return null;
            }

            var engineIntrinsics = executionContext.GetType()
                                                   .GetProperty("EngineIntrinsics", publicOrPrivateInstance)
                                                  ?.GetValue(executionContext) as EngineIntrinsics;
            if (engineIntrinsics == null)
            {
                return null;
            }

            return engineIntrinsics.SessionState;
        }

        internal static HistoryInfo[] GetHistory(this Runspace runspace, long id = 0L, long count = 0L, bool newest = true)
        {
            // The local runspace has an internal History property that can be used
            // to retrieve PowerShell command history without using the Get-History
            // command. For performance reasons, plus to avoid dealing with PowerShell
            // state and breakpoints, we'll expose the GetEntries method of this
            // property so that we can invoke it directly.
            Assembly smaAssembly = typeof(PowerShell).Assembly;
            Type localRunspaceType = smaAssembly.GetType("System.Management.Automation.Runspaces.LocalRunspace");
            if ((localRunspaceType == null) || !localRunspaceType.IsInstanceOfType(runspace))
            {
                return null;
            }

            var localRunspace = Convert.ChangeType(runspace, localRunspaceType);
            var history = localRunspaceType.GetProperty("History", publicOrPrivateInstance)
                                          ?.GetValue(localRunspace);
            if (history == null)
            {
                return null;
            }

            if (count == 0)
            {
                var capacity = history.GetType()
                                      .GetField("_capacity", publicOrPrivateInstance)
                                     ?.GetValue(history) as int?;
                count = capacity ?? 4096;
            }

            return history.GetType()
                          .GetMethod("GetEntries", publicOrPrivateInstance, null, new Type[] { typeof(long), typeof(long), typeof(SwitchParameter) }, null)
                         ?.Invoke(history, new object[] { id, count, new SwitchParameter(newest) } ) as HistoryInfo[];
        }

        internal static IEnumerable<CallStackFrame> GetCallStack(this Runspace runspace)
        {
            // The call stack is available via a GetCallStack method on the ScriptDebugger class;
            // however, since the ScriptDebugger class is not public, we cannot invoke its
            // GetCallStack method from within a binary module. This method removes that limitation
            // that shouldn't be there so that we can get the call stack via an API instead
            // of via a PowerShell cmdlet.
            Assembly smaAssembly = typeof(PowerShell).Assembly;
            Type scriptDebuggerType = smaAssembly.GetType("System.Management.Automation.ScriptDebugger");
            if ((scriptDebuggerType == null) || !scriptDebuggerType.IsInstanceOfType(runspace.Debugger))
            {
                return null;
            }

            var scriptDebugger = Convert.ChangeType(runspace.Debugger, scriptDebuggerType);
            return scriptDebuggerType.GetMethod("GetCallStack", publicOrPrivateInstance)
                                    ?.Invoke(scriptDebugger, new object[0]) as IEnumerable<CallStackFrame>;
        }

        internal static IScriptExtent GetExtent(this CallStackFrame callStackFrame)
        {
            // When working with the call stack, it is very useful to be able to review the
            // extent associated with any frame in the call stack. At the time this was written
            // the extent information was internal.
            var functionContext = callStackFrame.GetType()
                                                .GetProperty("FunctionContext", publicOrPrivateInstance)
                                               ?.GetValue(callStackFrame);

            return functionContext?.GetType()
                                   .GetProperty("CurrentPosition", publicOrPrivateInstance)
                                  ?.GetValue(functionContext) as IScriptExtent;
        }

        internal static ScriptBlockAst GetCurrentPipelineAst(this Runspace runspace)
        {
            // Use private field data to identify the current pipeline string and generate
            // the AST for that script block (I would much prefer use public APIs to get
            // this information, however the PowerShell SDK does not seem to expose this to
            // me anywhere -- InvocationInfo, the call stack, etc. do not provide this info
            // in every situation, such as when performing a simple assignment like $x = 42).

            // Use internal fields to drill into the current pipeline to get the pipeline string
            // (stored in runspace.pipelineThread.workItem._target._historyString).

            // Return null if this fails at any time, and the caller will know what to do.

            var pipelineThread = runspace.GetType()
                                         .GetField("pipelineThread", publicOrPrivateInstance)
                                        ?.GetValue(runspace);
            if (pipelineThread == null)
            {
                return null;
            }

            var workItem = pipelineThread.GetType()
                                         .GetField("workItem", publicOrPrivateInstance)
                                        ?.GetValue(pipelineThread);
            if (workItem == null)
            {
                return null;
            }

            var target = workItem.GetType()
                                 .GetField("_target", publicOrPrivateInstance)
                                ?.GetValue(workItem);
            if (target == null)
            {
                return null;
            }

            var pipelineString = target.GetType()
                                       .BaseType
                                       .GetField("_historyString", publicOrPrivateInstance)
                                       .GetValue(target) as string;
            if (pipelineString == null)
            {
                return null;
            }

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

        internal static object GetValueAtScope(this PSVariableIntrinsics psVariableIntrinsics, string name, string scopeId)
        {
            return psVariableIntrinsics.GetType()
                                       .GetMethod("GetValueAtScope", publicOrPrivateInstance, null, new Type[] { typeof(string), typeof(string) }, null)
                                       ?.Invoke(psVariableIntrinsics, new object[] { name, scopeId });
        }
    }
}
