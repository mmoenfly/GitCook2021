﻿// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.AssignablePropertyReference
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class AssignablePropertyReference : IAssignableValue
  {
    private PropertyReferenceNode _node;
    private PSObject _target;
    private object _property;

    public AssignablePropertyReference(
      PropertyReferenceNode node,
      PSObject target,
      object property)
    {
      this._node = node;
      this._target = target;
      this._property = property;
    }

    public object GetValue(ExecutionContext context) => this._node.GetValue(this._target, this._property, context);

    public void SetValue(object value, ExecutionContext context)
    {
      foreach (TypeLiteral typeLiteral in this._node.TypeConstraint)
        value = Parser.ConvertTo(value, typeLiteral.Type, this._node.NodeToken);
      this._node.SetValue(this._target, this._property, value, context);
    }
  }
}