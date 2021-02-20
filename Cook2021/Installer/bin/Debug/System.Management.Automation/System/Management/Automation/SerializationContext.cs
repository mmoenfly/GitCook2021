// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SerializationContext
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal class SerializationContext
  {
    private const int DefaultSerializationDepth = 2;
    internal readonly int depth;
    internal readonly SerializationOptions options;
    internal readonly PSRemotingCryptoHelper cryptoHelper;
    [TraceSource("SerializationContext", "SerializationContext class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (SerializationContext), "SerializationContext class");

    internal SerializationContext()
      : this(2, true)
    {
    }

    internal SerializationContext(int depth, bool useDepthFromTypes)
      : this(depth, (SerializationOptions) ((useDepthFromTypes ? 1 : 0) | 16), (PSRemotingCryptoHelper) null)
    {
    }

    internal SerializationContext(
      int depth,
      SerializationOptions options,
      PSRemotingCryptoHelper cryptoHelper)
    {
      this.depth = depth >= 1 ? depth : throw SerializationContext._trace.NewArgumentException("writer", "serialization", "DepthOfOneRequired");
      this.options = options;
      this.cryptoHelper = cryptoHelper;
    }
  }
}
