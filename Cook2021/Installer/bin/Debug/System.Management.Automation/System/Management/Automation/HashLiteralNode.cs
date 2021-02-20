// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HashLiteralNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class HashLiteralNode : ParseTreeNode
  {
    private readonly List<Token> _tokens = new List<Token>();
    private readonly ArrayList _keys = new ArrayList();
    private readonly List<ParseTreeNode> _expressions = new List<ParseTreeNode>();

    public void Add(Token token, object key, ParseTreeNode expression)
    {
      this._tokens.Add(token);
      this._keys.Add(key);
      this._expressions.Add(expression);
    }

    internal void ValidateForNamedAttributeArguments(Parser parser)
    {
      foreach (object key in this._keys)
      {
        if (key is ParseTreeNode && !((ParseTreeNode) key).IsConstant)
          parser.ReportException((object) null, typeof (ParseException), ((ParseTreeNode) key).NodeToken, "ParameterAttributeArgumentNeedsToBeConstantOrScriptBlock");
      }
      foreach (ParseTreeNode expression in this._expressions)
      {
        if (!expression.ValidAttributeArgument)
          parser.ReportException((object) null, typeof (ParseException), expression.NodeToken, "ParameterAttributeArgumentNeedsToBeConstantOrScriptBlock");
      }
    }

    private object BuildHashtable(
      ParseTreeNode.ParseTreeNodeExecutor parseTreeNodeExecutor)
    {
      Hashtable hashtable = new Hashtable((IEqualityComparer) StringComparer.CurrentCultureIgnoreCase);
      for (int index = 0; index < this._keys.Count; ++index)
      {
        object obj1 = this._keys[index];
        if (obj1 is ParseTreeNode)
          obj1 = parseTreeNodeExecutor((ParseTreeNode) obj1);
        object obj2 = PSObject.Base(obj1);
        if (obj2 == null || obj2 == AutomationNull.Value)
          throw InterpreterError.NewInterpreterException(obj2, typeof (RuntimeException), this._tokens[index], "NullArrayIndex");
        if (hashtable.ContainsKey(obj2))
        {
          string str = PSObject.ToStringParser((ExecutionContext) null, obj2);
          if (str.Length > 40)
            str = str.Substring(0, 40) + "...";
          throw InterpreterError.NewInterpreterException(obj2, typeof (RuntimeException), this._tokens[index], "DuplicateKeyInHashLiteral", (object) str);
        }
        hashtable[obj2] = parseTreeNodeExecutor(this._expressions[index]);
      }
      return (object) hashtable;
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      return this.BuildHashtable(new ParseTreeNode.ParseTreeNodeExecutor(new ParseTreeNode.InvokeExecute(input, (Pipe) null, context).Invoke));
    }

    internal override object GetConstValue() => this.BuildHashtable(new ParseTreeNode.ParseTreeNodeExecutor(ParseTreeNode.InvokeGetConstValue));

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      for (int index = 0; index < this._expressions.Count; ++index)
      {
        if (this._keys[index] is ParseTreeNode key)
          key.Accept(visitor);
        this._expressions[index].Accept(visitor);
      }
    }
  }
}
