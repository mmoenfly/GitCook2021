// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.AliasProvider
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
  [CmdletProvider("Alias", ProviderCapabilities.ShouldProcess)]
  public sealed class AliasProvider : SessionStateProviderBase
  {
    public const string ProviderName = "Alias";
    [TraceSource("AliasProvider", "The core command provider for shell aliases")]
    private static readonly PSTraceSource tracer = PSTraceSource.GetTracer(nameof (AliasProvider), "The CmdletProvider for shell aliases");

    public AliasProvider()
    {
      using (AliasProvider.tracer.TraceConstructor((object) this))
        ;
    }

    protected override Collection<PSDriveInfo> InitializeDefaultDrives()
    {
      using (AliasProvider.tracer.TraceMethod())
      {
        string resourceString = ResourceManagerCache.GetResourceString("SessionStateStrings", "AliasDriveDescription");
        return new Collection<PSDriveInfo>()
        {
          new PSDriveInfo("Alias", this.ProviderInfo, string.Empty, resourceString, (PSCredential) null)
        };
      }
    }

    protected override object NewItemDynamicParameters(
      string path,
      string type,
      object newItemValue)
    {
      return (object) new AliasProviderDynamicParameters();
    }

    protected override object SetItemDynamicParameters(string path, object value) => (object) new AliasProviderDynamicParameters();

    internal override object GetSessionStateItem(string name)
    {
      using (AliasProvider.tracer.TraceMethod(name, new object[0]))
        return (object) this.SessionState.Internal.GetAlias(name, this.Context.Origin);
    }

    internal override object GetValueOfItem(object item)
    {
      using (AliasProvider.tracer.TraceMethod())
      {
        object obj = item;
        if (item is AliasInfo aliasInfo)
          obj = (object) aliasInfo.Definition;
        return obj;
      }
    }

    internal override void SetSessionStateItem(string name, object value, bool writeItem)
    {
      using (AliasProvider.tracer.TraceMethod(name, new object[0]))
      {
        AliasProviderDynamicParameters dynamicParameters = this.DynamicParameters as AliasProviderDynamicParameters;
        AliasInfo aliasInfo1 = (AliasInfo) null;
        bool flag = dynamicParameters != null && dynamicParameters.OptionsSet;
        switch (value)
        {
          case null:
            if (flag)
            {
              aliasInfo1 = (AliasInfo) this.GetSessionStateItem(name);
              if (aliasInfo1 != null)
              {
                aliasInfo1.SetOptions(dynamicParameters.Options, (bool) this.Force);
                break;
              }
              break;
            }
            this.RemoveSessionStateItem(name);
            break;
          case string str:
            aliasInfo1 = !flag ? this.SessionState.Internal.SetAliasValue(name, str, (bool) this.Force, this.Context.Origin) : this.SessionState.Internal.SetAliasValue(name, str, dynamicParameters.Options, (bool) this.Force, this.Context.Origin);
            break;
          case AliasInfo aliasInfo:
            AliasInfo alias = new AliasInfo(name, aliasInfo.Definition, this.Context.ExecutionContext, aliasInfo.Options);
            if (flag)
              alias.SetOptions(dynamicParameters.Options, (bool) this.Force);
            aliasInfo1 = this.SessionState.Internal.SetAliasItem(alias, (bool) this.Force, this.Context.Origin);
            break;
          default:
            throw AliasProvider.tracer.NewArgumentException(nameof (value));
        }
        if (!writeItem || aliasInfo1 == null)
          return;
        this.WriteItemObject((object) aliasInfo1, aliasInfo1.Name, false);
      }
    }

    internal override void RemoveSessionStateItem(string name)
    {
      using (AliasProvider.tracer.TraceMethod(name, new object[0]))
        this.SessionState.Internal.RemoveAlias(name, (bool) this.Force);
    }

    internal override IDictionary GetSessionStateTable()
    {
      using (AliasProvider.tracer.TraceMethod())
        return (IDictionary) this.SessionState.Internal.GetAliasTable();
    }

    internal override bool CanRenameItem(object item)
    {
      using (AliasProvider.tracer.TraceMethod())
      {
        bool flag = false;
        if (item is AliasInfo aliasInfo)
        {
          if ((aliasInfo.Options & ScopedItemOptions.Constant) != ScopedItemOptions.None || (aliasInfo.Options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None && !(bool) this.Force)
          {
            SessionStateUnauthorizedAccessException unauthorizedAccessException = new SessionStateUnauthorizedAccessException(aliasInfo.Name, SessionStateCategory.Alias, "CannotRenameAlias");
            AliasProvider.tracer.TraceException((Exception) unauthorizedAccessException);
            throw unauthorizedAccessException;
          }
          flag = true;
        }
        AliasProvider.tracer.WriteLine("result = {0}", (object) flag);
        return flag;
      }
    }
  }
}
