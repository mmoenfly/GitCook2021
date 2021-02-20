// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.VariableDereferenceNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class VariableDereferenceNode : 
    ParseTreeNode,
    IAssignableParseTreeNode,
    IAssignableValue
  {
    private readonly string _varName;
    private readonly ScopedItemLookupPath _variablePath;
    private int _activationRecordSlot = -1;
    private readonly bool _useConstantValue;
    private readonly bool _inExpandableString;
    private readonly object _constantValue;
    private List<TypeLiteral> _typeConstraint;

    public VariableDereferenceNode(
      Token var,
      List<TypeLiteral> typeConstraint,
      bool inExpandableString)
    {
      this.NodeToken = var;
      this._varName = var.TokenText;
      this._variablePath = new ScopedItemLookupPath(this._varName);
      if (this.NodeToken.TokenText.Equals("true", StringComparison.OrdinalIgnoreCase))
      {
        this._useConstantValue = true;
        this._constantValue = (object) true;
        this.ValidAttributeArgument = true;
      }
      else if (this.NodeToken.TokenText.Equals("false", StringComparison.OrdinalIgnoreCase))
      {
        this._useConstantValue = true;
        this._constantValue = (object) false;
        this.ValidAttributeArgument = true;
      }
      else if (this.NodeToken.TokenText.Equals("null", StringComparison.OrdinalIgnoreCase))
      {
        this._useConstantValue = true;
        this._constantValue = (object) null;
        this.ValidAttributeArgument = true;
      }
      this._inExpandableString = inExpandableString;
      this._typeConstraint = typeConstraint;
    }

    internal VariableDereferenceNode(string varName, int variableSlot)
    {
      this._varName = varName;
      this._variablePath = new ScopedItemLookupPath(varName);
      this._activationRecordSlot = variableSlot;
      this.IsInternalCode = true;
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

    internal string VariableName => this._varName;

    internal bool IsScopedItem => this._variablePath.IsScopedItem;

    internal void SetActivationRecordSlot(int slot) => this._activationRecordSlot = slot;

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      return this.GetValue(context);
    }

    internal override object GetConstValue() => this._useConstantValue ? this._constantValue : base.GetConstValue();

    internal bool IsPredefinedConstantVariable => this._useConstantValue;

    public IAssignableValue GetAssignableValue(
      Array input,
      ExecutionContext context)
    {
      return (IAssignableValue) this;
    }

    public object GetValue(ExecutionContext context)
    {
      this.CheckTypeConversionViolation();
      object obj = this.PrivateGetValue(context);
      if (this._typeConstraint != null && this._typeConstraint.Count > 0)
      {
        foreach (TypeLiteral typeLiteral in this._typeConstraint)
          obj = Parser.ConvertTo(obj, typeLiteral.Type, this.NodeToken);
      }
      return obj;
    }

    public void SetValue(object value, ExecutionContext context)
    {
      try
      {
        this.CheckTypeConstraintViolation();
        if (context.PSDebug > 1 && !this.IsInternalCode)
        {
          string str = PSObject.ToStringParser(context, value);
          int length = 60 - this._varName.Length;
          if (str.Length > length)
            str = str.Substring(0, length) + "...";
          ScriptTrace.Trace(context, 1, "TraceVariableAssignment", (object) this._varName, (object) str);
        }
        this.PrivateSetValue(value, context);
      }
      catch (RuntimeException ex)
      {
        if (ex.ErrorRecord != null && ex.ErrorRecord.InvocationInfo == null)
          ex.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, this.NodeToken, context));
        throw;
      }
    }

    private object PrivateGetValue(ExecutionContext context)
    {
      try
      {
        if (this._useConstantValue)
          return this._constantValue;
        if (!this._variablePath.IsScopedItem)
        {
          CmdletProviderContext context1 = (CmdletProviderContext) null;
          SessionStateScope scope = (SessionStateScope) null;
          SessionStateInternal engineSessionState = context.EngineSessionState;
          return engineSessionState.GetVariableValueFromProvider(this._variablePath, out context1, out scope, engineSessionState.currentScope.ScopeOrigin);
        }
        PSVariable variable = this.GetVariable(context);
        if (this._typeConstraint != null && this._typeConstraint.Count == 1 && this._typeConstraint[0].Type.Equals(typeof (PSReference)))
          return variable != null ? (object) new PSReference((object) variable) : throw InterpreterError.NewInterpreterException((object) this.NodeToken, typeof (RuntimeException), this.NodeToken, "NonExistingVariableReference", (object) this.NodeToken);
        if (variable != null)
          return variable.Value;
        if (context.Debugger.IsOn)
          context.Debugger.CheckVariableRead(this._varName);
        return (object) null;
      }
      catch (RuntimeException ex)
      {
        if (ex.ErrorRecord != null && ex.ErrorRecord.InvocationInfo == null)
          ex.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, this.NodeToken, context));
        throw;
      }
    }

    internal PSVariable GetVariable(ExecutionContext context)
    {
      if (this._useConstantValue)
        return (PSVariable) null;
      SessionStateInternal engineSessionState = context.EngineSessionState;
      CommandOrigin scopeOrigin = engineSessionState.currentScope.ScopeOrigin;
      if (!this._variablePath.IsScopedItem)
      {
        SessionStateScope scope = (SessionStateScope) null;
        return engineSessionState.GetVariableItem(this._variablePath, out scope, scopeOrigin);
      }
      PSVariable variable = engineSessionState.CurrentActivationRecord.GetVariable(this._activationRecordSlot, scopeOrigin);
      if (variable == null)
      {
        SessionStateScope scope;
        variable = engineSessionState.GetVariableItem(this._variablePath, out scope, scopeOrigin);
        if (variable != null)
          engineSessionState.CurrentActivationRecord.SetVariable(variable, scope, this._activationRecordSlot);
        else if ((context.IsStrictVersion(2) || !this._inExpandableString && context.IsStrictVersion(1)) && !this.IsInternalCode)
          throw InterpreterError.NewInterpreterException((object) this.NodeToken, typeof (RuntimeException), this.NodeToken, "VariableIsUndefined", (object) this.NodeToken);
      }
      return variable;
    }

    private void PrivateSetValue(object value, ExecutionContext context)
    {
      SessionStateInternal engineSessionState = context.EngineSessionState;
      CommandOrigin scopeOrigin = engineSessionState.currentScope.ScopeOrigin;
      if (!this._variablePath.IsScopedItem)
      {
        engineSessionState.SetVariable(this._variablePath, value, true, scopeOrigin);
      }
      else
      {
        PSVariable variable1 = engineSessionState.CurrentActivationRecord.GetVariable(this._activationRecordSlot, scopeOrigin);
        if (variable1 != null)
        {
          this.UpdateVariable(variable1, value);
        }
        else
        {
          ScopedItemLookupPath variablePath = !this._variablePath.IsUnqualified ? this._variablePath : new ScopedItemLookupPath(this._variablePath, true);
          SessionStateScope scope;
          PSVariable variable2 = engineSessionState.GetVariableItem(variablePath, out scope, scopeOrigin);
          if (variable2 == null)
          {
            if (string.IsNullOrEmpty(variablePath.LookupPath.NamespaceSpecificString))
              throw InterpreterError.NewInterpreterException((object) this._varName, typeof (RuntimeException), this.NodeToken, "InvalidVariableReference");
            variable2 = new PSVariable(variablePath.LookupPath.NamespaceSpecificString, (object) null, ScopedItemOptions.None, new Collection<Attribute>());
            engineSessionState.SetVariable(variablePath, (object) variable2, false, scopeOrigin);
            if (variablePath.IsLocal)
              scope = context.EngineSessionState.CurrentScope;
          }
          engineSessionState.CurrentActivationRecord.SetVariable(variable2, scope, this._activationRecordSlot);
          this.UpdateVariable(variable2, value);
        }
      }
    }

    private void UpdateVariable(PSVariable variable, object value)
    {
      ArgumentTypeConverterAttribute typeConverter = this.GetTypeConverter();
      if (typeConverter != null)
      {
        this.RemoveTypeConverter(variable);
        object obj = typeConverter.Transform((EngineIntrinsics) null, value);
        variable.Value = obj;
        variable.Attributes.Add((Attribute) typeConverter);
      }
      else
        variable.Value = value;
    }

    private void RemoveTypeConverter(PSVariable variable)
    {
      if (variable == null || variable.Attributes == null)
        return;
      for (int index = variable.Attributes.Count - 1; index >= 0; --index)
      {
        if (variable.Attributes[index] is ArgumentTypeConverterAttribute)
        {
          variable.Attributes.RemoveAt(index);
          break;
        }
      }
    }

    private ArgumentTypeConverterAttribute GetTypeConverter()
    {
      if (this._typeConstraint == null || this._typeConstraint.Count == 0)
        return (ArgumentTypeConverterAttribute) null;
      List<TypeLiteral> typeLiterals = new List<TypeLiteral>();
      for (int index = 0; index < this._typeConstraint.Count; ++index)
        typeLiterals.Add(this._typeConstraint[index]);
      return new ArgumentTypeConverterAttribute(typeLiterals);
    }

    internal void CheckTypeConstraintViolation()
    {
      if (this._typeConstraint == null || this._typeConstraint.Count <= 1)
        return;
      for (int index = 0; index < this._typeConstraint.Count; ++index)
      {
        if (this._typeConstraint[index].IsRef)
          throw InterpreterError.NewInterpreterException((object) this.NodeToken, typeof (RuntimeException), this.NodeToken, "ReferenceNeedsToBeByItselfInTypeConstraint");
      }
    }

    internal void CheckTypeConversionViolation()
    {
      if (this._typeConstraint == null || this._typeConstraint.Count <= 1)
        return;
      for (int index = 0; index < this._typeConstraint.Count - 1; ++index)
      {
        if (this._typeConstraint[index].IsRef)
          throw InterpreterError.NewInterpreterException((object) this.NodeToken, typeof (RuntimeException), this.NodeToken, "ReferenceNeedsToBeLastTypeInTypeConversion");
      }
    }

    internal override void Accept(ParseTreeVisitor visitor) => visitor.Visit(this);
  }
}
