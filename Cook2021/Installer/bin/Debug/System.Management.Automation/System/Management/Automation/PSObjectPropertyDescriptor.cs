// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSObjectPropertyDescriptor
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.ComponentModel;

namespace System.Management.Automation
{
  public class PSObjectPropertyDescriptor : PropertyDescriptor
  {
    internal const string InvalidComponentMsg = "InvalidComponent";
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private bool _isReadOnly;
    private AttributeCollection _propertyAttributes;
    private Type _propertyType;

    internal event EventHandler<SettingValueExceptionEventArgs> SettingValueException;

    internal event EventHandler<GettingValueExceptionEventArgs> GettingValueException;

    internal PSObjectPropertyDescriptor(
      string propertyName,
      Type propertyType,
      bool isReadOnly,
      AttributeCollection propertyAttributes)
      : base(propertyName, new Attribute[0])
    {
      this._isReadOnly = isReadOnly;
      this._propertyAttributes = propertyAttributes;
      this._propertyType = propertyType;
    }

    public override AttributeCollection Attributes => this._propertyAttributes;

    public override bool IsReadOnly => this._isReadOnly;

    public override void ResetValue(object component)
    {
    }

    public override bool CanResetValue(object component) => false;

    public override bool ShouldSerializeValue(object component) => true;

    public override Type ComponentType => typeof (PSObject);

    public override Type PropertyType => this._propertyType;

    public override object GetValue(object component)
    {
      PSObject psObject = component != null ? PSObjectPropertyDescriptor.GetComponentPSObject(component) : throw PSObjectPropertyDescriptor.tracer.NewArgumentNullException(nameof (component));
      try
      {
        PSPropertyInfo property = psObject.Properties[this.Name];
        if (property != null)
          return property.Value;
        PSObjectTypeDescriptor.typeDescriptor.WriteLine("Could not find property \"{0}\" to get its value.", (object) this.Name);
        ExtendedTypeSystemException e = new ExtendedTypeSystemException("PropertyNotFoundInPropertyDescriptorGetValue", (Exception) null, "ExtendedTypeSystem", "PropertyNotFoundInTypeDescriptor", new object[1]
        {
          (object) this.Name
        });
        bool shouldThrow;
        object valueException = this.DealWithGetValueException(e, out shouldThrow);
        if (shouldThrow)
          throw e;
        return valueException;
      }
      catch (ExtendedTypeSystemException ex)
      {
        PSObjectTypeDescriptor.typeDescriptor.WriteLine("Exception getting the value of the property \"{0}\": \"{1}\".", (object) this.Name, (object) ex.Message);
        bool shouldThrow;
        object valueException = this.DealWithGetValueException(ex, out shouldThrow);
        if (!shouldThrow)
          return valueException;
        throw;
      }
    }

    private static PSObject GetComponentPSObject(object component)
    {
      switch (component)
      {
        case PSObject instance:
label_3:
          return instance;
        case PSObjectTypeDescriptor objectTypeDescriptor:
          instance = objectTypeDescriptor.Instance;
          goto label_3;
        default:
          throw PSObjectPropertyDescriptor.tracer.NewArgumentException(nameof (component), "ExtendedTypeSystem", "InvalidComponent", (object) nameof (component), (object) typeof (PSObject).Name, (object) typeof (PSObjectTypeDescriptor).Name);
      }
    }

    private object DealWithGetValueException(ExtendedTypeSystemException e, out bool shouldThrow)
    {
      GettingValueExceptionEventArgs e1 = new GettingValueExceptionEventArgs((Exception) e);
      if (this.GettingValueException != null)
      {
        this.GettingValueException((object) this, e1);
        PSObjectTypeDescriptor.typeDescriptor.WriteLine("GettingValueException event has been triggered resulting in ValueReplacement:\"{0}\".", e1.ValueReplacement);
      }
      shouldThrow = e1.ShouldThrow;
      return e1.ValueReplacement;
    }

    public override void SetValue(object component, object value)
    {
      PSObject psObject = component != null ? PSObjectPropertyDescriptor.GetComponentPSObject(component) : throw PSObjectPropertyDescriptor.tracer.NewArgumentNullException(nameof (component));
      try
      {
        PSPropertyInfo property = psObject.Properties[this.Name];
        if (property == null)
        {
          PSObjectTypeDescriptor.typeDescriptor.WriteLine("Could not find property \"{0}\" to set its value.", (object) this.Name);
          ExtendedTypeSystemException e = new ExtendedTypeSystemException("PropertyNotFoundInPropertyDescriptorSetValue", (Exception) null, "ExtendedTypeSystem", "PropertyNotFoundInTypeDescriptor", new object[1]
          {
            (object) this.Name
          });
          bool shouldThrow;
          this.DealWithSetValueException(e, out shouldThrow);
          if (!shouldThrow)
            return;
          throw e;
        }
        property.Value = value;
      }
      catch (ExtendedTypeSystemException ex)
      {
        PSObjectTypeDescriptor.typeDescriptor.WriteLine("Exception setting the value of the property \"{0}\": \"{1}\".", (object) this.Name, (object) ex.Message);
        bool shouldThrow;
        this.DealWithSetValueException(ex, out shouldThrow);
        if (shouldThrow)
          throw;
      }
      this.OnValueChanged(component, EventArgs.Empty);
    }

    private void DealWithSetValueException(ExtendedTypeSystemException e, out bool shouldThrow)
    {
      SettingValueExceptionEventArgs e1 = new SettingValueExceptionEventArgs((Exception) e);
      if (this.SettingValueException != null)
      {
        this.SettingValueException((object) this, e1);
        PSObjectTypeDescriptor.typeDescriptor.WriteLine("SettingValueException event has been triggered resulting in ShouldThrow:\"{0}\".", (object) e1.ShouldThrow);
      }
      shouldThrow = e1.ShouldThrow;
    }
  }
}
