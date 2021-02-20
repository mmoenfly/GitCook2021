// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.DeserializingTypeConverter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.PowerShell
{
  public sealed class DeserializingTypeConverter : PSTypeConverter
  {
    private static Dictionary<Type, Converter<PSObject, object>> converter;
    [TraceSource("DeserializingTypeConverter", "DeserializingTypeConverter class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (DeserializingTypeConverter), "DeserializingTypeConverter class");

    static DeserializingTypeConverter()
    {
      DeserializingTypeConverter.converter = new Dictionary<Type, Converter<PSObject, object>>();
      DeserializingTypeConverter.converter.Add(typeof (PSPrimitiveDictionary), new Converter<PSObject, object>(DeserializingTypeConverter.RehydratePrimitiveHashtable));
      DeserializingTypeConverter.converter.Add(typeof (SwitchParameter), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateSwitchParameter));
      DeserializingTypeConverter.converter.Add(typeof (PSListModifier), new Converter<PSObject, object>(DeserializingTypeConverter.RehydratePSListModifier));
      DeserializingTypeConverter.converter.Add(typeof (PSCredential), new Converter<PSObject, object>(DeserializingTypeConverter.RehydratePSCredential));
      DeserializingTypeConverter.converter.Add(typeof (IPAddress), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateIPAddress));
      DeserializingTypeConverter.converter.Add(typeof (MailAddress), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateMailAddress));
      DeserializingTypeConverter.converter.Add(typeof (CultureInfo), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateCultureInfo));
      DeserializingTypeConverter.converter.Add(typeof (X509Certificate2), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateX509Certificate2));
      DeserializingTypeConverter.converter.Add(typeof (X500DistinguishedName), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateX500DistinguishedName));
      DeserializingTypeConverter.converter.Add(typeof (DirectorySecurity), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateObjectSecurity<DirectorySecurity>));
      DeserializingTypeConverter.converter.Add(typeof (FileSecurity), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateObjectSecurity<FileSecurity>));
      DeserializingTypeConverter.converter.Add(typeof (RegistrySecurity), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateObjectSecurity<RegistrySecurity>));
      DeserializingTypeConverter.converter.Add(typeof (ParameterSetMetadata), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateParameterSetMetadata));
      DeserializingTypeConverter.converter.Add(typeof (ExtendedTypeDefinition), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateExtendedTypeDefinition));
      DeserializingTypeConverter.converter.Add(typeof (FormatViewDefinition), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateFormatViewDefinition));
      DeserializingTypeConverter.converter.Add(typeof (PSControl), new Converter<PSObject, object>(DeserializingTypeConverter.RehydratePSControl));
      DeserializingTypeConverter.converter.Add(typeof (DisplayEntry), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateDisplayEntry));
      DeserializingTypeConverter.converter.Add(typeof (TableControlColumnHeader), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateTableControlColumnHeader));
      DeserializingTypeConverter.converter.Add(typeof (TableControlRow), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateTableControlRow));
      DeserializingTypeConverter.converter.Add(typeof (TableControlColumn), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateTableControlColumn));
      DeserializingTypeConverter.converter.Add(typeof (ListControlEntry), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateListControlEntry));
      DeserializingTypeConverter.converter.Add(typeof (ListControlEntryItem), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateListControlEntryItem));
      DeserializingTypeConverter.converter.Add(typeof (WideControlEntryItem), new Converter<PSObject, object>(DeserializingTypeConverter.RehydrateWideControlEntryItem));
    }

    public override bool CanConvertFrom(PSObject sourceValue, Type destinationType)
    {
      foreach (Type key in DeserializingTypeConverter.converter.Keys)
      {
        if (Deserializer.IsDeserializedInstanceOfType((object) sourceValue, key))
          return true;
      }
      return false;
    }

    public override object ConvertFrom(
      PSObject sourceValue,
      Type destinationType,
      IFormatProvider formatProvider,
      bool ignoreCase)
    {
      if (destinationType == null)
        throw DeserializingTypeConverter._trace.NewArgumentNullException(nameof (destinationType));
      if (sourceValue == null)
        throw new PSInvalidCastException("InvalidCastWhenRehydratingFromNull", (Exception) DeserializingTypeConverter._trace.NewArgumentNullException(nameof (sourceValue)), "ExtendedTypeSystem", "InvalidCastFromNull", new object[1]
        {
          (object) destinationType.ToString()
        });
      foreach (KeyValuePair<Type, Converter<PSObject, object>> keyValuePair in DeserializingTypeConverter.converter)
      {
        Type key = keyValuePair.Key;
        Converter<PSObject, object> converter = keyValuePair.Value;
        if (Deserializer.IsDeserializedInstanceOfType((object) sourceValue, key))
          return this.ConvertFrom(sourceValue, converter);
      }
      throw new PSInvalidCastException("InvalidCastEnumFromTypeNotAString", (Exception) null, "ExtendedTypeSystem", "InvalidCastException", new object[2]
      {
        (object) sourceValue,
        (object) destinationType
      });
    }

    private object ConvertFrom(PSObject o, Converter<PSObject, object> converter)
    {
      PSObject input = o;
      object obj = converter(input);
      bool flag = false;
      PSObject psObject = PSObject.AsPSObject(obj);
      foreach (PSMemberInfo instanceMember in (PSMemberInfoCollection<PSMemberInfo>) input.InstanceMembers)
      {
        if (instanceMember.MemberType == (instanceMember.MemberType & (PSMemberTypes.Properties | PSMemberTypes.PropertySet | PSMemberTypes.MemberSet)) && psObject.Members[instanceMember.Name] == null)
        {
          psObject.InstanceMembers.Add(instanceMember);
          flag = true;
        }
      }
      return flag ? (object) psObject : obj;
    }

    public override bool CanConvertTo(object sourceValue, Type destinationType) => false;

    public override object ConvertTo(
      object sourceValue,
      Type destinationType,
      IFormatProvider formatProvider,
      bool ignoreCase)
    {
      throw DeserializingTypeConverter._trace.NewNotSupportedException();
    }

    public override bool CanConvertFrom(object sourceValue, Type destinationType) => throw new NotImplementedException();

    public override bool CanConvertTo(PSObject sourceValue, Type destinationType) => throw new NotImplementedException();

    public override object ConvertFrom(
      object sourceValue,
      Type destinationType,
      IFormatProvider formatProvider,
      bool ignoreCase)
    {
      throw new NotImplementedException();
    }

    public override object ConvertTo(
      PSObject sourceValue,
      Type destinationType,
      IFormatProvider formatProvider,
      bool ignoreCase)
    {
      throw new NotImplementedException();
    }

    private static T GetPropertyValue<T>(PSObject pso, string propertyName) => DeserializingTypeConverter.GetPropertyValue<T>(pso, propertyName, DeserializingTypeConverter.RehydrationFlags.NullValueBad);

    private static T GetPropertyValue<T>(
      PSObject pso,
      string propertyName,
      DeserializingTypeConverter.RehydrationFlags flags)
    {
      PSPropertyInfo property = pso.Properties[propertyName];
      if (property == null && DeserializingTypeConverter.RehydrationFlags.MissingPropertyOk == (flags & DeserializingTypeConverter.RehydrationFlags.MissingPropertyOk))
        return default (T);
      object valueToConvert = property.Value;
      return valueToConvert == null && DeserializingTypeConverter.RehydrationFlags.NullValueOk == (flags & DeserializingTypeConverter.RehydrationFlags.NullValueOk) ? default (T) : (T) LanguagePrimitives.ConvertTo(valueToConvert, typeof (T), (IFormatProvider) CultureInfo.InvariantCulture);
    }

    private static ListType RehydrateList<ListType, ItemType>(
      PSObject pso,
      string propertyName,
      DeserializingTypeConverter.RehydrationFlags flags)
      where ListType : IList, new()
    {
      ArrayList propertyValue = DeserializingTypeConverter.GetPropertyValue<ArrayList>(pso, propertyName, flags);
      if (propertyValue == null)
      {
        if (DeserializingTypeConverter.RehydrationFlags.NullValueMeansEmptyList != (flags & DeserializingTypeConverter.RehydrationFlags.NullValueMeansEmptyList))
          return default (ListType);
        return new ListType();
      }
      ListType listType = new ListType();
      foreach (object valueToConvert in propertyValue)
      {
        ItemType itemType = (ItemType) LanguagePrimitives.ConvertTo(valueToConvert, typeof (ItemType), (IFormatProvider) CultureInfo.InvariantCulture);
        listType.Add((object) itemType);
      }
      return listType;
    }

    private static object RehydratePrimitiveHashtable(PSObject pso) => (object) new PSPrimitiveDictionary((Hashtable) LanguagePrimitives.ConvertTo((object) pso, typeof (Hashtable), (IFormatProvider) CultureInfo.InvariantCulture));

    private static object RehydrateSwitchParameter(PSObject pso) => (object) DeserializingTypeConverter.GetPropertyValue<SwitchParameter>(pso, "IsPresent");

    private static IPAddress RehydrateIPAddress(PSObject pso) => IPAddress.Parse(pso.ToString());

    private static MailAddress RehydrateMailAddress(PSObject pso) => new MailAddress(pso.ToString());

    private static CultureInfo RehydrateCultureInfo(PSObject pso) => new CultureInfo(pso.ToString());

    private static PSListModifier RehydratePSListModifier(PSObject pso)
    {
      Hashtable hash = new Hashtable();
      PSPropertyInfo property1 = pso.Properties["Add"];
      if (property1 != null && property1.Value != null)
        hash.Add((object) "Add", property1.Value);
      PSPropertyInfo property2 = pso.Properties["Remove"];
      if (property2 != null && property2.Value != null)
        hash.Add((object) "Remove", property2.Value);
      PSPropertyInfo property3 = pso.Properties["Replace"];
      if (property3 != null && property3.Value != null)
        hash.Add((object) "Replace", property3.Value);
      return new PSListModifier(hash);
    }

    private static PSCredential RehydratePSCredential(PSObject pso) => new PSCredential(DeserializingTypeConverter.GetPropertyValue<string>(pso, "UserName"), DeserializingTypeConverter.GetPropertyValue<SecureString>(pso, "Password"));

    private static X509Certificate2 RehydrateX509Certificate2(PSObject pso) => new X509Certificate2(DeserializingTypeConverter.GetPropertyValue<byte[]>(pso, "RawData"));

    private static X500DistinguishedName RehydrateX500DistinguishedName(
      PSObject pso)
    {
      return new X500DistinguishedName(DeserializingTypeConverter.GetPropertyValue<byte[]>(pso, "RawData"));
    }

    private static T RehydrateObjectSecurity<T>(PSObject pso) where T : ObjectSecurity, new()
    {
      string propertyValue = DeserializingTypeConverter.GetPropertyValue<string>(pso, "SDDL");
      T obj = new T();
      obj.SetSecurityDescriptorSddlForm(propertyValue);
      return obj;
    }

    public static uint GetParameterSetMetadataFlags(PSObject instance)
    {
      if (instance == null)
        throw DeserializingTypeConverter._trace.NewArgumentNullException(nameof (instance));
      if (!(instance.BaseObject is ParameterSetMetadata baseObject))
        throw DeserializingTypeConverter._trace.NewArgumentNullException(nameof (instance));
      return (uint) baseObject.Flags;
    }

    private static ParameterSetMetadata RehydrateParameterSetMetadata(
      PSObject pso)
    {
      return new ParameterSetMetadata(DeserializingTypeConverter.GetPropertyValue<int>(pso, "Position"), (ParameterSetMetadata.ParameterFlags) DeserializingTypeConverter.GetPropertyValue<uint>(pso, "Flags"), DeserializingTypeConverter.GetPropertyValue<string>(pso, "HelpMessage"));
    }

    private static DisplayEntry RehydrateDisplayEntry(PSObject deserializedDisplayEntry) => new DisplayEntry()
    {
      Value = DeserializingTypeConverter.GetPropertyValue<string>(deserializedDisplayEntry, "Value"),
      ValueType = DeserializingTypeConverter.GetPropertyValue<DisplayEntryValueType>(deserializedDisplayEntry, "ValueType")
    };

    private static WideControlEntryItem RehydrateWideControlEntryItem(
      PSObject deserializedEntryItem)
    {
      return new WideControlEntryItem()
      {
        DisplayEntry = DeserializingTypeConverter.GetPropertyValue<DisplayEntry>(deserializedEntryItem, "DisplayEntry"),
        SelectedBy = DeserializingTypeConverter.RehydrateList<List<string>, string>(deserializedEntryItem, "SelectedBy", DeserializingTypeConverter.RehydrationFlags.NullValueOk)
      };
    }

    private static ListControlEntryItem RehydrateListControlEntryItem(
      PSObject deserializedEntryItem)
    {
      return new ListControlEntryItem()
      {
        DisplayEntry = DeserializingTypeConverter.GetPropertyValue<DisplayEntry>(deserializedEntryItem, "DisplayEntry"),
        Label = DeserializingTypeConverter.GetPropertyValue<string>(deserializedEntryItem, "Label", DeserializingTypeConverter.RehydrationFlags.NullValueOk)
      };
    }

    private static ListControlEntry RehydrateListControlEntry(
      PSObject deserializedEntry)
    {
      return new ListControlEntry()
      {
        Items = DeserializingTypeConverter.RehydrateList<List<ListControlEntryItem>, ListControlEntryItem>(deserializedEntry, "Items", DeserializingTypeConverter.RehydrationFlags.NullValueBad),
        SelectedBy = DeserializingTypeConverter.RehydrateList<List<string>, string>(deserializedEntry, "SelectedBy", DeserializingTypeConverter.RehydrationFlags.NullValueOk)
      };
    }

    private static TableControlColumnHeader RehydrateTableControlColumnHeader(
      PSObject deserializedHeader)
    {
      return new TableControlColumnHeader()
      {
        Alignment = DeserializingTypeConverter.GetPropertyValue<Alignment>(deserializedHeader, "Alignment"),
        Label = DeserializingTypeConverter.GetPropertyValue<string>(deserializedHeader, "Label", DeserializingTypeConverter.RehydrationFlags.NullValueOk),
        Width = DeserializingTypeConverter.GetPropertyValue<int>(deserializedHeader, "Width")
      };
    }

    private static TableControlColumn RehydrateTableControlColumn(
      PSObject deserializedColumn)
    {
      return new TableControlColumn()
      {
        Alignment = DeserializingTypeConverter.GetPropertyValue<Alignment>(deserializedColumn, "Alignment"),
        DisplayEntry = DeserializingTypeConverter.GetPropertyValue<DisplayEntry>(deserializedColumn, "DisplayEntry")
      };
    }

    private static TableControlRow RehydrateTableControlRow(PSObject deserializedRow) => new TableControlRow()
    {
      Columns = DeserializingTypeConverter.RehydrateList<List<TableControlColumn>, TableControlColumn>(deserializedRow, "Columns", DeserializingTypeConverter.RehydrationFlags.NullValueBad)
    };

    private static PSControl RehydratePSControl(PSObject deserializedControl)
    {
      if (Deserializer.IsDeserializedInstanceOfType((object) deserializedControl, typeof (TableControl)))
        return (PSControl) new TableControl()
        {
          Headers = DeserializingTypeConverter.RehydrateList<List<TableControlColumnHeader>, TableControlColumnHeader>(deserializedControl, "Headers", DeserializingTypeConverter.RehydrationFlags.NullValueBad),
          Rows = DeserializingTypeConverter.RehydrateList<List<TableControlRow>, TableControlRow>(deserializedControl, "Rows", DeserializingTypeConverter.RehydrationFlags.NullValueBad)
        };
      if (Deserializer.IsDeserializedInstanceOfType((object) deserializedControl, typeof (ListControl)))
        return (PSControl) new ListControl()
        {
          Entries = DeserializingTypeConverter.RehydrateList<List<ListControlEntry>, ListControlEntry>(deserializedControl, "Entries", DeserializingTypeConverter.RehydrationFlags.NullValueBad)
        };
      return Deserializer.IsDeserializedInstanceOfType((object) deserializedControl, typeof (WideControl)) ? (PSControl) new WideControl()
      {
        Alignment = DeserializingTypeConverter.GetPropertyValue<Alignment>(deserializedControl, "Alignment"),
        Columns = DeserializingTypeConverter.GetPropertyValue<uint>(deserializedControl, "Columns"),
        Entries = DeserializingTypeConverter.RehydrateList<List<WideControlEntryItem>, WideControlEntryItem>(deserializedControl, "Entries", DeserializingTypeConverter.RehydrationFlags.NullValueBad)
      } : throw DeserializingTypeConverter._trace.NewArgumentException("pso");
    }

    public static Guid GetFormatViewDefinitionInstanceId(PSObject instance)
    {
      if (instance == null)
        throw DeserializingTypeConverter._trace.NewArgumentNullException(nameof (instance));
      if (!(instance.BaseObject is FormatViewDefinition baseObject))
        throw DeserializingTypeConverter._trace.NewArgumentNullException(nameof (instance));
      return baseObject.InstanceId;
    }

    private static FormatViewDefinition RehydrateFormatViewDefinition(
      PSObject deserializedViewDefinition)
    {
      string propertyValue1 = DeserializingTypeConverter.GetPropertyValue<string>(deserializedViewDefinition, "Name");
      Guid propertyValue2 = DeserializingTypeConverter.GetPropertyValue<Guid>(deserializedViewDefinition, "InstanceId");
      PSControl propertyValue3 = DeserializingTypeConverter.GetPropertyValue<PSControl>(deserializedViewDefinition, "Control");
      return new FormatViewDefinition(propertyValue1, propertyValue3, propertyValue2);
    }

    private static ExtendedTypeDefinition RehydrateExtendedTypeDefinition(
      PSObject deserializedTypeDefinition)
    {
      return new ExtendedTypeDefinition(DeserializingTypeConverter.GetPropertyValue<string>(deserializedTypeDefinition, "TypeName"), DeserializingTypeConverter.RehydrateList<List<FormatViewDefinition>, FormatViewDefinition>(deserializedTypeDefinition, "FormatViewDefinition", DeserializingTypeConverter.RehydrationFlags.NullValueBad));
    }

    [System.Flags]
    private enum RehydrationFlags
    {
      NullValueBad = 0,
      NullValueOk = 1,
      NullValueMeansEmptyList = 3,
      MissingPropertyBad = 0,
      MissingPropertyOk = 4,
    }
  }
}
