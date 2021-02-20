// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.IETWTracer
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Internal
{
  internal abstract class IETWTracer : IDisposable
  {
    protected IChannelWriter operationalChannel;
    protected IChannelWriter analyticChannel;

    internal IChannelWriter OperationalChannel => this.operationalChannel;

    internal IChannelWriter AnalyticChannel => this.analyticChannel;

    internal abstract Guid SetActivityIdForCurrentThread(Guid activityId);

    internal abstract void ReplaceActivityIdForCurrentThread(
      Guid newActivityId,
      PSEventId eventForOperationalChannel,
      PSEventId eventForAnalyticChannel,
      PSKeyword keyword,
      PSTask task);

    internal abstract void WriteTransferEvent(
      Guid relatedActivityId,
      PSEventId eventForOperationalChannel,
      PSEventId eventForAnalyticChannel,
      PSKeyword keyword,
      PSTask task);

    public abstract void Dispose();
  }
}
