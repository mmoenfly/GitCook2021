// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ReferenceIdHandlerForSerializer`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Globalization;

namespace System.Management.Automation
{
  internal class ReferenceIdHandlerForSerializer<T> where T : class
  {
    private ulong seed;
    private IDictionary<T, ulong> object2refId;
    [TraceSource("ReferenceIdHandlerForSerializer", "ReferenceIdHandlerForSerializer class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (ReferenceIdHandlerForSerializer<T>), "ReferenceIdHandlerForSerializer class");

    private ulong GetNewReferenceId() => this.seed++;

    internal ReferenceIdHandlerForSerializer(IDictionary<T, ulong> dictionary) => this.object2refId = dictionary;

    internal string SetRefId(T t)
    {
      if (this.object2refId == null)
        return (string) null;
      ulong newReferenceId = this.GetNewReferenceId();
      this.object2refId.Add(t, newReferenceId);
      return newReferenceId.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    internal string GetRefId(T t)
    {
      ulong num;
      return this.object2refId != null && this.object2refId.TryGetValue(t, out num) ? num.ToString((IFormatProvider) CultureInfo.InvariantCulture) : (string) null;
    }
  }
}
