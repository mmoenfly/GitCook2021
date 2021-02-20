// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SwitchStatementNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation.Internal;
using System.Text.RegularExpressions;

namespace System.Management.Automation
{
  internal sealed class SwitchStatementNode : ParseTreeNode
  {
    private ParseTreeNode[] _clauses;
    private readonly SwitchMode _mode;
    private readonly ParseTreeNode _expression;
    private readonly string _label = "";
    private static readonly ScopedItemLookupPath _switchVariablePath = new ScopedItemLookupPath("local:switch");

    public SwitchStatementNode(
      Token nodeToken,
      string label,
      ParseTreeNode expression,
      ParseTreeNode[] clauses,
      SwitchMode mode)
    {
      this.NodeToken = nodeToken;
      this._label = label == null ? "" : label;
      this._expression = expression;
      this._clauses = clauses;
      this._mode = mode;
    }

    internal override void Execute(
      Array input,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      if ((this._mode & SwitchMode.File) == SwitchMode.File)
        this.ExecuteFileSwitch(input, outputPipe, ref resultList, context);
      else
        this.ExecuteValueSwitch(input, outputPipe, ref resultList, context);
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      this._expression.Accept(visitor);
      foreach (ParseTreeNode clause in this._clauses)
        clause?.Accept(visitor);
    }

    private void ExecuteFileSwitch(
      Array input,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      string filePath = this.GetFilePath(context);
      try
      {
        using (StreamReader streamReader = new StreamReader(filePath))
        {
          string str;
          while ((str = streamReader.ReadLine()) != null)
          {
            try
            {
              this.ProcessClauses((object) str, outputPipe, ref resultList, context);
            }
            catch (BreakException ex)
            {
              if (this.MatchLabel(ex.Label))
                break;
              throw;
            }
            catch (ContinueException ex)
            {
              if (!this.MatchLabel(ex.Label))
                throw;
            }
          }
        }
      }
      catch (BreakException ex)
      {
        throw;
      }
      catch (ContinueException ex)
      {
        throw;
      }
      catch (ReturnException ex)
      {
        throw;
      }
      catch (ExitException ex)
      {
        throw;
      }
      catch (TerminateException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw InterpreterError.NewInterpreterExceptionWithInnerException((object) filePath, typeof (RuntimeException), this._expression.NodeToken, "FileReadError", ex, (object) ex.Message);
      }
    }

    private string GetFilePath(ExecutionContext context)
    {
      try
      {
        object obj = this._expression.Execute((Array) null, (Pipe) null, context);
        string filePath = !(obj is FileInfo fileInfo) ? PSObject.ToStringParser(context, obj) : fileInfo.FullName;
        return !string.IsNullOrEmpty(filePath) ? this.ResolveFilePath(filePath, context) : throw InterpreterError.NewInterpreterException((object) filePath, typeof (RuntimeException), this._expression.NodeToken, "InvalidFilenameOption");
      }
      catch (RuntimeException ex)
      {
        if (ex.ErrorRecord != null && ex.ErrorRecord.InvocationInfo == null)
          ex.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, this.NodeToken, context));
        throw;
      }
    }

    private string ResolveFilePath(string filePath, ExecutionContext context)
    {
      ProviderInfo provider = (ProviderInfo) null;
      Collection<string> providerPathFromPsPath = new SessionState(context.EngineSessionState).Path.GetResolvedProviderPathFromPSPath(filePath, out provider);
      if (!provider.NameEquals(context.ProviderNames.FileSystem))
        throw InterpreterError.NewInterpreterException((object) filePath, typeof (RuntimeException), this._expression.NodeToken, "FileOpenError", (object) provider.FullName);
      if (providerPathFromPsPath == null || providerPathFromPsPath.Count < 1)
        throw InterpreterError.NewInterpreterException((object) filePath, typeof (RuntimeException), this._expression.NodeToken, "FileNotFound", (object) filePath);
      return providerPathFromPsPath.Count <= 1 ? providerPathFromPsPath[0] : throw InterpreterError.NewInterpreterException((object) providerPathFromPsPath, typeof (RuntimeException), this._expression.NodeToken, "AmbiguousPath");
    }

    private bool MatchLabel(string label) => string.IsNullOrEmpty(label) || label.Equals(this._label, StringComparison.OrdinalIgnoreCase);

    private void ExecuteValueSwitch(
      Array input,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      if (this._expression == null)
        return;
      object obj1 = this._expression.Execute(context);
      if (obj1 == AutomationNull.Value)
        return;
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(obj1);
      if (enumerator == null)
        enumerator = new object[1]{ obj1 }.GetEnumerator();
      object newValue = (object) null;
      SessionStateInternal engineSessionState = context.EngineSessionState;
      PSVariable psVariable = engineSessionState.GetVariableItem(SwitchStatementNode._switchVariablePath, out SessionStateScope _);
      if (psVariable == null)
      {
        psVariable = new PSVariable(SwitchStatementNode._switchVariablePath.LookupPath.NamespaceSpecificString.ToString(), (object) enumerator);
        engineSessionState.SetVariable(SwitchStatementNode._switchVariablePath, (object) psVariable, false, true, CommandOrigin.Internal);
      }
      else
      {
        newValue = psVariable.Value;
        psVariable.SetValueRaw((object) enumerator, false);
      }
      try
      {
        while (ParserOps.MoveNext(context, this.NodeToken, enumerator))
        {
          object obj2 = ParserOps.Current(this.NodeToken, enumerator);
          try
          {
            this.ProcessClauses(obj2, outputPipe, ref resultList, context);
          }
          catch (BreakException ex)
          {
            if (this.MatchLabel(ex.Label))
              break;
            throw;
          }
          catch (ContinueException ex)
          {
            if (!this.MatchLabel(ex.Label))
              throw;
          }
        }
      }
      finally
      {
        psVariable.SetValueRaw(newValue, false);
      }
    }

    private void ProcessClauses(
      object obj,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      object underbarVariable = context.UnderbarVariable;
      context.UnderbarVariable = (object) LanguagePrimitives.AsPSObjectOrNull(obj);
      try
      {
        bool flag = false;
        int i;
        for (i = 0; i < this._clauses.Length && this._clauses[i] != null; i += 2)
        {
          if (this.ConditionSatisfied(i, obj, context))
          {
            this._clauses[i + 1].Execute((Array) null, outputPipe, ref resultList, context);
            flag = true;
          }
        }
        if (flag || i >= this._clauses.Length || this._clauses[i] != null)
          return;
        this._clauses[i + 1].Execute((Array) null, outputPipe, ref resultList, context);
      }
      finally
      {
        context.UnderbarVariable = underbarVariable;
      }
    }

    private bool ConditionSatisfied(int i, object input, ExecutionContext context)
    {
      object condition = PSObject.Base(this._clauses[i].Execute((Array) null, (Pipe) null, context));
      if (condition is ScriptBlock scriptBlock)
        return LanguagePrimitives.IsTrue(scriptBlock.DoInvokeReturnAsIs((object) LanguagePrimitives.AsPSObjectOrNull(input), (object) AutomationNull.Value));
      string stringParser = PSObject.ToStringParser(context, input);
      if ((this._mode & SwitchMode.Regex) == SwitchMode.Regex)
        return this.ConditionSatisfiedRegex(condition, this._clauses[i].NodeToken, stringParser, context);
      if (condition is Regex regex)
        return this.ConditionSatisfiedRegex((object) regex, this._clauses[i].NodeToken, stringParser, context);
      if (condition is WildcardPattern wildcardPattern)
      {
        if ((wildcardPattern.Options & WildcardOptions.IgnoreCase) == WildcardOptions.None != ((this._mode & SwitchMode.CaseSensitive) != SwitchMode.None))
        {
          WildcardOptions options = WildcardOptions.None;
          if ((this._mode & SwitchMode.CaseSensitive) != SwitchMode.CaseSensitive)
            options = WildcardOptions.IgnoreCase;
          wildcardPattern = new WildcardPattern(wildcardPattern.Pattern, options);
        }
        return wildcardPattern.IsMatch(stringParser);
      }
      if ((this._mode & SwitchMode.Wildcard) == SwitchMode.Wildcard)
      {
        WildcardOptions options = WildcardOptions.None;
        if ((this._mode & SwitchMode.CaseSensitive) != SwitchMode.CaseSensitive)
          options = WildcardOptions.IgnoreCase;
        return new WildcardPattern(PSObject.ToStringParser(context, condition), options).IsMatch(stringParser);
      }
      StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;
      if ((this._mode & SwitchMode.CaseSensitive) == SwitchMode.CaseSensitive)
        comparisonType = StringComparison.CurrentCulture;
      return PSObject.ToStringParser(context, condition).Equals(stringParser, comparisonType);
    }

    private bool ConditionSatisfiedRegex(
      object condition,
      Token errorToken,
      string str,
      ExecutionContext context)
    {
      RegexOptions options = RegexOptions.None;
      if ((this._mode & SwitchMode.CaseSensitive) != SwitchMode.CaseSensitive)
        options = RegexOptions.IgnoreCase;
      try
      {
        Match match;
        if (condition is Regex regex && (regex.Options & RegexOptions.IgnoreCase) == RegexOptions.None != ((this._mode & SwitchMode.CaseSensitive) != SwitchMode.None))
        {
          match = regex.Match(str);
        }
        else
        {
          string stringParser = PSObject.ToStringParser(context, condition);
          match = Regex.Match(str, stringParser, options);
          if (match.Success && match.Groups.Count > 0)
            regex = new Regex(stringParser, options);
        }
        if (match.Success)
        {
          GroupCollection groups = match.Groups;
          if (groups.Count > 0)
          {
            Hashtable hashtable = new Hashtable((IEqualityComparer) StringComparer.CurrentCultureIgnoreCase);
            foreach (string groupName in regex.GetGroupNames())
            {
              Group group = groups[groupName];
              if (group.Success)
              {
                int result;
                if (int.TryParse(groupName, out result))
                  hashtable.Add((object) result, (object) group.ToString());
                else
                  hashtable.Add((object) groupName, (object) group.ToString());
              }
            }
            context.SetVariable("Matches", (object) hashtable);
          }
        }
        return match.Success;
      }
      catch (ArgumentException ex)
      {
        string stringParser = PSObject.ToStringParser(context, condition);
        throw InterpreterError.NewInterpreterExceptionWithInnerException((object) stringParser, typeof (RuntimeException), errorToken, "InvalidRegularExpression", (Exception) ex, (object) stringParser);
      }
    }
  }
}
