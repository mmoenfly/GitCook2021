// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.AttributeNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Reflection;

namespace System.Management.Automation
{
  internal sealed class AttributeNode : ParseTreeNode
  {
    private readonly string _name;
    private readonly object[] _arguments;
    private readonly HashLiteralNode _namedArguments;

    public AttributeNode(Token nameToken, object[] arguments, HashLiteralNode namedArguments)
    {
      this.NodeToken = nameToken;
      this._name = nameToken.TokenText;
      this._arguments = arguments;
      if (this._arguments == null)
        this._arguments = new object[0];
      this._namedArguments = namedArguments;
    }

    internal string Name => this._name;

    internal Attribute GetAttribute()
    {
      if (this.NodeToken.TokenId != TokenId.TypeToken)
        return this.GetCustomAttribute();
      return (Attribute) new ArgumentTypeConverterAttribute(new Type[1]
      {
        (this._arguments[0] as TypeLiteral).Type
      });
    }

    private Attribute GetCustomAttribute()
    {
      Attribute customAttributeObject = this.GetCustomAttributeObject();
      if (customAttributeObject != null && this._namedArguments != null)
      {
        Hashtable constValue = (Hashtable) this._namedArguments.GetConstValue();
        PSObject psObject = LanguagePrimitives.AsPSObjectOrNull((object) customAttributeObject);
        foreach (object key in (IEnumerable) constValue.Keys)
        {
          object obj = constValue[key];
          try
          {
            PSMemberInfo member = psObject.Members[key.ToString()];
            if (member != null)
              member.Value = obj;
            else
              throw InterpreterError.NewInterpreterException((object) this.NodeToken, typeof (RuntimeException), this.NodeToken, "PropertyNotFoundForType", (object) key.ToString(), (object) customAttributeObject.GetType().ToString());
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            throw InterpreterError.NewInterpreterExceptionByMessage(typeof (RuntimeException), this.NodeToken, ex.Message, "PropertyAssignmentException", ex);
          }
        }
      }
      return customAttributeObject;
    }

    private Attribute GetCustomAttributeObject()
    {
      Type attribute = LanguagePrimitives.ConvertStringToAttribute(this._name);
      if (attribute == null)
        throw InterpreterError.NewInterpreterException((object) this.NodeToken, typeof (RuntimeException), this.NodeToken, "CustomAttributeTypeNotFound", (object) this._name);
      if (!attribute.IsSubclassOf(typeof (Attribute)))
        throw InterpreterError.NewInterpreterException((object) this.NodeToken, typeof (RuntimeException), this.NodeToken, "InValidCustomAttributeType", (object) this._name);
      try
      {
        if (this._arguments.Length == 0)
        {
          ConstructorInfo constructor = attribute.GetConstructor(Type.EmptyTypes);
          if (constructor != null)
          {
            if (constructor.IsPublic)
              return (Attribute) DotNetAdapter.ConstructorInvokeDotNet(attribute, new ConstructorInfo[1]
              {
                constructor
              }, this._arguments);
          }
        }
        else
        {
          ConstructorInfo[] constructors = attribute.GetConstructors();
          if (constructors.Length != 0)
            return (Attribute) DotNetAdapter.ConstructorInvokeDotNet(attribute, constructors, this._arguments);
        }
      }
      catch (RuntimeException ex)
      {
        if (ex.ErrorRecord.InvocationInfo == null)
          ex.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, this.NodeToken));
        throw;
      }
      throw InterpreterError.NewInterpreterException((object) this.NodeToken, typeof (RuntimeException), this.NodeToken, "CannotFindConstructorForCustomAttribute", (object) this._name);
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      foreach (object obj in this._arguments)
      {
        if (obj is ParseTreeNode parseTreeNode)
          parseTreeNode.Accept(visitor);
      }
    }
  }
}
