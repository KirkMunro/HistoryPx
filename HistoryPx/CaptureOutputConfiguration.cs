using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace HistoryPx
{
    static public class CaptureOutputConfiguration
    {
        static public string VariableName = "__";
        static public int MaximumItemCount = 1000;
        static public List<string> ExcludedTypes = new List<string>(new string[] {
            "HistoryPx.ExtendedHistoryConfiguration",
            "HistoryPx.CaptureOutputConfiguration",
            "System.String",
            "System.Management.Automation.Runspaces.ConsolidatedString",
            "HelpInfoShort",
            "MamlCommandHelpInfo",
            "System.Management.Automation.CommandInfo",
            "Microsoft.PowerShell.Commands.GenericMeasureInfo",
            "System.Management.Automation.PSMemberInfo",
            "Microsoft.PowerShell.Commands.MemberDefinition",
            "System.Type",
            "System.Management.Automation.PSVariable"
        });
        static public bool CaptureValueTypes = false;
        static public bool CaptureNull = false;
        
        static internal string PowerShellVariableName {
            get
            {
                return string.Format("${0}", VariableName);
            }
        }
    }
}
