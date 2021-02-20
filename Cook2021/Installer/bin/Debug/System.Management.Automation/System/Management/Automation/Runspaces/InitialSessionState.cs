// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.InitialSessionState
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell;
using Microsoft.PowerShell.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation.Host;
using System.Management.Automation.Remoting;
using System.Reflection;
using System.Text;
using System.Threading;

namespace System.Management.Automation.Runspaces
{
  public class InitialSessionState
  {
    internal const string FormatEnumerationLimit = "FormatEnumerationLimit";
    internal const int DefaultFormatEnumerationLimit = 4;
    internal const ActionPreference defaultDebugPreference = ActionPreference.SilentlyContinue;
    internal const ActionPreference defaultErrorActionPreference = ActionPreference.Continue;
    internal const ActionPreference defaultProgressPreference = ActionPreference.Continue;
    internal const ActionPreference defaultVerbosePreference = ActionPreference.SilentlyContinue;
    internal const ActionPreference defaultWarningPreference = ActionPreference.Continue;
    internal const bool defaultWhatIfPreference = false;
    internal const ConfirmImpact defaultConfirmPreference = ConfirmImpact.High;
    private const string resTableName = "RunspaceInit";
    [TraceSource("InitialSessionState", "InitialSessionState")]
    private static PSTraceSource _tracer = PSTraceSource.GetTracer(nameof (InitialSessionState), nameof (InitialSessionState));
    private PSLanguageMode _languageMode = PSLanguageMode.NoLanguage;
    private bool _useFullLanguageModeInDebugger;
    private ApartmentState apartmentState = ApartmentState.Unknown;
    private PSThreadOptions createThreadOptions;
    private bool throwOnRunspaceOpenError;
    private AuthorizationManager _authorizationManager = (AuthorizationManager) new PSAuthorizationManager(Utils.DefaultPowerShellShellID);
    private Dictionary<string, int> _modulesToImport = new Dictionary<string, int>();
    private InitialSessionStateEntryCollection<SessionStateAssemblyEntry> _assemblies;
    private InitialSessionStateEntryCollection<SessionStateTypeEntry> _types;
    private InitialSessionStateEntryCollection<SessionStateFormatEntry> _formats;
    private InitialSessionStateEntryCollection<SessionStateProviderEntry> _providers;
    private InitialSessionStateEntryCollection<SessionStateCommandEntry> _commands;
    private InitialSessionStateEntryCollection<SessionStateVariableEntry> _variables;
    private object _syncObject = new object();
    private static string TabExpansionFunctionText = "\r\n            param($line, $lastWord)\r\n            & {\r\n                function Write-Members ($sep='.')\r\n                {\r\n                    Invoke-Expression ('$_val=' + $_expression)\r\n\r\n                    $_method = [Management.Automation.PSMemberTypes] `\r\n                        'Method,CodeMethod,ScriptMethod,ParameterizedProperty'\r\n                    if ($sep -eq '.')\r\n                    {\r\n                        $params = @{view = 'extended','adapted','base'}\r\n                    }\r\n                    else\r\n                    {\r\n                        $params = @{static=$true}\r\n                    }\r\n        \r\n                    foreach ($_m in ,$_val | Get-Member @params $_pat |\r\n                        Sort-Object membertype,name)\r\n                    {\r\n                        if ($_m.MemberType -band $_method)\r\n                        {\r\n                            # Return a method...\r\n                            $_base + $_expression + $sep + $_m.name + '('\r\n                        }\r\n                        else {\r\n                            # Return a property...\r\n                            $_base + $_expression + $sep + $_m.name\r\n                        }\r\n                    }\r\n                }\r\n\r\n                # If a command name contains any of these chars, it needs to be quoted\r\n                $_charsRequiringQuotes = ('`&@''#{}()$,;|<> ' + \"`t\").ToCharArray()\r\n\r\n                # If a variable name contains any of these characters it needs to be in braces\r\n                $_varsRequiringQuotes = ('-`&@''#{}()$,;|<> .\\/' + \"`t\").ToCharArray()\r\n\r\n                switch -regex ($lastWord)\r\n                {\r\n                    # Handle property and method expansion rooted at variables...\r\n                    # e.g. $a.b.<tab>\r\n                    '(^.*)(\\$(\\w|:|\\.)+)\\.([*\\w]*)$' {\r\n                        $_base = $matches[1]\r\n                        $_expression = $matches[2]\r\n                        $_pat = $matches[4] + '*'\r\n                        Write-Members\r\n                        break;\r\n                    }\r\n\r\n                    # Handle simple property and method expansion on static members...\r\n                    # e.g. [datetime]::n<tab>\r\n                    '(^.*)(\\[(\\w|\\.|\\+)+\\])(\\:\\:|\\.){0,1}([*\\w]*)$' {\r\n                        $_base = $matches[1]\r\n                        $_expression = $matches[2]\r\n                        $_pat = $matches[5] + '*'\r\n                        Write-Members $(if (! $matches[4]) {'::'} else {$matches[4]})\r\n                        break;\r\n                    }\r\n\r\n                    # Handle complex property and method expansion on static members\r\n                    # where there are intermediate properties...\r\n                    # e.g. [datetime]::now.d<tab>\r\n                    '(^.*)(\\[(\\w|\\.|\\+)+\\](\\:\\:|\\.)(\\w+\\.)+)([*\\w]*)$' {\r\n                        $_base = $matches[1]  # everything before the expression\r\n                        $_expression = $matches[2].TrimEnd('.') # expression less trailing '.'\r\n                        $_pat = $matches[6] + '*'  # the member to look for...\r\n                        Write-Members\r\n                        break;\r\n                    }\r\n\r\n                    # Handle variable name expansion...\r\n                    '(^.*\\$)([*\\w:]+)$' {\r\n                        $_prefix = $matches[1]\r\n                        $_varName = $matches[2]\r\n                        $_colonPos = $_varname.IndexOf(':')\r\n                        if ($_colonPos -eq -1)\r\n                        {\r\n                            $_varName = 'variable:' + $_varName\r\n                            $_provider = ''\r\n                        }\r\n                        else\r\n                        {\r\n                            $_provider = $_varname.Substring(0, $_colonPos+1)\r\n                        }\r\n\r\n                        foreach ($_v in Get-ChildItem ($_varName + '*') | sort Name)\r\n                        { \r\n                            $_nameFound = $_v.name\r\n                            $(if ($_nameFound.IndexOfAny($_varsRequiringQuotes) -eq -1) {'{0}{1}{2}'}\r\n                            else {'{0}{{{1}{2}}}'}) -f $_prefix, $_provider, $_nameFound\r\n                        }\r\n                        break;\r\n                    }\r\n\r\n                    # Do completion on parameters...\r\n                    '^-([*\\w0-9]*)' {\r\n                        $_pat = $matches[1] + '*'\r\n\r\n                        # extract the command name from the string\r\n                        # first split the string into statements and pipeline elements\r\n                        # This doesn't handle strings however.\r\n                        $_command = [regex]::Split($line, '[|;=]')[-1]\r\n\r\n                        #  Extract the trailing unclosed block e.g. ls | foreach { cp\r\n                        if ($_command -match '\\{([^\\{\\}]*)$')\r\n                        {\r\n                            $_command = $matches[1]\r\n                        }\r\n\r\n                        # Extract the longest unclosed parenthetical expression...\r\n                        if ($_command -match '\\(([^()]*)$')\r\n                        {\r\n                            $_command = $matches[1]\r\n                        }\r\n\r\n                        # take the first space separated token of the remaining string\r\n                        # as the command to look up. Trim any leading or trailing spaces\r\n                        # so you don't get leading empty elements.\r\n                        $_command = $_command.TrimEnd('-')\r\n                        $_command,$_arguments = $_command.Trim().Split()\r\n\r\n                        # now get the info object for it, -ArgumentList will force aliases to be resolved\r\n                        # it also retrieves dynamic parameters\r\n                        try\r\n                        {\r\n                            $_command = @(Get-Command -type 'Alias,Cmdlet,Function,Filter,ExternalScript' `\r\n                                -Name $_command -ArgumentList $_arguments)[0]\r\n                        }\r\n                        catch\r\n                        {\r\n                            # see if the command is an alias. If so, resolve it to the real command\r\n                            if(Test-Path alias:\\$_command)\r\n                            {\r\n                                $_command = @(Get-Command -Type Alias $_command)[0].Definition\r\n                            }\r\n\r\n                            # If we were unsuccessful retrieving the command, try again without the parameters\r\n                            $_command = @(Get-Command -type 'Cmdlet,Function,Filter,ExternalScript' `\r\n                                -Name $_command)[0]\r\n                        }\r\n\r\n                        # remove errors generated by the command not being found, and break\r\n                        if(-not $_command) { $error.RemoveAt(0); break; }\r\n\r\n                        # expand the parameter sets and emit the matching elements\r\n                        # need to use psbase.Keys in case 'keys' is one of the parameters\r\n                        # to the cmdlet\r\n                        foreach ($_n in $_command.Parameters.psbase.Keys)\r\n                        {\r\n                            if ($_n -like $_pat) { '-' + $_n }\r\n                        }\r\n                        break;\r\n                    }\r\n\r\n                    # Tab complete against history either #<pattern> or #<id>\r\n                    '^#(\\w*)' {\r\n                        $_pattern = $matches[1]\r\n                        if ($_pattern -match '^[0-9]+$')\r\n                        {\r\n                            Get-History -ea SilentlyContinue -Id $_pattern | Foreach { $_.CommandLine } \r\n                        }\r\n                        else\r\n                        {\r\n                            $_pattern = '*' + $_pattern + '*'\r\n                            Get-History -Count 32767 | Sort-Object -Descending Id| Foreach { $_.CommandLine } | where { $_ -like $_pattern }\r\n                        }\r\n                        break;\r\n                    }\r\n\r\n                    # try to find a matching command...\r\n                    default {\r\n                        # parse the script...\r\n                        $_tokens = [System.Management.Automation.PSParser]::Tokenize($line,\r\n                            [ref] $null)\r\n\r\n                        if ($_tokens)\r\n                        {\r\n                            $_lastToken = $_tokens[$_tokens.count - 1]\r\n                            if ($_lastToken.Type -eq 'Command')\r\n                            {\r\n                                $_cmd = $_lastToken.Content\r\n\r\n                                # don't look for paths...\r\n                                if ($_cmd.IndexOfAny('/\\:') -eq -1)\r\n                                {\r\n                                    # handle parsing errors - the last token string should be the last\r\n                                    # string in the line...\r\n                                    if ($lastword.Length -ge $_cmd.Length -and \r\n                                        $lastword.substring($lastword.length-$_cmd.length) -eq $_cmd)\r\n                                    {\r\n                                        $_pat = $_cmd + '*'\r\n                                        $_base = $lastword.substring(0, $lastword.length-$_cmd.length)\r\n\r\n                                        # get files in current directory first, then look for commands...\r\n                                        $( try {Resolve-Path -ea SilentlyContinue -Relative $_pat } catch {} ;\r\n                                           try { $ExecutionContext.InvokeCommand.GetCommandName($_pat, $true, $false) |\r\n                                               Sort-Object -Unique } catch {} ) |\r\n                                                   # If the command contains non-word characters (space, ) ] ; ) etc.)\r\n                                                   # then it needs to be quoted and prefixed with &\r\n                                                   ForEach-Object {\r\n                                                        if ($_.IndexOfAny($_charsRequiringQuotes) -eq -1) { $_ }\r\n                                                        elseif ($_.IndexOf('''') -ge 0) {'& ''{0}''' -f $_.Replace('''','''''') }\r\n                                                        else { '& ''{0}''' -f $_ }} |\r\n                                                   ForEach-Object {'{0}{1}' -f $_base,$_ }\r\n                                    }\r\n                                }\r\n                            }\r\n                        }\r\n                    }\r\n                }\r\n            }\r\n        ";
    private static string ImportSystemModulesText = "\r\n            $SnapIns = @(Get-PSSnapin -Registered -ErrorAction SilentlyContinue)\r\n            $Modules = @(Get-Module -ListAvailable -ErrorAction SilentlyContinue | ? { $_.ModuleBase -like \"$pshome*\" })\r\n            Import-LocalizedData -BindingVariable Messages -BaseDirectory $pshome -FileName ImportAllModules.psd1\r\n            $PreviousErrorCount = $error.Count\r\n\r\n            $LoadedModules = 0\r\n            $TotalModules = $SnapIns.Count + $Modules.Count\r\n\r\n            $SnapIns | % {\r\n                $LoadedModules ++\r\n                $Percentage = ($LoadedModules/$TotalModules) * 100\r\n                Write-Progress -Activity $Messages.LoadingSnapins -Status $_.Name -PercentComplete $Percentage\r\n                Add-PSSnapin $_ -ErrorAction SilentlyContinue\r\n            }\r\n\r\n            $Modules | % {\r\n                $LoadedModules ++\r\n                $Percentage = ($LoadedModules/$TotalModules) * 100\r\n                Write-Progress -Activity $Messages.ImportingModules -Status $_.Name -PercentComplete $Percentage\r\n\r\n                try\r\n                {\r\n                    Import-Module $_.Name -ErrorAction SilentlyContinue\r\n                }\r\n                catch [System.Management.Automation.PsSecurityException] { Write-Warning $_; $GLOBAL:error.RemoveAt(0) }\r\n            }\r\n\r\n            if ($error.Count -gt $PreviousErrorCount)\r\n            {\r\n                 Write-Host $Messages.ErrorInImport\r\n            }\r\n        ";
    internal static readonly ScopedItemLookupPath debugPreferenceVariablePath = new ScopedItemLookupPath("DebugPreference");
    internal static readonly ScopedItemLookupPath errorActionPreferenceVariablePath = new ScopedItemLookupPath("ErrorActionPreference");
    internal static readonly ScopedItemLookupPath progressPreferenceVariablePath = new ScopedItemLookupPath("ProgressPreference");
    internal static readonly ScopedItemLookupPath verbosePreferenceVariablePath = new ScopedItemLookupPath("VerbosePreference");
    internal static readonly ScopedItemLookupPath warningPreferenceVariablePath = new ScopedItemLookupPath("WarningPreference");
    internal static readonly ScopedItemLookupPath whatIfPreferenceVariablePath = new ScopedItemLookupPath("WhatIfPreference");
    internal static readonly ScopedItemLookupPath confirmPreferenceVariablePath = new ScopedItemLookupPath("ConfirmPreference");
    internal static SessionStateVariableEntry[] BuiltInVariables = new SessionStateVariableEntry[22]
    {
      new SessionStateVariableEntry("$", (object) null, string.Empty),
      new SessionStateVariableEntry("^", (object) null, string.Empty),
      new SessionStateVariableEntry("StackTrace", (object) null, string.Empty),
      new SessionStateVariableEntry("OutputEncoding", (object) Encoding.ASCII, ResourceManagerCache.GetResourceString("RunspaceInit", "OutputEncodingDescription"), ScopedItemOptions.None, (Attribute) new ArgumentTypeConverterAttribute(new Type[1]
      {
        typeof (Encoding)
      })),
      new SessionStateVariableEntry(InitialSessionState.confirmPreferenceVariablePath.LookupPath.NamespaceSpecificString, (object) ConfirmImpact.High, ResourceManagerCache.GetResourceString("RunspaceInit", "ConfirmPreferenceDescription"), ScopedItemOptions.None, (Attribute) new ArgumentTypeConverterAttribute(new Type[1]
      {
        typeof (ConfirmImpact)
      })),
      new SessionStateVariableEntry(InitialSessionState.debugPreferenceVariablePath.LookupPath.NamespaceSpecificString, (object) ActionPreference.SilentlyContinue, ResourceManagerCache.GetResourceString("RunspaceInit", "DebugPreferenceDescription"), ScopedItemOptions.None, (Attribute) new ArgumentTypeConverterAttribute(new Type[1]
      {
        typeof (ActionPreference)
      })),
      new SessionStateVariableEntry(InitialSessionState.errorActionPreferenceVariablePath.LookupPath.NamespaceSpecificString, (object) ActionPreference.Continue, ResourceManagerCache.GetResourceString("RunspaceInit", "ErrorActionPreferenceDescription"), ScopedItemOptions.None, (Attribute) new ArgumentTypeConverterAttribute(new Type[1]
      {
        typeof (ActionPreference)
      })),
      new SessionStateVariableEntry(InitialSessionState.progressPreferenceVariablePath.LookupPath.NamespaceSpecificString, (object) ActionPreference.Continue, ResourceManagerCache.GetResourceString("RunspaceInit", "ProgressPreferenceDescription"), ScopedItemOptions.None, (Attribute) new ArgumentTypeConverterAttribute(new Type[1]
      {
        typeof (ActionPreference)
      })),
      new SessionStateVariableEntry(InitialSessionState.verbosePreferenceVariablePath.LookupPath.NamespaceSpecificString, (object) ActionPreference.SilentlyContinue, ResourceManagerCache.GetResourceString("RunspaceInit", "VerbosePreferenceDescription"), ScopedItemOptions.None, (Attribute) new ArgumentTypeConverterAttribute(new Type[1]
      {
        typeof (ActionPreference)
      })),
      new SessionStateVariableEntry(InitialSessionState.warningPreferenceVariablePath.LookupPath.NamespaceSpecificString, (object) ActionPreference.Continue, ResourceManagerCache.GetResourceString("RunspaceInit", "WarningPreferenceDescription"), ScopedItemOptions.None, (Attribute) new ArgumentTypeConverterAttribute(new Type[1]
      {
        typeof (ActionPreference)
      })),
      new SessionStateVariableEntry("ErrorView", (object) "NormalView", ResourceManagerCache.GetResourceString("RunspaceInit", "ErrorViewDescription")),
      new SessionStateVariableEntry("NestedPromptLevel", (object) 0, ResourceManagerCache.GetResourceString("RunspaceInit", "NestedPromptLevelDescription")),
      new SessionStateVariableEntry("ReportErrorShowExceptionClass", (object) 0, ResourceManagerCache.GetResourceString("RunspaceInit", "ReportErrorShowExceptionClassDescription")),
      new SessionStateVariableEntry("ReportErrorShowInnerException", (object) 0, ResourceManagerCache.GetResourceString("RunspaceInit", "ReportErrorShowInnerExceptionDescription")),
      new SessionStateVariableEntry("ReportErrorShowSource", (object) 1, ResourceManagerCache.GetResourceString("RunspaceInit", "ReportErrorShowSourceDescription")),
      new SessionStateVariableEntry("ReportErrorShowStackTrace", (object) 0, ResourceManagerCache.GetResourceString("RunspaceInit", "ReportErrorShowStackTraceDescription")),
      new SessionStateVariableEntry(InitialSessionState.whatIfPreferenceVariablePath.LookupPath.NamespaceSpecificString, (object) false, ResourceManagerCache.GetResourceString("RunspaceInit", "WhatIfPreferenceDescription")),
      new SessionStateVariableEntry(nameof (FormatEnumerationLimit), (object) 4, ResourceManagerCache.GetResourceString("RunspaceInit", "FormatEnunmerationLimitDescription")),
      new SessionStateVariableEntry("PSEmailServer", (object) string.Empty, ResourceManagerCache.GetResourceString("RunspaceInit", "PSEmailServerDescription")),
      new SessionStateVariableEntry("PSSessionOption", (object) new PSSessionOption(), PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.PSDefaultSessionOptionDescription), ScopedItemOptions.None),
      new SessionStateVariableEntry("PSSessionConfigurationName", (object) "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.PSSessionConfigurationName), ScopedItemOptions.None),
      new SessionStateVariableEntry("PSSessionApplicationName", (object) "wsman", PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.PSSessionAppName), ScopedItemOptions.None)
    };
    internal static SessionStateFunctionEntry[] BuiltInFunctions = new SessionStateFunctionEntry[37]
    {
      new SessionStateFunctionEntry("prompt", "$(if (test-path variable:/PSDebugContext) { '[DBG]: ' } else { '' }) + 'PS ' + $(Get-Location) + $(if ($nestedpromptlevel -ge 1) { '>>' }) + '> '"),
      new SessionStateFunctionEntry("TabExpansion", InitialSessionState.TabExpansionFunctionText),
      new SessionStateFunctionEntry("Clear-Host", "$space = New-Object System.Management.Automation.Host.BufferCell\n$space.Character = ' '\n$space.ForegroundColor = $host.ui.rawui.ForegroundColor\n$space.BackgroundColor = $host.ui.rawui.BackgroundColor\n$rect = New-Object System.Management.Automation.Host.Rectangle\n$rect.Top = $rect.Bottom = $rect.Right = $rect.Left = -1\n$origin = New-Object System.Management.Automation.Host.Coordinates\n$Host.UI.RawUI.CursorPosition = $origin\n$Host.UI.RawUI.SetBufferContents($rect, $space)\n"),
      new SessionStateFunctionEntry("more", "param([string[]]$paths)\n\n$OutputEncoding = [System.Console]::OutputEncoding\n\nif($paths)\n{\n    foreach ($file in $paths)\n    {\n        Get-Content $file | more.com\n    }\n}\nelse\n{\n    $input | more.com\n}\n"),
      new SessionStateFunctionEntry("help", InitialSessionState.GetHelpPagingFunctionText()),
      new SessionStateFunctionEntry("mkdir", InitialSessionState.GetMkdirFunctionText()),
      new SessionStateFunctionEntry("Disable-PSRemoting", InitialSessionState.GetDisablePSRemotingFunctionText()),
      new SessionStateFunctionEntry("Get-Verb", InitialSessionState.GetGetVerbText()),
      new SessionStateFunctionEntry("A:", "Set-Location A:"),
      new SessionStateFunctionEntry("B:", "Set-Location B:"),
      new SessionStateFunctionEntry("C:", "Set-Location C:"),
      new SessionStateFunctionEntry("D:", "Set-Location D:"),
      new SessionStateFunctionEntry("E:", "Set-Location E:"),
      new SessionStateFunctionEntry("F:", "Set-Location F:"),
      new SessionStateFunctionEntry("G:", "Set-Location G:"),
      new SessionStateFunctionEntry("H:", "Set-Location H:"),
      new SessionStateFunctionEntry("I:", "Set-Location I:"),
      new SessionStateFunctionEntry("J:", "Set-Location J:"),
      new SessionStateFunctionEntry("K:", "Set-Location K:"),
      new SessionStateFunctionEntry("L:", "Set-Location L:"),
      new SessionStateFunctionEntry("M:", "Set-Location M:"),
      new SessionStateFunctionEntry("N:", "Set-Location N:"),
      new SessionStateFunctionEntry("O:", "Set-Location O:"),
      new SessionStateFunctionEntry("P:", "Set-Location P:"),
      new SessionStateFunctionEntry("Q:", "Set-Location Q:"),
      new SessionStateFunctionEntry("R:", "Set-Location R:"),
      new SessionStateFunctionEntry("S:", "Set-Location S:"),
      new SessionStateFunctionEntry("T:", "Set-Location T:"),
      new SessionStateFunctionEntry("U:", "Set-Location U:"),
      new SessionStateFunctionEntry("V:", "Set-Location V:"),
      new SessionStateFunctionEntry("W:", "Set-Location W:"),
      new SessionStateFunctionEntry("X:", "Set-Location X:"),
      new SessionStateFunctionEntry("Y:", "Set-Location Y:"),
      new SessionStateFunctionEntry("Z:", "Set-Location Z:"),
      new SessionStateFunctionEntry("cd..", "Set-Location .."),
      new SessionStateFunctionEntry("cd\\", "Set-Location \\"),
      new SessionStateFunctionEntry("ImportSystemModules", InitialSessionState.ImportSystemModulesText)
    };
    private static PSTraceSource _PSSnapInTracer = PSTraceSource.GetTracer("PSSnapInLoadUnload", "Loading and unloading mshsnapins", false);

    private static void RemoveDisallowedEntries<T>(
      InitialSessionStateEntryCollection<T> list,
      List<string> allowedNames,
      Converter<T, string> nameGetter)
      where T : InitialSessionStateEntry
    {
      List<string> stringList = new List<string>();
      foreach (T input in (IEnumerable<T>) list)
      {
        string entryName = nameGetter(input);
        if (!allowedNames.Exists((Predicate<string>) (allowedName => allowedName.Equals(entryName, StringComparison.OrdinalIgnoreCase))))
          stringList.Add(input.Name);
      }
      foreach (string name in stringList)
        list.Remove(name, (object) null);
    }

    private static void MakeDisallowedEntriesPrivate<T>(
      InitialSessionStateEntryCollection<T> list,
      List<string> allowedNames,
      Converter<T, string> nameGetter)
      where T : ConstrainedSessionStateEntry
    {
      foreach (T input in (IEnumerable<T>) list)
      {
        string entryName = nameGetter(input);
        if (!allowedNames.Exists((Predicate<string>) (allowedName => allowedName.Equals(entryName, StringComparison.OrdinalIgnoreCase))))
          input.Visibility = SessionStateEntryVisibility.Private;
      }
    }

    public static InitialSessionState CreateRestricted(
      SessionCapabilities sessionCapabilities)
    {
      return SessionCapabilities.RemoteServer == (sessionCapabilities & SessionCapabilities.RemoteServer) ? InitialSessionState.CreateRestrictedForRemoteServer() : InitialSessionState.Create();
    }

    private static InitialSessionState CreateRestrictedForRemoteServer()
    {
      InitialSessionState initialSessionState = InitialSessionState.Create();
      initialSessionState.LanguageMode = PSLanguageMode.NoLanguage;
      initialSessionState.ThrowOnRunspaceOpenError = true;
      initialSessionState.UseFullLanguageModeInDebugger = false;
      List<string> stringList = new List<string>();
      stringList.Add("Microsoft.PowerShell.Core");
      stringList.Add("Microsoft.PowerShell.Utility");
      stringList.Add("Microsoft.PowerShell.Security");
      using (IEnumerator<PSSnapInInfo> enumerator = PSSnapInReader.ReadEnginePSSnapIns().GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          PSSnapInInfo si = enumerator.Current;
          if (stringList.Exists((Predicate<string>) (allowed => allowed.Equals(si.Name, StringComparison.OrdinalIgnoreCase))))
            initialSessionState.ImportPSSnapIn(si, out PSSnapInException _);
        }
      }
      InitialSessionState.MakeDisallowedEntriesPrivate<SessionStateCommandEntry>(initialSessionState.Commands, new List<string>()
      {
        "Get-Command",
        "Get-FormatData",
        "Select-Object",
        "Get-Help",
        "Measure-Object",
        "Out-Default",
        "Exit-PSSession"
      }, (Converter<SessionStateCommandEntry, string>) (commandEntry => commandEntry.Name));
      InitialSessionState.RemoveDisallowedEntries<SessionStateFormatEntry>(initialSessionState.Formats, new List<string>()
      {
        "Certificate.Format.ps1xml",
        "Diagnostics.format.ps1xml",
        "DotNetTypes.Format.ps1xml",
        "FileSystem.Format.ps1xml",
        "Help.Format.ps1xml",
        "PowerShellCore.format.ps1xml",
        "PowerShellTrace.format.ps1xml",
        "Registry.format.ps1xml",
        "WSMan.format.ps1xml"
      }, (Converter<SessionStateFormatEntry, string>) (formatEntry => Path.GetFileName(formatEntry.FileName)));
      InitialSessionState.RemoveDisallowedEntries<SessionStateTypeEntry>(initialSessionState.Types, new List<string>()
      {
        "types.ps1xml"
      }, (Converter<SessionStateTypeEntry, string>) (typeEntry => Path.GetFileName(typeEntry.FileName)));
      initialSessionState.Providers.Clear();
      initialSessionState.Variables.Clear();
      foreach (KeyValuePair<string, CommandMetadata> restrictedCommand in CommandMetadata.GetRestrictedCommands(SessionCapabilities.RemoteServer))
      {
        string key = restrictedCommand.Key;
        initialSessionState.Commands[key][0].Visibility = SessionStateEntryVisibility.Private;
        string definition = ProxyCommand.Create(restrictedCommand.Value);
        initialSessionState.Commands.Add((SessionStateCommandEntry) new SessionStateFunctionEntry(key, definition));
      }
      return initialSessionState;
    }

    protected InitialSessionState()
    {
    }

    public static InitialSessionState Create() => new InitialSessionState();

    public static InitialSessionState CreateDefault()
    {
      InitialSessionState initialSessionState = new InitialSessionState();
      initialSessionState.Variables.Add((IEnumerable<SessionStateVariableEntry>) InitialSessionState.BuiltInVariables);
      initialSessionState.Commands.Add((SessionStateCommandEntry) new SessionStateApplicationEntry("*"));
      initialSessionState.Commands.Add((SessionStateCommandEntry) new SessionStateScriptEntry("*"));
      initialSessionState.Commands.Add((IEnumerable<SessionStateCommandEntry>) InitialSessionState.BuiltInFunctions);
      initialSessionState.Commands.Add((IEnumerable<SessionStateCommandEntry>) InitialSessionState.BuiltInAliases);
      foreach (PSSnapInInfo readEnginePsSnapIn in PSSnapInReader.ReadEnginePSSnapIns())
      {
        try
        {
          initialSessionState.ImportPSSnapIn(readEnginePsSnapIn, out PSSnapInException _);
        }
        catch (PSSnapInException ex)
        {
          Console.WriteLine("ErrorLoading Snapin: {0}", (object) ex);
          throw;
        }
      }
      initialSessionState.LanguageMode = PSLanguageMode.FullLanguage;
      initialSessionState.AuthorizationManager = (AuthorizationManager) new PSAuthorizationManager(Utils.DefaultPowerShellShellID);
      return initialSessionState;
    }

    public InitialSessionState Clone()
    {
      InitialSessionState initialSessionState = new InitialSessionState();
      initialSessionState.Variables.Add((IEnumerable<SessionStateVariableEntry>) this.Variables.Clone());
      initialSessionState.Commands.Add((IEnumerable<SessionStateCommandEntry>) this.Commands.Clone());
      initialSessionState.Assemblies.Add((IEnumerable<SessionStateAssemblyEntry>) this.Assemblies.Clone());
      initialSessionState.Types.Add((IEnumerable<SessionStateTypeEntry>) this.Types.Clone());
      initialSessionState.Formats.Add((IEnumerable<SessionStateFormatEntry>) this.Formats.Clone());
      initialSessionState.Providers.Add((IEnumerable<SessionStateProviderEntry>) this.Providers.Clone());
      initialSessionState.AuthorizationManager = this.AuthorizationManager;
      initialSessionState.LanguageMode = this.LanguageMode;
      initialSessionState.UseFullLanguageModeInDebugger = this.UseFullLanguageModeInDebugger;
      initialSessionState.ApartmentState = this.ApartmentState;
      initialSessionState.ThreadOptions = this.ThreadOptions;
      initialSessionState.ThrowOnRunspaceOpenError = this.ThrowOnRunspaceOpenError;
      foreach (string key in this.ModulesToImport.Keys)
        initialSessionState.ModulesToImport[key] = 1;
      return initialSessionState;
    }

    public static InitialSessionState Create(string snapInName) => new InitialSessionState();

    public static InitialSessionState Create(
      string[] snapInNameCollection,
      out PSConsoleLoadException warning)
    {
      warning = (PSConsoleLoadException) null;
      return new InitialSessionState();
    }

    public static InitialSessionState CreateFrom(
      string snapInPath,
      out PSConsoleLoadException warnings)
    {
      warnings = (PSConsoleLoadException) null;
      return new InitialSessionState();
    }

    public static InitialSessionState CreateFrom(
      string[] snapInPathCollection,
      out PSConsoleLoadException warnings)
    {
      warnings = (PSConsoleLoadException) null;
      return new InitialSessionState();
    }

    public PSLanguageMode LanguageMode
    {
      get => this._languageMode;
      set => this._languageMode = value;
    }

    public bool UseFullLanguageModeInDebugger
    {
      get => this._useFullLanguageModeInDebugger;
      set => this._useFullLanguageModeInDebugger = value;
    }

    public ApartmentState ApartmentState
    {
      get => this.apartmentState;
      set => this.apartmentState = value;
    }

    public PSThreadOptions ThreadOptions
    {
      get => this.createThreadOptions;
      set => this.createThreadOptions = value;
    }

    public bool ThrowOnRunspaceOpenError
    {
      get => this.throwOnRunspaceOpenError;
      set => this.throwOnRunspaceOpenError = value;
    }

    public virtual AuthorizationManager AuthorizationManager
    {
      get => this._authorizationManager;
      set => this._authorizationManager = value;
    }

    public void ImportPSModule(string[] name)
    {
      if (name == null)
        throw new ArgumentNullException(nameof (name));
      foreach (string key in name)
        this._modulesToImport[key] = 1;
    }

    internal Dictionary<string, int> ModulesToImport => this._modulesToImport;

    public virtual InitialSessionStateEntryCollection<SessionStateAssemblyEntry> Assemblies
    {
      get
      {
        lock (this._syncObject)
        {
          if (this._assemblies == null)
            this._assemblies = new InitialSessionStateEntryCollection<SessionStateAssemblyEntry>();
        }
        return this._assemblies;
      }
    }

    public virtual InitialSessionStateEntryCollection<SessionStateTypeEntry> Types
    {
      get
      {
        lock (this._syncObject)
        {
          if (this._types == null)
            this._types = new InitialSessionStateEntryCollection<SessionStateTypeEntry>();
        }
        return this._types;
      }
    }

    public virtual InitialSessionStateEntryCollection<SessionStateFormatEntry> Formats
    {
      get
      {
        lock (this._syncObject)
        {
          if (this._formats == null)
            this._formats = new InitialSessionStateEntryCollection<SessionStateFormatEntry>();
        }
        return this._formats;
      }
    }

    public virtual InitialSessionStateEntryCollection<SessionStateProviderEntry> Providers
    {
      get
      {
        lock (this._syncObject)
        {
          if (this._providers == null)
            this._providers = new InitialSessionStateEntryCollection<SessionStateProviderEntry>();
        }
        return this._providers;
      }
    }

    public virtual InitialSessionStateEntryCollection<SessionStateCommandEntry> Commands
    {
      get
      {
        lock (this._syncObject)
        {
          if (this._commands == null)
            this._commands = new InitialSessionStateEntryCollection<SessionStateCommandEntry>();
        }
        return this._commands;
      }
    }

    public virtual InitialSessionStateEntryCollection<SessionStateVariableEntry> Variables
    {
      get
      {
        lock (this._syncObject)
        {
          if (this._variables == null)
            this._variables = new InitialSessionStateEntryCollection<SessionStateVariableEntry>();
        }
        return this._variables;
      }
    }

    internal void Bind(System.Management.Automation.ExecutionContext context, bool updateOnly) => this.Bind(context, updateOnly, (PSModuleInfo) null);

    internal void Bind(System.Management.Automation.ExecutionContext context, bool updateOnly, PSModuleInfo module)
    {
      lock (this._syncObject)
      {
        SessionStateInternal engineSessionState = context.EngineSessionState;
        if (!updateOnly)
        {
          engineSessionState.Applications.Clear();
          engineSessionState.Scripts.Clear();
        }
        foreach (SessionStateAssemblyEntry assembly in (IEnumerable<SessionStateAssemblyEntry>) this.Assemblies)
        {
          if (!string.IsNullOrEmpty(assembly.FileName))
          {
            Exception error = (Exception) null;
            if (context.AddAssembly(assembly.Name, assembly.FileName, out error) == null || error != null)
            {
              if (error == null)
                error = (Exception) new DllNotFoundException(ResourceManagerCache.FormatResourceString("modules", "ModuleAssemblyFound", (object) assembly.Name));
              if (!string.IsNullOrEmpty(context.ModuleBeingProcessed) && Path.GetExtension(context.ModuleBeingProcessed).Equals(".psd1", StringComparison.OrdinalIgnoreCase))
                throw error;
              context.ReportEngineStartupError(error.Message);
            }
          }
        }
        foreach (SessionStateCommandEntry command in (IEnumerable<SessionStateCommandEntry>) this.Commands)
        {
          command.SetModule(module);
          switch (command)
          {
            case SessionStateCmdletEntry entry:
              engineSessionState.AddSessionStateEntry(entry);
              continue;
            case SessionStateFunctionEntry entry:
              engineSessionState.AddSessionStateEntry(entry);
              continue;
            case SessionStateAliasEntry entry:
              engineSessionState.AddSessionStateEntry(entry);
              continue;
            case SessionStateApplicationEntry entry:
              engineSessionState.AddSessionStateEntry(entry);
              continue;
            case SessionStateScriptEntry entry:
              engineSessionState.AddSessionStateEntry(entry);
              continue;
            default:
              continue;
          }
        }
        foreach (SessionStateProviderEntry provider in (IEnumerable<SessionStateProviderEntry>) this.Providers)
          engineSessionState.AddSessionStateEntry(provider);
        foreach (SessionStateVariableEntry variable in (IEnumerable<SessionStateVariableEntry>) this.Variables)
          engineSessionState.AddSessionStateEntry(variable);
        try
        {
          this.UpdateTypes(context, updateOnly);
        }
        catch (RuntimeException ex)
        {
          MshLog.LogEngineHealthEvent(context, 103, (Exception) ex, Severity.Warning);
          if (this.ThrowOnRunspaceOpenError)
            throw;
          else
            context.ReportEngineStartupError(ex.Message);
        }
        try
        {
          this.UpdateFormats(context, updateOnly);
        }
        catch (RuntimeException ex)
        {
          MshLog.LogEngineHealthEvent(context, 103, (Exception) ex, Severity.Warning);
          if (this.ThrowOnRunspaceOpenError)
            throw;
          else
            context.ReportEngineStartupError(ex.Message);
        }
        if (updateOnly)
          return;
        engineSessionState.LanguageMode = this.LanguageMode;
      }
    }

    internal void Unbind(System.Management.Automation.ExecutionContext context)
    {
      lock (this._syncObject)
      {
        SessionStateInternal engineSessionState = context.EngineSessionState;
        foreach (SessionStateAssemblyEntry assembly in (IEnumerable<SessionStateAssemblyEntry>) this.Assemblies)
          context.RemoveAssembly(assembly.Name);
        Dictionary<string, List<CmdletInfo>> cmdletCache = context.TopLevelSessionState.CmdletCache;
        foreach (SessionStateCommandEntry command in (IEnumerable<SessionStateCommandEntry>) this.Commands)
        {
          if (command is SessionStateCmdletEntry stateCmdletEntry && cmdletCache.ContainsKey(stateCmdletEntry.Name))
          {
            List<CmdletInfo> cmdletInfoList = cmdletCache[stateCmdletEntry.Name];
            for (int index = cmdletInfoList.Count - 1; index >= 0; --index)
            {
              if (cmdletInfoList[index].ModuleName.Equals(command.PSSnapIn.Name))
                cmdletInfoList.RemoveAt(index);
            }
            if (cmdletInfoList.Count == 0)
              cmdletCache.Remove(stateCmdletEntry.Name);
          }
        }
        if (this._providers == null || this._providers.Count <= 0)
          return;
        Dictionary<string, List<ProviderInfo>> providers = context.TopLevelSessionState.Providers;
        foreach (SessionStateProviderEntry provider in (IEnumerable<SessionStateProviderEntry>) this._providers)
        {
          if (providers.ContainsKey(provider.Name))
          {
            List<ProviderInfo> providerInfoList = providers[provider.Name];
            for (int index = providerInfoList.Count - 1; index >= 0; --index)
            {
              ProviderInfo pi = providerInfoList[index];
              if (pi.ImplementingType == provider.ImplementingType)
              {
                InitialSessionState.RemoveAllDrivesForProvider(pi, context.TopLevelSessionState);
                providerInfoList.RemoveAt(index);
              }
            }
            if (providerInfoList.Count == 0)
              providers.Remove(provider.Name);
          }
        }
      }
    }

    internal void UpdateTypes(System.Management.Automation.ExecutionContext context, bool updateOnly)
    {
      bool clearTable = !updateOnly;
      foreach (SessionStateTypeEntry type in (IEnumerable<SessionStateTypeEntry>) this.Types)
      {
        string moduleName = "";
        if (type.PSSnapIn != null && !string.IsNullOrEmpty(type.PSSnapIn.Name))
          moduleName = type.PSSnapIn.Name;
        Collection<string> errors = new Collection<string>();
        if (type.TypeTable != null)
        {
          if (!clearTable || this.Types.Count != 1)
            throw InitialSessionState._tracer.NewInvalidOperationException("initialsessionstate", "TypeTableCannotCoExist");
          context.TypeTable = type.TypeTable;
        }
        else
          context.TypeTable.Update(moduleName, type.FileName, errors, clearTable, context.AuthorizationManager, (PSHost) context.EngineHostInterface);
        clearTable = false;
        foreach (string str in errors)
        {
          if (!string.IsNullOrEmpty(str))
            context.ReportEngineStartupError("ExtendedTypeSystem", "TypesXmlError", (object) str);
        }
      }
    }

    internal void UpdateFormats(System.Management.Automation.ExecutionContext context, bool update)
    {
      Collection<PSSnapInTypeAndFormatErrors> mshsnapins = new Collection<PSSnapInTypeAndFormatErrors>();
      InitialSessionStateEntryCollection<SessionStateFormatEntry> formats;
      if (update && context.InitialSessionState != null)
      {
        formats = context.InitialSessionState.Formats;
        formats.Add((IEnumerable<SessionStateFormatEntry>) this.Formats);
      }
      else
        formats = this.Formats;
      FormatTable formatTable = (FormatTable) null;
      foreach (SessionStateFormatEntry stateFormatEntry in (IEnumerable<SessionStateFormatEntry>) formats)
      {
        string psSnapinName = stateFormatEntry.FileName;
        PSSnapInInfo psSnapIn = stateFormatEntry.PSSnapIn;
        if (psSnapIn != null && !string.IsNullOrEmpty(psSnapIn.Name))
          psSnapinName = psSnapIn.Name;
        if (stateFormatEntry.Formattable != null)
          mshsnapins.Add(new PSSnapInTypeAndFormatErrors(psSnapinName, stateFormatEntry.Formattable));
        else
          mshsnapins.Add(new PSSnapInTypeAndFormatErrors(psSnapinName, stateFormatEntry.FileName));
        if (stateFormatEntry.Formattable != null)
        {
          if (formats.Count != 1)
            throw InitialSessionState._tracer.NewInvalidOperationException("initialsessionstate", "FormatTableCannotCoExist");
          formatTable = stateFormatEntry.Formattable;
        }
      }
      if (formatTable != null)
      {
        context.FormatDBManager = formatTable.FormatDBManager;
      }
      else
      {
        if (mshsnapins.Count <= 0)
          return;
        context.FormatDBManager.UpdateDataBase(mshsnapins, context.AuthorizationManager, (PSHost) context.EngineHostInterface);
        foreach (PSSnapInTypeAndFormatErrors typeAndFormatErrors in mshsnapins)
        {
          if (typeAndFormatErrors.Errors != null && typeAndFormatErrors.Errors.Count > 0)
          {
            foreach (string error in typeAndFormatErrors.Errors)
            {
              if (!string.IsNullOrEmpty(error))
                context.ReportEngineStartupError("FormatAndOut.XmlLoading", "FormatLoadingErrors", (object) error);
            }
          }
        }
      }
    }

    public PSSnapInInfo ImportPSSnapIn(string name, out PSSnapInException warning)
    {
      if (string.IsNullOrEmpty(name))
        InitialSessionState._PSSnapInTracer.NewArgumentNullException(nameof (name));
      PSSnapInInfo psSnapInInfo = PSSnapInReader.Read("2", name);
      if (!Utils.IsVersionSupported(psSnapInInfo.PSVersion.ToString()))
      {
        InitialSessionState._PSSnapInTracer.TraceError("MshSnapin {0} and current monad engine's versions don't match.", (object) name);
        throw InitialSessionState._PSSnapInTracer.NewArgumentException("mshSnapInID", "ConsoleInfoErrorStrings", "AddPSSnapInBadMonadVersion", (object) psSnapInInfo.PSVersion.ToString(), (object) "2.0");
      }
      return this.ImportPSSnapIn(psSnapInInfo, out warning);
    }

    internal PSSnapInInfo ImportPSSnapIn(
      PSSnapInInfo psSnapInInfo,
      out PSSnapInException warning)
    {
      foreach (SessionStateAssemblyEntry assembly in (IEnumerable<SessionStateAssemblyEntry>) this.Assemblies)
      {
        PSSnapInInfo psSnapIn = assembly.PSSnapIn;
        if (psSnapIn != null)
        {
          string assemblyName = assembly.PSSnapIn.AssemblyName;
          if (!string.IsNullOrEmpty(assemblyName) && string.Equals(assemblyName, psSnapInInfo.AssemblyName, StringComparison.OrdinalIgnoreCase))
          {
            warning = (PSSnapInException) null;
            return psSnapIn;
          }
        }
      }
      Dictionary<string, SessionStateCmdletEntry> cmdlets = (Dictionary<string, SessionStateCmdletEntry>) null;
      Dictionary<string, SessionStateProviderEntry> providers = (Dictionary<string, SessionStateProviderEntry>) null;
      if (psSnapInInfo == null)
      {
        ArgumentNullException argumentNullException = new ArgumentNullException(nameof (psSnapInInfo));
        InitialSessionState._PSSnapInTracer.TraceException((Exception) argumentNullException);
        throw argumentNullException;
      }
      if (!string.IsNullOrEmpty(psSnapInInfo.CustomPSSnapInType))
      {
        this.LoadCustomPSSnapIn(psSnapInInfo);
        warning = (PSSnapInException) null;
        return (PSSnapInInfo) null;
      }
      try
      {
        InitialSessionState._PSSnapInTracer.WriteLine("Loading assembly for psSnapIn {0}", (object) psSnapInInfo.Name);
        Assembly assembly = PSSnapInHelpers.LoadPSSnapInAssembly(psSnapInInfo, out cmdlets, out providers);
        if (assembly == null)
        {
          InitialSessionState._PSSnapInTracer.TraceError("Loading assembly for psSnapIn {0} failed", (object) psSnapInInfo.Name);
          warning = (PSSnapInException) null;
          return (PSSnapInInfo) null;
        }
        InitialSessionState._PSSnapInTracer.WriteLine("Loading assembly for psSnapIn {0} succeeded", (object) psSnapInInfo.Name);
        PSSnapInHelpers.AnalyzePSSnapInAssembly(assembly, psSnapInInfo.Name, psSnapInInfo, out cmdlets, out providers);
      }
      catch (PSSnapInException ex)
      {
        InitialSessionState._PSSnapInTracer.TraceException((Exception) ex);
        throw;
      }
      foreach (string type in psSnapInInfo.Types)
      {
        SessionStateTypeEntry sessionStateTypeEntry = new SessionStateTypeEntry(Path.Combine(psSnapInInfo.ApplicationBase, type));
        sessionStateTypeEntry.SetPSSnapIn(psSnapInInfo);
        this.Types.Add(sessionStateTypeEntry);
      }
      foreach (string format in psSnapInInfo.Formats)
      {
        SessionStateFormatEntry stateFormatEntry = new SessionStateFormatEntry(Path.Combine(psSnapInInfo.ApplicationBase, format));
        stateFormatEntry.SetPSSnapIn(psSnapInInfo);
        this.Formats.Add(stateFormatEntry);
      }
      SessionStateAssemblyEntry stateAssemblyEntry = new SessionStateAssemblyEntry(psSnapInInfo.AssemblyName, psSnapInInfo.AbsoluteModulePath);
      stateAssemblyEntry.SetPSSnapIn(psSnapInInfo);
      this.Assemblies.Add(stateAssemblyEntry);
      if (cmdlets != null)
      {
        foreach (SessionStateCommandEntry stateCommandEntry in cmdlets.Values)
          this.Commands.Add(stateCommandEntry);
      }
      if (providers != null)
      {
        foreach (SessionStateProviderEntry stateProviderEntry in providers.Values)
          this.Providers.Add(stateProviderEntry);
      }
      warning = (PSSnapInException) null;
      return psSnapInInfo;
    }

    private void LoadCustomPSSnapIn(PSSnapInInfo psSnapInInfo)
    {
      if (psSnapInInfo == null || string.IsNullOrEmpty(psSnapInInfo.CustomPSSnapInType))
        return;
      Dictionary<string, SessionStateCmdletEntry> cmdlets = (Dictionary<string, SessionStateCmdletEntry>) null;
      Dictionary<string, SessionStateProviderEntry> providers = (Dictionary<string, SessionStateProviderEntry>) null;
      Assembly assembly;
      try
      {
        InitialSessionState._PSSnapInTracer.WriteLine("Loading assembly for mshsnapin {0}", (object) psSnapInInfo.Name);
        assembly = PSSnapInHelpers.LoadPSSnapInAssembly(psSnapInInfo, out cmdlets, out providers);
        if (assembly == null)
        {
          InitialSessionState._PSSnapInTracer.TraceError("Loading assembly for mshsnapin {0} failed", (object) psSnapInInfo.Name);
          return;
        }
      }
      catch (PSSnapInException ex)
      {
        InitialSessionState._PSSnapInTracer.TraceException((Exception) ex);
        throw;
      }
      CustomPSSnapIn customPSSnapIn = (CustomPSSnapIn) null;
      try
      {
        if (assembly.GetType(psSnapInInfo.CustomPSSnapInType, true) != null)
          customPSSnapIn = (CustomPSSnapIn) assembly.CreateInstance(psSnapInInfo.CustomPSSnapInType);
        InitialSessionState._PSSnapInTracer.WriteLine("Loading assembly for mshsnapin {0} succeeded", (object) psSnapInInfo.Name);
      }
      catch (TypeLoadException ex)
      {
        InitialSessionState._PSSnapInTracer.TraceException((Exception) ex);
        throw new PSSnapInException(psSnapInInfo.Name, ex.Message);
      }
      catch (ArgumentException ex)
      {
        InitialSessionState._PSSnapInTracer.TraceException((Exception) ex);
        throw new PSSnapInException(psSnapInInfo.Name, ex.Message);
      }
      catch (MissingMethodException ex)
      {
        InitialSessionState._PSSnapInTracer.TraceException((Exception) ex);
        throw new PSSnapInException(psSnapInInfo.Name, ex.Message);
      }
      catch (InvalidCastException ex)
      {
        InitialSessionState._PSSnapInTracer.TraceException((Exception) ex);
        throw new PSSnapInException(psSnapInInfo.Name, ex.Message);
      }
      catch (TargetInvocationException ex)
      {
        InitialSessionState._PSSnapInTracer.TraceException((Exception) ex);
        if (ex.InnerException != null)
          throw new PSSnapInException(psSnapInInfo.Name, ex.InnerException.Message);
        throw new PSSnapInException(psSnapInInfo.Name, ex.Message);
      }
      this.MergeCustomPSSnapIn(psSnapInInfo, customPSSnapIn);
    }

    private void MergeCustomPSSnapIn(PSSnapInInfo psSnapInInfo, CustomPSSnapIn customPSSnapIn)
    {
      if (psSnapInInfo == null || customPSSnapIn == null)
        return;
      InitialSessionState._PSSnapInTracer.WriteLine("Merging configuration from custom mshsnapin {0}", (object) psSnapInInfo.Name);
      if (customPSSnapIn.Cmdlets != null)
      {
        foreach (CmdletConfigurationEntry cmdlet in customPSSnapIn.Cmdlets)
        {
          SessionStateCmdletEntry stateCmdletEntry = new SessionStateCmdletEntry(cmdlet.Name, cmdlet.ImplementingType, cmdlet.HelpFileName);
          stateCmdletEntry.SetPSSnapIn(psSnapInInfo);
          this.Commands.Add((SessionStateCommandEntry) stateCmdletEntry);
        }
      }
      if (customPSSnapIn.Providers != null)
      {
        foreach (ProviderConfigurationEntry provider in customPSSnapIn.Providers)
        {
          SessionStateProviderEntry stateProviderEntry = new SessionStateProviderEntry(provider.Name, provider.ImplementingType, provider.HelpFileName);
          stateProviderEntry.SetPSSnapIn(psSnapInInfo);
          this.Providers.Add(stateProviderEntry);
        }
      }
      if (customPSSnapIn.Types != null)
      {
        foreach (TypeConfigurationEntry type in customPSSnapIn.Types)
        {
          SessionStateTypeEntry sessionStateTypeEntry = new SessionStateTypeEntry(Path.Combine(psSnapInInfo.ApplicationBase, type.FileName));
          sessionStateTypeEntry.SetPSSnapIn(psSnapInInfo);
          this.Types.Add(sessionStateTypeEntry);
        }
      }
      if (customPSSnapIn.Formats != null)
      {
        foreach (FormatConfigurationEntry format in customPSSnapIn.Formats)
        {
          SessionStateFormatEntry stateFormatEntry = new SessionStateFormatEntry(Path.Combine(psSnapInInfo.ApplicationBase, format.FileName));
          stateFormatEntry.SetPSSnapIn(psSnapInInfo);
          this.Formats.Add(stateFormatEntry);
        }
      }
      this.Assemblies.Add(new SessionStateAssemblyEntry(psSnapInInfo.AssemblyName, psSnapInInfo.AbsoluteModulePath));
    }

    internal void ImportCmdletsFromAssembly(string fileName, out PSSnapInException warning)
    {
      Dictionary<string, SessionStateCmdletEntry> cmdlets = (Dictionary<string, SessionStateCmdletEntry>) null;
      Dictionary<string, SessionStateProviderEntry> providers = (Dictionary<string, SessionStateProviderEntry>) null;
      if (fileName == null)
      {
        ArgumentNullException argumentNullException = new ArgumentNullException(nameof (fileName));
        InitialSessionState._PSSnapInTracer.TraceException((Exception) argumentNullException);
        throw argumentNullException;
      }
      Assembly assembly;
      try
      {
        InitialSessionState._PSSnapInTracer.WriteLine("Loading assembly for psSnapIn {0}", (object) fileName);
        assembly = Assembly.LoadFrom(fileName);
        if (assembly == null)
        {
          InitialSessionState._PSSnapInTracer.TraceError("Loading assembly for psSnapIn {0} failed", (object) fileName);
          warning = (PSSnapInException) null;
          return;
        }
        InitialSessionState._PSSnapInTracer.WriteLine("Loading assembly for psSnapIn {0} succeeded", (object) fileName);
        PSSnapInHelpers.AnalyzePSSnapInAssembly(assembly, fileName, (PSSnapInInfo) null, out cmdlets, out providers);
      }
      catch (PSSnapInException ex)
      {
        InitialSessionState._PSSnapInTracer.TraceException((Exception) ex);
        throw;
      }
      this.Assemblies.Add(new SessionStateAssemblyEntry(assembly.FullName, assembly.Location));
      if (cmdlets != null)
      {
        foreach (SessionStateCommandEntry stateCommandEntry in cmdlets.Values)
          this.Commands.Add(stateCommandEntry);
      }
      if (providers != null)
      {
        foreach (SessionStateProviderEntry stateProviderEntry in providers.Values)
          this.Providers.Add(stateProviderEntry);
      }
      warning = (PSSnapInException) null;
    }

    internal void ImportCmdletsFromAssembly(Assembly assembly, out PSSnapInException warning)
    {
      if (assembly == null)
      {
        ArgumentNullException argumentNullException = new ArgumentNullException(nameof (assembly));
        InitialSessionState._PSSnapInTracer.TraceException((Exception) argumentNullException);
        throw argumentNullException;
      }
      Dictionary<string, SessionStateCmdletEntry> cmdlets = (Dictionary<string, SessionStateCmdletEntry>) null;
      Dictionary<string, SessionStateProviderEntry> providers = (Dictionary<string, SessionStateProviderEntry>) null;
      PSSnapInHelpers.AnalyzePSSnapInAssembly(assembly, assembly.Location, (PSSnapInInfo) null, out cmdlets, out providers);
      this.Assemblies.Add(new SessionStateAssemblyEntry(assembly.FullName, assembly.Location));
      if (cmdlets != null)
      {
        foreach (SessionStateCommandEntry stateCommandEntry in cmdlets.Values)
          this.Commands.Add(stateCommandEntry);
      }
      if (providers != null)
      {
        foreach (SessionStateProviderEntry stateProviderEntry in providers.Values)
          this.Providers.Add(stateProviderEntry);
      }
      warning = (PSSnapInException) null;
    }

    internal static string GetHelpPagingFunctionText()
    {
      CommandMetadata commandMetadata = new CommandMetadata(typeof (GetHelpCommand));
      return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\r\n<#\r\n.FORWARDHELPTARGETNAME Get-Help\r\n.FORWARDHELPCATEGORY Cmdlet\r\n#>\r\n{0}\r\nparam({1})\r\n\r\n      #Set the outputencoding to Console::OutputEncoding. More.com doesn't work well with Unicode.\r\n      $outputEncoding=[System.Console]::OutputEncoding\r\n\r\n      Get-Help @PSBoundParameters | more\r\n", (object) commandMetadata.GetDecl(), (object) commandMetadata.GetParamBlock());
    }

    internal static string GetMkdirFunctionText() => "\r\n<#\r\n.FORWARDHELPTARGETNAME New-Item\r\n.FORWARDHELPCATEGORY Cmdlet\r\n#>\r\n[CmdletBinding(DefaultParameterSetName='pathSet',\r\n    SupportsShouldProcess=$true,  \r\n    SupportsTransactions=$true,\r\n    ConfirmImpact='Medium')]\r\nparam(\r\n    [Parameter(ParameterSetName='nameSet', Position=0, ValueFromPipelineByPropertyName=$true)]\r\n    [Parameter(ParameterSetName='pathSet', Mandatory=$true, Position=0, ValueFromPipelineByPropertyName=$true)]\r\n    [System.String[]]\r\n    ${Path},\r\n\r\n    [Parameter(ParameterSetName='nameSet', Mandatory=$true, ValueFromPipelineByPropertyName=$true)]\r\n    [AllowNull()]\r\n    [AllowEmptyString()]\r\n    [System.String]\r\n    ${Name},\r\n\r\n    [Parameter(ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]\r\n    [System.Object]\r\n    ${Value},\r\n\r\n    [Switch]\r\n    ${Force},\r\n\r\n    [Parameter(ValueFromPipelineByPropertyName=$true)]\r\n    [System.Management.Automation.PSCredential]\r\n    ${Credential}\r\n)\r\n\r\nbegin {\r\n\r\n    try {\r\n        $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand('New-Item', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n        $scriptCmd = {& $wrappedCmd -Type Directory @PSBoundParameters }\r\n        $steppablePipeline = $scriptCmd.GetSteppablePipeline()\r\n        $steppablePipeline.Begin($PSCmdlet)\r\n    } catch {\r\n        throw\r\n    }\r\n\r\n}\r\n\r\nprocess {\r\n\r\n    try {\r\n        $steppablePipeline.Process($_)\r\n    } catch {\r\n        throw\r\n    }\r\n\r\n}\r\n\r\nend {\r\n\r\n    try {\r\n        $steppablePipeline.End()\r\n    } catch {\r\n        throw\r\n    }\r\n\r\n}\r\n\r\n";

    internal static string GetDisablePSRemotingFunctionText() => "\r\n[CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=\"High\")]\r\nparam(\r\n    [Parameter()]\r\n    [switch]\r\n    $force = ($false)\r\n)\r\n\r\nprocess\r\n{\r\n    # Disable all Session Configurations\r\n    try {\r\n      $PSBoundParameters.Add(\"Name\",\"*\")\r\n      Disable-PSSessionConfiguration @PSBoundParameters\r\n    } catch {\r\n       throw\r\n    }     \r\n}\r\n\r\n#.ExternalHelp System.Management.Automation.dll-Help.xml\r\n";

    internal static string GetGetVerbText() => "\r\nparam(\r\n    [Parameter(ValueFromPipeline=$true)]\r\n    [string[]]\r\n    $verb = '*'\r\n)\r\nbegin {\r\n    $allVerbs = [PSObject].Assembly.GetTypes() |\r\n        Where-Object {$_.Name -match '^Verbs.'} |\r\n        Get-Member -type Properties -static |\r\n        Select-Object @{\r\n            Name='Verb'\r\n            Expression = {$_.Name}\r\n        }, @{\r\n            Name='Group'\r\n            Expression = {\r\n                $str = \"$($_.TypeName)\"\r\n                $str.Substring($str.LastIndexOf('Verbs') + 5)\r\n            }                \r\n        }        \r\n}\r\nprocess {\r\n    foreach ($v in $verb) {\r\n        $allVerbs | Where-Object { $_.Verb -like $v }\r\n    }       \r\n}\r\n";

    internal static SessionStateAliasEntry[] BuiltInAliases => new SessionStateAliasEntry[137]
    {
      new SessionStateAliasEntry("ac", "Add-Content", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("asnp", "Add-PSSnapIn", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("clc", "Clear-Content", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("cli", "Clear-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("clp", "Clear-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("clv", "Clear-Variable", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("compare", "Compare-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("cpi", "Copy-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("cpp", "Copy-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("cvpa", "Convert-Path", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("dbp", "Disable-PSBreakpoint", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("diff", "Compare-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ebp", "Enable-PSBreakpoint", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("epal", "Export-Alias", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("epcsv", "Export-Csv", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("fc", "Format-Custom", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("fl", "Format-List", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("foreach", "ForEach-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("%", "ForEach-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ft", "Format-Table", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("fw", "Format-Wide", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gal", "Get-Alias", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gbp", "Get-PSBreakpoint", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gc", "Get-Content", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gci", "Get-ChildItem", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gcm", "Get-Command", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gdr", "Get-PSDrive", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gcs", "Get-PSCallStack", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ghy", "Get-History", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gi", "Get-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gl", "Get-Location", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gm", "Get-Member", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gmo", "Get-Module", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gp", "Get-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gps", "Get-Process", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("group", "Group-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gsv", "Get-Service", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gsnp", "Get-PSSnapIn", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gu", "Get-Unique", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gv", "Get-Variable", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gwmi", "Get-WmiObject", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("iex", "Invoke-Expression", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ihy", "Invoke-History", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ii", "Invoke-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ipmo", "Import-Module", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("iwmi", "Invoke-WMIMethod", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ipal", "Import-Alias", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ipcsv", "Import-Csv", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("measure", "Measure-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("mi", "Move-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("mp", "Move-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("nal", "New-Alias", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ndr", "New-PSDrive", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ni", "New-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("nv", "New-Variable", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("nmo", "New-Module", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("oh", "Out-Host", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ogv", "Out-GridView", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ise", "powershell_ise.exe", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rbp", "Remove-PSBreakpoint", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rdr", "Remove-PSDrive", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ri", "Remove-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rni", "Rename-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rnp", "Rename-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rp", "Remove-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rmo", "Remove-Module", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rsnp", "Remove-PSSnapin", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rv", "Remove-Variable", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rwmi", "Remove-WMIObject", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rvpa", "Resolve-Path", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("sal", "Set-Alias", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("sasv", "Start-Service", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("sbp", "Set-PSBreakpoint", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("sc", "Set-Content", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("select", "Select-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("si", "Set-Item", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("sl", "Set-Location", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("swmi", "Set-WMIInstance", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("sleep", "Start-Sleep", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("sort", "Sort-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("sp", "Set-ItemProperty", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("saps", "Start-Process", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("start", "Start-Process", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("spps", "Stop-Process", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("spsv", "Stop-Service", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("sv", "Set-Variable", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("tee", "Tee-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("where", "Where-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("?", "Where-Object", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("write", "Write-Output", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("cat", "Get-Content", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("cd", "Set-Location", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("clear", "Clear-Host", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("cp", "Copy-Item", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("h", "Get-History", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("history", "Get-History", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("kill", "Stop-Process", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("lp", "Out-Printer", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ls", "Get-ChildItem", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("man", "help", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("mount", "New-PSDrive", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("md", "mkdir", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("mv", "Move-Item", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("popd", "Pop-Location", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ps", "Get-Process", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("pushd", "Push-Location", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("pwd", "Get-Location", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("r", "Invoke-History", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rm", "Remove-Item", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rmdir", "Remove-Item", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("echo", "Write-Output", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("cls", "Clear-Host", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("chdir", "Set-Location", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("copy", "Copy-Item", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("del", "Remove-Item", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("dir", "Get-ChildItem", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("erase", "Remove-Item", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("move", "Move-Item", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rd", "Remove-Item", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ren", "Rename-Item", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("set", "Set-Variable", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("type", "Get-Content", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("icm", "Invoke-Command", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("clhy", "Clear-History", "", ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gjb", "Get-Job", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rcjb", "Receive-Job", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rjb", "Remove-Job", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("sajb", "Start-Job", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("spjb", "Stop-Job", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("wjb", "Wait-Job", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("nsn", "New-PSSession", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("gsn", "Get-PSSession", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("rsn", "Remove-PSSession", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("ipsn", "Import-PSSession", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("epsn", "Export-PSSession", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("etsn", "Enter-PSSession", "", ScopedItemOptions.AllScope),
      new SessionStateAliasEntry("exsn", "Exit-PSSession", "", ScopedItemOptions.AllScope)
    };

    internal static void RemoveAllDrivesForProvider(ProviderInfo pi, SessionStateInternal ssi)
    {
      foreach (PSDriveInfo drive in ssi.GetDrivesForProvider(pi.FullName))
      {
        try
        {
          ssi.RemoveDrive(drive, true, (string) null);
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
        }
      }
    }
  }
}
