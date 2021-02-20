// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ValidateSetAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Text;

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
  public sealed class ValidateSetAttribute : ValidateEnumeratedArgumentsAttribute
  {
    private string[] validValues;
    private bool ignoreCase = true;

    public bool IgnoreCase
    {
      get => this.ignoreCase;
      set => this.ignoreCase = value;
    }

    public IList<string> ValidValues => (IList<string>) this.validValues;

    protected override void ValidateElement(object element)
    {
      string strB = element != null ? element.ToString() : throw new ValidationMetadataException("ArgumentIsEmpty", (Exception) null, "Metadata", "ValidateNotNullFailure", new object[0]);
      for (int index = 0; index < this.validValues.Length; ++index)
      {
        if (string.Compare(this.validValues[index], strB, this.ignoreCase, CultureInfo.InvariantCulture) == 0)
          return;
      }
      throw new ValidationMetadataException("ValidateSetFailure", (Exception) null, "Metadata", "ValidateSetFailure", new object[2]
      {
        (object) element.ToString(),
        (object) this.SetAsString()
      });
    }

    private string SetAsString()
    {
      string resourceString = ResourceManagerCache.GetResourceString("Metadata", "ValidateSetSeparator");
      StringBuilder stringBuilder = new StringBuilder();
      if (this.validValues.Length > 0)
      {
        foreach (string validValue in this.validValues)
        {
          stringBuilder.Append(validValue);
          stringBuilder.Append(resourceString);
        }
        stringBuilder.Remove(stringBuilder.Length - resourceString.Length, resourceString.Length);
      }
      return stringBuilder.ToString();
    }

    public ValidateSetAttribute(params string[] validValues)
    {
      if (validValues == null)
        throw CmdletMetadataAttribute.tracer.NewArgumentNullException(nameof (validValues));
      this.validValues = validValues.Length != 0 ? validValues : throw CmdletMetadataAttribute.tracer.NewArgumentOutOfRangeException(nameof (validValues), (object) validValues);
    }
  }
}
