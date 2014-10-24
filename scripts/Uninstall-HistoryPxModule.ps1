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

# This script should only be invoked when you want to uninstall HistoryPx.

[CmdletBinding(SupportsShouldProcess=$true)]
[OutputType([System.Void])]
param(
    [Parameter()]
    [System.Management.Automation.SwitchParameter]
    $RemovePersistentData
)
try {
    #region Get the currently installed module (if there is one).

    Write-Progress -Activity 'Uninstalling HistoryPx' -Status 'Looking for an installed HistoryPx module.'
    $module = Get-Module -ListAvailable | Where-Object {$_.Guid -eq [System.Guid]'1ceaf4bf-dc01-4790-a06d-c8224daa7027'}
    if ($module -is [System.Array]) {
        [System.String]$message = 'More than one version of HistoryPx is installed on this system. This is not supported. Manually remove the versions you are not using and then try again.'
        [System.Management.Automation.SessionStateException]$exception = New-Object -TypeName System.Management.Automation.SessionStateException -ArgumentList $message
        [System.Management.Automation.ErrorRecord]$errorRecord = New-Object -TypeName System.Management.Automation.ErrorRecord -ArgumentList $exception,'SessionStateException',([System.Management.Automation.ErrorCategory]::InvalidOperation),$module
        throw $errorRecord
    }

    #endregion

    #region Remove the module.

    if ($module) {
        Write-Progress -Activity 'Uninstalling HistoryPx' -Status 'Unloading and removing the installed HistoryPx module.'
        # Unload the module if it is currently loaded.
        if ($loadedModule = Get-Module | Where-Object {$_.Guid -eq $module.Guid}) {
            $loadedModule | Remove-Module -ErrorAction Stop
        }
        # Remove the currently installed module.
        Remove-Item -LiteralPath $module.ModuleBase -Recurse -Force -ErrorAction Stop
    }

    #endregion

    #region Now remove the persistent data for the module if the caller requested it.

    if ($PSCmdlet.MyInvocation.BoundParameters.ContainsKey('RemovePersistentData') -and $RemovePersistentData) {
        foreach ($mlsRoot in $env:LocalAppData,$env:ProgramData) {
            $mlsPath = Join-Path -Path $mlsRoot -ChildPath "WindowsPowerShell\Modules\$($module.Name)"
            if (Test-Path -LiteralPath $mlsPath) {
                Remove-Item -LiteralPath $mlsPath -Recurse -Force -ErrorAction Stop
            }
        }
    }

    #endregion
} catch {
    $PSCmdlet.ThrowTerminatingError($_)
}