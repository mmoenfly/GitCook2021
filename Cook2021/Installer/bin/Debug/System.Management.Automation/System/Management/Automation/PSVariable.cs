// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSVariable
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public class PSVariable : IHasSessionStateEntryVisibility
  {
    [TraceSource("SessionState", "Traces access to variables in session state.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (SessionState), "Traces access to variables in session state.");
    private string name = string.Empty;
    private string description = string.Empty;
    private object _value;
    private SessionStateEntryVisibility _visibility;
    private PSModuleInfo _module;
    private ScopedItemOptions options;
    private PSVariableAttributeCollection attributes;
    private bool _wasRemoved;
    private SessionStateInternal _sessionState;

    public PSVariable(string name)
      : this(name, (object) null, ScopedItemOptions.None, (Collection<Attribute>) null)
    {
    }

    public PSVariable(string name, object value)
      : this(name, value, ScopedItemOptions.None, (Collection<Attribute>) null)
    {
    }

    public PSVariable(string name, object value, ScopedItemOptions options)
      : this(name, value, options, (Collection<Attribute>) null)
    {
    }

    internal PSVariable(string name, object value, ScopedItemOptions options, string description)
      : this(name, value, options, (Collection<Attribute>) null)
      => this.description = description;

    internal PSVariable(
      string name,
      object value,
      ScopedItemOptions options,
      Collection<Attribute> attributes,
      string description)
      : this(name, value, options, attributes)
    {
      this.description = description;
    }

    public PSVariable(
      string name,
      object value,
      ScopedItemOptions options,
      Collection<Attribute> attributes)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSVariable.tracer.NewArgumentException(nameof (name));
      this.attributes = new PSVariableAttributeCollection(this);
      this.SetValueRaw(value, true);
      if (attributes != null)
      {
        foreach (Attribute attribute in attributes)
          this.attributes.Add(attribute);
      }
      this.options = options;
    }

    public string Name => this.name;

    public virtual string Description
    {
      get => this.description;
      set => this.description = value;
    }

    public virtual object Value
    {
      get
      {
        if (this._sessionState != null && this._sessionState.ExecutionContext.Debugger.IsOn)
          this._sessionState.ExecutionContext.Debugger.CheckVariableRead(this.Name);
        return this._value;
      }
      set => this.SetValue(value);
    }

    public SessionStateEntryVisibility Visibility
    {
      get => this._visibility;
      set => this._visibility = value;
    }

    public PSModuleInfo Module => this._module;

    internal void SetModule(PSModuleInfo module) => this._module = module;

    public string ModuleName => this._module != null ? this._module.Name : string.Empty;

    public virtual ScopedItemOptions Options
    {
      get => this.options;
      set => this.SetOptions(value, false);
    }

    internal void SetOptions(ScopedItemOptions newOptions, bool force)
    {
      if (this.IsConstant || !force && this.IsReadOnly)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.name, SessionStateCategory.Variable, "VariableNotWritable");
        PSVariable.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      if ((newOptions & ScopedItemOptions.Constant) != ScopedItemOptions.None)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.name, SessionStateCategory.Variable, "VariableCannotBeMadeConstant");
        PSVariable.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      if (this.IsAllScope && (newOptions & ScopedItemOptions.AllScope) == ScopedItemOptions.None)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.name, SessionStateCategory.Variable, "VariableAllScopeOptionCannotBeRemoved");
        PSVariable.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      this.options = newOptions;
    }

    public Collection<Attribute> Attributes
    {
      get
      {
        if (this.attributes == null)
          this.attributes = new PSVariableAttributeCollection(this);
        return (Collection<Attribute>) this.attributes;
      }
    }

    public virtual bool IsValidValue(object value)
    {
      bool flag = true;
      if (this.attributes != null && this.attributes.Count > 0)
      {
        foreach (Attribute attribute in (Collection<Attribute>) this.attributes)
        {
          if (!PSVariable.IsValidValue(value, attribute))
          {
            flag = false;
            break;
          }
        }
      }
      return flag;
    }

    internal static bool IsValidValue(object value, Attribute attribute)
    {
      bool flag = true;
      if (attribute is ValidateArgumentsAttribute argumentsAttribute)
      {
        try
        {
          ExecutionContext executionContextFromTls = LocalPipeline.GetExecutionContextFromTLS();
          EngineIntrinsics engineIntrinsics = (EngineIntrinsics) null;
          if (executionContextFromTls != null)
            engineIntrinsics = executionContextFromTls.EngineIntrinsics;
          argumentsAttribute.InternalValidate(value, engineIntrinsics);
        }
        catch (ValidationMetadataException ex)
        {
          flag = false;
        }
      }
      return flag;
    }

    internal object TransformValue(object value)
    {
      object inputData = value;
      if (this.attributes == null)
        return inputData;
      ExecutionContext executionContextFromTls = LocalPipeline.GetExecutionContextFromTLS();
      EngineIntrinsics engineIntrinsics = (EngineIntrinsics) null;
      if (executionContextFromTls != null)
        engineIntrinsics = executionContextFromTls.EngineIntrinsics;
      foreach (Attribute attribute in (Collection<Attribute>) this.attributes)
      {
        if (attribute is ArgumentTransformationAttribute transformationAttribute)
          inputData = transformationAttribute.Transform(engineIntrinsics, inputData);
      }
      return inputData;
    }

    internal void AddParameterAttributesNoChecks(Collection<Attribute> attributes)
    {
      foreach (Attribute attribute in attributes)
        this.attributes.AddAttributeNoCheck(attribute);
    }

    internal bool IsConstant => (this.options & ScopedItemOptions.Constant) != ScopedItemOptions.None;

    internal bool IsReadOnly => (this.options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None;

    internal bool IsPrivate => (this.options & ScopedItemOptions.Private) != ScopedItemOptions.None;

    internal bool IsAllScope => (this.options & ScopedItemOptions.AllScope) != ScopedItemOptions.None;

    internal bool WasRemoved
    {
      get => this._wasRemoved;
      set
      {
        this._wasRemoved = value;
        if (!value)
          return;
        this.options = ScopedItemOptions.None;
        this._value = (object) null;
        this._wasRemoved = true;
        this.attributes = (PSVariableAttributeCollection) null;
      }
    }

    internal SessionStateInternal SessionState
    {
      get => this._sessionState;
      set => this._sessionState = value;
    }

    private void SetValue(object value)
    {
      if ((this.options & (ScopedItemOptions.ReadOnly | ScopedItemOptions.Constant)) != ScopedItemOptions.None)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(this.name, SessionStateCategory.Variable, "VariableNotWritable");
        PSVariable.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      object obj = value;
      if (this.attributes != null && this.attributes.Count > 0)
      {
        obj = this.TransformValue(value);
        if (!this.IsValidValue(obj))
        {
          ValidationMetadataException metadataException = new ValidationMetadataException("ValidateSetFailure", (Exception) null, "Metadata", "InvalidValueFailure", new object[2]
          {
            (object) this.name,
            obj != null ? (object) obj.ToString() : (object) ""
          });
          PSVariable.tracer.TraceException((Exception) metadataException);
          throw metadataException;
        }
      }
      if (obj != null)
        obj = this.PreserveValueType(obj);
      if (this.SessionState != null && this.SessionState.ExecutionContext.Debugger.IsOn)
        this.SessionState.ExecutionContext.Debugger.CheckVariableWrite(this.Name);
      this._value = obj;
    }

    internal void SetValueRaw(object newValue, bool preserveValueTypeSemantics)
    {
      if (preserveValueTypeSemantics)
        this._value = this.PreserveValueType(newValue);
      else
        this._value = newValue;
    }

    internal void WrapValue()
    {
      if (this.IsConstant || this._value == null)
        return;
      this._value = (object) PSObject.AsPSObject(this._value);
    }

    private object PreserveValueType(object value)
    {
      if (value == null)
        return (object) null;
      Type type1 = value.GetType();
      if (type1.IsPrimitive)
        return value;
      if (type1.Equals(typeof (PSObject)))
      {
        PSObject psObject = value as PSObject;
        object baseObject = psObject.BaseObject;
        if (baseObject != null)
        {
          Type type2 = baseObject.GetType();
          if (type2.IsValueType && !type2.IsPrimitive)
            return (object) psObject.Copy();
        }
      }
      else if (type1.IsValueType)
      {
        Array instance = Array.CreateInstance(type1, 1);
        instance.SetValue(value, 0);
        return instance.GetValue(0);
      }
      return value;
    }
  }
}
