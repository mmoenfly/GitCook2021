// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DeserializationContext
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;
using System.Xml;

namespace System.Management.Automation
{
  internal class DeserializationContext
  {
    private int totalDataProcessedSoFar;
    private int? maxAllowedMemory;
    internal readonly DeserializationOptions options;
    internal readonly PSRemotingCryptoHelper cryptoHelper;

    internal DeserializationContext()
      : this(DeserializationOptions.None, (PSRemotingCryptoHelper) null)
    {
    }

    internal DeserializationContext(
      DeserializationOptions options,
      PSRemotingCryptoHelper cryptoHelper)
    {
      this.options = options;
      this.cryptoHelper = cryptoHelper;
    }

    internal int? MaximumAllowedMemory
    {
      set => this.maxAllowedMemory = value;
      get => this.maxAllowedMemory;
    }

    internal void LogExtraMemoryUsage(int amountOfExtraMemory)
    {
      if (amountOfExtraMemory < 0 || !this.maxAllowedMemory.HasValue)
        return;
      if (amountOfExtraMemory > this.maxAllowedMemory.Value - this.totalDataProcessedSoFar)
        throw new XmlException(ResourceManagerCache.FormatResourceString("Serialization", "DeserializationMemoryQuota"));
      this.totalDataProcessedSoFar += amountOfExtraMemory;
    }
  }
}
