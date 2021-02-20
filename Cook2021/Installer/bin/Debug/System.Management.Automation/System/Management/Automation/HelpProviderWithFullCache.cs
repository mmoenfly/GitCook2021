// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HelpProviderWithFullCache
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal abstract class HelpProviderWithFullCache : HelpProviderWithCache
  {
    [TraceSource("HelpProviderWithFullCache", "HelpProviderWithFullCache")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (HelpProviderWithFullCache), nameof (HelpProviderWithFullCache));

    internal HelpProviderWithFullCache(HelpSystem helpSystem)
      : base(helpSystem)
    {
      using (HelpProviderWithFullCache.tracer.TraceConstructor((object) this))
        ;
    }

    internal override sealed IEnumerable<HelpInfo> ExactMatchHelp(
      HelpRequest helpRequest)
    {
      using (HelpProviderWithFullCache.tracer.TraceMethod())
      {
        if (!this.CacheFullyLoaded || this.AreSnapInsSupported())
          this.LoadCache();
        this.CacheFullyLoaded = true;
        return base.ExactMatchHelp(helpRequest);
      }
    }

    internal override sealed void DoExactMatchHelp(HelpRequest helpRequest)
    {
      using (HelpProviderWithFullCache.tracer.TraceMethod())
        ;
    }

    internal override sealed IEnumerable<HelpInfo> SearchHelp(
      HelpRequest helpRequest,
      bool searchOnlyContent)
    {
      using (HelpProviderWithFullCache.tracer.TraceMethod())
      {
        if (!this.CacheFullyLoaded || this.AreSnapInsSupported())
          this.LoadCache();
        this.CacheFullyLoaded = true;
        return base.SearchHelp(helpRequest, searchOnlyContent);
      }
    }

    internal override sealed IEnumerable<HelpInfo> DoSearchHelp(
      HelpRequest helpRequest)
    {
      using (HelpProviderWithFullCache.tracer.TraceMethod())
        return (IEnumerable<HelpInfo>) null;
    }

    internal virtual void LoadCache()
    {
    }
  }
}
