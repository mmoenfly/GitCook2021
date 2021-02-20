// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.FunctionProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace Microsoft.PowerShell.Commands
{
  [CmdletProvider("Function", ProviderCapabilities.ShouldProcess)]
  public sealed class FunctionProvider : SessionStateProviderBase
  {
    public const string ProviderName = "Function";
    [TraceSource("VariableProvider", "The core command provider for shell variables")]
    private static readonly PSTraceSource tracer = PSTraceSource.GetTracer("VariableProvider", "The core command provider for shell variables");

    protected override Collection<PSDriveInfo> InitializeDefaultDrives()
    {
      string resourceString = ResourceManagerCache.GetResourceString("SessionStateStrings", "FunctionDriveDescription");
      return new Collection<PSDriveInfo>()
      {
        new PSDriveInfo("Function", this.ProviderInfo, string.Empty, resourceString, (PSCredential) null)
      };
    }

    protected override object NewItemDynamicParameters(
      string path,
      string type,
      object newItemValue)
    {
      return (object) new FunctionProviderDynamicParameters();
    }

    protected override object SetItemDynamicParameters(string path, object value) => (object) new FunctionProviderDynamicParameters();

    internal override object GetSessionStateItem(string name) => (object) this.SessionState.Internal.GetFunction(name, this.Context.Origin);

    internal override void SetSessionStateItem(string name, object value, bool writeItem)
    {
      FunctionProviderDynamicParameters dynamicParameters = this.DynamicParameters as FunctionProviderDynamicParameters;
      bool flag = dynamicParameters != null && dynamicParameters.OptionsSet;
      if (value == null)
      {
        if (flag)
        {
          CommandInfo sessionStateItem = (CommandInfo) this.GetSessionStateItem(name);
          if (sessionStateItem == null)
            return;
          FunctionProvider.SetOptions(sessionStateItem, dynamicParameters.Options);
        }
        else
          this.RemoveSessionStateItem(name);
      }
      else
      {
        if (value is PSObject psObject)
          value = psObject.BaseObject;
        CommandInfo commandInfo;
        if (value is ScriptBlock function)
          commandInfo = !flag ? (CommandInfo) this.SessionState.Internal.SetFunction(name, function, (bool) this.Force, this.Context.Origin) : (CommandInfo) this.SessionState.Internal.SetFunction(name, function, dynamicParameters.Options, (bool) this.Force, this.Context.Origin);
        else if (value is FunctionInfo functionInfo)
        {
          ScopedItemOptions options = functionInfo.Options;
          if (flag)
            options = dynamicParameters.Options;
          commandInfo = (CommandInfo) this.SessionState.Internal.SetFunction(name, functionInfo.ScriptBlock, options, (bool) this.Force, this.Context.Origin);
        }
        else if (value is FilterInfo filterInfo)
        {
          ScopedItemOptions options = filterInfo.Options;
          if (flag)
            options = dynamicParameters.Options;
          commandInfo = (CommandInfo) this.SessionState.Internal.SetFunction(name, filterInfo.ScriptBlock, options, (bool) this.Force, this.Context.Origin);
        }
        else
        {
          if (!(value is string script))
            throw FunctionProvider.tracer.NewArgumentException(nameof (value));
          ScriptBlock function = ScriptBlock.Create(this.Context.ExecutionContext, script);
          commandInfo = !flag ? (CommandInfo) this.SessionState.Internal.SetFunction(name, function, (bool) this.Force, this.Context.Origin) : (CommandInfo) this.SessionState.Internal.SetFunction(name, function, dynamicParameters.Options, (bool) this.Force, this.Context.Origin);
        }
        if (!writeItem || commandInfo == null)
          return;
        this.WriteItemObject((object) commandInfo, commandInfo.Name, false);
      }
    }

    private static void SetOptions(CommandInfo function, ScopedItemOptions options)
    {
      if (function is FilterInfo filterInfo)
        filterInfo.Options = options;
      else
        ((FunctionInfo) function).Options = options;
    }

    internal override void RemoveSessionStateItem(string name) => this.SessionState.Internal.RemoveFunction(name, (bool) this.Force);

    internal override object GetValueOfItem(object item)
    {
      object obj = item;
      switch (item)
      {
        case FunctionInfo functionInfo:
          obj = (object) functionInfo.ScriptBlock;
          break;
        case FilterInfo filterInfo:
          obj = (object) filterInfo.ScriptBlock;
          break;
      }
      return obj;
    }

    internal override IDictionary GetSessionStateTable() => this.SessionState.Internal.GetFunctionTable();

    internal override bool CanRenameItem(object item)
    {
      bool flag = false;
      switch (item)
      {
        case FunctionInfo functionInfo:
          if ((functionInfo.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None || (functionInfo.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None && !(bool) this.Force)
          {
            SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(functionInfo.Name, SessionStateCategory.Function, "CannotRenameFunction");
            FunctionProvider.tracer.TraceException((Exception) unauthorizedAccessException);
            throw unauthorizedAccessException;
          }
          flag = true;
          break;
        case FilterInfo filterInfo:
          if ((filterInfo.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None || (filterInfo.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None && !(bool) this.Force)
          {
            SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(filterInfo.Name, SessionStateCategory.Filter, "CannotRenameFilter");
            FunctionProvider.tracer.TraceException((Exception) unauthorizedAccessException);
            throw unauthorizedAccessException;
          }
          flag = true;
          break;
      }
      FunctionProvider.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }
  }
}
