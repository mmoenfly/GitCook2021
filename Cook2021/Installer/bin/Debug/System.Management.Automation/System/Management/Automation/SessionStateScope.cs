// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SessionStateScope
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  internal sealed class SessionStateScope
  {
    [TraceSource("SessionStateScope", "A scope of session state that holds virtual drives")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (SessionStateScope), "A scope of session state that holds virtual drives");
    private CommandOrigin _scopeOrigin = CommandOrigin.Internal;
    private List<SessionStateScope> _children;
    private SessionStateScope scriptScope;
    private Version strictModeVersion;
    private SessionStateScope parent;
    private Dictionary<string, PSDriveInfo> _drives;
    private Dictionary<string, PSDriveInfo> _automountedDrives;
    private Dictionary<string, PSVariable> _variables;
    private Dictionary<string, AliasInfo> _alias;
    private Dictionary<string, FunctionInfo> _functions;
    private Dictionary<string, FunctionInfo> _allScopeFunctions;
    private Dictionary<string, Runspace> _runspaces;
    private SessionStateCapacityVariable _errorCapacity;
    private SessionStateCapacityVariable variableCapacity;
    private SessionStateCapacityVariable functionCapacity;
    private SessionStateCapacityVariable aliasCapacity;
    private SessionStateCapacityVariable driveCapacity;
    private static readonly PSVariable trueVar = new PSVariable("true", (object) true, ScopedItemOptions.Constant | ScopedItemOptions.AllScope, "Boolean True");
    private static readonly PSVariable falseVar = new PSVariable("false", (object) false, ScopedItemOptions.Constant | ScopedItemOptions.AllScope, "Boolean False");
    private static readonly NullVariable nullVar = new NullVariable();

    internal SessionStateScope(SessionStateScope parentScope)
    {
      using (SessionStateScope.tracer.TraceConstructor((object) this))
      {
        this.parent = parentScope;
        if (parentScope != null)
        {
          parentScope.Children.Add(this);
          this.scriptScope = parentScope.ScriptScope;
        }
        else
          this.scriptScope = this;
      }
    }

    private SessionStateCapacityVariable CreateCapacityVariable(
      string variableName,
      int defaultCapacity,
      int maxCapacity,
      int minCapacity,
      string descriptionResourceId)
    {
      SessionStateCapacityVariable sharedCapacityVariable = (SessionStateCapacityVariable) null;
      if (this.parent != null)
        sharedCapacityVariable = this.parent.GetVariable(variableName) as SessionStateCapacityVariable;
      SessionStateCapacityVariable capacityVariable = sharedCapacityVariable != null ? new SessionStateCapacityVariable(variableName, sharedCapacityVariable, ScopedItemOptions.None) : new SessionStateCapacityVariable(variableName, defaultCapacity, maxCapacity, minCapacity, ScopedItemOptions.None);
      if (string.IsNullOrEmpty(capacityVariable.Description))
      {
        string resourceString = ResourceManagerCache.GetResourceString("SessionStateStrings", descriptionResourceId);
        capacityVariable.Description = resourceString;
      }
      return capacityVariable;
    }

    internal SessionStateScope Parent
    {
      get => this.parent;
      set => this.parent = value;
    }

    internal CommandOrigin ScopeOrigin
    {
      get => this._scopeOrigin;
      set => this._scopeOrigin = value;
    }

    internal List<SessionStateScope> Children
    {
      get
      {
        if (this._children == null)
          this._children = new List<SessionStateScope>();
        return this._children;
      }
    }

    internal SessionStateScope ScriptScope
    {
      get => this.scriptScope;
      set => this.scriptScope = value != null ? value : throw SessionStateScope.tracer.NewArgumentNullException(nameof (value));
    }

    internal Version StrictModeVersion
    {
      get => this.strictModeVersion;
      set => this.strictModeVersion = value;
    }

    internal void NewDrive(PSDriveInfo newDrive)
    {
      if (newDrive == (PSDriveInfo) null)
        throw SessionStateScope.tracer.NewArgumentNullException(nameof (newDrive));
      if (this.PrivateDrives.ContainsKey(newDrive.Name))
      {
        SessionStateException sessionStateException = new SessionStateException(newDrive.Name, SessionStateCategory.Drive, "DriveAlreadyExists", ErrorCategory.ResourceExists, new object[0]);
        SessionStateScope.tracer.TraceException((Exception) sessionStateException);
        throw sessionStateException;
      }
      if (!newDrive.IsAutoMounted && this.PrivateDrives.Count > this.DriveCapacity.FastValue - 1)
      {
        SessionStateOverflowException overflowException = new SessionStateOverflowException(newDrive.Name, SessionStateCategory.Drive, "DriveOverflow", new object[1]
        {
          (object) this.DriveCapacity.FastValue
        });
        SessionStateScope.tracer.TraceException((Exception) overflowException);
        throw overflowException;
      }
      if (!newDrive.IsAutoMounted)
      {
        this.PrivateDrives.Add(newDrive.Name, newDrive);
      }
      else
      {
        if (this.PrivateAutomountedDrives.ContainsKey(newDrive.Name))
          return;
        this.PrivateAutomountedDrives.Add(newDrive.Name, newDrive);
      }
    }

    internal void RemoveDrive(PSDriveInfo drive)
    {
      if (drive == (PSDriveInfo) null)
        throw SessionStateScope.tracer.NewArgumentNullException(nameof (drive));
      if (this._drives == null)
        return;
      if (this.PrivateDrives.ContainsKey(drive.Name))
      {
        this.PrivateDrives.Remove(drive.Name);
      }
      else
      {
        if (!this.PrivateAutomountedDrives.ContainsKey(drive.Name))
          return;
        this.PrivateAutomountedDrives[drive.Name].IsAutoMountedManuallyRemoved = true;
      }
    }

    internal void RemoveAllDrives()
    {
      this.PrivateDrives.Clear();
      this.PrivateAutomountedDrives.Clear();
    }

    internal PSDriveInfo GetDrive(string name)
    {
      if (name == null)
        throw SessionStateScope.tracer.NewArgumentNullException(nameof (name));
      PSDriveInfo psDriveInfo = (PSDriveInfo) null;
      if (this.PrivateDrives.ContainsKey(name))
        psDriveInfo = this.PrivateDrives[name];
      else if (this.PrivateAutomountedDrives.ContainsKey(name))
        psDriveInfo = this.PrivateAutomountedDrives[name];
      return psDriveInfo;
    }

    internal IEnumerable<PSDriveInfo> Drives
    {
      get
      {
        Collection<PSDriveInfo> collection = new Collection<PSDriveInfo>();
        foreach (PSDriveInfo psDriveInfo in this.PrivateDrives.Values)
          collection.Add(psDriveInfo);
        foreach (PSDriveInfo psDriveInfo in this.PrivateAutomountedDrives.Values)
        {
          if (!psDriveInfo.IsAutoMountedManuallyRemoved)
            collection.Add(psDriveInfo);
        }
        return (IEnumerable<PSDriveInfo>) collection;
      }
    }

    internal IDictionary<string, PSVariable> Variables => (IDictionary<string, PSVariable>) this.PrivateVariables;

    internal PSVariable GetVariable(string name, CommandOrigin origin)
    {
      PSVariable psVariable = (PSVariable) null;
      if (this.PrivateVariables.ContainsKey(name))
      {
        psVariable = this.PrivateVariables[name];
        SessionState.ThrowIfNotVisible(origin, (object) psVariable);
      }
      return psVariable;
    }

    internal PSVariable GetVariable(string name) => this.GetVariable(name, this._scopeOrigin);

    internal PSVariable SetVariable(
      string name,
      object value,
      bool asValue,
      bool force,
      SessionStateInternal sessionState)
    {
      return this.SetVariable(name, value, asValue, force, sessionState, CommandOrigin.Internal);
    }

    internal PSVariable SetVariable(
      string name,
      object value,
      bool asValue,
      bool force,
      SessionStateInternal sessionState,
      CommandOrigin origin)
    {
      PSVariable psVariable1 = value as PSVariable;
      bool flag = this.PrivateVariables.ContainsKey(name);
      PSVariable psVariable2;
      if (!asValue && psVariable1 != null)
      {
        if (flag)
        {
          psVariable2 = this.GetVariable(name);
          if (psVariable2 == null || psVariable2.IsConstant || !force && psVariable2.IsReadOnly)
          {
            SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Variable, "VariableNotWritable");
            SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
            throw unauthorizedAccessException;
          }
          if (psVariable2.IsReadOnly && force)
          {
            this.PrivateVariables.Remove(name);
            flag = false;
            psVariable2 = new PSVariable(name, psVariable1.Value, psVariable1.Options, psVariable1.Attributes);
            psVariable2.Description = psVariable1.Description;
          }
          else
          {
            psVariable2.Attributes.Clear();
            psVariable2.Value = psVariable1.Value;
            psVariable2.Options = psVariable1.Options;
            psVariable2.Description = psVariable1.Description;
            foreach (Attribute attribute in psVariable1.Attributes)
              psVariable2.Attributes.Add(attribute);
          }
        }
        else
          psVariable2 = psVariable1;
      }
      else
      {
        psVariable2 = this.GetVariable(name);
        if (psVariable2 != null)
          psVariable2.Value = value;
        else
          psVariable2 = new PSVariable(name, value);
      }
      if (!flag && this.PrivateVariables.Count > this.VariableCapacity.FastValue - 1)
      {
        SessionStateOverflowException overflowException = new SessionStateOverflowException(name, SessionStateCategory.Variable, "VariableOverflow", new object[1]
        {
          (object) this.VariableCapacity.FastValue
        });
        SessionStateScope.tracer.TraceException((Exception) overflowException);
        throw overflowException;
      }
      this.PrivateVariables[name] = psVariable2;
      psVariable2.SessionState = sessionState;
      return psVariable2;
    }

    internal PSVariable NewVariable(
      PSVariable newVariable,
      bool force,
      SessionStateInternal sessionState)
    {
      bool flag = this.PrivateVariables.ContainsKey(newVariable.Name);
      PSVariable psVariable;
      if (flag)
      {
        PSVariable variable = this.GetVariable(newVariable.Name);
        if (variable == null || variable.IsConstant || !force && variable.IsReadOnly)
        {
          SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(newVariable.Name, SessionStateCategory.Variable, "VariableNotWritable");
          SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
          throw unauthorizedAccessException;
        }
        variable.WasRemoved = true;
        psVariable = newVariable;
      }
      else
        psVariable = newVariable;
      if (!flag && this.PrivateVariables.Count > this.VariableCapacity.FastValue - 1)
      {
        SessionStateOverflowException overflowException = new SessionStateOverflowException(newVariable.Name, SessionStateCategory.Variable, "VariableOverflow", new object[1]
        {
          (object) this.VariableCapacity.FastValue
        });
        SessionStateScope.tracer.TraceException((Exception) overflowException);
        throw overflowException;
      }
      this.PrivateVariables[psVariable.Name] = psVariable;
      psVariable.SessionState = sessionState;
      return psVariable;
    }

    internal void RemoveVariable(string name, bool force)
    {
      PSVariable variable = this.GetVariable(name);
      if (variable.IsConstant || variable.IsReadOnly && !force)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Variable, "VariableNotRemovable");
        SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      if (variable is SessionStateCapacityVariable)
      {
        SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Variable, "VariableNotRemovableSystem");
        SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
        throw unauthorizedAccessException;
      }
      this.PrivateVariables.Remove(name);
      variable.WasRemoved = true;
    }

    internal IEnumerable<AliasInfo> AliasTable => (IEnumerable<AliasInfo>) this.PrivateAlias.Values;

    internal AliasInfo GetAlias(string name)
    {
      AliasInfo aliasInfo = (AliasInfo) null;
      if (this.PrivateAlias.ContainsKey(name))
        aliasInfo = this.PrivateAlias[name];
      return aliasInfo;
    }

    internal AliasInfo SetAliasValue(
      string name,
      string value,
      ExecutionContext context,
      bool force,
      CommandOrigin origin)
    {
      if (!this.PrivateAlias.ContainsKey(name))
      {
        if (this.PrivateAlias.Count > this.AliasCapacity.FastValue - 1)
        {
          SessionStateOverflowException overflowException = new SessionStateOverflowException(name, SessionStateCategory.Alias, "AliasOverflow", new object[1]
          {
            (object) this.AliasCapacity.FastValue
          });
          SessionStateScope.tracer.TraceException((Exception) overflowException);
          throw overflowException;
        }
        this.PrivateAlias[name] = new AliasInfo(name, value, context);
      }
      else
      {
        AliasInfo privateAlia = this.PrivateAlias[name];
        if ((privateAlia.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None || !force && (privateAlia.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None)
        {
          SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Alias, "AliasNotWritable");
          SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
          throw unauthorizedAccessException;
        }
        SessionState.ThrowIfNotVisible(origin, (object) privateAlia);
        if (force)
        {
          this.PrivateAlias.Remove(name);
          AliasInfo aliasInfo = new AliasInfo(name, value, context);
          this.PrivateAlias[name] = aliasInfo;
        }
        else
          privateAlia.SetDefinition(value, force);
      }
      return this.PrivateAlias[name];
    }

    internal AliasInfo SetAliasValue(
      string name,
      string value,
      ScopedItemOptions options,
      ExecutionContext context,
      bool force,
      CommandOrigin origin)
    {
      if (!this.PrivateAlias.ContainsKey(name))
      {
        if (this.PrivateAlias.Count > this.AliasCapacity.FastValue - 1)
        {
          SessionStateOverflowException overflowException = new SessionStateOverflowException(name, SessionStateCategory.Alias, "AliasOverflow", new object[1]
          {
            (object) this.AliasCapacity.FastValue
          });
          SessionStateScope.tracer.TraceException((Exception) overflowException);
          throw overflowException;
        }
        AliasInfo aliasInfo = new AliasInfo(name, value, context, options);
        this.PrivateAlias[name] = aliasInfo;
      }
      else
      {
        AliasInfo privateAlia = this.PrivateAlias[name];
        if ((privateAlia.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None || !force && (privateAlia.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None)
        {
          SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Alias, "AliasNotWritable");
          SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
          throw unauthorizedAccessException;
        }
        if ((options & ScopedItemOptions.Constant) != ScopedItemOptions.None)
        {
          SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Alias, "AliasCannotBeMadeConstant");
          SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
          throw unauthorizedAccessException;
        }
        if ((options & ScopedItemOptions.AllScope) == ScopedItemOptions.None && (privateAlia.Options & ScopedItemOptions.AllScope) != ScopedItemOptions.None)
        {
          SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Alias, "AliasAllScopeOptionCannotBeRemoved");
          SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
          throw unauthorizedAccessException;
        }
        SessionState.ThrowIfNotVisible(origin, (object) privateAlia);
        if (force)
        {
          this.PrivateAlias.Remove(name);
          AliasInfo aliasInfo = new AliasInfo(name, value, context, options);
          this.PrivateAlias[name] = aliasInfo;
        }
        else
        {
          privateAlia.Options = options;
          privateAlia.SetDefinition(value, force);
        }
      }
      return this.PrivateAlias[name];
    }

    internal AliasInfo SetAliasItem(AliasInfo aliasToSet, bool force) => this.SetAliasItem(aliasToSet, force, CommandOrigin.Internal);

    internal AliasInfo SetAliasItem(
      AliasInfo aliasToSet,
      bool force,
      CommandOrigin origin)
    {
      if (!this.PrivateAlias.ContainsKey(aliasToSet.Name))
      {
        if (this.PrivateAlias.Count > this.AliasCapacity.FastValue - 1)
        {
          SessionStateOverflowException overflowException = new SessionStateOverflowException(aliasToSet.Name, SessionStateCategory.Alias, "AliasOverflow", new object[1]
          {
            (object) this.AliasCapacity.FastValue
          });
          SessionStateScope.tracer.TraceException((Exception) overflowException);
          throw overflowException;
        }
        this.PrivateAlias[aliasToSet.Name] = aliasToSet;
      }
      else
      {
        AliasInfo privateAlia = this.PrivateAlias[aliasToSet.Name];
        SessionState.ThrowIfNotVisible(origin, (object) privateAlia);
        if ((privateAlia.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None || (privateAlia.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None && !force)
        {
          SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(aliasToSet.Name, SessionStateCategory.Alias, "AliasNotWritable");
          SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
          throw unauthorizedAccessException;
        }
        if ((aliasToSet.Options & ScopedItemOptions.AllScope) == ScopedItemOptions.None && (privateAlia.Options & ScopedItemOptions.AllScope) != ScopedItemOptions.None)
        {
          SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(aliasToSet.Name, SessionStateCategory.Alias, "AliasAllScopeOptionCannotBeRemoved");
          SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
          throw unauthorizedAccessException;
        }
        this.PrivateAlias[aliasToSet.Name] = aliasToSet;
      }
      return this.PrivateAlias[aliasToSet.Name];
    }

    internal void RemoveAlias(string name, bool force)
    {
      if (this.PrivateAlias.ContainsKey(name))
      {
        AliasInfo privateAlia = this.PrivateAlias[name];
        if ((privateAlia.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None || !force && (privateAlia.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None)
        {
          SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Alias, "AliasNotRemovable");
          SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
          throw unauthorizedAccessException;
        }
      }
      this.PrivateAlias.Remove(name);
    }

    internal Dictionary<string, FunctionInfo> FunctionTable => this.PrivateFunctions;

    internal FunctionInfo GetFunction(string name)
    {
      FunctionInfo functionInfo = (FunctionInfo) null;
      if (this.PrivateFunctions.ContainsKey(name))
        functionInfo = this.PrivateFunctions[name];
      return functionInfo;
    }

    internal FunctionInfo SetFunction(
      string name,
      ScriptBlock function,
      bool force,
      CommandOrigin origin,
      ExecutionContext context)
    {
      if (!this.PrivateFunctions.ContainsKey(name))
      {
        if (this.PrivateFunctions.Count > this.FunctionCapacity.FastValue - 1)
        {
          SessionStateOverflowException overflowException = new SessionStateOverflowException(name, SessionStateCategory.Function, "FunctionOverflow", new object[1]
          {
            (object) this.FunctionCapacity.FastValue
          });
          SessionStateScope.tracer.TraceException((Exception) overflowException);
          throw overflowException;
        }
        FunctionInfo functionInfo = !function.IsFilter ? new FunctionInfo(name, function, context) : (FunctionInfo) new FilterInfo(name, function, context);
        this.PrivateFunctions[name] = functionInfo;
      }
      else
      {
        FunctionInfo privateFunction = this.PrivateFunctions[name];
        SessionState.ThrowIfNotVisible(origin, (object) privateFunction);
        if (SessionStateScope.IsFunctionOptionSet(privateFunction, ScopedItemOptions.Constant) || !force && SessionStateScope.IsFunctionOptionSet(privateFunction, ScopedItemOptions.ReadOnly))
        {
          SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Function, "FunctionNotWritable");
          SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
          throw unauthorizedAccessException;
        }
        FunctionInfo functionInfo1 = privateFunction;
        if (functionInfo1 != null)
        {
          if ((functionInfo1.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None && force)
          {
            FunctionInfo functionInfo2 = new FunctionInfo(name, function, context);
            this.PrivateFunctions[name] = functionInfo2;
          }
          else
            functionInfo1.SetScriptBlock(function, force);
        }
        else if (privateFunction is FilterInfo filterInfo)
        {
          if ((filterInfo.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None && force)
          {
            FilterInfo filterInfo = new FilterInfo(name, function, context);
            this.PrivateFunctions[name] = (FunctionInfo) filterInfo;
          }
          else
            filterInfo.SetScriptBlock(function, force);
        }
      }
      return this.PrivateFunctions[name];
    }

    internal FunctionInfo SetFunction(
      string name,
      ScriptBlock function,
      ScopedItemOptions options,
      bool force,
      CommandOrigin origin,
      ExecutionContext context)
    {
      if (!this.PrivateFunctions.ContainsKey(name))
      {
        if (this.PrivateFunctions.Count > this.FunctionCapacity.FastValue - 1)
        {
          SessionStateOverflowException overflowException = new SessionStateOverflowException(name, SessionStateCategory.Function, "FunctionOverflow", new object[1]
          {
            (object) this.FunctionCapacity.FastValue
          });
          SessionStateScope.tracer.TraceException((Exception) overflowException);
          throw overflowException;
        }
        FunctionInfo function1 = !function.IsFilter ? new FunctionInfo(name, function, options, context) : (FunctionInfo) new FilterInfo(name, function, options, context);
        this.PrivateFunctions[name] = function1;
        if (SessionStateScope.IsFunctionOptionSet(function1, ScopedItemOptions.AllScope))
          this.PrivateAllScopeFunctions[name] = function1;
      }
      else
      {
        FunctionInfo privateFunction = this.PrivateFunctions[name];
        SessionState.ThrowIfNotVisible(origin, (object) privateFunction);
        if (SessionStateScope.IsFunctionOptionSet(privateFunction, ScopedItemOptions.Constant) || !force && SessionStateScope.IsFunctionOptionSet(privateFunction, ScopedItemOptions.ReadOnly))
        {
          SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Function, "FunctionNotWritable");
          SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
          throw unauthorizedAccessException;
        }
        if ((options & ScopedItemOptions.Constant) != ScopedItemOptions.None)
        {
          SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Function, "FunctionCannotBeMadeConstant");
          SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
          throw unauthorizedAccessException;
        }
        if ((options & ScopedItemOptions.AllScope) == ScopedItemOptions.None && SessionStateScope.IsFunctionOptionSet(privateFunction, ScopedItemOptions.AllScope))
        {
          SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Function, "FunctionAllScopeOptionCannotBeRemoved");
          SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
          throw unauthorizedAccessException;
        }
        bool force1 = force || (options & ScopedItemOptions.ReadOnly) == ScopedItemOptions.None;
        FunctionInfo functionInfo1 = privateFunction;
        if (functionInfo1 != null)
        {
          if ((functionInfo1.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None && force)
          {
            FunctionInfo functionInfo2 = new FunctionInfo(name, function, options, context);
            this.PrivateFunctions[name] = functionInfo2;
          }
          else
          {
            functionInfo1.SetScriptBlock(function, force1);
            functionInfo1.Options = options;
          }
        }
        else if (privateFunction is FilterInfo filterInfo)
        {
          if ((filterInfo.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None && force)
          {
            FilterInfo filterInfo = new FilterInfo(name, function, options, context);
            this.PrivateFunctions[name] = (FunctionInfo) filterInfo;
          }
          else
          {
            filterInfo.SetScriptBlock(function, force1);
            filterInfo.Options = options;
          }
        }
      }
      return this.PrivateFunctions[name];
    }

    internal void RemoveFunction(string name, bool force)
    {
      if (this.PrivateFunctions.ContainsKey(name))
      {
        FunctionInfo privateFunction = this.PrivateFunctions[name];
        if (SessionStateScope.IsFunctionOptionSet(privateFunction, ScopedItemOptions.Constant) || !force && SessionStateScope.IsFunctionOptionSet(privateFunction, ScopedItemOptions.ReadOnly))
        {
          SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(name, SessionStateCategory.Function, "FunctionNotRemovable");
          SessionStateScope.tracer.TraceException((Exception) unauthorizedAccessException);
          throw unauthorizedAccessException;
        }
        if (SessionStateScope.IsFunctionOptionSet(privateFunction, ScopedItemOptions.AllScope))
          this.PrivateAllScopeFunctions.Remove(name);
      }
      this.PrivateFunctions.Remove(name);
    }

    internal IDictionary<string, Runspace> RunspaceTable => (IDictionary<string, Runspace>) this.PrivateRunspaces;

    internal Runspace GetRunspace(string id)
    {
      Runspace runspace = (Runspace) null;
      if (this.PrivateRunspaces.ContainsKey(id))
        runspace = this.PrivateRunspaces[id];
      return runspace;
    }

    internal Runspace SetRunspace(Runspace value)
    {
      string key = value.InstanceId.ToString();
      this.PrivateRunspaces[key] = value;
      return this.PrivateRunspaces[key];
    }

    internal void RemoveRunspace(string id) => this.PrivateRunspaces.Remove(id);

    internal void CloseRunspaces()
    {
      foreach (Runspace runspace in this.PrivateRunspaces.Values)
      {
        int state = (int) runspace.RunspaceStateInfo.State;
        if (runspace.RunspaceStateInfo.State == RunspaceState.Opened)
          runspace.Close();
      }
    }

    private static bool IsFunctionOptionSet(FunctionInfo function, ScopedItemOptions options)
    {
      FunctionInfo functionInfo = function;
      return functionInfo == null ? (function.Options & options) != ScopedItemOptions.None : (functionInfo.Options & options) != ScopedItemOptions.None;
    }

    private Dictionary<string, PSDriveInfo> PrivateDrives
    {
      get
      {
        if (this._drives == null)
          this._drives = new Dictionary<string, PSDriveInfo>((IEqualityComparer<string>) StringComparer.Create(CultureInfo.InvariantCulture, true));
        return this._drives;
      }
    }

    private Dictionary<string, PSDriveInfo> PrivateAutomountedDrives
    {
      get
      {
        if (this._automountedDrives == null)
          this._automountedDrives = new Dictionary<string, PSDriveInfo>((IEqualityComparer<string>) StringComparer.Create(CultureInfo.InvariantCulture, true));
        return this._automountedDrives;
      }
    }

    private Dictionary<string, PSVariable> PrivateVariables => this.InitializePrivateVariables();

    private Dictionary<string, PSVariable> InitializePrivateVariables()
    {
      if (this._variables == null)
      {
        this._variables = new Dictionary<string, PSVariable>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        if (this.parent == null)
        {
          this._variables.Add(SessionStateScope.nullVar.Name, (PSVariable) SessionStateScope.nullVar);
          this._variables.Add(SessionStateScope.falseVar.Name, SessionStateScope.falseVar);
          this._variables.Add(SessionStateScope.trueVar.Name, SessionStateScope.trueVar);
        }
        else
        {
          foreach (PSVariable psVariable in this.parent.PrivateVariables.Values)
          {
            if (psVariable.IsAllScope)
              this._variables.Add(psVariable.Name, psVariable);
          }
        }
        string str1 = "MaximumErrorCount";
        this._errorCapacity = this.CreateCapacityVariable(str1, 256, 32768, 256, "MaxErrorCountDescription");
        this._variables.Add(str1, (PSVariable) this._errorCapacity);
        string str2 = "MaximumVariableCount";
        this.variableCapacity = this.CreateCapacityVariable(str2, 4096, 32768, 1024, "MaxVariableCountDescription");
        this._variables.Add(str2, (PSVariable) this.variableCapacity);
        string str3 = "MaximumFunctionCount";
        this.functionCapacity = this.CreateCapacityVariable(str3, 4096, 32768, 1024, "MaxFunctionCountDescription");
        this._variables.Add(str3, (PSVariable) this.functionCapacity);
        string str4 = "MaximumAliasCount";
        this.aliasCapacity = this.CreateCapacityVariable(str4, 4096, 32768, 1024, "MaxAliasCountDescription");
        this._variables.Add(str4, (PSVariable) this.aliasCapacity);
        string str5 = "MaximumDriveCount";
        this.driveCapacity = this.CreateCapacityVariable(str5, 4096, 32768, 1024, "MaxDriveCountDescription");
        this._variables.Add(str5, (PSVariable) this.driveCapacity);
      }
      return this._variables;
    }

    private Dictionary<string, AliasInfo> PrivateAlias
    {
      get
      {
        if (this._alias == null)
        {
          this._alias = new Dictionary<string, AliasInfo>((IEqualityComparer<string>) StringComparer.Create(CultureInfo.InvariantCulture, true));
          if (this.parent != null)
          {
            foreach (AliasInfo aliasInfo in this.parent.PrivateAlias.Values)
            {
              if ((aliasInfo.Options & ScopedItemOptions.AllScope) != ScopedItemOptions.None)
                this._alias.Add(aliasInfo.Name, aliasInfo);
            }
          }
        }
        return this._alias;
      }
    }

    private Dictionary<string, FunctionInfo> PrivateFunctions
    {
      get
      {
        if (this._functions == null)
        {
          this._functions = new Dictionary<string, FunctionInfo>((IEqualityComparer<string>) StringComparer.Create(CultureInfo.InvariantCulture, true));
          if (this.parent != null && this.parent._allScopeFunctions != null)
          {
            foreach (FunctionInfo functionInfo in this.parent._allScopeFunctions.Values)
              this._functions.Add(functionInfo.Name, functionInfo);
          }
        }
        return this._functions;
      }
    }

    private Dictionary<string, FunctionInfo> PrivateAllScopeFunctions
    {
      get
      {
        if (this._allScopeFunctions == null)
        {
          if (this.parent != null && this.parent._allScopeFunctions != null)
            return this.parent._allScopeFunctions;
          this._allScopeFunctions = new Dictionary<string, FunctionInfo>((IEqualityComparer<string>) StringComparer.Create(CultureInfo.InvariantCulture, true));
        }
        return this._allScopeFunctions;
      }
    }

    private Dictionary<string, Runspace> PrivateRunspaces
    {
      get
      {
        if (this._runspaces == null)
        {
          StringComparer.Create(CultureInfo.InvariantCulture, true);
          this._runspaces = new Dictionary<string, Runspace>();
        }
        return this._runspaces;
      }
    }

    internal SessionStateCapacityVariable ErrorCapacity
    {
      get
      {
        this.InitializePrivateVariables();
        return this._errorCapacity;
      }
    }

    internal SessionStateCapacityVariable VariableCapacity
    {
      get
      {
        this.InitializePrivateVariables();
        return this.variableCapacity;
      }
    }

    private SessionStateCapacityVariable FunctionCapacity
    {
      get
      {
        this.InitializePrivateVariables();
        return this.functionCapacity;
      }
    }

    private SessionStateCapacityVariable AliasCapacity
    {
      get
      {
        this.InitializePrivateVariables();
        return this.aliasCapacity;
      }
    }

    private SessionStateCapacityVariable DriveCapacity
    {
      get
      {
        this.InitializePrivateVariables();
        return this.driveCapacity;
      }
    }
  }
}
