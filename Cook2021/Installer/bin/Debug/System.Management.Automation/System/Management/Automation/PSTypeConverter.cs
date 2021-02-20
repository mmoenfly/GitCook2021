// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSTypeConverter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public abstract class PSTypeConverter
  {
    private static object GetSourceValueAsObject(PSObject sourceValue)
    {
      if (sourceValue == null)
        return (object) null;
      return sourceValue.BaseObject is PSCustomObject ? (object) sourceValue : PSObject.Base((object) sourceValue);
    }

    public abstract bool CanConvertFrom(object sourceValue, Type destinationType);

    public virtual bool CanConvertFrom(PSObject sourceValue, Type destinationType) => this.CanConvertFrom(PSTypeConverter.GetSourceValueAsObject(sourceValue), destinationType);

    public abstract object ConvertFrom(
      object sourceValue,
      Type destinationType,
      IFormatProvider formatProvider,
      bool ignoreCase);

    public virtual object ConvertFrom(
      PSObject sourceValue,
      Type destinationType,
      IFormatProvider formatProvider,
      bool ignoreCase)
    {
      return this.ConvertFrom(PSTypeConverter.GetSourceValueAsObject(sourceValue), destinationType, formatProvider, ignoreCase);
    }

    public abstract bool CanConvertTo(object sourceValue, Type destinationType);

    public virtual bool CanConvertTo(PSObject sourceValue, Type destinationType) => this.CanConvertTo(PSTypeConverter.GetSourceValueAsObject(sourceValue), destinationType);

    public abstract object ConvertTo(
      object sourceValue,
      Type destinationType,
      IFormatProvider formatProvider,
      bool ignoreCase);

    public virtual object ConvertTo(
      PSObject sourceValue,
      Type destinationType,
      IFormatProvider formatProvider,
      bool ignoreCase)
    {
      return this.ConvertTo(PSTypeConverter.GetSourceValueAsObject(sourceValue), destinationType, formatProvider, ignoreCase);
    }
  }
}
