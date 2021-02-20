// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ConvertThroughString
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public class ConvertThroughString : PSTypeConverter
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");

    public override bool CanConvertFrom(object sourceValue, Type destinationType) => !(sourceValue is string);

    public override object ConvertFrom(
      object sourceValue,
      Type destinationType,
      IFormatProvider formatProvider,
      bool ignoreCase)
    {
      return LanguagePrimitives.ConvertTo((object) (string) LanguagePrimitives.ConvertTo(sourceValue, typeof (string), formatProvider), destinationType, formatProvider);
    }

    public override bool CanConvertTo(object sourceValue, Type destinationType) => false;

    public override object ConvertTo(
      object sourceValue,
      Type destinationType,
      IFormatProvider formatProvider,
      bool ignoreCase)
    {
      throw ConvertThroughString.tracer.NewNotSupportedException();
    }
  }
}
