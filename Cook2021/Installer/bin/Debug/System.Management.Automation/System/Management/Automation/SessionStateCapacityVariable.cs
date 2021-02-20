// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SessionStateCapacityVariable
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;

namespace System.Management.Automation
{
  internal class SessionStateCapacityVariable : PSVariable
  {
    private int _fastValue;
    private int minCapacity;
    private int maxCapacity = int.MaxValue;
    private SessionStateCapacityVariable sharedCapacityVariable;

    internal SessionStateCapacityVariable(
      string name,
      int defaultCapacity,
      int maxCapacity,
      int minCapacity,
      ScopedItemOptions options)
      : base(name, (object) defaultCapacity, options)
    {
      ValidateRangeAttribute validateRangeAttribute = new ValidateRangeAttribute((object) minCapacity, (object) maxCapacity);
      this.minCapacity = minCapacity;
      this.maxCapacity = maxCapacity;
      this.Attributes.Add((Attribute) validateRangeAttribute);
      this._fastValue = defaultCapacity;
    }

    public SessionStateCapacityVariable(
      string name,
      SessionStateCapacityVariable sharedCapacityVariable,
      ScopedItemOptions options)
      : base(name, sharedCapacityVariable.Value, options)
    {
      this.Attributes.Add((Attribute) new ValidateRangeAttribute((object) 0, (object) int.MaxValue));
      this.sharedCapacityVariable = sharedCapacityVariable;
      this.Description = sharedCapacityVariable.Description;
      this._fastValue = (int) sharedCapacityVariable.Value;
    }

    public override object Value
    {
      get => this.sharedCapacityVariable == null ? base.Value : this.sharedCapacityVariable.Value;
      set
      {
        this.sharedCapacityVariable = (SessionStateCapacityVariable) null;
        base.Value = LanguagePrimitives.ConvertTo(value, typeof (int), (IFormatProvider) CultureInfo.InvariantCulture);
        this._fastValue = (int) base.Value;
      }
    }

    internal int FastValue => this._fastValue;

    public override bool IsValidValue(object value)
    {
      int num = (int) value;
      return num >= this.minCapacity && num <= this.maxCapacity || base.IsValidValue(value);
    }
  }
}
