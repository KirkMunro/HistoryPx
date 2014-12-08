using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.PowerShell.Commands;

namespace HistoryPx
{
    [Cmdlet(
        VerbsData.Out,
        "Default",
        HelpUri = "http://go.microsoft.com/fwlink/p/?linkid=289600"
    )]
    [OutputType(typeof(void))]
    public class OutDefaultCommand : Microsoft.PowerShell.Commands.OutDefaultCommand
    {
        ProxyCmdletHelper outDefaultProxyHelper = null;

        private static string writeErrorStream = "writeErrorStream";
        private static string writeWarningStream = "writeWarningStream";

        protected Collection<PSObject> historicalOutput = new Collection<PSObject>();
        protected Collection<PSObject> capturedOutput = new Collection<PSObject>();
        protected bool adjustHistoryId = false;
        protected int removedObjectCount = 0;
        protected int removedHistoryInfoCount = 0;
        protected long historyId = -1;
        ScriptBlockAst pipelineAst = null;

        protected override void BeginProcessing()
        {
            // Reset OutDefaultCommand helper variables
            historicalOutput.Clear();
            capturedOutput.Clear();
            adjustHistoryId = false;
            removedObjectCount = 0;
            removedHistoryInfoCount = 0;

            // Look up the history id for the current command
            historyId = MyInvocation.HistoryId;

            // Get the pipeline for the current command
            pipelineAst = Runspace.DefaultRunspace.GetCurrentPipelineAst();

            // Let the proxy target do its work
            outDefaultProxyHelper = new ProxyCmdletHelper(this);
            outDefaultProxyHelper.Begin(true);
        }

        protected override void ProcessRecord()
        {
            // Process input objects according to the stream to which they are being sent
            if (MyInvocation.BoundParameters.ContainsKey("InputObject") && (InputObject != null))
            {
                // If the inputobject contains an error record, isolate the ErrorRecord instance
                ErrorRecord errorRecord = null;
                if (InputObject.BaseObject is ErrorRecord)
                {
                    errorRecord = (ErrorRecord)InputObject.BaseObject;
                }
                else if ((InputObject.BaseObject as IContainsErrorRecord) != null)
                {
                    errorRecord = (ErrorRecord)InputObject.Properties["ErrorRecord"].Value;
                }

                // If an error record that is being sent to the error stream was found, adjust
                // the history id according to the error record's history id value; otherwise, if
                // the object is not of type HistoryInfo, add it to the output collection
                if ((errorRecord != null) && (InputObject.Properties[writeErrorStream] != null))
                {
                    if (errorRecord.InvocationInfo != null)
                    {
                        // If history id is -1, and if the exception was thrown from a throw
                        // statement, then we need to remove one from our history id in the
                        // EndProcessing method; otherwise, if it is  less than our current
                        // history id, we can immediately adjust our history id accordingly
                        if (errorRecord.InvocationInfo.HistoryId == -1)
                        {
                            if (((errorRecord.Exception is RuntimeException) && ((RuntimeException)errorRecord.Exception).WasThrownFromThrowStatement) ||
                                (errorRecord.Exception is ParentContainsErrorRecordException))
                            {
                                adjustHistoryId = true;
                            }
                        }
                        else if (errorRecord.InvocationInfo.HistoryId < historyId)
                        {
                            historyId = errorRecord.InvocationInfo.HistoryId;
                        }
                    }
                }
                else
                {
                    if ((!(InputObject.BaseObject is HistoryInfo)) && (!(InputObject.BaseObject is ExtendedHistoryInfo)))
                    {
                        // HistoryInfo and ExtendedHistoryInfo instances are omitted from extended
                        // history information data to keep the memory footprint under control
                        if (historicalOutput.Count < ExtendedHistoryTable.MaximumItemCountPerEntry)
                        {
                            historicalOutput.Add(InputObject);
                        }
                        else
                        {
                            removedObjectCount++;
                        }
                    }
                    else
                    {
                        removedHistoryInfoCount++;
                    }
                    if ((capturedOutput.Count < CaptureOutputConfiguration.MaximumItemCount) &&
                        (CaptureOutputConfiguration.ExcludedTypes != null) &&
                        (CaptureOutputConfiguration.ExcludedTypes.Count > 0) &&
                        (!CaptureOutputConfiguration.ExcludedTypes.Intersect(
                            InputObject.TypeNames
                                .Select(x => Regex.Replace(x, @"^(Deserialized|Selected)\.", ""))
                                .SelectMany(x => new string[] {
                                    x,
                                    string.Format("Deserialized.{0}", x),
                                    string.Format("Selected.{0}", x)
                                })).Any()))
                    {
                        // Specific types may be optionally excluded from the last captured output collection
                        capturedOutput.Add(InputObject);
                    }
                }
            }

            // Let the proxy target do its work
            outDefaultProxyHelper.Process(InputObject);
        }

        protected override void EndProcessing()
        {
            // Get the current value of the $? variable
            bool commandSucceeded = (bool)SessionState.PSVariable.Get("?").Value;

            // If the history id needs to be updated (because we received error records
            // with no history id, meaning they come from the previous command), update
            // it
            if (adjustHistoryId)
            {
                historyId--;
            }

            // If something was output, calculate the actual number of items output
            int outputCount = historicalOutput.Count + removedHistoryInfoCount + removedObjectCount;

            // Add a warning if appropriate
            if (removedHistoryInfoCount > 0)
            {
                PSObject warningRecord = new PSObject(new WarningRecord(string.Format("<Omitting {0} history information objects>", removedHistoryInfoCount)));
                warningRecord.Properties.Add(new PSNoteProperty(writeWarningStream, true));
                historicalOutput.Insert(0, warningRecord);
            }

            // Update the last output collection in the last captured output variable, allowing for HistoryInfo objects
            // to be returned as part of that collection, but only when:
            // a) the user isn't accessing a member of the variable or the variable itself in the statements invoked
            // b) the user isn't accessing a member of a variable or a variable itself without a pipeline
            // c) statements other than assignments or function/filter/workflow declarations are being invoked
            bool keepLastDoubleUnderbarValue = ((pipelineAst == null) ||
                                                (pipelineAst.EndBlock == null) ||
                                                (pipelineAst.EndBlock.Statements.Count == 0));
            if (!keepLastDoubleUnderbarValue)
            {
                keepLastDoubleUnderbarValue = true;
                for (int index = 0; index < pipelineAst.EndBlock.Statements.Count; index++)
                {
                    AssignmentStatementAst assignast = pipelineAst.EndBlock.Statements[index] as AssignmentStatementAst;
                    if (assignast != null)
                    {
                        // Don't update the last captured output when assigning a value (=, +=, -=, *=, /=, %=)
                        continue;
                    }
                    else
                    {
                        FunctionDefinitionAst functionast = pipelineAst.EndBlock.Statements[index] as FunctionDefinitionAst;
                        if (functionast != null)
                        {
                            // Don't update the last captured output when defining a function
                            continue;
                        }
                        else
                        {
                            PipelineAst pipeast = pipelineAst.EndBlock.Statements[index] as PipelineAst;
                            if ((pipeast != null) &&
                                (pipeast.PipelineElements.Count == 1))
                            {
                                CommandExpressionAst cmdexast = pipeast.PipelineElements[0] as CommandExpressionAst;
                                if (cmdexast != null)
                                {
                                    UnaryExpressionAst uexast = cmdexast.Expression as UnaryExpressionAst;
                                    if (uexast != null)
                                    {
                                        if ((uexast.TokenKind == TokenKind.PlusPlus) ||
                                            (uexast.TokenKind == TokenKind.MinusMinus) ||
                                            (uexast.TokenKind == TokenKind.PostfixPlusPlus) ||
                                            (uexast.TokenKind == TokenKind.PostfixMinusMinus))
                                        {
                                            // Don't update the last captured output when incrementing or decrementing a value (++, --)
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        var expRoot = cmdexast.Expression;
                                        do
                                        {
                                            IndexExpressionAst idxast = expRoot as IndexExpressionAst;
                                            if (idxast != null)
                                            {
                                                // When we encounter an index, look at the Target property
                                                expRoot = idxast.Target;
                                            }
                                            else
                                            {
                                                MemberExpressionAst mexast = expRoot as MemberExpressionAst;
                                                if (mexast != null)
                                                {
                                                    // When we encounter a member expression, look at the parent Expression property
                                                    expRoot = mexast.Expression;
                                                }
                                                else
                                                {
                                                    // Try to convert the current Expression into a variable
                                                    expRoot = expRoot as VariableExpressionAst;
                                                }
                                            }
                                        } while ((expRoot != null) && !(expRoot is VariableExpressionAst));
                                        if (expRoot is VariableExpressionAst)
                                        {
                                            // Don't update the last captured output when referencing a variable
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // If we reach this point, then there are statements other than assignments,
                    // incrementors/decrementors, function/filter/workflow declarations, or
                    // variable references in the statement list
                    keepLastDoubleUnderbarValue = false;
                    break;
                }
            }

            // If we haven't yet identified that we're going to keep the last last captured output
            // value, then check to see if the pipeline contains the last captured output variable,
            // and if so, keep the last value
            if (!keepLastDoubleUnderbarValue)
            {
                keepLastDoubleUnderbarValue = (pipelineAst.Find(x => ((x is VariableExpressionAst) &&
                                                                      (string.Compare(((VariableExpressionAst)x).Extent.Text, CaptureOutputConfiguration.PowerShellVariableName, true) == 0)) ||
                                                                     ((x is IndexExpressionAst) &&
                                                                      (string.Compare(((IndexExpressionAst)x).Target.Extent.Text, CaptureOutputConfiguration.PowerShellVariableName, true) == 0)), true) != null);
            }

            // If we are not keeping the last value, then update the variable appropriately
            if (!keepLastDoubleUnderbarValue)
            {
                if ((capturedOutput.Count == 0) ||
                    ((capturedOutput.Count == 1) && (capturedOutput[0] == null)))
                {
                    if (CaptureOutputConfiguration.CaptureNull)
                    {
                        SessionState.PSVariable.Set(CaptureOutputConfiguration.VariableName, null);
                    }
                }
                else if (capturedOutput.Count == 1)
                {
                    // Don't store value types, since they are easy to remember; we only care
                    // about more complex object data
                    if (!capturedOutput[0].BaseObject.GetType().IsValueType || CaptureOutputConfiguration.CaptureValueTypes)
                    {
                        SessionState.PSVariable.Set(CaptureOutputConfiguration.VariableName, capturedOutput[0]);
                    }
                }
                else if (capturedOutput.Count > 1)
                {
                    SessionState.PSVariable.Set(
                        CaptureOutputConfiguration.VariableName,
                        capturedOutput.ToArray()
                        );
                }
            }

            // Add the error log entries that have been added since the last change in runspace
            // availability to the error collection
            ArrayList errorLog = (ArrayList)SessionState.PSVariable.Get("Error").Value;
            Collection<PSObject> commandErrors = new Collection<PSObject>();
            foreach (var errorLogEntry in errorLog)
            {
                if ((ExtendedHistoryTable.Watermark != -1) && (errorLogEntry.GetHashCode() == ExtendedHistoryTable.Watermark))
                {
                    // Stop when we reach our error log watermark
                    break;
                }
                if (errorLogEntry is IncompleteParseException)
                {
                    // Skip all incomplete parse exceptions
                    continue;
                }
                if (errorLogEntry is ActionPreferenceStopException)
                {
                    // Skip all action preference stop exceptions
                    continue;
                }
                commandErrors.Add(new PSObject(errorLogEntry));
            }

            if (commandErrors.Count > 0)
            {
                // If we found some errors, update our watermark and reverse the error list
                ExtendedHistoryTable.Watermark = commandErrors[0].BaseObject.GetHashCode();
                commandErrors = new Collection<PSObject>(commandErrors.Reverse().ToList());
            }

            // If we have no output or errors, then mark the command a success
            if ((outputCount == 0) && (commandErrors.Count == 0))
            {
                commandSucceeded = true;
            }

            // If we just removed the module, reset the OnRemove flag; otherwise, add a
            // detailed history entry to the detailed history table
            if (ExtendedHistoryTable.OnRemove)
            {
                ExtendedHistoryTable.OnRemove = false;
            }
            else
            {
                ExtendedHistoryTable.Add(historyId, commandSucceeded, historicalOutput, outputCount, commandErrors);
            }

            // Let the proxy target do its work
            outDefaultProxyHelper.End();
        }
    }
}