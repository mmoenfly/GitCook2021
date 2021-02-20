// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Fragmentor
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.IO;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Xml;

namespace System.Management.Automation.Remoting
{
  internal class Fragmentor
  {
    private const int SerializationDepthForRemoting = 1;
    [TraceSource("Fragmentor", "Fragmentor")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (Fragmentor), nameof (Fragmentor));
    private static UTF8Encoding _utf8Encoding = new UTF8Encoding();
    private int _fragmentSize;
    private SerializationContext _serializationContext;
    private DeserializationContext _deserializationContext;
    private TypeTable _typeTable;

    internal Fragmentor(int fragmentSize, PSRemotingCryptoHelper cryptoHelper)
    {
      this._fragmentSize = fragmentSize;
      this._serializationContext = new SerializationContext(1, SerializationOptions.RemotingOptions, cryptoHelper);
      this._deserializationContext = new DeserializationContext(DeserializationOptions.RemotingOptions, cryptoHelper);
    }

    internal void Fragment<T>(RemoteDataObject<T> obj, SerializedDataStream dataToBeSent)
    {
      dataToBeSent.Enter();
      try
      {
        obj.Serialize((Stream) dataToBeSent, this);
      }
      finally
      {
        dataToBeSent.Exit();
      }
    }

    internal DeserializationContext DeserializationContext => this._deserializationContext;

    internal int FragmentSize
    {
      get => this._fragmentSize;
      set => this._fragmentSize = value;
    }

    internal TypeTable TypeTable
    {
      get => this._typeTable;
      set => this._typeTable = value;
    }

    internal void SerializeToBytes(object obj, Stream streamToWriteTo)
    {
      using (XmlWriter writer = XmlWriter.Create(streamToWriteTo, new XmlWriterSettings()
      {
        CheckCharacters = false,
        Indent = false,
        CloseOutput = false,
        Encoding = Encoding.UTF8,
        NewLineHandling = NewLineHandling.None,
        OmitXmlDeclaration = true,
        ConformanceLevel = ConformanceLevel.Fragment
      }))
      {
        Serializer serializer = new Serializer(writer, this._serializationContext);
        serializer.TypeTable = this._typeTable;
        serializer.Serialize(obj);
        serializer.Done();
        writer.Flush();
      }
    }

    internal PSObject DeserializeToPSObject(Stream serializedDataStream)
    {
      object obj = (object) null;
      using (XmlTextReader xmlTextReader = new XmlTextReader(serializedDataStream))
      {
        Deserializer deserializer = new Deserializer((XmlReader) xmlTextReader, this._deserializationContext);
        deserializer.TypeTable = this._typeTable;
        obj = deserializer.Deserialize();
        deserializer.Done();
      }
      return obj != null ? PSObject.AsPSObject(obj) : throw new PSRemotingDataStructureException(PSRemotingErrorId.DeserializedObjectIsNull, new object[0]);
    }
  }
}
