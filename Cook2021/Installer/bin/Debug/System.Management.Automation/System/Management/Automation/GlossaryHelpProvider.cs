// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.GlossaryHelpProvider
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
  internal class GlossaryHelpProvider : HelpProviderWithFullCache
  {
    private Hashtable _helpFiles = new Hashtable();
    [TraceSource("GlossaryHelpProvider", "GlossaryHelpProvider")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (GlossaryHelpProvider), nameof (GlossaryHelpProvider));

    internal GlossaryHelpProvider(HelpSystem helpSystem)
      : base(helpSystem)
    {
      using (GlossaryHelpProvider.tracer.TraceConstructor((object) this))
        this.HasCustomMatch = true;
    }

    internal override string Name
    {
      get
      {
        using (GlossaryHelpProvider.tracer.TraceProperty())
          return "Glossary Help Provider";
      }
    }

    internal override HelpCategory HelpCategory
    {
      get
      {
        using (GlossaryHelpProvider.tracer.TraceProperty())
          return HelpCategory.Glossary;
      }
    }

    protected override sealed bool CustomMatch(string target, string key)
    {
      if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(key))
        return false;
      string str1 = key;
      char[] chArray = new char[1]{ ',' };
      foreach (string str2 in str1.Split(chArray))
      {
        if (str2.Trim().Equals(target, StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }

    internal override sealed void LoadCache()
    {
      Collection<string> collection = MUIFileSearcher.SearchFiles("*.glossary.xml", this.GetSearchPaths());
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
      using (GlossaryHelpProvider.tracer.TraceMethod())
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
            if (childNode.NodeType == XmlNodeType.Element && string.Compare(childNode.Name, "glossary", StringComparison.OrdinalIgnoreCase) == 0)
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
            if (childNode.NodeType == XmlNodeType.Element && string.Compare(childNode.Name, "glossaryEntry", StringComparison.OrdinalIgnoreCase) == 0)
            {
              HelpInfo helpInfo = (HelpInfo) GlossaryHelpInfo.Load(childNode);
              if (helpInfo != null)
              {
                this.HelpSystem.TraceErrors(helpInfo.Errors);
                this.AddCache(helpInfo.Name, helpInfo);
              }
            }
            else if (childNode.NodeType == XmlNodeType.Element && string.Compare(childNode.Name, "glossaryDiv", StringComparison.OrdinalIgnoreCase) == 0)
              this.LoadGlossaryDiv(childNode);
          }
        }
      }
    }

    private void LoadGlossaryDiv(XmlNode xmlNode)
    {
      if (xmlNode == null)
        return;
      for (int i = 0; i < xmlNode.ChildNodes.Count; ++i)
      {
        XmlNode childNode = xmlNode.ChildNodes[i];
        if (childNode.NodeType == XmlNodeType.Element && string.Compare(childNode.Name, "glossaryEntry", StringComparison.OrdinalIgnoreCase) == 0)
        {
          HelpInfo helpInfo = (HelpInfo) GlossaryHelpInfo.Load(childNode);
          if (helpInfo != null)
          {
            this.HelpSystem.TraceErrors(helpInfo.Errors);
            this.AddCache(helpInfo.Name, helpInfo);
          }
        }
      }
    }

    internal override void Reset()
    {
      using (GlossaryHelpProvider.tracer.TraceMethod())
      {
        base.Reset();
        this._helpFiles.Clear();
      }
    }
  }
}
