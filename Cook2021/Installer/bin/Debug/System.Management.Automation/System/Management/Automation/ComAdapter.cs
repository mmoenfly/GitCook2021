// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ComAdapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace System.Management.Automation
{
  internal class ComAdapter : Adapter
  {
    private object comObject;
    private ComTypeInfo info;
    [TraceSource("COM", "Tracing for COM interop calls")]
    protected new static PSTraceSource tracer = PSTraceSource.GetTracer("COM", "Tracing for COM interop calls");

    internal ComAdapter(object o, ComTypeInfo typeinfo)
    {
      using (ComAdapter.tracer.TraceConstructor((object) this))
      {
        this.comObject = o;
        this.info = typeinfo;
      }
    }

    private ComTypeInfo GetTypeInfo()
    {
      using (ComAdapter.tracer.TraceMethod())
      {
        if (this.info == null)
          this.info = ComTypeInfo.GetDispatchTypeInfo(this.comObject);
        return this.info;
      }
    }

    internal static string GetComTypeName(string clsid)
    {
      StringBuilder stringBuilder = new StringBuilder("System.__ComObject");
      stringBuilder.Append("#{");
      stringBuilder.Append(clsid);
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }

    protected override Collection<string> GetTypeNameHierarchy(object obj)
    {
      Collection<string> typeNameHierarchy = base.GetTypeNameHierarchy(obj);
      Collection<string> collection = new Collection<string>();
      collection.Add(ComAdapter.GetComTypeName(this.GetTypeInfo().Clsid));
      foreach (string str in typeNameHierarchy)
        collection.Add(str);
      return collection;
    }

    protected override T GetMember<T>(object obj, string memberName)
    {
      using (ComAdapter.tracer.TraceMethod())
      {
        ComTypeInfo typeInfo = this.GetTypeInfo();
        if (typeInfo != null && typeInfo.Properties.ContainsKey(memberName))
        {
          ComProperty property = typeInfo.Properties[memberName];
          if (property.IsParameterized)
          {
            if (typeof (T).IsAssignableFrom(typeof (PSParameterizedProperty)))
              return new PSParameterizedProperty(property.Name, (Adapter) this, obj, (object) property) as T;
          }
          else if (typeof (T).IsAssignableFrom(typeof (PSProperty)))
            return new PSProperty(property.Name, (Adapter) this, obj, (object) property) as T;
        }
        if (!typeof (T).IsAssignableFrom(typeof (PSMethod)) || typeInfo == null || !typeInfo.Methods.ContainsKey(memberName))
          return default (T);
        ComMethod method = typeInfo.Methods[memberName];
        return new PSMethod(method.Name, (Adapter) this, obj, (object) method) as T;
      }
    }

    protected override PSMemberInfoInternalCollection<T> GetMembers<T>(
      object obj)
    {
      using (ComAdapter.tracer.TraceMethod())
      {
        ComTypeInfo typeInfo = this.GetTypeInfo();
        PSMemberInfoInternalCollection<T> internalCollection = new PSMemberInfoInternalCollection<T>();
        if (typeInfo != null)
        {
          bool flag1 = typeof (T).IsAssignableFrom(typeof (PSProperty));
          bool flag2 = typeof (T).IsAssignableFrom(typeof (PSParameterizedProperty));
          if (flag1 || flag2)
          {
            foreach (ComProperty comProperty in typeInfo.Properties.Values)
            {
              if (comProperty.IsParameterized)
              {
                if (flag2)
                  internalCollection.Add(new PSParameterizedProperty(comProperty.Name, (Adapter) this, obj, (object) comProperty) as T);
              }
              else if (flag1)
                internalCollection.Add(new PSProperty(comProperty.Name, (Adapter) this, obj, (object) comProperty) as T);
            }
          }
          if (typeof (T).IsAssignableFrom(typeof (PSMethod)))
          {
            foreach (ComMethod comMethod in typeInfo.Methods.Values)
            {
              PSMethod psMethod = new PSMethod(comMethod.Name, (Adapter) this, obj, (object) comMethod);
              if (!internalCollection.hashedMembers.Contains((object) comMethod.Name))
                internalCollection.Add(psMethod as T);
            }
          }
        }
        return internalCollection;
      }
    }

    protected override AttributeCollection PropertyAttributes(PSProperty property) => new AttributeCollection(new Attribute[0]);

    protected override object PropertyGet(PSProperty property)
    {
      using (ComAdapter.tracer.TraceMethod())
        return ((ComProperty) property.adapterData).GetValue(property.baseObject);
    }

    protected override void PropertySet(
      PSProperty property,
      object setValue,
      bool convertIfPossible)
    {
      using (ComAdapter.tracer.TraceMethod())
        ((ComProperty) property.adapterData).SetValue(property.baseObject, setValue);
    }

    protected override bool PropertyIsSettable(PSProperty property)
    {
      using (ComAdapter.tracer.TraceMethod())
        return ((ComProperty) property.adapterData).IsSettable;
    }

    protected override bool PropertyIsGettable(PSProperty property)
    {
      using (ComAdapter.tracer.TraceMethod())
        return ((ComProperty) property.adapterData).IsGettable;
    }

    protected override string PropertyType(PSProperty property)
    {
      using (ComAdapter.tracer.TraceMethod())
        return ((ComProperty) property.adapterData).Type.FullName;
    }

    protected override string PropertyToString(PSProperty property)
    {
      using (ComAdapter.tracer.TraceMethod())
        return ((ComProperty) property.adapterData).ToString();
    }

    protected override object MethodInvoke(PSMethod method, object[] arguments)
    {
      using (ComAdapter.tracer.TraceMethod())
        return ((ComMethod) method.adapterData).InvokeMethod(method, arguments);
    }

    protected override Collection<string> MethodDefinitions(PSMethod method)
    {
      using (ComAdapter.tracer.TraceMethod())
        return ((ComMethod) method.adapterData).MethodDefinitions();
    }

    protected override string ParameterizedPropertyType(PSParameterizedProperty property)
    {
      using (ComAdapter.tracer.TraceMethod())
        return ((ComProperty) property.adapterData).Type.FullName;
    }

    protected override bool ParameterizedPropertyIsSettable(PSParameterizedProperty property)
    {
      using (ComAdapter.tracer.TraceMethod())
        return ((ComProperty) property.adapterData).IsSettable;
    }

    protected override bool ParameterizedPropertyIsGettable(PSParameterizedProperty property)
    {
      using (ComAdapter.tracer.TraceMethod())
        return ((ComProperty) property.adapterData).IsGettable;
    }

    protected override object ParameterizedPropertyGet(
      PSParameterizedProperty property,
      object[] arguments)
    {
      using (ComAdapter.tracer.TraceMethod())
        return ((ComProperty) property.adapterData).GetValue(property.baseObject, arguments);
    }

    protected override void ParameterizedPropertySet(
      PSParameterizedProperty property,
      object setValue,
      object[] arguments)
    {
      using (ComAdapter.tracer.TraceMethod())
        ((ComProperty) property.adapterData).SetValue(property.baseObject, setValue, arguments);
    }

    protected override string ParameterizedPropertyToString(PSParameterizedProperty property)
    {
      using (ComAdapter.tracer.TraceMethod())
        return ((ComProperty) property.adapterData).ToString();
    }

    protected override Collection<string> ParameterizedPropertyDefinitions(
      PSParameterizedProperty property)
    {
      using (ComAdapter.tracer.TraceMethod())
        return new Collection<string>()
        {
          ((ComProperty) property.adapterData).GetDefinition()
        };
    }
  }
}
