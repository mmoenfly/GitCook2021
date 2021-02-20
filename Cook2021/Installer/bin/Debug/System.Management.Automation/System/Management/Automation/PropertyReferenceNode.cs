// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PropertyReferenceNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class PropertyReferenceNode : ParseTreeNode, IAssignableParseTreeNode
  {
    private readonly ParseTreeNode _target;
    private readonly object _property;
    private List<TypeLiteral> _typeConstraint;

    public PropertyReferenceNode(Token nodeToken, ParseTreeNode target, object property)
    {
      this.NodeToken = nodeToken;
      this._target = target;
      this._property = property;
    }

    public List<TypeLiteral> TypeConstraint
    {
      get
      {
        if (this._typeConstraint == null)
          this._typeConstraint = new List<TypeLiteral>();
        return this._typeConstraint;
      }
    }

    private bool IsStatic => this.NodeToken.Is("::");

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      object obj = this.GetValue(this.GetTarget(input, context), this.GetProperty(input, context), context);
      if (this._typeConstraint != null)
      {
        foreach (TypeLiteral typeLiteral in this._typeConstraint)
          obj = Parser.ConvertTo(obj, typeLiteral.Type, this.NodeToken);
      }
      return obj;
    }

    internal object GetValue(PSObject obj, object property, ExecutionContext context)
    {
      if (!LanguagePrimitives.IsNull((object) obj))
      {
        if (!LanguagePrimitives.IsNull(property))
        {
          try
          {
            IDictionary dictionary = PSObject.Base((object) obj) as IDictionary;
            try
            {
              if (dictionary != null)
              {
                if (dictionary.Contains(property))
                  return dictionary[property];
              }
            }
            catch (InvalidOperationException ex)
            {
            }
            PSMemberInfo memberInfo = this.GetMemberInfo(obj, property, context);
            if (memberInfo != null)
              return memberInfo.Value;
          }
          catch (TerminateException ex)
          {
            throw;
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            return (object) null;
          }
        }
      }
      if (context.IsStrictVersion(2))
        throw InterpreterError.NewInterpreterException((object) this.NodeToken, typeof (RuntimeException), this.NodeToken, "PropertyNotFoundStrict", property);
      return (object) null;
    }

    internal void SetValue(PSObject obj, object property, object value, ExecutionContext context)
    {
      if (!LanguagePrimitives.IsNull((object) obj))
      {
        if (!LanguagePrimitives.IsNull(property))
        {
          try
          {
            if (PSObject.Base((object) obj) is IDictionary dictionary)
            {
              dictionary[property] = value;
              return;
            }
            PSMemberInfo memberInfo = this.GetMemberInfo(obj, property, context);
            if (memberInfo == null)
              throw InterpreterError.NewInterpreterException(property, typeof (RuntimeException), this.NodeToken, "PropertyNotFound", property);
            memberInfo.Value = value;
            return;
          }
          catch (TerminateException ex)
          {
            throw;
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            throw InterpreterError.NewInterpreterExceptionByMessage(typeof (RuntimeException), this.NodeToken, ex.Message, "PropertyAssignmentException", ex);
          }
        }
      }
      throw InterpreterError.NewInterpreterException(property, typeof (RuntimeException), this.NodeToken, "PropertyNotFound", property);
    }

    private PSObject GetTarget(Array input, ExecutionContext context)
    {
      if (this._target is VariableDereferenceNode target)
        target.GetVariable(context)?.WrapValue();
      return LanguagePrimitives.AsPSObjectOrNull(this._target.Execute(input, (Pipe) null, context));
    }

    private object GetProperty(Array input, ExecutionContext context)
    {
      if (this._property is Token property)
        return (object) property.TokenText;
      object obj = this._property;
      if (obj is ParseTreeNode parseTreeNode)
        obj = parseTreeNode.Execute(input, (Pipe) null, context);
      return LanguagePrimitives.IsNull(obj) ? (object) null : PSObject.Base(obj);
    }

    private PSMemberInfo GetMemberInfo(
      PSObject target,
      object member,
      ExecutionContext context)
    {
      if (!(member is string str))
        str = PSObject.ToStringParser(context, member);
      return this.IsStatic ? PSObject.GetStaticCLRMember((object) target, str) : target.Members[str];
    }

    public IAssignableValue GetAssignableValue(
      Array input,
      ExecutionContext context)
    {
      return (IAssignableValue) new AssignablePropertyReference(this, this.GetTarget(input, context), this.GetProperty(input, context));
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      this._target.Accept(visitor);
      if (!(this._property is ParseTreeNode property))
        return;
      property.Accept(visitor);
    }
  }
}
