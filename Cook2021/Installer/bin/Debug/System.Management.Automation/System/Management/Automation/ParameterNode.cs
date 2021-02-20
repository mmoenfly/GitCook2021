// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParameterNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  internal sealed class ParameterNode : ParseTreeNode
  {
    private VariableDereferenceNode _parameter;
    private ParseTreeNode _initializer;
    private List<AttributeNode> _attributes;

    internal ParameterNode(
      VariableDereferenceNode parameter,
      List<AttributeNode> attributes,
      ParseTreeNode initializer)
    {
      if (parameter != null)
        this.NodeToken = parameter.NodeToken;
      this._parameter = parameter;
      this._initializer = initializer;
      this._attributes = attributes;
      this.IsVoidable = true;
    }

    internal string ParameterName => this._parameter != null ? this._parameter.VariableName : (string) null;

    internal bool IsSwitchParameter()
    {
      TypeLiteral lastType = ParameterNode.GetLastType(this._parameter.TypeConstraint);
      return lastType != null && lastType.IsSwitchParameter;
    }

    internal bool HasParameterAttribute()
    {
      foreach (AttributeNode attribute in this._attributes)
      {
        if (attribute.GetAttribute() is ParameterAttribute)
          return true;
      }
      return false;
    }

    internal RuntimeDefinedParameter GetRuntimeDefinedParameter(bool isCmdlet)
    {
      TypeLiteral lastType = ParameterNode.GetLastType(this._parameter.TypeConstraint);
      Collection<Attribute> attributes = new Collection<Attribute>();
      bool flag = false;
      foreach (AttributeNode attribute1 in this._attributes)
      {
        Attribute attribute2 = attribute1.GetAttribute();
        if (attribute2 != null)
        {
          if (attribute2 is ParameterAttribute)
            flag = true;
          attributes.Add(attribute2);
        }
      }
      if (isCmdlet && !flag)
      {
        ParameterAttribute parameterAttribute = new ParameterAttribute();
        attributes.Insert(0, (Attribute) parameterAttribute);
      }
      RuntimeDefinedParameter definedParameter = lastType != null ? new RuntimeDefinedParameter(this._parameter.VariableName, lastType, attributes) : new RuntimeDefinedParameter(this._parameter.VariableName, typeof (object), attributes);
      if (this._initializer != null)
        definedParameter.Value = (object) this._initializer;
      else if (definedParameter.ParameterType == typeof (string))
        definedParameter.Value = (object) "";
      else if (definedParameter.ParameterType == typeof (bool))
        definedParameter.Value = (object) false;
      else if (definedParameter.ParameterType == typeof (SwitchParameter))
        definedParameter.Value = (object) new SwitchParameter(false);
      else if (LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(definedParameter.ParameterType)))
        definedParameter.Value = (object) 0;
      return definedParameter;
    }

    private static TypeLiteral GetLastType(List<TypeLiteral> typeConstraint) => typeConstraint == null || typeConstraint.Count == 0 ? (TypeLiteral) null : typeConstraint[typeConstraint.Count - 1];

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      if (this._parameter != null)
        this._parameter.Accept(visitor);
      foreach (ParseTreeNode attribute in this._attributes)
        attribute.Accept(visitor);
      if (this._initializer == null)
        return;
      this._initializer.Accept(visitor);
    }
  }
}
