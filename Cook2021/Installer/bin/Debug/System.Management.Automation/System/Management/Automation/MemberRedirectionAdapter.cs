// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MemberRedirectionAdapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace System.Management.Automation
{
  internal abstract class MemberRedirectionAdapter : Adapter
  {
    internal MemberRedirectionAdapter()
    {
    }

    protected override AttributeCollection PropertyAttributes(PSProperty property) => new AttributeCollection(new Attribute[0]);

    protected override object PropertyGet(PSProperty property) => throw Adapter.tracer.NewNotSupportedException();

    protected override void PropertySet(
      PSProperty property,
      object setValue,
      bool convertIfPossible)
    {
      throw Adapter.tracer.NewNotSupportedException();
    }

    protected override bool PropertyIsSettable(PSProperty property) => throw Adapter.tracer.NewNotSupportedException();

    protected override bool PropertyIsGettable(PSProperty property) => throw Adapter.tracer.NewNotSupportedException();

    protected override string PropertyType(PSProperty property) => throw Adapter.tracer.NewNotSupportedException();

    protected override string PropertyToString(PSProperty property) => throw Adapter.tracer.NewNotSupportedException();

    protected override object MethodInvoke(PSMethod method, object[] arguments) => throw Adapter.tracer.NewNotSupportedException();

    protected override Collection<string> MethodDefinitions(PSMethod method) => throw Adapter.tracer.NewNotSupportedException();
  }
}
