// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.LoadContext
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Xml;

namespace System.Management.Automation.Runspaces
{
  internal struct LoadContext
  {
    private const string FileLineError = "FileLineError";
    private const string FileLineTypeError = "FileLineTypeError";
    internal const string FileError = "FileError";
    internal int lineNumber;
    internal XmlTextReader reader;
    internal Collection<string> errors;
    internal string fileName;
    internal string PSSnapinName;

    internal LoadContext(string PSSnapinName, string fileName, Collection<string> errors)
    {
      this.reader = (XmlTextReader) null;
      this.fileName = fileName;
      this.errors = errors;
      this.PSSnapinName = PSSnapinName;
      this.lineNumber = 0;
    }

    internal bool Read()
    {
      this.lineNumber = this.reader.LineNumber;
      return this.reader.Read();
    }

    internal void AddError(string resourceId, params object[] formatArguments) => this.errors.Add(ResourceManagerCache.FormatResourceString("TypesXml", "FileError", (object) this.PSSnapinName, (object) this.fileName, (object) ResourceManagerCache.FormatResourceString("TypesXml", resourceId, formatArguments)));

    internal void AddError(int errorLineNumber, string resourceId, params object[] formatArguments)
    {
      string str = ResourceManagerCache.FormatResourceString("TypesXml", resourceId, formatArguments);
      this.errors.Add(ResourceManagerCache.FormatResourceString("TypesXml", "FileLineError", (object) this.PSSnapinName, (object) this.fileName, (object) errorLineNumber, (object) str));
    }

    internal void AddError(
      string typeName,
      int errorLineNumber,
      string resourceId,
      params object[] formatArguments)
    {
      string str = ResourceManagerCache.FormatResourceString("TypesXml", resourceId, formatArguments);
      this.errors.Add(ResourceManagerCache.FormatResourceString("TypesXml", "FileLineTypeError", (object) this.PSSnapinName, (object) this.fileName, (object) errorLineNumber, (object) typeName, (object) str));
    }
  }
}
