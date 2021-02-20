// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.TypeSerializationInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class TypeSerializationInfo
  {
    private Type _type;
    private readonly string _itemTag;
    private readonly string _propertyTag;
    private TypeSerializerDelegate _serializer;
    private TypeDeserializerDelegate _deserializer;

    internal TypeSerializationInfo(
      Type type,
      string itemTag,
      string propertyTag,
      TypeSerializerDelegate serializer,
      TypeDeserializerDelegate deserializer)
    {
      this._type = type;
      this._serializer = serializer;
      this._deserializer = deserializer;
      this._itemTag = itemTag;
      this._propertyTag = propertyTag;
    }

    internal Type Type => this._type;

    internal string ItemTag => this._itemTag;

    internal string PropertyTag => this._propertyTag;

    internal TypeSerializerDelegate Serializer => this._serializer;

    internal TypeDeserializerDelegate Deserializer => this._deserializer;
  }
}
