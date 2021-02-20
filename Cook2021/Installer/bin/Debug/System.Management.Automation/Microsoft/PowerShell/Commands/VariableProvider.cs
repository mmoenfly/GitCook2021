// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.VariableProvider
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
  [CmdletProvider("Variable", ProviderCapabilities.ShouldProcess)]
  public sealed class VariableProvider : SessionStateProviderBase
  {
    public const string ProviderName = "Variable";
    [TraceSource("VariableProvider", "The core command provider for shell variables")]
    private static readonly PSTraceSource tracer = PSTraceSource.GetTracer(nameof (VariableProvider), "The core command provider for shell variables");

    public VariableProvider()
    {
      using (VariableProvider.tracer.TraceConstructor((object) this))
        ;
    }

    protected override Collection<PSDriveInfo> InitializeDefaultDrives()
    {
      using (VariableProvider.tracer.TraceMethod())
      {
        string resourceString = ResourceManagerCache.GetResourceString("SessionStateStrings", "VariableDriveDescription");
        return new Collection<PSDriveInfo>()
        {
          new PSDriveInfo("Variable", this.ProviderInfo, string.Empty, resourceString, (PSCredential) null)
        };
      }
    }

    internal override object GetSessionStateItem(string name)
    {
      using (VariableProvider.tracer.TraceMethod(name, new object[0]))
        return (object) this.SessionState.Internal.GetVariable(name, this.Context.Origin);
    }

    internal override void SetSessionStateItem(string name, object value, bool writeItem)
    {
      using (VariableProvider.tracer.TraceMethod(name, new object[0]))
      {
        if (value != null)
        {
          if (!(value is PSVariable variable))
            variable = new PSVariable(name, value);
          else if (!string.Equals(name, variable.Name, StringComparison.OrdinalIgnoreCase))
            variable = new PSVariable(name, variable.Value, variable.Options, variable.Attributes)
            {
              Description = variable.Description
            };
        }
        else
          variable = new PSVariable(name, (object) null);
        PSVariable psVariable = this.SessionState.Internal.SetVariable(variable, (bool) this.Force, this.Context.Origin) as PSVariable;
        if (!writeItem || psVariable == null)
          return;
        this.WriteItemObject((object) psVariable, psVariable.Name, false);
      }
    }

    internal override void RemoveSessionStateItem(string name)
    {
      using (VariableProvider.tracer.TraceMethod(name, new object[0]))
        this.SessionState.Internal.RemoveVariable(name, (bool) this.Force);
    }

    internal override IDictionary GetSessionStateTable()
    {
      using (VariableProvider.tracer.TraceMethod())
        return (IDictionary) this.SessionState.Internal.GetVariableTable();
    }

    internal override object GetValueOfItem(object item)
    {
      using (VariableProvider.tracer.TraceMethod())
      {
        object valueOfItem = base.GetValueOfItem(item);
        if (item is PSVariable psVariable)
          valueOfItem = psVariable.Value;
        return valueOfItem;
      }
    }

    internal override bool CanRenameItem(object item)
    {
      using (VariableProvider.tracer.TraceMethod())
      {
        bool flag = false;
        if (item is PSVariable psVariable)
        {
          if ((psVariable.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None || (psVariable.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None && !(bool) this.Force)
          {
            SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(psVariable.Name, SessionStateCategory.Variable, "CannotRenameVariable");
            VariableProvider.tracer.TraceException((Exception) unauthorizedAccessException);
            throw unauthorizedAccessException;
          }
          flag = true;
        }
        VariableProvider.tracer.WriteLine("result = {0}", (object) flag);
        return flag;
      }
    }
  }
}
