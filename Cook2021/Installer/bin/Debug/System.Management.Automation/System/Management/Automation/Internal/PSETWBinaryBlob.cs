// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSETWBinaryBlob
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Internal
{
  internal sealed class PSETWBinaryBlob
  {
    public readonly byte[] blob;
    public readonly int offset;
    public readonly int length;

    public PSETWBinaryBlob(byte[] blob, int offset, int length)
    {
      this.blob = blob;
      this.offset = offset;
      this.length = length;
    }
  }
}
