﻿## HistoryPx

### Overview

HistoryPx uses proxy commands to add extended history information to
PowerShell. This includes the duration of a command, a flag indicating whether
a command was successful or not, the output generated by a command (limited to
a configurable maximum value), the error generated by a command, and the
actual number of objects returned as output and as error records.  HistoryPx
also adds a "__" variable to PowerShell that captures the last output that you
may have wanted to capture, and includes commands to configure how it decides
when output should be captured.  Lastly, HistoryPx includes commands to manage
the memory footprint that is used by extended history information.

### Minimum requirements

- PowerShell 3.0
- SnippetPx module

### License and Copyright

Copyright 2016 Kirk Munro

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

### Installing the HistoryPx module

HistoryPx is dependent on the SnippetPx module. You can download and install the
latest versions of HistoryPx and SnippetPx using any of the following methods:

#### PowerShellGet

If you don't know what PowerShellGet is, it's the way of the future for PowerShell
package management. If you're curious to find out more, you should read this:
<a href="http://blogs.msdn.com/b/mvpawardprogram/archive/2014/10/06/package-management-for-powershell-modules-with-powershellget.aspx" target="_blank">Package Management for PowerShell Modules with PowerShellGet</a>

Note that these commands require that you have the PowerShellGet module installed
on the system where they are invoked.

```powershell
# If you don’t have HistoryPx installed already and you want to install it for all
# all users (recommended, requires elevation)
Install-Module HistoryPx,SnippetPx

# If you don't have HistoryPx installed already and you want to install it for the
# current user only
Install-Module HistoryPx,SnippetPx -Scope CurrentUser

# If you have HistoryPx installed and you want to update it
Update-Module
```

#### PowerShell 3.0 or Later

To install from PowerShell 3.0 or later, open a native PowerShell console (not ISE,
unless you want it to take longer), and invoke one of the following commands:

```powershell
# If you want to install HistoryPx for all users or update a version already installed
# (recommended, requires elevation for new install for all users)
& ([scriptblock]::Create((iwr -uri http://tinyurl.com/Install-GitHubHostedModule).Content)) -ModuleName HistoryPx,SnippetPx

# If you want to install HistoryPx for the current user
& ([scriptblock]::Create((iwr -uri http://tinyurl.com/Install-GitHubHostedModule).Content)) -ModuleName HistoryPx,SnippetPx -Scope CurrentUser
```

### Loading the HistoryPx module

When it comes to module auto-loading, HistoryPx does not function the same way
that other modules do, because the proxy commands it includes are part of core
PowerShell modules that take priority during command lookup. As a result, you
should manually load HistoryPx in order to get extended history support in
PowerShell by invoking the following command:

```powershell
Import-Module HistoryPx
```

If you use HistoryPx on a regular basis, you can add that to your profile
script so that it is automatically loaded in every session.

### Using the HistoryPx module

The HistoryPx module is designed to work transparently within PowerShell. For
example, once the module is loaded and after you have invoked several commands,
you can display history information by invoking Get-History. With HistoryPx
loaded, this will show you output similar to the following:

```
  Id CommandLine              Duration       Success #Errors Output
  -- -----------              --------       ------- ------- ------
   1 ipmo HistoryPx           00:00:00.264   True
   2 Get-Service              00:00:00.267   True            {AdobeARMserv...
   3 Get-Process              00:00:00.877   True            {System.Diagn...
   4 function Test-Throw {... 00:00:00.000   True
   5 Test-Throw               00:00:00.003   False   1 !
```

This output provides you with much richer historical information, including the
command duration, a flag indicating whether the command was successful or not
according to how PowerShell measures command success, the objects that were
output by the command, and the number of errors that were generated by the
command (including an exclamation mark if a terminating error was raised). This
view is the default view for extended history information. You can see even
more details by passing the results of Get-History to Format-List, as follows:

```powershell
Get-History | Format-List *
```

Being able to view extended history information can come at a cost though,
because it requires keeping output in memory. In order to manage that requirement
properly, the HistoryPx module includes commands to configure the maximum number
of extended history entries that will be retained in the current session as well
as the maximum number of output items that will be retained per extended history
entry. By default, HistoryPx will retain 200 extended history entries and 1000
output items per entry. To change those values, you can use the
Set-ExtendedHistoryConfiguration command, as follows:

```powershell
Set-ExtendedHistoryConfiguration -MaximumEntryCount 100 -MaximumItemCountPerEntry 500
```

Also, if there are specific entries in the history that you want to remove, you
can simply use Clear-History, which is a proxy cmdlet in HistoryPx. It will clear
the appropriate history entries as well as any extended history information that
is associated with those entries.

Aside from providing extended history information, HistoryPx also adds support for
the automatic capture of output. This feature is like having an assistant watching
over your shoulder, capturing output when you want it and ignoring output when you
don't. If you invoke a command that outputs data you might want to do more with,
HistoryPx automatically captures this output in a variable. By default, it uses a
double-underscore ("__") variable name, but you can change that to use a variable
name of your choosing. Once output has been captured, it will retain that output
until you invoke another command that generates other output that you would like
to capture. It decides when you want to capture new output in the last captured
output variable by following a few simple rules:

1. Single value types (integer, datetime, etc.) are not captured by default (this
is configurable);
2. Null is not captured by default (this is configurable);
3. Objects of the following type are not captured by default (this is configurable):
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
   For these object types, their Deserialized and Selected counterparts are also
   not captured.

In addition, the last output will not be captured when you do any of the following:

1. Define a function, workflow or filter;
2. Assign a value to a variable (e.g. =, +=, -=, *=, /=, %=);
3. Increment or decrement the value in a variable (e.g. ++ or --);
4. Reference a variable (e.g. $pid, $svc.DisplayName, $p.foreach([int]));
5. Use the last captured output variable in a command (e.g. Stop-Service $__.Name).

This feature is very handy, because it greatly reduces the need to use temporary
variables while you are doing ad-hoc scripting. Here's a short transcript showing
this feature in action:

```
PS C:\> gsv c*

Status   Name               DisplayName
------   ----               -----------
Stopped  c2wts              Claims to Windows Token Service
Running  CertPropSvc        Certificate Propagation
Stopped  COMSysApp          COM+ System Application
Stopped  cphs               Intel(R) Content Protection HECI Se...
Running  CrmSqlStartupSvc   SQL Server (CRM) On-Demand Shutdown
Running  CryptSvc           Cryptographic Services
Running  CscService         Offline Files


PS C:\> $__

Status   Name               DisplayName
------   ----               -----------
Stopped  c2wts              Claims to Windows Token Service
Running  CertPropSvc        Certificate Propagation
Stopped  COMSysApp          COM+ System Application
Stopped  cphs               Intel(R) Content Protection HECI Se...
Running  CrmSqlStartupSvc   SQL Server (CRM) On-Demand Shutdown
Running  CryptSvc           Cryptographic Services
Running  CscService         Offline Files


PS C:\> $__.where{$_.Status -eq 'Stopped'}

Status   Name               DisplayName
------   ----               -----------
Stopped  c2wts              Claims to Windows Token Service
Stopped  COMSysApp          COM+ System Application
Stopped  cphs               Intel(R) Content Protection HECI Se...


PS C:\> get-help Start-Service

NAME
    Start-Service

SYNOPSIS
    Starts one or more stopped services.


SYNTAX
    Start-Service [-InputObject] <ServiceController[]> [-Exclude <String[]>] [-Include <String[]>] [-PassThru]
    [<CommonParameters>]

    Start-Service [-Exclude <String[]>] [-Include <String[]>] [-PassThru] -DisplayName <String[]> [<CommonParameters>]

    Start-Service [-Name] <String[]> [-Exclude <String[]>] [-Include <String[]>] [-PassThru] [<CommonParameters>]


DESCRIPTION
    The Start-Service cmdlet sends a start message to the Windows Service Controller for each of the specified
    services. If a service is already running, the message is ignored without error. You can specify the services by
    their service names or display names, or you can use the InputObject parameter to supply a service object
    representing the services that you want to start.


RELATED LINKS
    Online Version: http://go.microsoft.com/fwlink/p/?linkid=293919
    Get-Service
    New-Service
    Restart-Service
    Resume-Service
    Set-Service
    Stop-Service
    Suspend-Service

REMARKS
    To see the examples, type: "get-help Start-Service -examples".
    For more information, type: "get-help Start-Service -detailed".
    For technical information, type: "get-help Start-Service -full".
    For online help, type: "get-help Start-Service -online"



PS C:\> $__

Status   Name               DisplayName
------   ----               -----------
Stopped  c2wts              Claims to Windows Token Service
Running  CertPropSvc        Certificate Propagation
Stopped  COMSysApp          COM+ System Application
Stopped  cphs               Intel(R) Content Protection HECI Se...
Running  CrmSqlStartupSvc   SQL Server (CRM) On-Demand Shutdown
Running  CryptSvc           Cryptographic Services
Running  CscService         Offline Files


PS C:\> gcm -noun service

CommandType     Name                                               ModuleName
-----------     ----                                               ----------
Cmdlet          Get-Service                                        Microsoft.PowerShell.Management
Cmdlet          New-Service                                        Microsoft.PowerShell.Management
Cmdlet          Restart-Service                                    Microsoft.PowerShell.Management
Cmdlet          Resume-Service                                     Microsoft.PowerShell.Management
Cmdlet          Set-Service                                        Microsoft.PowerShell.Management
Cmdlet          Start-Service                                      Microsoft.PowerShell.Management
Cmdlet          Stop-Service                                       Microsoft.PowerShell.Management
Cmdlet          Suspend-Service                                    Microsoft.PowerShell.Management


PS C:\> $__

Status   Name               DisplayName
------   ----               -----------
Stopped  c2wts              Claims to Windows Token Service
Running  CertPropSvc        Certificate Propagation
Stopped  COMSysApp          COM+ System Application
Stopped  cphs               Intel(R) Content Protection HECI Se...
Running  CrmSqlStartupSvc   SQL Server (CRM) On-Demand Shutdown
Running  CryptSvc           Cryptographic Services
Running  CscService         Offline Files


PS C:\> get-command start-service -syntax

Start-Service [-InputObject] <ServiceController[]> [-PassThru] [-Include <string[]>] [-Exclude <string[]>] [-WhatIf]
[-Confirm] [<CommonParameters>]

Start-Service [-Name] <string[]> [-PassThru] [-Include <string[]>] [-Exclude <string[]>] [-WhatIf] [-Confirm]
[<CommonParameters>]

Start-Service -DisplayName <string[]> [-PassThru] [-Include <string[]>] [-Exclude <string[]>] [-WhatIf] [-Confirm]
[<CommonParameters>]

PS C:\> $__

Status   Name               DisplayName
------   ----               -----------
Stopped  c2wts              Claims to Windows Token Service
Running  CertPropSvc        Certificate Propagation
Stopped  COMSysApp          COM+ System Application
Stopped  cphs               Intel(R) Content Protection HECI Se...
Running  CrmSqlStartupSvc   SQL Server (CRM) On-Demand Shutdown
Running  CryptSvc           Cryptographic Services
Running  CscService         Offline Files


PS C:\> get-verb s*

Verb                                                        Group
----                                                        -----
Search                                                      Common
Select                                                      Common
Set                                                         Common
Show                                                        Common
Skip                                                        Common
Split                                                       Common
Step                                                        Common
Switch                                                      Common
Save                                                        Data
Sync                                                        Data
Start                                                       Lifecycle
Stop                                                        Lifecycle
Submit                                                      Lifecycle
Suspend                                                     Lifecycle
Send                                                        Communications


PS C:\> $__

Status   Name               DisplayName
------   ----               -----------
Stopped  c2wts              Claims to Windows Token Service
Running  CertPropSvc        Certificate Propagation
Stopped  COMSysApp          COM+ System Application
Stopped  cphs               Intel(R) Content Protection HECI Se...
Running  CrmSqlStartupSvc   SQL Server (CRM) On-Demand Shutdown
Running  CryptSvc           Cryptographic Services
Running  CscService         Offline Files

PS C:\> get-process -id $pid

Handles  NPM(K)    PM(K)      WS(K) VM(M)   CPU(s)     Id ProcessName
-------  ------    -----      ----- -----   ------     -- -----------
    501      30   135644     146328   624     2.88  26628 powershell


PS C:\> $__.WorkingSet64
149839872
PS C:\> $__

Handles  NPM(K)    PM(K)      WS(K) VM(M)   CPU(s)     Id ProcessName
-------  ------    -----      ----- -----   ------     -- -----------
    501      30   135644     146328   624     2.95  26628 powershell

```

Note how the last output variable is not updated until you run a command that
returns other object data that passes the tests listed above. This magic allows
you to just get things done without worrying about so many temporary variables.

If you want to configure how output is captured, see the help documentation for
the following two commands:

Get-CaptureOutputConfiguration
Set-CaptureOutputConfiguration

That should give you a good idea of what is included in this module. If you have
ideas on what else you might like to see related to command history, please
let me know on the GitHub page.