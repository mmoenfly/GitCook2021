// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSPropertyAdapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public abstract class PSPropertyAdapter
  {
    public virtual Collection<string> GetTypeNameHierarchy(object baseObject)
    {
      if (baseObject == null)
        throw new ArgumentNullException(nameof (baseObject));
      Collection<string> collection = new Collection<string>();
      for (Type type = baseObject.GetType(); type != null; type = type.BaseType)
        collection.Add(type.FullName);
      return collection;
    }

    public abstract Collection<PSAdaptedProperty> GetProperties(
      object baseObject);

    public abstract PSAdaptedProperty GetProperty(
      object baseObject,
      string propertyName);

    public abstract bool IsSettable(PSAdaptedProperty adaptedProperty);

    public abstract bool IsGettable(PSAdaptedProperty adaptedProperty);

    public abstract object GetPropertyValue(PSAdaptedProperty adaptedProperty);

    public abstract void SetPropertyValue(PSAdaptedProperty adaptedProperty, object value);

    public abstract string GetPropertyTypeName(PSAdaptedProperty adaptedProperty);
  }
}
