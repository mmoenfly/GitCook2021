// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandCompletionBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Management.Automation
{
  internal abstract class CommandCompletionBase
  {
    private const string wildcardMetachars = "[]?*";
    private string[] matchSet;
    private int currentMatchIndex;
    protected CommandCompletionBase.CompletionExecutionHelperBase exec;
    private int currentReplacementIndex;
    private string currentClosingQuote;
    private bool currentAddQuotes;
    private string lastCompletedInput;
    private static Regex cmdletTabRegex = new Regex("^[\\w\\*\\?]+-[\\w\\*\\?]*");
    private static char[] charsRequiringQuotedString = "`&@'#{}()$,;|<> \t".ToCharArray();
    [TraceSource("CommandCompletion", "Command completion functionality")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CommandCompletion", "Command completion functionality");

    protected CommandCompletionBase(
      CommandCompletionBase.CompletionExecutionHelperBase exec)
    {
      this.exec = exec;
      this.Reset();
    }

    internal void Reset()
    {
      this.matchSet = (string[]) null;
      this.currentMatchIndex = -1;
      this.currentReplacementIndex = 0;
      this.currentClosingQuote = string.Empty;
      this.currentAddQuotes = false;
      this.lastCompletedInput = (string) null;
    }

    internal string GetNextCompletionMatch(string input, bool lookForward)
    {
      try
      {
        switch (input)
        {
          case "":
          case null:
            return input + "    ";
          default:
            if (input.Trim().Length != 0)
            {
              if (this.exec.CancelTabCompletion || input.Length == 0 || input != this.lastCompletedInput)
              {
                this.exec.CancelTabCompletion = false;
                this.matchSet = this.GetFreshMatches(ref input, ref this.currentAddQuotes, out this.currentClosingQuote, ref this.currentReplacementIndex);
                this.currentMatchIndex = -1;
              }
              string completionText = (string) null;
              if (this.matchSet != null)
                completionText = CommandCompletionBase.GetNextMatch(lookForward, this.matchSet, ref this.currentMatchIndex);
              this.lastCompletedInput = CommandCompletionBase.ComposeCompletedInput(input, completionText, this.currentReplacementIndex, this.currentClosingQuote, this.currentAddQuotes);
              return this.lastCompletedInput;
            }
            goto case "";
        }
      }
      catch (Exception ex)
      {
        CommandCompletionBase.tracer.TraceException(ex);
        CommandProcessorBase.CheckForSevereException(ex);
        return string.Empty;
      }
    }

    internal bool IsCommandCompletionRunning => this.exec.IsRunning;

    private string[] GetFreshMatches(
      ref string input,
      ref bool addQuotes,
      out string closingQuote,
      ref int replacementIndex)
    {
      int startIndex = -1;
      if (input.Length > 2)
      {
        startIndex = "'\"".IndexOf(input[input.Length - 2]);
        if (startIndex != -1)
          input = input.Remove(input.Length - 2, 1);
      }
      string lastWord = new CommandCompletionBase.LastWordFinder(input).FindLastWord(out replacementIndex, out closingQuote);
      string[] matchSet = this.GenerateMatchSet(input, lastWord, CultureInfo.CurrentCulture, replacementIndex == 0, ref addQuotes);
      if (matchSet == null && startIndex != -1)
        input = input.Insert(input.Length - 1, "'\"".Substring(startIndex, 1));
      return matchSet;
    }

    private string[] GenerateMatchSet(
      string input,
      string lastWord,
      CultureInfo ci,
      bool completingAtStartOfLine,
      ref bool addQuotes)
    {
      bool isPSSnapInSpecified = false;
      bool flag = CommandCompletionBase.IsCommandLikeCmdlet(lastWord, out isPSSnapInSpecified);
      string[] strArray1 = (string[]) null;
      string[] matchSetFromCmdlet = this.GenerateMatchSetFromCmdlet(input, lastWord);
      if (matchSetFromCmdlet != null)
      {
        addQuotes = false;
        return matchSetFromCmdlet;
      }
      if (flag)
        strArray1 = this.GenerateMatchSetOfCmdlets(lastWord, isPSSnapInSpecified, ci);
      string[] matchSetOfFiles = this.GenerateMatchSetOfFiles(lastWord, ci, completingAtStartOfLine);
      string[] strArray2;
      if (strArray1 == null)
        strArray2 = matchSetOfFiles;
      else if (matchSetOfFiles == null)
      {
        strArray2 = strArray1;
      }
      else
      {
        strArray2 = new string[strArray1.Length + matchSetOfFiles.Length];
        matchSetOfFiles.CopyTo((Array) strArray2, 0);
        strArray1.CopyTo((Array) strArray2, matchSetOfFiles.Length);
      }
      addQuotes = true;
      return strArray2;
    }

    private static string ComposeCompletedInput(
      string input,
      string completionText,
      int replacementIndex,
      string closingQuote,
      bool addQuotes)
    {
      using (CommandCompletionBase.tracer.TraceMethod(completionText, new object[0]))
      {
        string format;
        if (completionText != null)
        {
          bool flag1 = closingQuote.Length == 0 && addQuotes && completionText.IndexOfAny(CommandCompletionBase.charsRequiringQuotedString) != -1;
          bool flag2 = flag1 && replacementIndex == 0;
          string str1 = flag1 ? "'" : closingQuote;
          StringBuilder stringBuilder = new StringBuilder("");
          string str2 = completionText;
          if (flag1 && str2.IndexOf('\'') != -1)
            str2 = str2.Replace("'", "''");
          if (flag2)
            stringBuilder.Append("& ");
          if (flag1)
            stringBuilder.Append("'");
          format = input.Substring(0, replacementIndex) + stringBuilder.ToString() + str2 + str1;
        }
        else
          format = input;
        CommandCompletionBase.tracer.WriteLine(format, new object[0]);
        return format;
      }
    }

    private static string GetNextMatch(
      bool lookForward,
      string[] matchSet,
      ref int currentMatchIndex)
    {
      using (CommandCompletionBase.tracer.TraceMethod())
      {
        string str = (string) null;
        if (matchSet != null && matchSet.Length > 0)
        {
          currentMatchIndex += lookForward ? 1 : -1;
          if (currentMatchIndex >= matchSet.Length)
            currentMatchIndex = 0;
          else if (currentMatchIndex < 0)
            currentMatchIndex = matchSet.Length - 1;
          str = matchSet[currentMatchIndex];
        }
        return str;
      }
    }

    private bool ShouldFullyQualifyPathsPath(string lastWord, string path)
    {
      if (lastWord.StartsWith("~", StringComparison.OrdinalIgnoreCase) || lastWord.StartsWith("\\", StringComparison.OrdinalIgnoreCase) || lastWord.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        return true;
      bool? resultAsBool = this.exec.ExecuteCommandAndGetResultAsBool(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "& {{ trap {{ continue }} ; split-path {0} -IsAbsolute }}", (object) path));
      return resultAsBool.HasValue && resultAsBool.Value;
    }

    private static bool IsCommandLikeCmdlet(string lastWord, out bool isPSSnapInSpecified)
    {
      isPSSnapInSpecified = false;
      string[] strArray = lastWord.Split('\\');
      if (strArray.Length == 1)
        return CommandCompletionBase.cmdletTabRegex.IsMatch(lastWord);
      if (strArray.Length == 2)
      {
        isPSSnapInSpecified = PSSnapInInfo.IsPSSnapinIdValid(strArray[0]);
        if (isPSSnapInSpecified)
          return CommandCompletionBase.cmdletTabRegex.IsMatch(strArray[1]);
      }
      return false;
    }

    private string[] GenerateMatchSetOfCmdlets(
      string lastWord,
      bool isPSSnapInSpecified,
      CultureInfo ci)
    {
      using (CommandCompletionBase.tracer.TraceMethod(lastWord, new object[0]))
      {
        string[] strArray = (string[]) null;
        Collection<PSObject> collection = this.exec.ExecuteCommand(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "& {{ trap {{ continue }} ; get-command '{0}' -type Cmdlet }}", (object) (lastWord + "*")));
        if (collection != null && collection.Count > 0)
        {
          PSSnapinQualifiedName[] snapinQualifiedNameArray = new PSSnapinQualifiedName[collection.Count];
          for (int index = 0; index < collection.Count; ++index)
          {
            string fullName = CmdletInfo.GetFullName(collection[index]);
            snapinQualifiedNameArray[index] = PSSnapinQualifiedName.GetInstance(fullName);
          }
          Array.Sort<PSSnapinQualifiedName>(snapinQualifiedNameArray, new Comparison<PSSnapinQualifiedName>(CommandCompletionBase.PSSnapinQualifiedNameComparison));
          if (isPSSnapInSpecified)
          {
            strArray = new string[collection.Count];
            for (int index = 0; index < snapinQualifiedNameArray.Length; ++index)
              strArray[index] = snapinQualifiedNameArray[index].FullName;
          }
          else
            strArray = CommandCompletionBase.PrependPSSnapInNameForSameCmdletNames(snapinQualifiedNameArray);
        }
        return strArray;
      }
    }

    private string[] GenerateMatchSetOfFiles(
      string lastWord,
      CultureInfo ci,
      bool completingAtStartOfLine)
    {
      bool matchFoundInCurrentLocation = false;
      bool flag1 = (!lastWord.StartsWith("'", StringComparison.CurrentCulture) || !lastWord.EndsWith("'", StringComparison.CurrentCulture)) && (!lastWord.StartsWith("\"", StringComparison.CurrentCulture) || !lastWord.EndsWith("\"", StringComparison.CurrentCulture));
      bool flag2 = string.IsNullOrEmpty(lastWord);
      bool flag3 = lastWord.EndsWith("*", StringComparison.CurrentCulture);
      bool flag4 = WildcardPattern.ContainsWildcardCharacters(lastWord);
      string path = flag1 ? "'" + lastWord + "*'" : lastWord + "*";
      bool shouldFullyQualifyPaths = this.ShouldFullyQualifyPathsPath(lastWord, path);
      bool flag5 = false;
      if (lastWord.StartsWith("\\\\", StringComparison.CurrentCulture) || lastWord.StartsWith("//", StringComparison.CurrentCulture))
        flag5 = true;
      string[] s1 = (string[]) null;
      string[] s2 = (string[]) null;
      if (flag4 && !flag2)
        s1 = this.FindMatches(flag1 ? "'" + lastWord + "'" : lastWord, shouldFullyQualifyPaths, ci, ref matchFoundInCurrentLocation);
      if (!flag3)
        s2 = this.FindMatches(path, shouldFullyQualifyPaths, ci, ref matchFoundInCurrentLocation);
      string[] strArray = CommandCompletionBase.CombineMatchSets(s1, s2, ci);
      if (strArray != null)
      {
        for (int index = 0; index < strArray.Length; ++index)
        {
          string s = CommandCompletionBase.EscapeSpecialCharacters(strArray[index]);
          strArray[index] = completingAtStartOfLine && matchFoundInCurrentLocation || s == "~" ? ".\\" + s : (!flag5 ? s : this.RemoveProviderQualifier(s));
        }
      }
      return strArray;
    }

    private string[] GenerateMatchSetFromCmdlet(string line, string lastWord)
    {
      string[] strArray = (string[]) null;
      try
      {
        Collection<PSObject> collection = this.exec.ExecuteCommand("TabExpansion", false, out Exception _, new Hashtable()
        {
          [(object) nameof (line)] = (object) line,
          [(object) nameof (lastWord)] = (object) lastWord
        });
        if (collection != null)
        {
          if (collection.Count > 0)
          {
            List<string> list = new List<string>();
            for (int index = 0; index < collection.Count; ++index)
            {
              if (collection[index] != null)
                CommandCompletionBase.CompletionExecutionHelperBase.SafeAddToStringList(list, (object) collection[index]);
            }
            strArray = list.ToArray();
          }
        }
      }
      catch (RuntimeException ex)
      {
      }
      return strArray;
    }

    private static string[] CombineMatchSets(string[] s1, string[] s2, CultureInfo ci)
    {
      string[] strArray;
      if (s1 == null || s1.Length < 1)
        strArray = s2;
      else if (s2 == null || s2.Length < 1)
      {
        strArray = s1;
      }
      else
      {
        strArray = new string[s2.Length];
        s1.CopyTo((Array) strArray, 0);
        int length = s1.Length;
        int index1 = 0;
        int index2 = 0;
        for (; index1 < s2.Length && length < strArray.Length; ++index1)
        {
          if (index2 < s1.Length && string.Compare(s2[index1], s1[index2], false, ci) == 0)
            ++index2;
          else
            strArray[length++] = s2[index1];
        }
      }
      return strArray;
    }

    private string[] FindMatches(
      string path,
      bool shouldFullyQualifyPaths,
      CultureInfo ci,
      ref bool matchFoundInCurrentLocation)
    {
      Collection<PSObject> collection;
      if (!shouldFullyQualifyPaths)
        collection = this.exec.ExecuteCommand(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "& {{ trap {{ continue }} ; resolve-path {0} -Relative }}", (object) path));
      else
        collection = this.exec.ExecuteCommand(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "& {{ trap {{ continue }} ; resolve-path {0} }}", (object) path));
      if (collection == null || collection.Count == 0)
        return (string[]) null;
      List<string> list = new List<string>();
      for (int index = 0; index < collection.Count; ++index)
      {
        if (collection[index] != null)
          CommandCompletionBase.CompletionExecutionHelperBase.SafeAddToStringList(list, (object) collection[index]);
      }
      string[] array = list.ToArray();
      if (array == null)
        CommandCompletionBase.tracer.WriteLine("no matches found", new object[0]);
      else if (array.Length > 1)
        Array.Sort((Array) array, (IComparer) new System.Collections.Comparer(ci));
      return array;
    }

    private string RemoveProviderQualifier(string s)
    {
      string str = (string) null;
      try
      {
        str = this.exec.ExecuteCommandAndGetResultAsString("split-path -noqualifier -path '" + s + "'");
      }
      catch (RuntimeException ex)
      {
      }
      return str;
    }

    private static string[] PrependPSSnapInNameForSameCmdletNames(
      PSSnapinQualifiedName[] mshsnapinQNames)
    {
      string[] strArray = new string[mshsnapinQNames.Length];
      int index1 = 0;
      bool flag = false;
      while (true)
      {
        int index2 = index1 + 1;
        if (index2 < mshsnapinQNames.Length)
        {
          if (string.Compare(mshsnapinQNames[index1].ShortName, mshsnapinQNames[index2].ShortName, StringComparison.OrdinalIgnoreCase) == 0)
          {
            strArray[index1] = mshsnapinQNames[index1].FullName;
            flag = true;
          }
          else
          {
            strArray[index1] = flag ? mshsnapinQNames[index1].FullName : mshsnapinQNames[index1].ShortName;
            flag = false;
          }
          ++index1;
        }
        else
          break;
      }
      strArray[index1] = flag ? mshsnapinQNames[index1].FullName : mshsnapinQNames[index1].ShortName;
      return strArray;
    }

    private static int PSSnapinQualifiedNameComparison(
      PSSnapinQualifiedName first,
      PSSnapinQualifiedName second)
    {
      int num = string.Compare(first.ShortName, second.ShortName, StringComparison.OrdinalIgnoreCase);
      return num == 0 ? string.Compare(first.PSSnapInName, second.PSSnapInName, StringComparison.OrdinalIgnoreCase) : num;
    }

    private static string EscapeSpecialCharacters(string unescaped)
    {
      if (string.IsNullOrEmpty(unescaped))
        return unescaped;
      StringBuilder stringBuilder = new StringBuilder(unescaped.Length * 2);
      for (int index = 0; index < unescaped.Length; ++index)
      {
        char ch = unescaped[index];
        if ("[]?*".IndexOf(ch) != -1)
          stringBuilder.Append("`");
        stringBuilder.Append(ch);
      }
      return stringBuilder.ToString();
    }

    private class LastWordFinder
    {
      private string sentence;
      private char[] wordBuffer;
      private int wordBufferIndex;
      private int replacementIndex;
      private int sentenceIndex;
      private bool sequenceDueToEnd;

      internal LastWordFinder(string sentence) => this.sentence = sentence;

      internal string FindLastWord(out int replacementIndexOut, out string closingQuote)
      {
        bool flag1 = false;
        bool flag2 = false;
        this.ReplacementIndex = 0;
        for (this.sentenceIndex = 0; this.sentenceIndex < this.sentence.Length; ++this.sentenceIndex)
        {
          char c = this.sentence[this.sentenceIndex];
          switch (c)
          {
            case '"':
              this.HandleQuote(ref flag2, ref flag1, c);
              break;
            case '\'':
              this.HandleQuote(ref flag1, ref flag2, c);
              break;
            case '`':
              this.Consume(c);
              if (++this.sentenceIndex < this.sentence.Length)
              {
                this.Consume(this.sentence[this.sentenceIndex]);
                break;
              }
              break;
            default:
              if (CommandCompletionBase.LastWordFinder.IsWhitespace(c))
              {
                if (this.sequenceDueToEnd)
                {
                  this.sequenceDueToEnd = false;
                  if (flag1)
                    flag1 = false;
                  if (flag2)
                    flag2 = false;
                  this.ReplacementIndex = this.sentenceIndex + 1;
                  break;
                }
                if (flag1 || flag2)
                {
                  this.Consume(c);
                  break;
                }
                this.ReplacementIndex = this.sentenceIndex + 1;
                break;
              }
              this.Consume(c);
              break;
          }
        }
        string format = new string(this.wordBuffer, 0, this.wordBufferIndex);
        closingQuote = !flag1 ? (!flag2 ? "" : "\"") : "'";
        replacementIndexOut = this.ReplacementIndex;
        CommandCompletionBase.tracer.WriteLine(format, new object[0]);
        CommandCompletionBase.tracer.WriteLine((object) replacementIndexOut);
        CommandCompletionBase.tracer.WriteLine(closingQuote, new object[0]);
        return format;
      }

      private void HandleQuote(ref bool inQuote, ref bool inOppositeQuote, char c)
      {
        if (inOppositeQuote)
          this.Consume(c);
        else if (inQuote)
        {
          if (this.sequenceDueToEnd)
            this.ReplacementIndex = this.sentenceIndex + 1;
          this.sequenceDueToEnd = !this.sequenceDueToEnd;
        }
        else
        {
          inQuote = true;
          this.ReplacementIndex = this.sentenceIndex + 1;
        }
      }

      private void Consume(char c) => this.wordBuffer[this.wordBufferIndex++] = c;

      private int ReplacementIndex
      {
        get => this.replacementIndex;
        set
        {
          this.wordBuffer = new char[this.sentence.Length];
          this.wordBufferIndex = 0;
          this.replacementIndex = value;
        }
      }

      private static bool IsWhitespace(char c) => c == ' ' || c == '\t';
    }

    protected abstract class CompletionExecutionHelperBase
    {
      [TraceSource("ExecutionHelper", "CommandCompletion execution helper")]
      protected static PSTraceSource tracer = PSTraceSource.GetTracer("ExecutionHelper", "CommandCompletion execution helper");
      private bool cancelTabCompletion;

      internal abstract bool IsRunning { get; }

      internal abstract bool IsStopped { get; }

      internal bool CancelTabCompletion
      {
        get => this.cancelTabCompletion;
        set => this.cancelTabCompletion = value;
      }

      internal Collection<PSObject> ExecuteCommand(string command) => this.ExecuteCommand(command, true, out Exception _, (Hashtable) null);

      internal bool? ExecuteCommandAndGetResultAsBool(string command)
      {
        if (command == null)
          throw CommandCompletionBase.CompletionExecutionHelperBase.tracer.NewArgumentNullException(nameof (command));
        bool? nullable = new bool?();
        Exception exceptionThrown;
        Collection<PSObject> collection = this.ExecuteCommand(command, true, out exceptionThrown, (Hashtable) null);
        if (exceptionThrown != null)
          CommandCompletionBase.CompletionExecutionHelperBase.tracer.TraceException(exceptionThrown);
        else if (collection == null || collection.Count == 0)
          CommandCompletionBase.CompletionExecutionHelperBase.tracer.WriteLine("no results returned", new object[0]);
        else
          nullable = new bool?(collection.Count > 1 || LanguagePrimitives.IsTrue((object) collection[0]));
        return nullable;
      }

      internal string ExecuteCommandAndGetResultAsString(string command)
      {
        if (command == null)
          return string.Empty;
        string str = (string) null;
        Exception exceptionThrown;
        Collection<PSObject> collection = this.ExecuteCommand(command, true, out exceptionThrown, (Hashtable) null);
        if (exceptionThrown != null)
          CommandCompletionBase.CompletionExecutionHelperBase.tracer.TraceException(exceptionThrown);
        else if (collection == null || collection.Count == 0)
        {
          CommandCompletionBase.CompletionExecutionHelperBase.tracer.WriteLine("no results returned", new object[0]);
        }
        else
        {
          if (collection[0] == null)
            return string.Empty;
          str = CommandCompletionBase.CompletionExecutionHelperBase.SafeToString((object) collection[0]);
        }
        return str;
      }

      internal static string SafeToString(object obj)
      {
        if (obj == null)
          return string.Empty;
        try
        {
          string str;
          if (obj is PSObject psObject)
          {
            object baseObject = psObject.BaseObject;
            switch (baseObject)
            {
              case null:
              case PSCustomObject _:
                str = psObject.ToString();
                break;
              default:
                str = baseObject.ToString();
                break;
            }
          }
          else
            str = obj.ToString();
          return str;
        }
        catch (Exception ex)
        {
          CommandCompletionBase.CompletionExecutionHelperBase.tracer.TraceException(ex);
          CommandProcessorBase.CheckForSevereException(ex);
          return string.Empty;
        }
      }

      internal static void SafeAddToStringList(List<string> list, object obj)
      {
        if (list == null)
          return;
        string str = CommandCompletionBase.CompletionExecutionHelperBase.SafeToString(obj);
        if (string.IsNullOrEmpty(str))
          return;
        list.Add(str);
      }

      internal abstract Collection<PSObject> ExecuteCommand(Command command);

      internal Collection<PSObject> ExecuteCommand(
        string command,
        bool isScript,
        out Exception exceptionThrown,
        Hashtable args)
      {
        if (command == null)
          throw CommandCompletionBase.CompletionExecutionHelperBase.tracer.NewArgumentNullException(nameof (command));
        exceptionThrown = (Exception) null;
        if (this.CancelTabCompletion)
          return new Collection<PSObject>();
        Command command1 = new Command(command, isScript);
        if (args != null)
        {
          foreach (DictionaryEntry dictionaryEntry in args)
            command1.Parameters.Add((string) dictionaryEntry.Key, dictionaryEntry.Value);
        }
        Collection<PSObject> collection = (Collection<PSObject>) null;
        try
        {
          collection = this.ExecuteCommand(command1);
          if (this.IsStopped)
          {
            collection = new Collection<PSObject>();
            this.CancelTabCompletion = true;
          }
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          exceptionThrown = ex;
        }
        return collection;
      }
    }

    protected class CompletionExecutionHelper : CommandCompletionBase.CompletionExecutionHelperBase
    {
      private RunspaceRef runspaceRef;
      private Pipeline currentPipeline;

      internal CompletionExecutionHelper(RunspaceRef runspace) => this.runspaceRef = runspace != null ? runspace : throw CommandCompletionBase.CompletionExecutionHelperBase.tracer.NewArgumentNullException(nameof (runspace));

      internal override bool IsRunning => this.currentPipeline != null && this.currentPipeline.PipelineStateInfo.State == PipelineState.Running;

      internal override bool IsStopped => this.currentPipeline != null && this.currentPipeline.PipelineStateInfo.State == PipelineState.Stopped;

      internal override Collection<PSObject> ExecuteCommand(Command command)
      {
        if (command == null)
          throw CommandCompletionBase.CompletionExecutionHelperBase.tracer.NewArgumentNullException(nameof (command));
        this.currentPipeline = !(this.runspaceRef.Runspace is LocalRunspace) ? this.runspaceRef.CreatePipeline() : (this.runspaceRef.Runspace.ExecutionContext.EngineHostInterface.NestedPromptCount <= 0 ? this.runspaceRef.CreatePipeline() : this.runspaceRef.CreateNestedPipeline());
        this.currentPipeline.Commands.Add(command);
        return this.currentPipeline.Invoke();
      }
    }

    protected class CompletionExecutionHelperV2 : CommandCompletionBase.CompletionExecutionHelperBase
    {
      private PowerShell currentPowerShell;

      internal CompletionExecutionHelperV2(PowerShell powershell) => this.currentPowerShell = powershell != null ? powershell : throw CommandCompletionBase.CompletionExecutionHelperBase.tracer.NewArgumentNullException(nameof (powershell));

      internal PowerShell CurrentPowerShell
      {
        get => this.currentPowerShell;
        set => this.currentPowerShell = value != null ? value : throw CommandCompletionBase.CompletionExecutionHelperBase.tracer.NewArgumentNullException(nameof (CurrentPowerShell));
      }

      internal override bool IsRunning => this.currentPowerShell.InvocationStateInfo.State == PSInvocationState.Running;

      internal override bool IsStopped => this.currentPowerShell.InvocationStateInfo.State == PSInvocationState.Stopped;

      internal override Collection<PSObject> ExecuteCommand(Command command)
      {
        this.currentPowerShell.Commands = command != null ? new PSCommand(command) : throw CommandCompletionBase.CompletionExecutionHelperBase.tracer.NewArgumentNullException(nameof (command));
        return this.currentPowerShell.Invoke();
      }
    }
  }
}
