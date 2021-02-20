// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CallsiteCacheEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class CallsiteCacheEntry : IEquatable<CallsiteCacheEntry>
  {
    private string methodName;
    private CallsiteSignature signature;

    internal CallsiteCacheEntry(string methodName, CallsiteSignature signature)
    {
      this.methodName = methodName;
      this.signature = signature;
    }

    public override int GetHashCode() => this.methodName.ToLowerInvariant().GetHashCode() ^ this.signature.GetHashCode();

    public bool Equals(CallsiteCacheEntry other) => this.methodName.Equals(other.methodName, StringComparison.OrdinalIgnoreCase) && this.signature.Equals(other.signature);

    public override bool Equals(object other) => other is CallsiteCacheEntry other1 && this.Equals(other1);
  }
}
