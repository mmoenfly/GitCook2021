// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.XmlLoaderBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using System.Xml;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal abstract class XmlLoaderBase : IDisposable
  {
    [TraceSource("XmlLoaderBase", "XmlLoaderBase")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (XmlLoaderBase), nameof (XmlLoaderBase));
    private DatabaseLoadingInfo loadingInfo = new DatabaseLoadingInfo();
    protected MshExpressionFactory expressionFactory;
    protected DisplayResourceManagerCache displayResourceManagerCache;
    private bool verifyStringResources = true;
    private int maxNumberOfErrors = 30;
    private int currentErrorCount;
    private bool logStackActivity;
    private Stack executionStack = new Stack();
    private XmlLoaderLogger logger = new XmlLoaderLogger();

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing || this.logger == null)
        return;
      this.logger.Dispose();
      this.logger = (XmlLoaderLogger) null;
    }

    internal List<XmlLoaderLoggerEntry> LogEntries => this.logger.LogEntries;

    internal bool HasErrors => this.logger.HasErrors;

    protected IDisposable StackFrame(XmlNode n) => this.StackFrame(n, -1);

    protected IDisposable StackFrame(XmlNode n, int index)
    {
      XmlLoaderBase.XmlLoaderStackFrame loaderStackFrame = new XmlLoaderBase.XmlLoaderStackFrame(this, n, index);
      this.executionStack.Push((object) loaderStackFrame);
      if (this.logStackActivity)
        this.WriteStackLocation("Enter");
      return (IDisposable) loaderStackFrame;
    }

    private void RemoveStackFrame()
    {
      if (this.logStackActivity)
        this.WriteStackLocation("Exit");
      this.executionStack.Pop();
    }

    protected void ProcessUnknownNode(XmlNode n)
    {
      if (XmlLoaderBase.IsFilteredOutNode(n))
        return;
      this.ReportIllegalXmlNode(n);
    }

    protected void ProcessUnknownAttribute(XmlAttribute a) => this.ReportIllegalXmlAttribute(a);

    protected static bool IsFilteredOutNode(XmlNode n) => n is XmlComment || n is XmlWhitespace;

    protected bool VerifyNodeHasNoChildren(XmlNode n)
    {
      if (n.ChildNodes.Count == 0 || n.ChildNodes.Count == 1 && n.ChildNodes[0] is XmlText)
        return true;
      this.ReportError(XmlLoadingResourceManager.FormatString("NoChildrenAllowed", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) n.Name));
      return false;
    }

    internal string GetMandatoryInnerText(XmlNode n)
    {
      if (!string.IsNullOrEmpty(n.InnerText))
        return n.InnerText;
      this.ReportEmptyNode(n);
      return (string) null;
    }

    internal string GetMandatoryAttributeValue(XmlAttribute a)
    {
      if (!string.IsNullOrEmpty(a.Value))
        return a.Value;
      this.ReportEmptyAttribute(a);
      return (string) null;
    }

    private bool MatchNodeNameHelper(XmlNode n, string s, bool allowAttributes)
    {
      bool flag = false;
      if (string.Equals(n.Name, s, StringComparison.Ordinal))
        flag = true;
      else if (string.Equals(n.Name, s, StringComparison.OrdinalIgnoreCase))
      {
        this.ReportTrace(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "XML tag differ in case only {0} {1}", (object) n.Name, (object) s));
        flag = true;
      }
      if (flag && !allowAttributes && (n is XmlElement xmlElement && xmlElement.Attributes.Count > 0))
        this.ReportError(XmlLoadingResourceManager.FormatString("AttributesNotAllowed", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) n.Name));
      return flag;
    }

    internal bool MatchNodeNameWithAttributes(XmlNode n, string s) => this.MatchNodeNameHelper(n, s, true);

    internal bool MatchNodeName(XmlNode n, string s) => this.MatchNodeNameHelper(n, s, false);

    internal bool MatchAttributeName(XmlAttribute a, string s)
    {
      if (string.Equals(a.Name, s, StringComparison.Ordinal))
        return true;
      if (!string.Equals(a.Name, s, StringComparison.OrdinalIgnoreCase))
        return false;
      this.ReportTrace(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "XML attribute differ in case only {0} {1}", (object) a.Name, (object) s));
      return true;
    }

    internal void ProcessDuplicateNode(XmlNode n) => this.ReportLogEntryHelper(XmlLoadingResourceManager.FormatString("DuplicatedNode", (object) this.ComputeCurrentXPath(), (object) this.FilePath), XmlLoaderLoggerEntry.EntryType.Error);

    internal void ProcessDuplicateAlternateNode(string node1, string node2) => this.ReportLogEntryHelper(XmlLoadingResourceManager.FormatString("MutuallyExclusiveNode", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) node1, (object) node2), XmlLoaderLoggerEntry.EntryType.Error);

    internal void ProcessDuplicateAlternateNode(XmlNode n, string node1, string node2) => this.ReportLogEntryHelper(XmlLoadingResourceManager.FormatString("ThreeMutuallyExclusiveNode", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) n.Name, (object) node1, (object) node2), XmlLoaderLoggerEntry.EntryType.Error);

    private void ReportIllegalXmlNode(XmlNode n) => this.ReportLogEntryHelper(XmlLoadingResourceManager.FormatString("UnknownNode", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) n.Name), XmlLoaderLoggerEntry.EntryType.Error);

    private void ReportIllegalXmlAttribute(XmlAttribute a) => this.ReportLogEntryHelper(XmlLoadingResourceManager.FormatString("UnknownAttribute", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) a.Name), XmlLoaderLoggerEntry.EntryType.Error);

    protected void ReportMissingAttribute(string name) => this.ReportLogEntryHelper(XmlLoadingResourceManager.FormatString("MissingAttribute", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) name), XmlLoaderLoggerEntry.EntryType.Error);

    protected void ReportMissingNode(string name) => this.ReportLogEntryHelper(XmlLoadingResourceManager.FormatString("MissingNode", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) name), XmlLoaderLoggerEntry.EntryType.Error);

    protected void ReportMissingNodes(string[] names)
    {
      string str = string.Join(", ", names);
      this.ReportLogEntryHelper(XmlLoadingResourceManager.FormatString("MissingNodeFromList", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) str), XmlLoaderLoggerEntry.EntryType.Error);
    }

    protected void ReportEmptyNode(XmlNode n) => this.ReportLogEntryHelper(XmlLoadingResourceManager.FormatString("EmptyNode", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) n.Name), XmlLoaderLoggerEntry.EntryType.Error);

    protected void ReportEmptyAttribute(XmlAttribute a) => this.ReportLogEntryHelper(XmlLoadingResourceManager.FormatString("EmptyAttribute", (object) this.ComputeCurrentXPath(), (object) this.FilePath, (object) a.Name), XmlLoaderLoggerEntry.EntryType.Error);

    protected void ReportTrace(string message) => this.ReportLogEntryHelper(message, XmlLoaderLoggerEntry.EntryType.Trace);

    protected void ReportError(string message) => this.ReportLogEntryHelper(message, XmlLoaderLoggerEntry.EntryType.Error);

    private void ReportLogEntryHelper(string message, XmlLoaderLoggerEntry.EntryType entryType)
    {
      string currentXpath = this.ComputeCurrentXPath();
      this.logger.LogEntry(new XmlLoaderLoggerEntry()
      {
        entryType = entryType,
        filePath = this.FilePath,
        xPath = currentXpath,
        message = message
      });
      if (entryType != XmlLoaderLoggerEntry.EntryType.Error)
        return;
      ++this.currentErrorCount;
      if (this.currentErrorCount >= this.maxNumberOfErrors)
      {
        if (this.maxNumberOfErrors > 1)
        {
          this.logger.LogEntry(new XmlLoaderLoggerEntry()
          {
            entryType = XmlLoaderLoggerEntry.EntryType.Error,
            filePath = this.FilePath,
            xPath = currentXpath,
            message = XmlLoadingResourceManager.FormatString("TooManyErrors", (object) this.FilePath)
          });
          ++this.currentErrorCount;
        }
        throw new TooManyErrorsException()
        {
          errorCount = this.currentErrorCount
        };
      }
    }

    private void WriteStackLocation(string label) => this.ReportTrace(label);

    protected string ComputeCurrentXPath()
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (XmlLoaderBase.XmlLoaderStackFrame execution in this.executionStack)
      {
        stringBuilder.Insert(0, "/");
        if (execution.index != -1)
          stringBuilder.Insert(1, string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}[{1}]", (object) execution.node.Name, (object) (execution.index + 1)));
        else
          stringBuilder.Insert(1, execution.node.Name);
      }
      return stringBuilder.Length <= 0 ? (string) null : stringBuilder.ToString();
    }

    protected XmlDocument LoadXmlDocumentFromFileLoadingInfo(
      AuthorizationManager authorizationManager,
      PSHost host)
    {
      ExternalScriptInfo externalScriptInfo = new ExternalScriptInfo(this.FilePath, this.FilePath);
      string scriptContents = externalScriptInfo.ScriptContents;
      if (authorizationManager != null)
      {
        try
        {
          authorizationManager.ShouldRunInternal((CommandInfo) externalScriptInfo, CommandOrigin.Internal, host);
        }
        catch (PSSecurityException ex)
        {
          this.ReportError(ResourceManagerCache.FormatResourceString("TypesXml", "ValidationException", (object) string.Empty, (object) this.FilePath, (object) ex.Message));
          return (XmlDocument) null;
        }
      }
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.PreserveWhitespace = true;
      try
      {
        using (StringReader stringReader = new StringReader(scriptContents))
          xmlDocument.Load((TextReader) stringReader);
      }
      catch (XmlException ex)
      {
        this.ReportError(XmlLoadingResourceManager.FormatString("ErrorInFile", (object) this.FilePath, (object) ex.Message));
        this.ReportTrace("XmlDocument discarded");
        return (XmlDocument) null;
      }
      catch (Exception ex)
      {
        XmlLoaderBase.tracer.TraceException(ex);
        CommandProcessorBase.CheckForSevereException(ex);
        throw;
      }
      this.ReportTrace("XmlDocument loaded OK");
      return xmlDocument;
    }

    protected string FilePath => this.loadingInfo.filePath;

    protected void SetDatabaseLoadingInfo(XmlFileLoadInfo info)
    {
      this.loadingInfo.fileDirectory = info.fileDirectory;
      this.loadingInfo.filePath = info.filePath;
    }

    protected DatabaseLoadingInfo LoadingInfo => new DatabaseLoadingInfo()
    {
      filePath = this.loadingInfo.filePath,
      fileDirectory = this.loadingInfo.fileDirectory
    };

    internal bool VerifyStringResources => this.verifyStringResources;

    private sealed class XmlLoaderStackFrame : IDisposable
    {
      private XmlLoaderBase loader;
      internal XmlNode node;
      internal int index = -1;

      internal XmlLoaderStackFrame(XmlLoaderBase loader, XmlNode n, int index)
      {
        this.loader = loader;
        this.node = n;
        this.index = index;
      }

      public void Dispose()
      {
        if (this.loader == null)
          return;
        this.loader.RemoveStackFrame();
        this.loader = (XmlLoaderBase) null;
      }
    }
  }
}
