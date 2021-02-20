// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.XmlLoaderLogger
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal class XmlLoaderLogger : IDisposable
  {
    [TraceSource("FormatFileLoading", "Loading format files")]
    private static PSTraceSource formatFileLoadingtracer = PSTraceSource.GetTracer("FormatFileLoading", "Loading format files", false);
    private bool saveInMemory = true;
    private List<XmlLoaderLoggerEntry> entries = new List<XmlLoaderLoggerEntry>();
    private bool hasErrors;

    internal void LogEntry(XmlLoaderLoggerEntry entry)
    {
      if (entry.entryType == XmlLoaderLoggerEntry.EntryType.Error)
        this.hasErrors = true;
      if (this.saveInMemory)
        this.entries.Add(entry);
      if ((XmlLoaderLogger.formatFileLoadingtracer.Options | PSTraceSourceOptions.WriteLine) == PSTraceSourceOptions.None)
        return;
      this.WriteToTracer(entry);
    }

    private void WriteToTracer(XmlLoaderLoggerEntry entry)
    {
      if (entry.entryType == XmlLoaderLoggerEntry.EntryType.Error)
      {
        XmlLoaderLogger.formatFileLoadingtracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "ERROR:\r\n FilePath: {0}\r\n XPath: {1}\r\n Message = {2}", (object) entry.filePath, (object) entry.xPath, (object) entry.message), new object[0]);
      }
      else
      {
        if (entry.entryType != XmlLoaderLoggerEntry.EntryType.Trace)
          return;
        XmlLoaderLogger.formatFileLoadingtracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "TRACE:\r\n FilePath: {0}\r\n XPath: {1}\r\n Message = {2}", (object) entry.filePath, (object) entry.xPath, (object) entry.message), new object[0]);
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      int num = disposing ? 1 : 0;
    }

    internal List<XmlLoaderLoggerEntry> LogEntries => this.entries;

    internal bool HasErrors => this.hasErrors;
  }
}
