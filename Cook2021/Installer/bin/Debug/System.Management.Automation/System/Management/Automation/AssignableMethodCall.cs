// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.AssignableMethodCall
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class AssignableMethodCall : IAssignableValue
  {
    private MethodCallNode _node;
    private object _target;
    private object[] _arguments;

    internal AssignableMethodCall(MethodCallNode node, object target, object[] arguments)
    {
      this._node = node;
      this._target = target;
      this._arguments = arguments;
    }

    public object GetValue(ExecutionContext context) => this._node.InvokeMethod(this._target, this._arguments, (object) AutomationNull.Value);

    public void SetValue(object value, ExecutionContext context)
    {
      foreach (TypeLiteral typeLiteral in this._node.TypeConstraint)
        value = Parser.ConvertTo(value, typeLiteral.Type, this._node.NodeToken);
      this._node.InvokeMethod(this._target, this._arguments, value);
    }
  }
}
