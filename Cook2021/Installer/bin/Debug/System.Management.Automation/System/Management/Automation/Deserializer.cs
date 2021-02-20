// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Deserializer
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Xml;

namespace System.Management.Automation
{
  internal class Deserializer
  {
    private const string DeserializationTypeNamePrefix = "Deserialized.";
    private XmlReader _reader;
    private InternalDeserializer _deserializer;
    private DeserializationContext _context;
    private bool _done;
    [TraceSource("Deserializer", "Deserializer class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (Deserializer), "Deserializer class");

    internal Deserializer(XmlReader reader)
      : this(reader, new DeserializationContext())
    {
    }

    internal Deserializer(XmlReader reader, DeserializationContext context)
    {
      this._reader = reader != null ? reader : throw Deserializer._trace.NewArgumentNullException(nameof (reader));
      this._context = context;
      this._deserializer = new InternalDeserializer(this._reader, this._context);
      try
      {
        this.Start();
      }
      catch (XmlException ex)
      {
        this.ReportExceptionForETW(ex);
        throw;
      }
    }

    private void ReportExceptionForETW(XmlException exception)
    {
      using (IETWTracer etwTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Serializer))
        etwTracer.AnalyticChannel.WriteError(PSEventId.Serializer_XmlExceptionWhenDeserializing, PSOpcode.Exception, PSTask.Serialization, (object) exception.LineNumber, (object) exception.LinePosition, (object) exception.ToString());
    }

    internal TypeTable TypeTable
    {
      get => this._deserializer.TypeTable;
      set => this._deserializer.TypeTable = value;
    }

    private void Start()
    {
      this._reader.Read();
      string version = "1.1.0.1";
      if (DeserializationOptions.NoRootElement == (this._context.options & DeserializationOptions.NoRootElement))
      {
        this._done = this._reader.EOF;
      }
      else
      {
        int content = (int) this._reader.MoveToContent();
        string attribute = this._reader.GetAttribute("Version");
        if (attribute != null)
          version = attribute;
        if (!this._deserializer.ReadStartElementAndHandleEmpty("Objs"))
          this._done = true;
      }
      this._deserializer.ValidateVersion(version);
    }

    internal bool Done()
    {
      if (!this._done)
      {
        if (DeserializationOptions.NoRootElement == (this._context.options & DeserializationOptions.NoRootElement))
          this._done = this._reader.EOF;
        else if (this._reader.NodeType == XmlNodeType.EndElement)
        {
          try
          {
            this._reader.ReadEndElement();
          }
          catch (XmlException ex)
          {
            this.ReportExceptionForETW(ex);
            throw;
          }
          this._done = true;
        }
      }
      return this._done;
    }

    internal void Stop() => this._deserializer.Stop();

    internal object Deserialize() => this.Deserialize(out string _);

    internal object Deserialize(out string streamName)
    {
      if (this.Done())
        throw Deserializer._trace.NewInvalidOperationException("Serialization", "ReadCalledAfterDone");
      try
      {
        return this._deserializer.ReadOneObject(out streamName);
      }
      catch (XmlException ex)
      {
        this.ReportExceptionForETW(ex);
        throw;
      }
    }

    internal static void AddDeserializationPrefix(ref string type)
    {
      if (type.StartsWith("Deserialized.", StringComparison.OrdinalIgnoreCase))
        return;
      type = type.Insert(0, "Deserialized.");
    }

    internal static bool IsInstanceOfType(object o, Type type)
    {
      if (type == null)
        throw Deserializer._trace.NewArgumentNullException(nameof (type));
      if (o == null)
        return false;
      return type.IsAssignableFrom(PSObject.Base(o).GetType()) || Deserializer.IsDeserializedInstanceOfType(o, type);
    }

    internal static bool IsDeserializedInstanceOfType(object o, Type type)
    {
      if (type == null)
        throw Deserializer._trace.NewArgumentNullException(nameof (type));
      if (o == null || !(o is PSObject psObject))
        return false;
      IEnumerable<string> typeNames = (IEnumerable<string>) psObject.TypeNames;
      if (typeNames != null)
      {
        foreach (string str in typeNames)
        {
          if (str.Equals("Deserialized." + type.FullName, StringComparison.OrdinalIgnoreCase))
            return true;
        }
      }
      return false;
    }

    internal static Collection<string> MaskDeserializationPrefix(
      Collection<string> typeNames)
    {
      bool flag = false;
      Collection<string> collection = new Collection<string>();
      foreach (string typeName in typeNames)
      {
        if (typeName.StartsWith("Deserialized.", StringComparison.OrdinalIgnoreCase))
        {
          flag = true;
          collection.Add(typeName.Substring("Deserialized.".Length));
        }
        else
          collection.Add(typeName);
      }
      return flag ? collection : (Collection<string>) null;
    }
  }
}
