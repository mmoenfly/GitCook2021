// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.EnvironmentProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace Microsoft.PowerShell.Commands
{
  [CmdletProvider("Environment", ProviderCapabilities.ShouldProcess)]
  public sealed class EnvironmentProvider : SessionStateProviderBase
  {
    public const string ProviderName = "Environment";
    [TraceSource("EnvironmentProvider", "The core command provider for environment variables")]
    private static readonly PSTraceSource tracer = PSTraceSource.GetTracer(nameof (EnvironmentProvider), "The core command provider for environment variables");

    public EnvironmentProvider()
    {
      using (EnvironmentProvider.tracer.TraceConstructor((object) this))
        ;
    }

    protected override Collection<PSDriveInfo> InitializeDefaultDrives()
    {
      using (EnvironmentProvider.tracer.TraceMethod())
      {
        string resourceString = ResourceManagerCache.GetResourceString("SessionStateStrings", "EnvironmentDriveDescription");
        return new Collection<PSDriveInfo>()
        {
          new PSDriveInfo("Env", this.ProviderInfo, string.Empty, resourceString, (PSCredential) null)
        };
      }
    }

    internal override object GetSessionStateItem(string name)
    {
      using (EnvironmentProvider.tracer.TraceMethod(name, new object[0]))
      {
        object obj = (object) null;
        string environmentVariable = Environment.GetEnvironmentVariable(name);
        if (environmentVariable != null)
          obj = (object) new DictionaryEntry((object) name, (object) environmentVariable);
        return obj;
      }
    }

    internal override void SetSessionStateItem(string name, object value, bool writeItem)
    {
      using (EnvironmentProvider.tracer.TraceMethod(name, new object[0]))
      {
        if (value == null)
        {
          Environment.SetEnvironmentVariable(name, (string) null);
        }
        else
        {
          if (value is DictionaryEntry dictionaryEntry)
            value = dictionaryEntry.Value;
          if (!(value is string str))
            str = PSObject.AsPSObject(value).ToString();
          Environment.SetEnvironmentVariable(name, str);
          DictionaryEntry dictionaryEntry1 = new DictionaryEntry((object) name, (object) str);
          if (!writeItem)
            return;
          this.WriteItemObject((object) dictionaryEntry1, name, false);
        }
      }
    }

    internal override void RemoveSessionStateItem(string name)
    {
      using (EnvironmentProvider.tracer.TraceMethod(name, new object[0]))
        Environment.SetEnvironmentVariable(name, (string) null);
    }

    internal override IDictionary GetSessionStateTable()
    {
      using (EnvironmentProvider.tracer.TraceMethod())
      {
        Dictionary<string, DictionaryEntry> dictionary = new Dictionary<string, DictionaryEntry>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
          dictionary.Add((string) environmentVariable.Key, environmentVariable);
        return (IDictionary) dictionary;
      }
    }

    internal override object GetValueOfItem(object item)
    {
      using (EnvironmentProvider.tracer.TraceMethod())
      {
        object obj = item;
        if (item is DictionaryEntry dictionaryEntry)
          obj = dictionaryEntry.Value;
        return obj;
      }
    }
  }
}
