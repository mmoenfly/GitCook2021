// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ThirdPartyAdapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  internal class ThirdPartyAdapter : PropertyOnlyAdapter
  {
    private Type adaptedType;
    private PSPropertyAdapter externalAdapter;

    internal ThirdPartyAdapter(Type adaptedType, PSPropertyAdapter externalAdapter)
    {
      this.adaptedType = adaptedType;
      this.externalAdapter = externalAdapter;
    }

    internal Type AdaptedType => this.adaptedType;

    protected override Collection<string> GetTypeNameHierarchy(object obj)
    {
      Collection<string> typeNameHierarchy;
      try
      {
        typeNameHierarchy = this.externalAdapter.GetTypeNameHierarchy(obj);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw new ExtendedTypeSystemException("PSPropertyAdapter.GetTypeNameHierarchyError", ex, "ExtendedTypeSystem", "GetTypeNameHierarchyError", new object[1]
        {
          (object) obj.ToString()
        });
      }
      return typeNameHierarchy != null ? typeNameHierarchy : throw new ExtendedTypeSystemException("PSPropertyAdapter.NullReturnValueError", (Exception) null, "ExtendedTypeSystem", "NullReturnValueError", new object[1]
      {
        (object) "PSPropertyAdapter.GetTypeNameHierarchy"
      });
    }

    protected override void DoAddAllProperties<T>(
      object obj,
      PSMemberInfoInternalCollection<T> members)
    {
      Collection<PSAdaptedProperty> properties;
      try
      {
        properties = this.externalAdapter.GetProperties(obj);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw new ExtendedTypeSystemException("PSPropertyAdapter.GetProperties", ex, "ExtendedTypeSystem", "GetProperties", new object[1]
        {
          (object) obj.ToString()
        });
      }
      if (properties == null)
        throw new ExtendedTypeSystemException("PSPropertyAdapter.NullReturnValueError", (Exception) null, "ExtendedTypeSystem", "NullReturnValueError", new object[1]
        {
          (object) "PSPropertyAdapter.GetProperties"
        });
      foreach (PSAdaptedProperty property in properties)
      {
        this.InitializeProperty(property, obj);
        members.Add(property as T);
      }
    }

    protected override PSProperty DoGetProperty(object obj, string propertyName)
    {
      PSAdaptedProperty property;
      try
      {
        property = this.externalAdapter.GetProperty(obj, propertyName);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw new ExtendedTypeSystemException("PSPropertyAdapter.GetProperty", ex, "ExtendedTypeSystem", "GetProperty", new object[2]
        {
          (object) propertyName,
          (object) obj.ToString()
        });
      }
      if (property != null)
        this.InitializeProperty(property, obj);
      return (PSProperty) property;
    }

    private void InitializeProperty(PSAdaptedProperty property, object baseObject)
    {
      if (property.adapter != null)
        return;
      property.adapter = (Adapter) this;
      property.baseObject = baseObject;
    }

    protected override bool PropertyIsSettable(PSProperty property)
    {
      PSAdaptedProperty adaptedProperty = property as PSAdaptedProperty;
      try
      {
        return this.externalAdapter.IsSettable(adaptedProperty);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw new ExtendedTypeSystemException("PSPropertyAdapter.PropertyIsSettableError", ex, "ExtendedTypeSystem", "PropertyIsSettableError", new object[1]
        {
          (object) property.Name
        });
      }
    }

    protected override bool PropertyIsGettable(PSProperty property)
    {
      PSAdaptedProperty adaptedProperty = property as PSAdaptedProperty;
      try
      {
        return this.externalAdapter.IsGettable(adaptedProperty);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw new ExtendedTypeSystemException("PSPropertyAdapter.PropertyIsGettableError", ex, "ExtendedTypeSystem", "PropertyIsGettableError", new object[1]
        {
          (object) property.Name
        });
      }
    }

    protected override object PropertyGet(PSProperty property)
    {
      PSAdaptedProperty adaptedProperty = property as PSAdaptedProperty;
      try
      {
        return this.externalAdapter.GetPropertyValue(adaptedProperty);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw new ExtendedTypeSystemException("PSPropertyAdapter.PropertyGetError", ex, "ExtendedTypeSystem", "PropertyGetError", new object[1]
        {
          (object) property.Name
        });
      }
    }

    protected override void PropertySet(
      PSProperty property,
      object setValue,
      bool convertIfPossible)
    {
      PSAdaptedProperty adaptedProperty = property as PSAdaptedProperty;
      try
      {
        this.externalAdapter.SetPropertyValue(adaptedProperty, setValue);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw new ExtendedTypeSystemException("PSPropertyAdapter.PropertySetError", ex, "ExtendedTypeSystem", "PropertySetError", new object[1]
        {
          (object) property.Name
        });
      }
    }

    protected override string PropertyType(PSProperty property)
    {
      PSAdaptedProperty adaptedProperty = property as PSAdaptedProperty;
      string propertyTypeName;
      try
      {
        propertyTypeName = this.externalAdapter.GetPropertyTypeName(adaptedProperty);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw new ExtendedTypeSystemException("PSPropertyAdapter.PropertyTypeError", ex, "ExtendedTypeSystem", "PropertyTypeError", new object[1]
        {
          (object) property.Name
        });
      }
      return propertyTypeName ?? "System.Object";
    }
  }
}
