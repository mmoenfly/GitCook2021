// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ProviderNameAmbiguousException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class ProviderNameAmbiguousException : ProviderNotFoundException
  {
    private ReadOnlyCollection<ProviderInfo> _possibleMatches;

    internal ProviderNameAmbiguousException(
      string providerName,
      string errorIdAndResourceId,
      Collection<ProviderInfo> possibleMatches,
      params object[] messageArgs)
      : base(providerName, SessionStateCategory.CmdletProvider, errorIdAndResourceId, messageArgs)
    {
      this._possibleMatches = new ReadOnlyCollection<ProviderInfo>((IList<ProviderInfo>) possibleMatches);
    }

    public ProviderNameAmbiguousException()
    {
    }

    public ProviderNameAmbiguousException(string message)
      : base(message)
    {
    }

    public ProviderNameAmbiguousException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected ProviderNameAmbiguousException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public ReadOnlyCollection<ProviderInfo> PossibleMatches => this._possibleMatches;
  }
}
