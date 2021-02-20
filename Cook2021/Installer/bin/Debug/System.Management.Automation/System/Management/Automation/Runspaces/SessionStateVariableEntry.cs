// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.SessionStateVariableEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
  public sealed class SessionStateVariableEntry : ConstrainedSessionStateEntry
  {
    private object _value;
    private string _description = string.Empty;
    private ScopedItemOptions _options;
    private Collection<Attribute> _attributes;

    public SessionStateVariableEntry(string name, object value, string description)
      : base(name, SessionStateEntryVisibility.Public)
    {
      this._value = value;
      this._description = description;
    }

    public SessionStateVariableEntry(
      string name,
      object value,
      string description,
      ScopedItemOptions options)
      : base(name, SessionStateEntryVisibility.Public)
    {
      this._value = value;
      this._description = description;
      this._options = options;
    }

    public SessionStateVariableEntry(
      string name,
      object value,
      string description,
      ScopedItemOptions options,
      Collection<Attribute> attributes)
      : base(name, SessionStateEntryVisibility.Public)
    {
      this._value = value;
      this._description = description;
      this._options = options;
      this._attributes = attributes;
    }

    public SessionStateVariableEntry(
      string name,
      object value,
      string description,
      ScopedItemOptions options,
      Attribute attribute)
      : base(name, SessionStateEntryVisibility.Public)
    {
      this._value = value;
      this._description = description;
      this._options = options;
      this._attributes = new Collection<Attribute>();
      this._attributes.Add(attribute);
    }

    internal SessionStateVariableEntry(
      string name,
      object value,
      string description,
      ScopedItemOptions options,
      Collection<Attribute> attributes,
      SessionStateEntryVisibility visibility)
      : base(name, visibility)
    {
      this._value = value;
      this._description = description;
      this._options = options;
      this._attributes = new Collection<Attribute>();
      this._attributes = attributes;
    }

    public override InitialSessionStateEntry Clone()
    {
      Collection<Attribute> attributes = (Collection<Attribute>) null;
      if (this._attributes != null && this._attributes.Count > 0)
        attributes = new Collection<Attribute>((IList<Attribute>) this._attributes);
      return (InitialSessionStateEntry) new SessionStateVariableEntry(this.Name, this._value, this._description, this._options, attributes, this.Visibility);
    }

    public object Value => this._value;

    public string Description => this._description;

    public ScopedItemOptions Options => this._options;

    public Collection<Attribute> Attributes
    {
      get
      {
        if (this._attributes == null)
          this._attributes = new Collection<Attribute>();
        return this._attributes;
      }
    }
  }
}
