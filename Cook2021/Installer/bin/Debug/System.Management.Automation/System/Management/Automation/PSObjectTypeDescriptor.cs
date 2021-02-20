// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSObjectTypeDescriptor
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.ComponentModel;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public class PSObjectTypeDescriptor : CustomTypeDescriptor
  {
    internal static PSTraceSource typeDescriptor = PSTraceSource.GetTracer("TypeDescriptor", "Traces the behavior of PSObjectTypeDescriptor, PSObjectTypeDescriptionProvider and PSObjectPropertyDescriptor.", false);
    private PSObject _instance;

    public event EventHandler<SettingValueExceptionEventArgs> SettingValueException;

    public event EventHandler<GettingValueExceptionEventArgs> GettingValueException;

    public PSObjectTypeDescriptor(PSObject instance) => this._instance = instance;

    public PSObject Instance => this._instance;

    private void CheckAndAddProperty(
      PSPropertyInfo propertyInfo,
      Attribute[] attributes,
      ref PropertyDescriptorCollection returnValue)
    {
      using (PSObjectTypeDescriptor.typeDescriptor.TraceScope("Checking property \"{0}\".", (object) propertyInfo.Name))
      {
        if (!propertyInfo.IsGettable)
        {
          PSObjectTypeDescriptor.typeDescriptor.WriteLine("Property \"{0}\" is write-only so it has been skipped.", (object) propertyInfo.Name);
        }
        else
        {
          AttributeCollection propertyAttributes = (AttributeCollection) null;
          Type propertyType = typeof (object);
          if (attributes != null && attributes.Length != 0 && propertyInfo is PSProperty psProperty)
          {
            if (!(psProperty.adapterData is DotNetAdapter.PropertyCacheEntry adapterData))
              PSObjectTypeDescriptor.typeDescriptor.WriteLine("Skipping attribute check for property \"{0}\" because it is an adapted property (not a .NET property).", (object) psProperty.Name);
            else if (psProperty.isDeserialized)
            {
              PSObjectTypeDescriptor.typeDescriptor.WriteLine("Skipping attribute check for property \"{0}\" because it has been deserialized.", (object) psProperty.Name);
            }
            else
            {
              propertyType = adapterData.propertyType;
              propertyAttributes = adapterData.Attributes;
              foreach (Attribute attribute in attributes)
              {
                if (!propertyAttributes.Contains(attribute))
                {
                  PSObjectTypeDescriptor.typeDescriptor.WriteLine("Property \"{0}\" does not contain attribute \"{1}\" so it has been skipped.", (object) psProperty.Name, (object) attribute);
                  return;
                }
              }
            }
          }
          if (propertyAttributes == null)
            propertyAttributes = new AttributeCollection(new Attribute[0]);
          PSObjectTypeDescriptor.typeDescriptor.WriteLine("Adding property \"{0}\".", (object) propertyInfo.Name);
          PSObjectPropertyDescriptor propertyDescriptor = new PSObjectPropertyDescriptor(propertyInfo.Name, propertyType, !propertyInfo.IsSettable, propertyAttributes);
          propertyDescriptor.SettingValueException += this.SettingValueException;
          propertyDescriptor.GettingValueException += this.GettingValueException;
          returnValue.Add((PropertyDescriptor) propertyDescriptor);
        }
      }
    }

    public override PropertyDescriptorCollection GetProperties() => this.GetProperties((Attribute[]) null);

    public override PropertyDescriptorCollection GetProperties(
      Attribute[] attributes)
    {
      using (PSObjectTypeDescriptor.typeDescriptor.TraceScope("Getting properties."))
      {
        PropertyDescriptorCollection returnValue = new PropertyDescriptorCollection((PropertyDescriptor[]) null);
        if (this._instance == null)
          return returnValue;
        foreach (PSPropertyInfo property in this._instance.Properties)
          this.CheckAndAddProperty(property, attributes, ref returnValue);
        return returnValue;
      }
    }

    public override bool Equals(object obj)
    {
      if (!(obj is PSObjectTypeDescriptor objectTypeDescriptor))
        return false;
      return this.Instance == null || objectTypeDescriptor.Instance == null ? object.ReferenceEquals((object) this, (object) objectTypeDescriptor) : objectTypeDescriptor.Instance.Equals((object) this.Instance);
    }

    public override int GetHashCode() => this.Instance == null ? base.GetHashCode() : this.Instance.GetHashCode();

    public override PropertyDescriptor GetDefaultProperty()
    {
      if (this.Instance == null)
        return (PropertyDescriptor) null;
      string b = (string) null;
      if (this.Instance.Members["PSStandardMembers"] is PSMemberSet member && member.Properties["DefaultDisplayProperty"] is PSNoteProperty property)
        b = property.Value as string;
      if (b == null)
      {
        object[] customAttributes = this.Instance.BaseObject.GetType().GetCustomAttributes(typeof (DefaultPropertyAttribute), true);
        if (customAttributes.Length == 1 && customAttributes[0] is DefaultPropertyAttribute propertyAttribute)
          b = propertyAttribute.Name;
      }
      PropertyDescriptorCollection properties = this.GetProperties();
      if (b != null)
      {
        foreach (PropertyDescriptor propertyDescriptor in properties)
        {
          if (string.Equals(propertyDescriptor.Name, b, StringComparison.OrdinalIgnoreCase))
            return propertyDescriptor;
        }
      }
      return (PropertyDescriptor) null;
    }

    public override TypeConverter GetConverter()
    {
      if (this.Instance == null)
        return new TypeConverter();
      object baseObject = this.Instance.BaseObject;
      if (!(LanguagePrimitives.GetConverter(baseObject.GetType(), (TypeTable) null) is TypeConverter converter))
        converter = TypeDescriptor.GetConverter(baseObject);
      return converter;
    }

    public override object GetPropertyOwner(PropertyDescriptor pd) => (object) this.Instance;

    public override EventDescriptor GetDefaultEvent() => this.Instance == null ? (EventDescriptor) null : TypeDescriptor.GetDefaultEvent(this.Instance.BaseObject);

    public override EventDescriptorCollection GetEvents() => this.Instance == null ? new EventDescriptorCollection((EventDescriptor[]) null) : TypeDescriptor.GetEvents(this.Instance.BaseObject);

    public override EventDescriptorCollection GetEvents(
      Attribute[] attributes)
    {
      return this.Instance == null ? (EventDescriptorCollection) null : TypeDescriptor.GetEvents(this.Instance.BaseObject, attributes);
    }

    public override AttributeCollection GetAttributes() => this.Instance == null ? new AttributeCollection(new Attribute[0]) : TypeDescriptor.GetAttributes(this.Instance.BaseObject);

    public override string GetClassName() => this.Instance == null ? (string) null : TypeDescriptor.GetClassName(this.Instance.BaseObject);

    public override string GetComponentName() => this.Instance == null ? (string) null : TypeDescriptor.GetComponentName(this.Instance.BaseObject);

    public override object GetEditor(Type editorBaseType) => this.Instance == null ? (object) null : TypeDescriptor.GetEditor(this.Instance.BaseObject, editorBaseType);
  }
}
