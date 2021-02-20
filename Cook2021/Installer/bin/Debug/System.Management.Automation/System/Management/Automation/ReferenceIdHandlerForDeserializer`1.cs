// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ReferenceIdHandlerForDeserializer`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal class ReferenceIdHandlerForDeserializer<T> where T : class
  {
    private Dictionary<string, T> refId2object = new Dictionary<string, T>();
    [TraceSource("ReferenceIdHandlerForDeserializer", "ReferenceIdHandlerForDeserializer class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (ReferenceIdHandlerForDeserializer<T>), "ReferenceIdHandlerForDeserializer class");

    internal void SetRefId(T o, string refId, bool duplicateRefIdsAllowed) => this.refId2object[refId] = o;

    internal T GetReferencedObject(string refId)
    {
      T obj;
      return this.refId2object.TryGetValue(refId, out obj) ? obj : default (T);
    }
  }
}
