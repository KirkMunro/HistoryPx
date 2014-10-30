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

#region Initialize the module.

Invoke-Snippet -Name Module.Initialize

#endregion

#region Import helper (private) function definitions.

Invoke-Snippet -Name ScriptFile.Import -Parameters @{
    Path = Join-Path -Path $PSModuleRoot -ChildPath helpers
}

#endregion

#region Import public function definitions.

Invoke-Snippet -Name ScriptFile.Import -Parameters @{
    Path = Join-Path -Path $PSModuleRoot -ChildPath functions
}

#endregion

<#
Ideas:
- when module is loaded, get current history id from MyInvocation and store it
- if history is missing for an id greater than the one where the module was loaded, then an unhandled exception was thrown; try to identify the errors and set the success property accordingly, or set a special unhandled exception flag
- Should status be Success, Fail, UnhandledException?
- how to identify the unhandled exception error messages?
#>

#region Export commands defined in nested modules.

. $PSModuleRoot\scripts\Export-BinaryModule.ps1

#endregion

#region Set the watermark to the hash code for the most recent error.

if ($global:Error.Count -gt 0) {
    [HistoryPx.ExtendedHistoryTable]::Watermark = $global:Error[0].GetHashCode()
}

#endregion

#region Define the OnAvailabilityChanged event handler

#$OnAvailabilityChanged = [System.EventHandler[System.Management.Automation.Runspaces.RunspaceAvailabilityEventArgs]]{
#    param(
#        [System.Object]
#        $Sender,
#
#        [System.Management.Automation.Runspaces.RunspaceAvailabilityEventArgs]
#        $RunspaceAvailabilityEventArgs
#    )
#    if (@([System.Management.Automation.Runspaces.RunspaceAvailability]::Available,[System.Management.Automation.Runspaces.RunspaceAvailability]::AvailableForNestedCommand) -contains $RunspaceAvailabilityEventArgs.RunspaceAvailability) {
#        if ($global:Error.Count -gt 0) {
#            [HistoryPx.ExtendedHistoryTable]::Watermark = $global:Error[0].GetHashCode()
#        } else {
#            [HistoryPx.ExtendedHistoryTable]::Watermark = -1
#        }
#    }
#}

#$Host.Runspace.add_AvailabilityChanged($OnAvailabilityChanged)

#endregion

#region Clean-up the module when it is removed.

$PSModule.OnRemove = {
    #region Remove the AvailabilityChanged event handler.

#    $Host.Runspace.remove_AvailabilityChanged($OnAvailabilityChanged)

    #endregion
}

#endregion