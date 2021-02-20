// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSETWEmptyTracer
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Internal
{
  internal sealed class PSETWEmptyTracer : IETWTracer
  {
    internal PSETWEmptyTracer()
    {
      this.operationalChannel = (IChannelWriter) new PSETWEmptyChannelWriter();
      this.analyticChannel = (IChannelWriter) new PSETWEmptyChannelWriter();
    }

    internal override Guid SetActivityIdForCurrentThread(Guid activityId) => Guid.Empty;

    internal override void ReplaceActivityIdForCurrentThread(
      Guid newActivityId,
      PSEventId eventForOperationalChannel,
      PSEventId eventForAnalyticChannel,
      PSKeyword keyword,
      PSTask task)
    {
    }

    internal override void WriteTransferEvent(
      Guid relatedActivityId,
      PSEventId eventForOperationalChannel,
      PSEventId eventForAnalyticChannel,
      PSKeyword keyword,
      PSTask task)
    {
    }

    public override void Dispose()
    {
    }
  }
}
