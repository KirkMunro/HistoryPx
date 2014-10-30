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
.SYNOPSIS
    Gets the extended history configuration for the session.
.DESCRIPTION
    The Get-ExtendedHistoryConfiguration command gets the extended history configuration for the session.
.INPUTS
    None
.OUTPUTS
    HistoryPx.ExtendedHistoryConfiguration
.NOTES
    By default, extended history is configured to use a maximum entry count of 200 and a maximum item count per entry of 1000.
.EXAMPLE
    PS C:\> Get-ExtendedHistoryConfiguration

    Get the extended history configuration for the session.
.LINK
    Set-ExtendedHistoryConfiguration
.LINK
    Get-History
.LINK
    Clear-History
#>
function Get-ExtendedHistoryConfiguration {
    [CmdletBinding()]
    [OutputType('HistoryPx.ExtendedHistoryConfiguration')]
    param()
    try {
        #region Return a custom object representing the extended history configuration to the caller.

        [pscustomobject]@{
                          PSTypeName = 'HistoryPx.ExtendedHistoryConfiguration'
                   MaximumEntryCount = [HistoryPx.ExtendedHistoryTable]::MaximumEntryCount
            MaximumItemCountPerEntry = [HistoryPx.ExtendedHistoryTable]::MaximumItemCountPerEntry
        }

        #endregion
    } catch {
        $PSCmdlet.ThrowTerminatingError($_)
    }
}

Export-ModuleMember -Function Get-ExtendedHistoryConfiguration