// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ExceptionTypeList
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  internal sealed class ExceptionTypeList : Collection<TypeLiteral>
  {
    internal static bool CompareEqual(Type t1, Type t2) => t1.Equals(t2);

    internal static bool CompareSubclass(Type t1, Type t2) => t1.IsSubclassOf(t2);

    internal bool Matches(ExceptionTypeList.Comparer comparer, Type type, out Type matchedType)
    {
      matchedType = (Type) null;
      foreach (TypeLiteral typeLiteral in (Collection<TypeLiteral>) this)
      {
        if (comparer(type, typeLiteral.Type))
        {
          matchedType = typeLiteral.Type;
          return true;
        }
      }
      return false;
    }

    internal delegate bool Comparer(Type t1, Type t2);
  }
}
