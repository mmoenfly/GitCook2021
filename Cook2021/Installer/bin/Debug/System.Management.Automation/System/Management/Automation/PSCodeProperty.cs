// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSCodeProperty
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Reflection;
using System.Text;

namespace System.Management.Automation
{
  public class PSCodeProperty : PSPropertyInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private MethodInfo getterCodeReference;
    private MethodInfo setterCodeReference;

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.TypeNameOfValue);
      stringBuilder.Append(" ");
      stringBuilder.Append(this.Name);
      stringBuilder.Append("{");
      if (this.IsGettable)
      {
        stringBuilder.Append("get=");
        stringBuilder.Append(this.getterCodeReference.Name);
        stringBuilder.Append(";");
      }
      if (this.IsSettable)
      {
        stringBuilder.Append("set=");
        stringBuilder.Append(this.setterCodeReference.Name);
        stringBuilder.Append(";");
      }
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }

    internal void SetGetterFromTypeTable(Type type, string methodName)
    {
      MemberInfo[] member = type.GetMember(methodName, MemberTypes.Method, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public);
      if (member.Length != 1)
        throw new ExtendedTypeSystemException("GetterFormatFromTypeTable", (Exception) null, "ExtendedTypeSystem", "CodePropertyGetterFormat", new object[0]);
      this.SetGetter((MethodInfo) member[0]);
    }

    internal void SetSetterFromTypeTable(Type type, string methodName)
    {
      MemberInfo[] member = type.GetMember(methodName, MemberTypes.Method, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public);
      if (member.Length != 1)
        throw new ExtendedTypeSystemException("SetterFormatFromTypeTable", (Exception) null, "ExtendedTypeSystem", "CodePropertySetterFormat", new object[0]);
      this.SetSetter((MethodInfo) member[0], this.getterCodeReference);
    }

    internal void SetGetter(MethodInfo methodForGet)
    {
      if (methodForGet == null)
      {
        this.getterCodeReference = (MethodInfo) null;
      }
      else
      {
        ParameterInfo[] parameters = methodForGet.GetParameters();
        if (!methodForGet.IsPublic || !methodForGet.IsStatic || (methodForGet.ReturnType.Equals(typeof (void)) || parameters.Length != 1) || !parameters[0].ParameterType.Equals(typeof (PSObject)))
          throw new ExtendedTypeSystemException("GetterFormat", (Exception) null, "ExtendedTypeSystem", "CodePropertyGetterFormat", new object[0]);
        this.getterCodeReference = methodForGet;
      }
    }

    private void SetSetter(MethodInfo methodForSet, MethodInfo methodForGet)
    {
      if (methodForSet == null)
      {
        if (methodForGet == null)
          throw new ExtendedTypeSystemException("SetterAndGetterNullFormat", (Exception) null, "ExtendedTypeSystem", "CodePropertyGetterAndSetterNull", new object[0]);
        this.setterCodeReference = (MethodInfo) null;
      }
      else
      {
        ParameterInfo[] parameters = methodForSet.GetParameters();
        if (!methodForSet.IsPublic || !methodForSet.IsStatic || (!methodForSet.ReturnType.Equals(typeof (void)) || parameters.Length != 2) || (!parameters[0].ParameterType.Equals(typeof (PSObject)) || methodForGet != null && !methodForGet.ReturnType.Equals(parameters[1].ParameterType)))
          throw new ExtendedTypeSystemException("SetterFormat", (Exception) null, "ExtendedTypeSystem", "CodePropertySetterFormat", new object[0]);
        this.setterCodeReference = methodForSet;
      }
    }

    internal PSCodeProperty(string name) => this.name = !string.IsNullOrEmpty(name) ? name : throw PSCodeProperty.tracer.NewArgumentException(nameof (name));

    public PSCodeProperty(string name, MethodInfo getterCodeReference)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSCodeProperty.tracer.NewArgumentException(nameof (name));
      if (getterCodeReference == null)
        throw PSCodeProperty.tracer.NewArgumentNullException(nameof (getterCodeReference));
      this.SetGetter(getterCodeReference);
    }

    public PSCodeProperty(
      string name,
      MethodInfo getterCodeReference,
      MethodInfo setterCodeReference)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSCodeProperty.tracer.NewArgumentException(nameof (name));
      if (getterCodeReference == null && setterCodeReference == null)
        throw PSCodeProperty.tracer.NewArgumentNullException("getterCodeReference setterCodeReference");
      this.SetGetter(getterCodeReference);
      this.SetSetter(setterCodeReference, getterCodeReference);
    }

    public MethodInfo GetterCodeReference => this.getterCodeReference;

    public MethodInfo SetterCodeReference => this.setterCodeReference;

    public override PSMemberInfo Copy()
    {
      PSCodeProperty psCodeProperty = new PSCodeProperty(this.name, this.getterCodeReference, this.setterCodeReference);
      this.CloneBaseProperties((PSMemberInfo) psCodeProperty);
      return (PSMemberInfo) psCodeProperty;
    }

    public override PSMemberTypes MemberType => PSMemberTypes.CodeProperty;

    public override bool IsSettable => this.SetterCodeReference != null;

    public override bool IsGettable => this.getterCodeReference != null;

    public override object Value
    {
      get
      {
        if (this.getterCodeReference == null)
          throw new GetValueException("GetWithoutGetterFromCodePropertyValue", (Exception) null, "ExtendedTypeSystem", "GetWithoutGetterException", new object[1]
          {
            (object) this.Name
          });
        try
        {
          return this.getterCodeReference.Invoke((object) null, new object[1]
          {
            (object) this.instance
          });
        }
        catch (TargetInvocationException ex)
        {
          Exception innerException = ex.InnerException == null ? (Exception) ex : ex.InnerException;
          throw new GetValueInvocationException("CatchFromCodePropertyGetTI", innerException, "ExtendedTypeSystem", "ExceptionWhenGetting", new object[2]
          {
            (object) this.name,
            (object) innerException.Message
          });
        }
        catch (Exception ex)
        {
          if (ex is GetValueException)
          {
            throw;
          }
          else
          {
            CommandProcessorBase.CheckForSevereException(ex);
            throw new GetValueInvocationException("CatchFromCodePropertyGet", ex, "ExtendedTypeSystem", "ExceptionWhenGetting", new object[2]
            {
              (object) this.name,
              (object) ex.Message
            });
          }
        }
      }
      set
      {
        if (this.setterCodeReference == null)
          throw new SetValueException("SetWithoutSetterFromCodeProperty", (Exception) null, "ExtendedTypeSystem", "SetWithoutSetterException", new object[1]
          {
            (object) this.Name
          });
        try
        {
          this.setterCodeReference.Invoke((object) null, new object[2]
          {
            (object) this.instance,
            value
          });
        }
        catch (TargetInvocationException ex)
        {
          Exception innerException = ex.InnerException == null ? (Exception) ex : ex.InnerException;
          throw new SetValueInvocationException("CatchFromCodePropertySetTI", innerException, "ExtendedTypeSystem", "ExceptionWhenSetting", new object[2]
          {
            (object) this.name,
            (object) innerException.Message
          });
        }
        catch (Exception ex)
        {
          if (ex is SetValueException)
          {
            throw;
          }
          else
          {
            CommandProcessorBase.CheckForSevereException(ex);
            throw new SetValueInvocationException("CatchFromCodePropertySet", ex, "ExtendedTypeSystem", "ExceptionWhenSetting", new object[2]
            {
              (object) this.name,
              (object) ex.Message
            });
          }
        }
      }
    }

    public override string TypeNameOfValue
    {
      get
      {
        if (this.getterCodeReference == null)
          throw new GetValueException("GetWithoutGetterFromCodePropertyTypeOfValue", (Exception) null, "ExtendedTypeSystem", "GetWithoutGetterException", new object[1]
          {
            (object) this.Name
          });
        return this.getterCodeReference.ReturnType.FullName;
      }
    }
  }
}
