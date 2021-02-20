// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HostUtilities
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace System.Management.Automation
{
  internal static class HostUtilities
  {
    [TraceSource("HostUtilities", "tracer for HostUtilities")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (HostUtilities), "tracer for HostUtilities");
    private static string checkForCommandInCurrentDirectoryScript = "\r\n        $foundSuggestion = $false\r\n        \r\n        if($lastError -and\r\n            ($lastError.Exception -is \"System.Management.Automation.CommandNotFoundException\"))\r\n        {\r\n            $escapedCommand = [System.Management.Automation.WildcardPattern]::Escape($lastError.TargetObject)\r\n            $foundSuggestion = @(Get-Command (Join-Path . $escapedCommand) -ErrorAction SilentlyContinue).Count -gt 0\r\n            if(-not $foundSuggestion)\r\n            {\r\n                $GLOBAL:error.RemoveAt(0)\r\n            }\r\n        }\r\n\r\n        $foundSuggestion\r\n        ";
    private static ArrayList suggestions = new ArrayList((ICollection) new Hashtable[3]
    {
      HostUtilities.NewSuggestion(1, "Transactions", SuggestionMatchType.Command, "^Start-Transaction", ResourceManagerCache.GetResourceString("SuggestionStrings", "Suggestion_StartTransaction"), true),
      HostUtilities.NewSuggestion(2, "Transactions", SuggestionMatchType.Command, "^Use-Transaction", ResourceManagerCache.GetResourceString("SuggestionStrings", "Suggestion_UseTransaction"), true),
      HostUtilities.NewSuggestion(3, "General", SuggestionMatchType.Dynamic, ScriptBlock.Create(HostUtilities.checkForCommandInCurrentDirectoryScript), ScriptBlock.Create(ResourceManagerCache.FormatResourceString("SuggestionStrings", "Suggestion_CommandExistsInCurrentDirectory", (object) "$($lastError.TargetObject)", (object) ".\\$($lastError.TargetObject)")), true)
    });

    internal static PSObject GetDollarProfile(
      string allUsersAllHosts,
      string allUsersCurrentHost,
      string currentUserAllHosts,
      string currentUserCurrentHost)
    {
      return new PSObject((object) currentUserCurrentHost)
      {
        Properties = {
          (PSPropertyInfo) new PSNoteProperty("AllUsersAllHosts", (object) allUsersAllHosts),
          (PSPropertyInfo) new PSNoteProperty("AllUsersCurrentHost", (object) allUsersCurrentHost),
          (PSPropertyInfo) new PSNoteProperty("CurrentUserAllHosts", (object) currentUserAllHosts),
          (PSPropertyInfo) new PSNoteProperty("CurrentUserCurrentHost", (object) currentUserCurrentHost)
        }
      };
    }

    public static PSCommand[] GetProfileCommands(string shellId) => HostUtilities.GetProfileCommands(shellId, false);

    internal static PSCommand[] GetProfileCommands(string shellId, bool useTestProfile)
    {
      List<PSCommand> psCommandList = new List<PSCommand>();
      string fullProfileFileName1 = HostUtilities.GetFullProfileFileName((string) null, false, useTestProfile);
      string fullProfileFileName2 = HostUtilities.GetFullProfileFileName(shellId, false, useTestProfile);
      string fullProfileFileName3 = HostUtilities.GetFullProfileFileName((string) null, true, useTestProfile);
      string fullProfileFileName4 = HostUtilities.GetFullProfileFileName(shellId, true, useTestProfile);
      PSObject dollarProfile = HostUtilities.GetDollarProfile(fullProfileFileName1, fullProfileFileName2, fullProfileFileName3, fullProfileFileName4);
      PSCommand psCommand1 = new PSCommand();
      psCommand1.AddCommand("set-variable");
      psCommand1.AddParameter("Name", (object) "profile");
      psCommand1.AddParameter("Value", (object) dollarProfile);
      psCommand1.AddParameter("Option", (object) ScopedItemOptions.None);
      psCommandList.Add(psCommand1);
      string[] strArray = new string[4]
      {
        fullProfileFileName1,
        fullProfileFileName2,
        fullProfileFileName3,
        fullProfileFileName4
      };
      foreach (string str in strArray)
      {
        if (File.Exists(str))
        {
          PSCommand psCommand2 = new PSCommand();
          psCommand2.AddCommand(str, false);
          psCommandList.Add(psCommand2);
        }
      }
      return psCommandList.ToArray();
    }

    internal static string GetFullProfileFileName(string shellId, bool forCurrentUser)
    {
      using (HostUtilities.tracer.TraceMethod())
        return HostUtilities.GetFullProfileFileName(shellId, forCurrentUser, false);
    }

    internal static string GetFullProfileFileName(
      string shellId,
      bool forCurrentUser,
      bool useTestProfile)
    {
      using (HostUtilities.tracer.TraceMethod())
      {
        string str = (string) null;
        string path1;
        if (forCurrentUser)
        {
          path1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Utils.ProductNameForDirectory);
        }
        else
        {
          path1 = HostUtilities.GetAllUsersFolderPath(shellId);
          if (string.IsNullOrEmpty(path1))
          {
            HostUtilities.tracer.WriteLine("could not locate all users folder", new object[0]);
            return "";
          }
        }
        string path2 = useTestProfile ? "profile_test.ps1" : "profile.ps1";
        if (!string.IsNullOrEmpty(shellId))
          path2 = shellId + "_" + path2;
        return str = Path.Combine(path1, path2);
      }
    }

    private static string GetAllUsersFolderPath(string shellId)
    {
      using (HostUtilities.tracer.TraceMethod())
      {
        string str = string.Empty;
        try
        {
          str = Utils.GetApplicationBase(shellId);
        }
        catch (SecurityException ex)
        {
          HostUtilities.tracer.TraceException((Exception) ex);
        }
        return str;
      }
    }

    internal static ArrayList GetSuggestion(Runspace runspace)
    {
      if (!(runspace is LocalRunspace localRunspace))
        return new ArrayList();
      bool markVariableValue = localRunspace.ExecutionContext.QuestionMarkVariableValue;
      HistoryInfo[] entries = localRunspace.History.GetEntries(-1L, 1L, (SwitchParameter) true);
      if (entries.Length == 0)
        return new ArrayList();
      HistoryInfo lastHistory = entries[0];
      ArrayList dollarErrorVariable = (ArrayList) localRunspace.GetExecutionContext.DollarErrorVariable;
      object lastError = (object) null;
      if (dollarErrorVariable.Count > 0)
      {
        lastError = (object) (dollarErrorVariable[0] as Exception);
        ErrorRecord errorRecord = (ErrorRecord) null;
        if (lastError == null)
          errorRecord = dollarErrorVariable[0] as ErrorRecord;
        else if (lastError is RuntimeException)
          errorRecord = ((RuntimeException) lastError).ErrorRecord;
        if (errorRecord != null && errorRecord.InvocationInfo != null)
          lastError = errorRecord.InvocationInfo.HistoryId != lastHistory.Id ? (object) null : (object) errorRecord;
      }
      Runspace runspace1 = (Runspace) null;
      bool flag = false;
      if (Runspace.DefaultRunspace != runspace)
      {
        runspace1 = Runspace.DefaultRunspace;
        flag = true;
        Runspace.DefaultRunspace = runspace;
      }
      ArrayList arrayList = (ArrayList) null;
      try
      {
        arrayList = HostUtilities.GetSuggestion(lastHistory, lastError, dollarErrorVariable);
      }
      finally
      {
        if (flag)
          Runspace.DefaultRunspace = runspace1;
      }
      localRunspace.ExecutionContext.QuestionMarkVariableValue = markVariableValue;
      return arrayList;
    }

    internal static ArrayList GetSuggestion(
      HistoryInfo lastHistory,
      object lastError,
      ArrayList errorList)
    {
      ArrayList arrayList = new ArrayList();
      PSModuleInfo invocationModule = new PSModuleInfo(true);
      invocationModule.SessionState.PSVariable.Set(nameof (lastHistory), (object) lastHistory);
      invocationModule.SessionState.PSVariable.Set(nameof (lastError), lastError);
      foreach (Hashtable suggestion in HostUtilities.suggestions)
      {
        int count = errorList.Count;
        if (LanguagePrimitives.IsTrue(suggestion[(object) "Enabled"]))
        {
          SuggestionMatchType suggestionMatchType = (SuggestionMatchType) LanguagePrimitives.ConvertTo(suggestion[(object) "MatchType"], typeof (SuggestionMatchType), (IFormatProvider) CultureInfo.InvariantCulture);
          if (suggestionMatchType == SuggestionMatchType.Dynamic)
          {
            if (!(suggestion[(object) "Rule"] is ScriptBlock sb))
            {
              suggestion[(object) "Enabled"] = (object) false;
              throw new ArgumentException(ResourceManagerCache.GetResourceString("SuggestionStrings", "RuleMustBeScriptBlock"), "Rule");
            }
            object obj;
            try
            {
              obj = invocationModule.Invoke(sb, (object[]) null);
            }
            catch (Exception ex)
            {
              CommandProcessorBase.CheckForSevereException(ex);
              suggestion[(object) "Enabled"] = (object) false;
              continue;
            }
            if (LanguagePrimitives.IsTrue(obj))
            {
              string suggestionText = HostUtilities.GetSuggestionText(suggestion[(object) "Suggestion"], invocationModule);
              if (!string.IsNullOrEmpty(suggestionText))
              {
                string str = string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, "Suggestion [{0},{1}]: {2}", (object) (int) suggestion[(object) "Id"], (object) (string) suggestion[(object) "Category"], (object) suggestionText);
                arrayList.Add((object) str);
              }
            }
          }
          else
          {
            string input = string.Empty;
            if (suggestionMatchType == SuggestionMatchType.Command)
              input = lastHistory.CommandLine;
            else if (suggestionMatchType == SuggestionMatchType.Error)
            {
              if (lastError != null)
                input = !(lastError is Exception exception) ? lastError.ToString() : exception.Message;
            }
            else
            {
              suggestion[(object) "Enabled"] = (object) false;
              throw new ArgumentException(ResourceManagerCache.GetResourceString("SuggestionStrings", "InvalidMatchType"), "MatchType");
            }
            if (Regex.IsMatch(input, (string) suggestion[(object) "Rule"], RegexOptions.IgnoreCase))
            {
              string suggestionText = HostUtilities.GetSuggestionText(suggestion[(object) "Suggestion"], invocationModule);
              if (!string.IsNullOrEmpty(suggestionText))
              {
                string str = string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, "Suggestion [{0},{1}]: {2}", (object) (int) suggestion[(object) "Id"], (object) (string) suggestion[(object) "Category"], (object) suggestionText);
                arrayList.Add((object) str);
              }
            }
          }
          if (errorList.Count != count)
            suggestion[(object) "Enabled"] = (object) false;
        }
      }
      return arrayList;
    }

    private static Hashtable NewSuggestion(
      int id,
      string category,
      SuggestionMatchType matchType,
      string rule,
      string suggestion,
      bool enabled)
    {
      return new Hashtable((IEqualityComparer) StringComparer.CurrentCultureIgnoreCase)
      {
        [(object) "Id"] = (object) id,
        [(object) "Category"] = (object) category,
        [(object) "MatchType"] = (object) matchType,
        [(object) "Rule"] = (object) rule,
        [(object) "Suggestion"] = (object) suggestion,
        [(object) "Enabled"] = (object) enabled
      };
    }

    private static Hashtable NewSuggestion(
      int id,
      string category,
      SuggestionMatchType matchType,
      ScriptBlock rule,
      ScriptBlock suggestion,
      bool enabled)
    {
      return new Hashtable((IEqualityComparer) StringComparer.CurrentCultureIgnoreCase)
      {
        [(object) "Id"] = (object) id,
        [(object) "Category"] = (object) category,
        [(object) "MatchType"] = (object) matchType,
        [(object) "Rule"] = (object) rule,
        [(object) "Suggestion"] = (object) suggestion,
        [(object) "Enabled"] = (object) enabled
      };
    }

    private static string GetSuggestionText(object suggestion, PSModuleInfo invocationModule)
    {
      if (!(suggestion is ScriptBlock))
        return (string) LanguagePrimitives.ConvertTo(suggestion, typeof (string), (IFormatProvider) Thread.CurrentThread.CurrentCulture);
      ScriptBlock sb = (ScriptBlock) suggestion;
      object valueToConvert;
      try
      {
        valueToConvert = invocationModule.Invoke(sb, (object[]) null);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        return string.Empty;
      }
      return (string) LanguagePrimitives.ConvertTo(valueToConvert, typeof (string), (IFormatProvider) Thread.CurrentThread.CurrentCulture);
    }

    internal static PSCredential CredUIPromptForCredential(
      string caption,
      string message,
      string userName,
      string targetName,
      PSCredentialTypes allowedCredentialTypes,
      PSCredentialUIOptions options,
      IntPtr parentHWND)
    {
      using (HostUtilities.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(caption))
          caption = ResourceManagerCache.GetResourceString("CredUI", "PromptForCredential_DefaultCaption");
        if (string.IsNullOrEmpty(message))
          message = ResourceManagerCache.GetResourceString("CredUI", "PromptForCredential_DefaultMessage");
        HostUtilities.CREDUI_INFO pUiInfo = new HostUtilities.CREDUI_INFO();
        pUiInfo.pszCaptionText = caption;
        pUiInfo.pszMessageText = message;
        StringBuilder pszUserName = new StringBuilder(userName, 513);
        StringBuilder pszPassword = new StringBuilder(256);
        int int32 = Convert.ToInt32(false);
        pUiInfo.cbSize = Marshal.SizeOf((object) pUiInfo);
        pUiInfo.hwndParent = parentHWND;
        HostUtilities.CREDUI_FLAGS dwFlags = HostUtilities.CREDUI_FLAGS.DO_NOT_PERSIST;
        if ((allowedCredentialTypes & PSCredentialTypes.Domain) != PSCredentialTypes.Domain)
        {
          dwFlags |= HostUtilities.CREDUI_FLAGS.GENERIC_CREDENTIALS;
          if ((options & PSCredentialUIOptions.AlwaysPrompt) == PSCredentialUIOptions.AlwaysPrompt)
            dwFlags |= HostUtilities.CREDUI_FLAGS.ALWAYS_SHOW_UI;
        }
        HostUtilities.CredUIReturnCodes credUiReturnCodes = HostUtilities.CredUIReturnCodes.ERROR_INVALID_PARAMETER;
        if (pszUserName.Length <= 513 && pszPassword.Length <= 256)
          credUiReturnCodes = HostUtilities.CredUIPromptForCredentials(ref pUiInfo, targetName, IntPtr.Zero, 0, pszUserName, 513, pszPassword, 256, ref int32, dwFlags);
        PSCredential psCredential;
        switch (credUiReturnCodes)
        {
          case HostUtilities.CredUIReturnCodes.NO_ERROR:
            string userName1 = (string) null;
            if (pszUserName != null)
              userName1 = pszUserName.ToString();
            SecureString password = new SecureString();
            for (int index = 0; index < pszPassword.Length; ++index)
            {
              password.AppendChar(pszPassword[index]);
              pszPassword[index] = char.MinValue;
            }
            psCredential = string.IsNullOrEmpty(userName1) ? (PSCredential) null : new PSCredential(userName1, password);
            break;
          case HostUtilities.CredUIReturnCodes.ERROR_CANCELLED:
            psCredential = (PSCredential) null;
            break;
          default:
            HostUtilities.tracer.TraceError("CredUIPromptForCredentials returned an error: " + credUiReturnCodes.ToString());
            goto case HostUtilities.CredUIReturnCodes.ERROR_CANCELLED;
        }
        return psCredential;
      }
    }

    [DllImport("credui", EntryPoint = "CredUIPromptForCredentialsW", CharSet = CharSet.Unicode)]
    private static extern HostUtilities.CredUIReturnCodes CredUIPromptForCredentials(
      ref HostUtilities.CREDUI_INFO pUiInfo,
      string pszTargetName,
      IntPtr Reserved,
      int dwAuthError,
      StringBuilder pszUserName,
      int ulUserNameMaxChars,
      StringBuilder pszPassword,
      int ulPasswordMaxChars,
      ref int pfSave,
      HostUtilities.CREDUI_FLAGS dwFlags);

    internal static string GetRemotePrompt(RemoteRunspace runspace, string basePrompt) => string.Format((IFormatProvider) CultureInfo.InvariantCulture, "[{0}]: {1}", (object) runspace.ConnectionInfo.ComputerName, (object) basePrompt);

    [System.Flags]
    private enum CREDUI_FLAGS
    {
      INCORRECT_PASSWORD = 1,
      DO_NOT_PERSIST = 2,
      REQUEST_ADMINISTRATOR = 4,
      EXCLUDE_CERTIFICATES = 8,
      REQUIRE_CERTIFICATE = 16, // 0x00000010
      SHOW_SAVE_CHECK_BOX = 64, // 0x00000040
      ALWAYS_SHOW_UI = 128, // 0x00000080
      REQUIRE_SMARTCARD = 256, // 0x00000100
      PASSWORD_ONLY_OK = 512, // 0x00000200
      VALIDATE_USERNAME = 1024, // 0x00000400
      COMPLETE_USERNAME = 2048, // 0x00000800
      PERSIST = 4096, // 0x00001000
      SERVER_CREDENTIAL = 16384, // 0x00004000
      EXPECT_CONFIRMATION = 131072, // 0x00020000
      GENERIC_CREDENTIALS = 262144, // 0x00040000
      USERNAME_TARGET_CREDENTIALS = 524288, // 0x00080000
      KEEP_USERNAME = 1048576, // 0x00100000
    }

    private struct CREDUI_INFO
    {
      public int cbSize;
      public IntPtr hwndParent;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string pszMessageText;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string pszCaptionText;
      public IntPtr hbmBanner;
    }

    private enum CredUIReturnCodes
    {
      NO_ERROR = 0,
      ERROR_INVALID_PARAMETER = 87, // 0x00000057
      ERROR_INSUFFICIENT_BUFFER = 122, // 0x0000007A
      ERROR_INVALID_FLAGS = 1004, // 0x000003EC
      ERROR_NOT_FOUND = 1168, // 0x00000490
      ERROR_CANCELLED = 1223, // 0x000004C7
      ERROR_NO_SUCH_LOGON_SESSION = 1312, // 0x00000520
      ERROR_INVALID_ACCOUNT_NAME = 1315, // 0x00000523
    }

    internal class DebuggerCommand
    {
      private DebuggerResumeAction? resumeAction;
      private string command;
      private bool repeatOnEnter;
      private bool executedByDebugger;

      public DebuggerCommand(
        string command,
        DebuggerResumeAction? action,
        bool repeatOnEnter,
        bool executedByDebugger)
      {
        this.resumeAction = action;
        this.command = command;
        this.repeatOnEnter = repeatOnEnter;
        this.executedByDebugger = executedByDebugger;
      }

      public DebuggerResumeAction? ResumeAction => this.resumeAction;

      public string Command => this.command;

      public bool RepeatOnEnter => this.repeatOnEnter;

      public bool ExecutedByDebugger => this.executedByDebugger;
    }

    internal class DebuggerCommandProcessor
    {
      private const string ContinueCommand = "continue";
      private const string ContinueShortcut = "c";
      private const string GetStackTraceShortcut = "k";
      private const string HelpCommand = "h";
      private const string HelpShortcut = "?";
      private const string ListCommand = "list";
      private const string ListShortcut = "l";
      private const string StepCommand = "stepInto";
      private const string StepShortcut = "s";
      private const string StepOutCommand = "stepOut";
      private const string StepOutShortcut = "o";
      private const string StepOverCommand = "stepOver";
      private const string StepOverShortcut = "v";
      private const string StopCommand = "quit";
      private const string StopShortcut = "q";
      private const int DefaultListLineCount = 16;
      private Dictionary<string, HostUtilities.DebuggerCommand> commandTable;
      private HostUtilities.DebuggerCommand helpCommand;
      private HostUtilities.DebuggerCommand listCommand;
      private HostUtilities.DebuggerCommand lastCommand;
      private string[] lines;
      private int lastLineDisplayed;

      public DebuggerCommandProcessor()
      {
        this.commandTable = new Dictionary<string, HostUtilities.DebuggerCommand>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        this.commandTable["stepInto"] = this.commandTable["s"] = new HostUtilities.DebuggerCommand("stepInto", new DebuggerResumeAction?(DebuggerResumeAction.StepInto), true, false);
        this.commandTable["stepOut"] = this.commandTable["o"] = new HostUtilities.DebuggerCommand("stepOut", new DebuggerResumeAction?(DebuggerResumeAction.StepOut), false, false);
        this.commandTable["stepOver"] = this.commandTable["v"] = new HostUtilities.DebuggerCommand("stepOver", new DebuggerResumeAction?(DebuggerResumeAction.StepOver), true, false);
        this.commandTable["continue"] = this.commandTable["c"] = new HostUtilities.DebuggerCommand("continue", new DebuggerResumeAction?(DebuggerResumeAction.Continue), false, false);
        this.commandTable["quit"] = this.commandTable["q"] = new HostUtilities.DebuggerCommand("quit", new DebuggerResumeAction?(DebuggerResumeAction.Stop), false, false);
        this.commandTable["k"] = new HostUtilities.DebuggerCommand("get-pscallstack", new DebuggerResumeAction?(), false, false);
        Dictionary<string, HostUtilities.DebuggerCommand> commandTable1 = this.commandTable;
        Dictionary<string, HostUtilities.DebuggerCommand> commandTable2 = this.commandTable;
        DebuggerResumeAction? action1 = new DebuggerResumeAction?();
        HostUtilities.DebuggerCommand debuggerCommand1;
        HostUtilities.DebuggerCommand debuggerCommand2 = debuggerCommand1 = this.helpCommand = new HostUtilities.DebuggerCommand("h", action1, false, true);
        commandTable2["?"] = debuggerCommand1;
        HostUtilities.DebuggerCommand debuggerCommand3 = debuggerCommand2;
        commandTable1["h"] = debuggerCommand3;
        Dictionary<string, HostUtilities.DebuggerCommand> commandTable3 = this.commandTable;
        Dictionary<string, HostUtilities.DebuggerCommand> commandTable4 = this.commandTable;
        DebuggerResumeAction? action2 = new DebuggerResumeAction?();
        HostUtilities.DebuggerCommand debuggerCommand4;
        HostUtilities.DebuggerCommand debuggerCommand5 = debuggerCommand4 = this.listCommand = new HostUtilities.DebuggerCommand("list", action2, true, true);
        commandTable4["l"] = debuggerCommand4;
        HostUtilities.DebuggerCommand debuggerCommand6 = debuggerCommand5;
        commandTable3["list"] = debuggerCommand6;
        this.commandTable[string.Empty] = new HostUtilities.DebuggerCommand(string.Empty, new DebuggerResumeAction?(), false, true);
      }

      public void Reset() => this.lines = (string[]) null;

      public HostUtilities.DebuggerCommand ProcessCommand(
        PSHost host,
        string command,
        InvocationInfo invocationInfo)
      {
        return this.lastCommand = this.DoProcessCommand(host, command, invocationInfo);
      }

      private HostUtilities.DebuggerCommand DoProcessCommand(
        PSHost host,
        string command,
        InvocationInfo invocationInfo)
      {
        if (command.Length == 0 && this.lastCommand != null && this.lastCommand.RepeatOnEnter)
        {
          if (this.lastCommand == this.listCommand)
          {
            if (this.lastLineDisplayed < this.lines.Length)
              this.DisplayScript(host, invocationInfo, this.lastLineDisplayed + 1, 16);
            return this.listCommand;
          }
          command = this.lastCommand.Command;
        }
        Match match = new Regex("^l(ist)?(\\s+(?<start>\\S+))?(\\s+(?<count>\\S+))?$", RegexOptions.IgnoreCase).Match(command);
        if (match.Success)
        {
          this.DisplayScript(host, invocationInfo, match);
          return this.listCommand;
        }
        HostUtilities.DebuggerCommand debuggerCommand = (HostUtilities.DebuggerCommand) null;
        if (!this.commandTable.TryGetValue(command, out debuggerCommand))
          return new HostUtilities.DebuggerCommand(command, new DebuggerResumeAction?(), false, false);
        if (debuggerCommand == this.helpCommand)
          this.DisplayHelp(host);
        return debuggerCommand;
      }

      private void DisplayHelp(PSHost host)
      {
        host.UI.WriteLine("");
        host.UI.WriteLine(this.FormatResourceString("StepHelp", (object) "s", (object) "stepInto"));
        host.UI.WriteLine(this.FormatResourceString("StepOverHelp", (object) "v", (object) "stepOver"));
        host.UI.WriteLine(this.FormatResourceString("StepOutHelp", (object) "o", (object) "stepOut"));
        host.UI.WriteLine("");
        host.UI.WriteLine(this.FormatResourceString("ContinueHelp", (object) "c", (object) "continue"));
        host.UI.WriteLine(this.FormatResourceString("StopHelp", (object) "q", (object) "quit"));
        host.UI.WriteLine("");
        host.UI.WriteLine(this.FormatResourceString("GetStackTraceHelp", (object) "k"));
        host.UI.WriteLine("");
        host.UI.WriteLine(this.FormatResourceString("ListHelp", (object) "l", (object) "list"));
        host.UI.WriteLine(this.FormatResourceString("AdditionalListHelp1"));
        host.UI.WriteLine(this.FormatResourceString("AdditionalListHelp2"));
        host.UI.WriteLine(this.FormatResourceString("AdditionalListHelp3"));
        host.UI.WriteLine("");
        host.UI.WriteLine(this.FormatResourceString("EnterHelp", (object) "stepInto", (object) "stepOver", (object) "list"));
        host.UI.WriteLine("");
        host.UI.WriteLine(this.FormatResourceString("HelpCommandHelp", (object) "?", (object) "h"));
        host.UI.WriteLine("\n");
        host.UI.WriteLine(this.FormatResourceString("PromptHelp"));
        host.UI.WriteLine("");
      }

      private void DisplayScript(PSHost host, InvocationInfo invocationInfo, Match match)
      {
        if (this.lines == null)
        {
          if (invocationInfo.ScriptToken == null || string.IsNullOrEmpty(invocationInfo.ScriptToken.Script))
          {
            host.UI.WriteErrorLine(this.FormatResourceString("NoSourceCode"));
            return;
          }
          this.lines = invocationInfo.ScriptToken.Script.Split(new string[2]
          {
            "\r\n",
            "\n"
          }, StringSplitOptions.None);
        }
        int start = Math.Max(invocationInfo.ScriptLineNumber - 5, 1);
        if (match.Groups["start"].Value.Length > 0)
        {
          try
          {
            start = int.Parse(match.Groups["start"].Value, (IFormatProvider) CultureInfo.CurrentCulture.NumberFormat);
          }
          catch
          {
            host.UI.WriteErrorLine(this.FormatResourceString("BadStartFormat", (object) this.lines.Length));
            return;
          }
          if (start <= 0 || start > this.lines.Length)
          {
            host.UI.WriteErrorLine(this.FormatResourceString("BadStartFormat", (object) this.lines.Length));
            return;
          }
        }
        int count = 16;
        if (match.Groups["count"].Value.Length > 0)
        {
          try
          {
            count = int.Parse(match.Groups["count"].Value, (IFormatProvider) CultureInfo.CurrentCulture.NumberFormat);
          }
          catch
          {
            host.UI.WriteErrorLine(this.FormatResourceString("BadCountFormat", (object) this.lines.Length));
            return;
          }
          if (count <= 0 || count > this.lines.Length)
          {
            host.UI.WriteErrorLine(this.FormatResourceString("BadCountFormat", (object) this.lines.Length));
            return;
          }
        }
        this.DisplayScript(host, invocationInfo, start, count);
      }

      private void DisplayScript(PSHost host, InvocationInfo invocationInfo, int start, int count)
      {
        host.UI.WriteLine();
        for (int index = start; index <= this.lines.Length && index < start + count; ++index)
        {
          PSHostUserInterface ui = host.UI;
          string str;
          if (index != invocationInfo.ScriptLineNumber)
            str = string.Format((IFormatProvider) CultureInfo.CurrentCulture, "{0,5}:  {1}", (object) index, (object) this.lines[index - 1]);
          else
            str = string.Format((IFormatProvider) CultureInfo.CurrentCulture, "{0,5}:* {1}", (object) index, (object) this.lines[index - 1]);
          ui.WriteLine(str);
          this.lastLineDisplayed = index;
        }
        host.UI.WriteLine();
      }

      private string FormatResourceString(string resource, params object[] args) => ResourceManagerCache.FormatResourceString("DebuggerStrings", resource, args);
    }
  }
}
