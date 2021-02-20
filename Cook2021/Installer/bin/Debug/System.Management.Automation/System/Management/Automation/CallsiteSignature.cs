// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CallsiteSignature
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class CallsiteSignature : IEquatable<CallsiteSignature>
  {
    private Type targetType;
    private Type[] argumentTypes;
    private CallsiteCacheEntryFlags flags;

    internal CallsiteSignature(
      Type targetType,
      Type[] argumentTypes,
      CallsiteCacheEntryFlags flags)
    {
      this.targetType = targetType;
      this.argumentTypes = argumentTypes;
      this.flags = flags;
    }

    internal CallsiteSignature(Type targetType, object[] arguments, CallsiteCacheEntryFlags flags)
      : this(targetType, new Type[arguments.Length], flags)
    {
      for (int index = 0; index < arguments.Length; ++index)
        this.argumentTypes[index] = Adapter.EffectiveArgumentType(arguments[index]);
    }

    public override int GetHashCode()
    {
      int num = (int) ((CallsiteCacheEntryFlags) this.targetType.GetHashCode() ^ this.flags);
      for (int index = 0; index < this.argumentTypes.Length; ++index)
        num += this.argumentTypes[index].GetHashCode() << index;
      return num;
    }

    public bool Equals(CallsiteSignature other)
    {
      if (this.targetType != other.targetType || this.flags != other.flags || this.argumentTypes.Length != other.argumentTypes.Length)
        return false;
      for (int index = 0; index < this.argumentTypes.Length; ++index)
      {
        if (this.argumentTypes[index] != other.argumentTypes[index])
          return false;
      }
      return true;
    }

    public override bool Equals(object other) => other is CallsiteSignature other1 && this.Equals(other1);
  }
}
