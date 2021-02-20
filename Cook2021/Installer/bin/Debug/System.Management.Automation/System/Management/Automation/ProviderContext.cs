// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ProviderContext
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Provider;
using System.Xml;

namespace System.Management.Automation
{
  internal class ProviderContext
  {
    [TraceSource("ProviderCategory", "ProviderCategory Class")]
    private static PSTraceSource _tracer = PSTraceSource.GetTracer("ProviderCategory", "ProviderCategory Class");
    private string _requestedPath;
    private ExecutionContext _executionContext;
    private PathIntrinsics _pathIntrinsics;

    internal string RequestedPath => this._requestedPath;

    internal ProviderContext(
      string requestedPath,
      ExecutionContext executionContext,
      PathIntrinsics pathIntrinsics)
    {
      this._requestedPath = requestedPath;
      this._executionContext = executionContext;
      this._pathIntrinsics = pathIntrinsics;
    }

    internal MamlCommandHelpInfo GetProviderSpecificHelpInfo(string helpItemName)
    {
      ProviderInfo provider = (ProviderInfo) null;
      PSDriveInfo drive = (PSDriveInfo) null;
      string str = (string) null;
      CmdletProviderContext cmdletProviderContext = new CmdletProviderContext(this._executionContext);
      try
      {
        string path = this._requestedPath;
        if (string.IsNullOrEmpty(this._requestedPath))
          path = this._pathIntrinsics.CurrentLocation.Path;
        str = this._executionContext.LocationGlobber.GetProviderPath(path, cmdletProviderContext, out provider, out drive);
      }
      catch (ArgumentNullException ex)
      {
      }
      catch (ProviderNotFoundException ex)
      {
      }
      catch (DriveNotFoundException ex)
      {
      }
      catch (ProviderInvocationException ex)
      {
      }
      catch (NotSupportedException ex)
      {
      }
      catch (InvalidOperationException ex)
      {
      }
      catch (ItemNotFoundException ex)
      {
      }
      if (provider == null)
        return (MamlCommandHelpInfo) null;
      CmdletProvider instance = provider.CreateInstance();
      if (!(instance is ICmdletProviderSupportsHelp providerSupportsHelp))
        return (MamlCommandHelpInfo) null;
      if (str == null)
        throw new ItemNotFoundException(this._requestedPath, "PathNotFound");
      instance.Start(provider, cmdletProviderContext);
      string path1 = str;
      string helpMaml = providerSupportsHelp.GetHelpMaml(helpItemName, path1);
      if (string.IsNullOrEmpty(helpMaml))
        return (MamlCommandHelpInfo) null;
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(helpMaml);
      return MamlCommandHelpInfo.Load((XmlNode) xmlDocument.DocumentElement, HelpCategory.Provider);
    }
  }
}
