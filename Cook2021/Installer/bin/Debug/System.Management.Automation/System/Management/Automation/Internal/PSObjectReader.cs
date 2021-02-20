// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSObjectReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace System.Management.Automation.Internal
{
  internal class PSObjectReader : ObjectReaderBase<PSObject>
  {
    public PSObjectReader([In, Out] ObjectStream stream)
      : base((ObjectStreamBase) stream)
    {
    }

    public override Collection<PSObject> Read(int count)
    {
      using (ObjectReaderBase<PSObject>._trace.TraceMethod())
        return PSObjectReader.MakePSObjectCollection(this._stream.Read(count));
    }

    public override PSObject Read()
    {
      using (ObjectReaderBase<PSObject>._trace.TraceMethod())
        return PSObjectReader.MakePSObject(this._stream.Read());
    }

    public override Collection<PSObject> ReadToEnd()
    {
      using (ObjectReaderBase<PSObject>._trace.TraceMethod())
        return PSObjectReader.MakePSObjectCollection(this._stream.ReadToEnd());
    }

    public override Collection<PSObject> NonBlockingRead()
    {
      using (ObjectReaderBase<PSObject>._trace.TraceMethod())
        return PSObjectReader.MakePSObjectCollection(this._stream.NonBlockingRead(int.MaxValue));
    }

    public override Collection<PSObject> NonBlockingRead(int maxRequested)
    {
      using (ObjectReaderBase<PSObject>._trace.TraceMethod())
        return PSObjectReader.MakePSObjectCollection(this._stream.NonBlockingRead(maxRequested));
    }

    public override PSObject Peek()
    {
      using (ObjectReaderBase<PSObject>._trace.TraceMethod())
        return PSObjectReader.MakePSObject(this._stream.Peek());
    }

    protected override void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      this._stream.Close();
    }

    private static PSObject MakePSObject(object o) => o == null ? (PSObject) null : PSObject.AsPSObject(o);

    private static Collection<PSObject> MakePSObjectCollection(
      Collection<object> coll)
    {
      if (coll == null)
        return (Collection<PSObject>) null;
      Collection<PSObject> collection = new Collection<PSObject>();
      foreach (object o in coll)
        collection.Add(PSObjectReader.MakePSObject(o));
      return collection;
    }
  }
}
