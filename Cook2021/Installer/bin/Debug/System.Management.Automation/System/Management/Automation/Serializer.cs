// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Serializer
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Runspaces;
using System.Xml;

namespace System.Management.Automation
{
  internal class Serializer
  {
    private InternalSerializer _serializer;
    [TraceSource("Serializer", "Serializer class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (Serializer), "Serializer class");

    internal Serializer(XmlWriter writer)
      : this(writer, new SerializationContext())
    {
    }

    internal Serializer(XmlWriter writer, int depth, bool useDepthFromTypes)
      : this(writer, new SerializationContext(depth, useDepthFromTypes))
    {
    }

    internal Serializer(XmlWriter writer, SerializationContext context)
    {
      if (writer == null)
        throw Serializer._trace.NewArgumentException(nameof (writer));
      this._serializer = context != null ? new InternalSerializer(writer, context) : throw Serializer._trace.NewArgumentException(nameof (context));
      this._serializer.Start();
    }

    internal TypeTable TypeTable
    {
      get => this._serializer.TypeTable;
      set => this._serializer.TypeTable = value;
    }

    internal void Serialize(object source) => this.Serialize(source, (string) null);

    internal void Serialize(object source, string streamName) => this._serializer.WriteOneTopLevelObject(source, streamName);

    internal void Done() => this._serializer.End();

    internal void Stop() => this._serializer.Stop();
  }
}
