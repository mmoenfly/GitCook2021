// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RegistryStringResourceIndirect
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Security;

namespace System.Management.Automation
{
  internal sealed class RegistryStringResourceIndirect : IDisposable
  {
    [TraceSource("RegistryStringResourceIndirect", "Loads a resource using a pointer to the resources in the registry.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (RegistryStringResourceIndirect), "Loads a resource using a pointer to the resources in the registry.");
    private bool _disposed;
    private AppDomain _domain;
    private ResourceRetriever _resourceRetriever;

    internal static RegistryStringResourceIndirect GetResourceIndirectReader() => new RegistryStringResourceIndirect();

    public void Dispose()
    {
      using (RegistryStringResourceIndirect.tracer.TraceDispose((object) this))
      {
        if (!this._disposed && this._domain != null)
        {
          AppDomain.Unload(this._domain);
          this._domain = (AppDomain) null;
          this._resourceRetriever = (ResourceRetriever) null;
        }
        this._disposed = true;
      }
    }

    private void CreateAppDomain()
    {
      using (RegistryStringResourceIndirect.tracer.TraceMethod())
      {
        if (this._domain != null)
          return;
        this._domain = AppDomain.CreateDomain("ResourceIndirectDomain");
        this._resourceRetriever = (ResourceRetriever) this._domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, "System.Management.Automation.ResourceRetriever");
      }
    }

    internal string GetResourceStringIndirect(
      RegistryKey key,
      string valueName,
      string assemblyName,
      string modulePath)
    {
      using (RegistryStringResourceIndirect.tracer.TraceMethod(valueName, new object[0]))
      {
        if (this._disposed)
          throw RegistryStringResourceIndirect.tracer.NewInvalidOperationException("PSSnapinInfo", "ResourceReaderDisposed");
        if (key == null)
          throw RegistryStringResourceIndirect.tracer.NewArgumentNullException(nameof (key));
        if (string.IsNullOrEmpty(valueName))
          throw RegistryStringResourceIndirect.tracer.NewArgumentException(nameof (valueName));
        if (string.IsNullOrEmpty(assemblyName))
          throw RegistryStringResourceIndirect.tracer.NewArgumentException(nameof (assemblyName));
        if (string.IsNullOrEmpty(modulePath))
          throw RegistryStringResourceIndirect.tracer.NewArgumentException(nameof (modulePath));
        string format = (string) null;
        string keyValueAsString = RegistryStringResourceIndirect.GetRegKeyValueAsString(key, valueName);
        if (keyValueAsString != null)
          format = this.GetResourceStringIndirect(assemblyName, modulePath, keyValueAsString);
        RegistryStringResourceIndirect.tracer.WriteLine(format, new object[0]);
        return format;
      }
    }

    internal string GetResourceStringIndirect(
      string assemblyName,
      string modulePath,
      string baseNameRIDPair)
    {
      using (RegistryStringResourceIndirect.tracer.TraceMethod(baseNameRIDPair, new object[0]))
      {
        if (this._disposed)
          throw RegistryStringResourceIndirect.tracer.NewInvalidOperationException("PSSnapinInfo", "ResourceReaderDisposed");
        if (string.IsNullOrEmpty(assemblyName))
          throw RegistryStringResourceIndirect.tracer.NewArgumentException(nameof (assemblyName));
        if (string.IsNullOrEmpty(modulePath))
          throw RegistryStringResourceIndirect.tracer.NewArgumentException(nameof (modulePath));
        if (string.IsNullOrEmpty(baseNameRIDPair))
          throw RegistryStringResourceIndirect.tracer.NewArgumentException(nameof (baseNameRIDPair));
        string format = (string) null;
        if (this._resourceRetriever == null)
          this.CreateAppDomain();
        if (this._resourceRetriever != null)
        {
          string[] strArray = baseNameRIDPair.Split(',');
          if (strArray.Length != 2)
          {
            RegistryStringResourceIndirect.tracer.WriteLine("The value from the registry was not in the property format. Need two strings separated by a comma. {0}", (object) baseNameRIDPair);
          }
          else
          {
            string baseName = strArray[0];
            string resourceID = strArray[1];
            format = this._resourceRetriever.GetStringResource(assemblyName, modulePath, baseName, resourceID);
          }
        }
        RegistryStringResourceIndirect.tracer.WriteLine(format, new object[0]);
        return format;
      }
    }

    private static string GetRegKeyValueAsString(RegistryKey key, string valueName)
    {
      using (RegistryStringResourceIndirect.tracer.TraceMethod(valueName, new object[0]))
      {
        string format = (string) null;
        try
        {
          if (key.GetValueKind(valueName) == RegistryValueKind.String)
            format = key.GetValue(valueName) as string;
        }
        catch (ArgumentException ex)
        {
        }
        catch (IOException ex)
        {
        }
        catch (SecurityException ex)
        {
        }
        RegistryStringResourceIndirect.tracer.WriteLine(format, new object[0]);
        return format;
      }
    }
  }
}
