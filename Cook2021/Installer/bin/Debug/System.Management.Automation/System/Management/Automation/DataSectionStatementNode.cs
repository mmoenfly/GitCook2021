// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DataSectionStatementNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class DataSectionStatementNode : ParseTreeNode
  {
    private readonly Token _name;
    private readonly ParseTreeNode _body;
    private readonly ParseTreeNode[] _commandsAllowed;

    internal DataSectionStatementNode(
      Token token,
      Token name,
      ParseTreeNode body,
      ParseTreeNode[] commandsAllowed)
    {
      this.NodeToken = token;
      this._name = name;
      this._body = body;
      this._commandsAllowed = commandsAllowed;
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      object obj = (object) null;
      IEnumerable<string> commandsAllowed = (IEnumerable<string>) this.GetCommandsAllowed(context);
      if (this._body != null)
      {
        RestrictedLanguageModeChecker.Check(context.Engine.EngineParser, this._body, commandsAllowed);
        PSLanguageMode languageMode = context.LanguageMode;
        try
        {
          context.LanguageMode = PSLanguageMode.RestrictedLanguage;
          obj = this._body.Execute(input, (Pipe) null, context);
        }
        finally
        {
          context.LanguageMode = languageMode;
        }
      }
      if (this._name == null)
        return obj;
      string tokenText = this._name.TokenText;
      PSVariable variableAtScope = context.EngineSessionState.GetVariableAtScope(tokenText, "local");
      if (variableAtScope == null)
      {
        PSVariable variable = new PSVariable(tokenText, obj, ScopedItemOptions.None);
        context.EngineSessionState.NewVariableAtScope(variable, "local", true);
      }
      else
        variableAtScope.Value = obj;
      return (object) AutomationNull.Value;
    }

    private List<string> GetCommandsAllowed(ExecutionContext context)
    {
      List<string> stringList = new List<string>();
      stringList.Add("ConvertFrom-StringData");
      if (this._commandsAllowed != null && this._commandsAllowed.Length != 0)
      {
        foreach (ParseTreeNode parseTreeNode in this._commandsAllowed)
        {
          object obj1 = parseTreeNode.Execute(context);
          IEnumerable enumerable = LanguagePrimitives.GetEnumerable(obj1);
          if (enumerable != null)
          {
            foreach (object obj2 in enumerable)
              stringList.Add(Parser.ConvertTo<string>(obj2, this.NodeToken));
          }
          else
            stringList.Add(Parser.ConvertTo<string>(obj1, this.NodeToken));
        }
      }
      return stringList;
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      if (this._commandsAllowed != null)
      {
        foreach (ParseTreeNode parseTreeNode in this._commandsAllowed)
          parseTreeNode.Accept(visitor);
      }
      this._body.Accept(visitor);
    }
  }
}
