// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.FormatTableLoadException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation.Runspaces
{
  [Serializable]
  public class FormatTableLoadException : RuntimeException
  {
    private Collection<string> errors;

    public FormatTableLoadException() => this.SetDefaultErrorRecord();

    public FormatTableLoadException(string message)
      : base(message)
      => this.SetDefaultErrorRecord();

    public FormatTableLoadException(string message, Exception innerException)
      : base(message, innerException)
      => this.SetDefaultErrorRecord();

    internal FormatTableLoadException(Collection<string> loadErrors)
      : base(ResourceManagerCache.FormatResourceString("FormatAndOut.XmlLoading", "FormattableLoadErrors"))
    {
      this.errors = loadErrors;
      this.SetDefaultErrorRecord();
    }

    protected FormatTableLoadException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      int num = info != null ? info.GetInt32("ErrorCount") : throw new PSArgumentNullException(nameof (info));
      if (num <= 0)
        return;
      this.errors = new Collection<string>();
      for (int index = 0; index < num; ++index)
      {
        string name = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Error{0}", (object) index);
        this.errors.Add(info.GetString(name));
      }
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new PSArgumentNullException(nameof (info));
      base.GetObjectData(info, context);
      if (this.errors == null)
        return;
      int count = this.errors.Count;
      info.AddValue("ErrorCount", count);
      for (int index = 0; index < count; ++index)
      {
        string name = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Error{0}", (object) index);
        info.AddValue(name, (object) this.errors[index]);
      }
    }

    protected void SetDefaultErrorRecord()
    {
      this.SetErrorCategory(ErrorCategory.InvalidData);
      this.SetErrorId(typeof (FormatTableLoadException).FullName);
    }

    public Collection<string> Errors => this.errors;
  }
}
