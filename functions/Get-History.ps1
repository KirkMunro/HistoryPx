<#############################################################################
DESCRIPTION

Copyright 2014 Kirk Munro

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
#############################################################################>

<#
.ForwardHelpTargetName Get-History
.ForwardHelpCategory   Cmdlet
#>
function Get-History {
    [CmdletBinding(HelpUri='http://go.microsoft.com/fwlink/?LinkID=113317')]
    [OutputType('Microsoft.PowerShell.Commands.HistoryInfo#Extended')]
    param()
    dynamicparam {
        Invoke-Snippet -Name ProxyFunction.DynamicParameters -Parameters @{
            CommandName = 'Microsoft.PowerShell.Core\Get-History'
            CommandType = [System.Management.Automation.CommandTypes]::Cmdlet
        }
    }
    begin {
        Invoke-Snippet -Name ProxyFunction.Begin -Parameters @{
              CommandName = 'Microsoft.PowerShell.Core\Get-History'
              CommandType = [System.Management.Automation.CommandTypes]::Cmdlet
            PipeToCommand = {Add-ExtendedHistoryInformation}
        }
    }
    process {
        Invoke-Snippet -Name ProxyFunction.Process.Pipeline
    }
    end {
        Invoke-Snippet -Name ProxyFunction.End
    }
}

Export-ModuleMember -Function Get-History