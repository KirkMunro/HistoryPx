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

@{
      ModuleToProcess = 'HistoryPx.dll'

        ModuleVersion = '1.0.0.0'

                 GUID = '1ceaf4bf-dc01-4790-a06d-c8224daa7027'

               Author = 'Kirk Munro'

          CompanyName = 'Poshoholic Studios'

            Copyright = 'Copyright 2014 Kirk Munro'

          Description = ''

    PowerShellVersion = '3.0'

      CmdletsToExport = @(
                        'Clear-History'
                        'Out-Default'
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
                        'LICENSE'
                        'NOTICE'
                        'types.ps1xml'
                        'en-us\HistoryPx.dll-Help.xml'
                        'scripts\Install-HistoryPxModule.ps1'
                        'scripts\Uninstall-HistoryPxModule.ps1'
                        )

          PrivateData = @{
                            PSData = @{
                                Tags = 'history Clear-History Get-History Out-Default'
                                LicenseUri = 'http://apache.org/licenses/LICENSE-2.0.txt'
                                ProjectUri = 'https://github.com/KirkMunro/HistoryPx'
                                IconUri = ''
                                ReleaseNotes = ''
                            }
                        }
}