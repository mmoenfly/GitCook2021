// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Provider.CmdletProviderAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Provider
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class CmdletProviderAttribute : Attribute
  {
    [TraceSource("CmdletProviderAttribute", "The attribute that declares that a class is an implementation of a Core Command Provider.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CmdletProviderAttribute), "The attribute that declares that a class is an implementation of a Core Command Provider.");
    private char[] illegalCharacters = new char[6]
    {
      ':',
      '\\',
      '[',
      ']',
      '?',
      '*'
    };
    private string provider = string.Empty;
    private ProviderCapabilities providerCapabilities;

    public CmdletProviderAttribute(string providerName, ProviderCapabilities providerCapabilities)
    {
      using (CmdletProviderAttribute.tracer.TraceConstructor((object) this))
      {
        if (string.IsNullOrEmpty(providerName))
          throw CmdletProviderAttribute.tracer.NewArgumentNullException(nameof (providerName));
        this.provider = providerName.IndexOfAny(this.illegalCharacters) == -1 ? providerName : throw CmdletProviderAttribute.tracer.NewArgumentException(nameof (providerName), "SessionStateStrings", "ProviderNameNotValid", (object) providerName);
        this.providerCapabilities = providerCapabilities;
      }
    }

    public string ProviderName
    {
      get
      {
        using (CmdletProviderAttribute.tracer.TraceProperty(this.provider, new object[0]))
          return this.provider;
      }
    }

    public ProviderCapabilities ProviderCapabilities
    {
      get
      {
        using (CmdletProviderAttribute.tracer.TraceProperty())
          return this.providerCapabilities;
      }
    }
  }
}
