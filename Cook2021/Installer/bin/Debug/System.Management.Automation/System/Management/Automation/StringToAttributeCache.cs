// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.StringToAttributeCache
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class StringToAttributeCache : TypeCache
  {
    internal StringToAttributeCache()
    {
      this.Reset();
      this.AddAssemblyLoadEventHandler();
    }

    internal override sealed void Reset()
    {
      lock (this.cache)
      {
        this.cache.Clear();
        this.cache.Add("Alias", typeof (AliasAttribute));
        this.cache.Add("AllowEmptyCollection", typeof (AllowEmptyCollectionAttribute));
        this.cache.Add("AllowEmptyString", typeof (AllowEmptyStringAttribute));
        this.cache.Add("AllowNull", typeof (AllowNullAttribute));
        this.cache.Add("CmdletBinding", typeof (CmdletBindingAttribute));
        this.cache.Add("Parameter", typeof (ParameterAttribute));
        this.cache.Add("OutputType", typeof (OutputTypeAttribute));
        this.cache.Add("ValidateCount", typeof (ValidateCountAttribute));
        this.cache.Add("ValidateLength", typeof (ValidateLengthAttribute));
        this.cache.Add("ValidateNotNull", typeof (ValidateNotNullAttribute));
        this.cache.Add("ValidateNotNullOrEmpty", typeof (ValidateNotNullOrEmptyAttribute));
        this.cache.Add("ValidatePattern", typeof (ValidatePatternAttribute));
        this.cache.Add("ValidateRange", typeof (ValidateRangeAttribute));
        this.cache.Add("ValidateScript", typeof (ValidateScriptAttribute));
        this.cache.Add("ValidateSet", typeof (ValidateSetAttribute));
      }
    }
  }
}
