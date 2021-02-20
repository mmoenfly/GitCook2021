// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.GeneralHelpProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Security;
using System.Xml;

namespace System.Management.Automation
{
  internal class GeneralHelpProvider : HelpProviderWithFullCache
  {
    private CommandDiscovery _commandDiscovery;
    private Hashtable _helpFiles = new Hashtable();
    [TraceSource("GeneralHelpProvider", "GeneralHelpProvider")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (GeneralHelpProvider), nameof (GeneralHelpProvider));

    internal GeneralHelpProvider(HelpSystem helpSystem)
      : base(helpSystem)
    {
      using (GeneralHelpProvider.tracer.TraceConstructor((object) this))
        this._commandDiscovery = helpSystem.ExecutionContext.CommandDiscovery;
    }

    internal override string Name
    {
      get
      {
        using (GeneralHelpProvider.tracer.TraceProperty())
          return "General Help Provider";
      }
    }

    internal override HelpCategory HelpCategory
    {
      get
      {
        using (GeneralHelpProvider.tracer.TraceProperty())
          return HelpCategory.General;
      }
    }

    internal override sealed void LoadCache()
    {
      Collection<string> collection = MUIFileSearcher.SearchFiles("*.concept.xml", this.GetSearchPaths());
      if (collection == null)
        return;
      foreach (string helpFile in collection)
      {
        if (!this._helpFiles.ContainsKey((object) helpFile))
        {
          this.LoadHelpFile(helpFile);
          this._helpFiles[(object) helpFile] = (object) 0;
        }
      }
    }

    private void LoadHelpFile(string helpFile)
    {
      using (GeneralHelpProvider.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(helpFile))
          return;
        XmlDocument xmlDocument = new XmlDocument();
        try
        {
          xmlDocument.Load(helpFile);
        }
        catch (IOException ex)
        {
          this.HelpSystem.LastErrors.Add(new ErrorRecord((Exception) ex, "HelpFileLoadFailure", ErrorCategory.OpenError, (object) null)
          {
            ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "HelpFileLoadFailure", new object[2]
            {
              (object) helpFile,
              (object) ex.Message
            })
          });
          return;
        }
        catch (SecurityException ex)
        {
          this.HelpSystem.LastErrors.Add(new ErrorRecord((Exception) ex, "HelpFileNotAccessible", ErrorCategory.OpenError, (object) null)
          {
            ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "HelpFileNotAccessible", new object[2]
            {
              (object) helpFile,
              (object) ex.Message
            })
          });
          return;
        }
        catch (XmlException ex)
        {
          this.HelpSystem.LastErrors.Add(new ErrorRecord((Exception) ex, "HelpFileNotValid", ErrorCategory.SyntaxError, (object) null)
          {
            ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "HelpFileNotValid", new object[2]
            {
              (object) helpFile,
              (object) ex.Message
            })
          });
          return;
        }
        XmlNode xmlNode = (XmlNode) null;
        if (xmlDocument.HasChildNodes)
        {
          for (int i = 0; i < xmlDocument.ChildNodes.Count; ++i)
          {
            XmlNode childNode = xmlDocument.ChildNodes[i];
            if (childNode.NodeType == XmlNodeType.Element && string.Compare(childNode.Name, "conceptuals", StringComparison.OrdinalIgnoreCase) == 0)
            {
              xmlNode = childNode;
              break;
            }
          }
        }
        if (xmlNode == null)
          return;
        using (this.HelpSystem.Trace(helpFile))
        {
          if (!xmlNode.HasChildNodes)
            return;
          for (int i = 0; i < xmlNode.ChildNodes.Count; ++i)
          {
            XmlNode childNode = xmlNode.ChildNodes[i];
            if (childNode.NodeType == XmlNodeType.Element && string.Compare(childNode.Name, "conceptual", StringComparison.OrdinalIgnoreCase) == 0)
            {
              HelpInfo helpInfo = (HelpInfo) GeneralHelpInfo.Load(childNode);
              if (helpInfo != null)
              {
                this.HelpSystem.TraceErrors(helpInfo.Errors);
                this.AddCache(helpInfo.Name, helpInfo);
              }
            }
          }
        }
      }
    }

    internal override void Reset()
    {
      using (GeneralHelpProvider.tracer.TraceMethod())
      {
        base.Reset();
        this._helpFiles.Clear();
      }
    }
  }
}
