﻿<#############################################################################
HistoryPx uses proxy commands to add extended history information to
PowerShell. This includes the duration of a command, a flag indicating whether
a command was successful or not, the output generated by a command (limited to
a configurable maximum value), the error generated by a command, and the
actual number of objects returned as output and as error records.  HistoryPx
also adds a "__" variable to PowerShell that contains the output from the last
command, even if the last command did not generate any output.  Lastly,
HistoryPx includes commands to manage the memory footprint that is used by
extended history information.

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

@{
      ModuleToProcess = 'HistoryPx.psm1'

        ModuleVersion = '1.0.0.1'

                 GUID = '1ceaf4bf-dc01-4790-a06d-c8224daa7027'

               Author = 'Kirk Munro'

          CompanyName = 'Poshoholic Studios'

            Copyright = 'Copyright 2014 Kirk Munro'

          Description = 'HistoryPx uses proxy commands to add extended history information to PowerShell. This includes the duration of a command, a flag indicating whether a command was successful or not, the output generated by a command (limited to a configurable maximum value), the error generated by a command, and the actual number of objects returned as output and as error records.  HistoryPx also adds a "__" variable to PowerShell that contains the output from the last command, even if the last command did not generate any output.  Lastly, HistoryPx includes commands to manage the memory footprint that is used by extended history information.'

    PowerShellVersion = '3.0'

      RequiredModules = @(
                        'SnippetPx'
                        )

        NestedModules = @(
                        'HistoryPx.dll'
                        )

      CmdletsToExport = @(
                        'Clear-History'
                        'Out-Default'
                        )

    FunctionsToExport = @(
                        'Get-ExtendedHistoryConfiguration'
                        'Get-History'
                        'Set-ExtendedHistoryConfiguration'
                        )

       TypesToProcess = @(
                        'types.ps1xml'
                        )

      FormatsToProcess = @(
                        'format.ps1xml'
                        )

             FileList = @(
                        'format.ps1xml'
                        'HistoryPx.dll'
                        'HistoryPx.psd1'
                        'HistoryPx.psm1'
                        'LICENSE'
                        'NOTICE'
                        'types.ps1xml'
                        'functions\Get-ExtendedHistoryConfiguration.ps1'
                        'functions\Get-History.ps1'
                        'functions\Set-ExtendedHistoryConfiguration.ps1'
                        'helpers\Add-ExtendedHistoryInfo.ps1'
                        'scripts\Export-BinaryModule.ps1'
                        'scripts\Install-HistoryPxModule.ps1'
                        'scripts\Uninstall-HistoryPxModule.ps1'
                        )

          PrivateData = @{
                            PSData = @{
                                Tags = 'history Clear-History Get-History Out-Default'
                                LicenseUri = 'http://apache.org/licenses/LICENSE-2.0.txt'
                                ProjectUri = 'https://github.com/KirkMunro/HistoryPx'
                                IconUri = ''
                                ReleaseNotes = 'This module will not automatically load by invoking a *-History command because the native *-History cmdlets are loaded first in PowerShell. To start using HistoryPx, you should explicitly import the module either at the command line or as part of your profile by invoking "Import-Module HistoryPx".'
                            }
                        }
}