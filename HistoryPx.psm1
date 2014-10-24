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

#region Import public function definitions.

#Invoke-Snippet -Name ScriptFile.Import -Parameters @{
#    Path = Join-Path -Path $PSModuleRoot -ChildPath functions
#}

#endregion

#region Export commands defined in nested modules.

. $PSModuleRoot\scripts\Export-BinaryModule.ps1

#endregion