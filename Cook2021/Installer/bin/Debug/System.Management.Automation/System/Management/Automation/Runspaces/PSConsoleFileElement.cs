// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.PSConsoleFileElement
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.Management.Automation.Runspaces
{
  internal class PSConsoleFileElement
  {
    private const string MSHCONSOLEFILE = "PSConsoleFile";
    private const string CSCHEMAVERSION = "ConsoleSchemaVersion";
    private const string CSCHEMAVERSIONNUMBER = "1.0";
    private const string PSVERSION = "PSVersion";
    private const string SNAPINS = "PSSnapIns";
    private const string SNAPIN = "PSSnapIn";
    private const string SNAPINNAME = "Name";
    private string monadVersion;
    private Collection<string> mshsnapins;
    [TraceSource("PSConsoleFileElement", "PSConsoleFileElement")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PSConsoleFileElement), nameof (PSConsoleFileElement));
    private static PSTraceSource _mshsnapinTracer = PSTraceSource.GetTracer("MshSnapinLoadUnload", "Loading and unloading mshsnapins", false);

    internal string MonadVersion => this.monadVersion;

    internal Collection<string> PSSnapIns => this.mshsnapins;

    private PSConsoleFileElement(string version)
    {
      this.monadVersion = version;
      this.mshsnapins = new Collection<string>();
    }

    internal static void WriteToFile(MshConsoleInfo consoleInfo, string path)
    {
      using (PSConsoleFileElement.tracer.TraceMethod())
      {
        PSConsoleFileElement._mshsnapinTracer.WriteLine("Saving console info to file {0}.", (object) path);
        using (XmlWriter xmlWriter = XmlWriter.Create(path, new XmlWriterSettings()
        {
          Indent = true,
          Encoding = Encoding.UTF8
        }))
        {
          xmlWriter.WriteStartDocument();
          xmlWriter.WriteStartElement("PSConsoleFile");
          xmlWriter.WriteAttributeString("ConsoleSchemaVersion", "1.0");
          xmlWriter.WriteStartElement("PSVersion");
          xmlWriter.WriteString(consoleInfo.PSVersion.ToString());
          xmlWriter.WriteEndElement();
          xmlWriter.WriteStartElement("PSSnapIns");
          foreach (PSSnapInInfo externalPsSnapIn in consoleInfo.ExternalPSSnapIns)
          {
            xmlWriter.WriteStartElement("PSSnapIn");
            xmlWriter.WriteAttributeString("Name", externalPsSnapIn.Name);
            xmlWriter.WriteEndElement();
          }
          xmlWriter.WriteEndElement();
          xmlWriter.WriteEndElement();
          xmlWriter.WriteEndDocument();
          xmlWriter.Close();
        }
        PSConsoleFileElement._mshsnapinTracer.WriteLine("Saving console info succeeded.", new object[0]);
      }
    }

    internal static PSConsoleFileElement CreateFromFile(string path)
    {
      using (PSConsoleFileElement.tracer.TraceMethod())
      {
        PSConsoleFileElement._mshsnapinTracer.WriteLine("Loading console info from file {0}.", (object) path);
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(path);
        if (xmlDocument["PSConsoleFile"] == null)
        {
          PSConsoleFileElement._mshsnapinTracer.TraceError("Console file {0} doesn't contain tag {1}.", (object) path, (object) "PSConsoleFile");
          throw new XmlException(ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "MonadConsoleNotFound", (object) path));
        }
        if (xmlDocument["PSConsoleFile"]["PSVersion"] == null || string.IsNullOrEmpty(xmlDocument["PSConsoleFile"]["PSVersion"].InnerText))
        {
          PSConsoleFileElement._mshsnapinTracer.TraceError("Console file {0} doesn't contain tag {1}.", (object) path, (object) "PSVersion");
          throw new XmlException(ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "MonadVersionNotFound", (object) path));
        }
        XmlElement xmlElement1 = xmlDocument["PSConsoleFile"];
        if (xmlElement1.HasAttribute("ConsoleSchemaVersion"))
        {
          if (!xmlElement1.GetAttribute("ConsoleSchemaVersion").Equals("1.0", StringComparison.OrdinalIgnoreCase))
          {
            string str = string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "BadConsoleVersion", (object) path), (object) "1.0");
            PSConsoleFileElement._mshsnapinTracer.TraceError(str);
            throw new XmlException(str);
          }
          PSConsoleFileElement consoleFileElement = new PSConsoleFileElement(xmlDocument["PSConsoleFile"]["PSVersion"].InnerText.Trim());
          bool flag1 = false;
          bool flag2 = false;
          for (XmlNode xmlNode1 = xmlDocument["PSConsoleFile"].FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
          {
            if (xmlNode1.NodeType != XmlNodeType.Comment)
            {
              if (!(xmlNode1 is XmlElement xmlElement))
                throw new XmlException(ResourceManagerCache.GetResourceString("ConsoleInfoErrorStrings", "BadXMLFormat"));
              if (xmlElement.Name == "PSVersion")
              {
                if (flag2)
                {
                  PSConsoleFileElement._mshsnapinTracer.TraceError("Console file {0} contains more than one  msh versions", (object) path);
                  throw new XmlException(ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "MultipleMshSnapinsElementNotSupported", (object) "PSVersion"));
                }
                flag2 = true;
              }
              else
              {
                if (xmlElement.Name != "PSSnapIns")
                {
                  PSConsoleFileElement._mshsnapinTracer.TraceError("Tag {0} is not supported in console file", (object) xmlElement.Name);
                  throw new XmlException(ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "BadXMLElementFound", (object) xmlElement.Name, (object) "PSConsoleFile", (object) "PSVersion", (object) "PSSnapIns"));
                }
                if (flag1)
                {
                  PSConsoleFileElement._mshsnapinTracer.TraceError("Console file {0} contains more than one mshsnapin lists", (object) path);
                  throw new XmlException(ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "MultipleMshSnapinsElementNotSupported", (object) "PSSnapIns"));
                }
                flag1 = true;
                for (XmlNode xmlNode2 = xmlElement.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                {
                  if (!(xmlNode2 is XmlElement xmlElement) || xmlElement.Name != "PSSnapIn")
                    throw new XmlException(ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "PSSnapInNotFound", (object) xmlNode2.Name));
                  string attribute = xmlElement.GetAttribute("Name");
                  if (string.IsNullOrEmpty(attribute))
                    throw new XmlException(ResourceManagerCache.GetResourceString("ConsoleInfoErrorStrings", "IDNotFound"));
                  consoleFileElement.mshsnapins.Add(attribute);
                  PSConsoleFileElement._mshsnapinTracer.WriteLine("Found in mshsnapin {0} in console file {1}", (object) attribute, (object) path);
                }
              }
            }
          }
          return consoleFileElement;
        }
        PSConsoleFileElement._mshsnapinTracer.TraceError("Console file {0} doesn't contain tag schema version.", (object) path);
        throw new XmlException(ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "BadConsoleVersion", (object) path));
      }
    }
  }
}
