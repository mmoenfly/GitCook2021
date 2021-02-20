// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParameterCollectionTypeInformation
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Management.Automation
{
  internal class ParameterCollectionTypeInformation
  {
    [TraceSource("ParameterCollectionTypeInformation", "A class that wraps up the type information about a parameter")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ParameterCollectionTypeInformation), "A class that wraps up the type information about a parameter");
    private ParameterCollectionType parameterCollectionType;
    private Type elementType;

    internal ParameterCollectionTypeInformation(Type type)
    {
      if (type == null)
        throw ParameterCollectionTypeInformation.tracer.NewArgumentNullException(nameof (type));
      ParameterCollectionTypeInformation.tracer.WriteLine("Extracting collection type information for type: {0}", (object) type);
      if (type.IsSubclassOf(typeof (Array)))
      {
        this.parameterCollectionType = ParameterCollectionType.Array;
        this.elementType = type.GetElementType();
      }
      else
      {
        bool flag = type.GetInterface(typeof (IList).Name) != null;
        if (flag && type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Collection<>))
        {
          this.parameterCollectionType = ParameterCollectionType.IList;
          this.elementType = type.GetGenericArguments()[0];
        }
        else
        {
          Type[] interfaces = type.FindInterfaces(new TypeFilter(ParameterCollectionTypeInformation.filterImplementsGeneric), (object) typeof (ICollection<>));
          if (interfaces != null && interfaces.Length >= 1)
          {
            this.parameterCollectionType = ParameterCollectionType.ICollectionGeneric;
            this.elementType = interfaces[0].GetGenericArguments()[0];
          }
          else
          {
            if (!flag)
              return;
            this.parameterCollectionType = ParameterCollectionType.IList;
          }
        }
      }
    }

    public static bool filterImplementsGeneric(Type filter, object filterCriteria) => filter.IsGenericType && filter.GetGenericTypeDefinition() == (Type) filterCriteria;

    internal ParameterCollectionType ParameterCollectionType
    {
      get => this.parameterCollectionType;
      set => this.parameterCollectionType = value;
    }

    internal Type ElementType
    {
      get => this.elementType;
      set => this.elementType = value;
    }
  }
}
