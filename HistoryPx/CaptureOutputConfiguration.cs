using System.Collections.Generic;
using System.Collections.Immutable;

namespace HistoryPx
{
    static public class CaptureOutputConfiguration
    {
        static public string VariableName = "__";
        static public int MaximumItemCount = 1000;
        static public ImmutableList<string> ExcludedTypes = new List<string>(new string[] {
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
            "System.Management.Automation.PSVariable",
            "Microsoft.PowerShell.Commands.HistoryInfo#Extended"
        }).ToImmutableList<string>();
        static public bool CaptureValueTypes = false;
        static public bool CaptureNull = false;
        
        static internal string PowerShellVariableIdentifier {
            get
            {
                return string.Format("${0}", VariableName);
            }
        }
    }
}
