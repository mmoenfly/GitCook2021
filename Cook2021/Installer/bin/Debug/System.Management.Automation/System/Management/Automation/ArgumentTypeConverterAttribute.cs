// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ArgumentTypeConverterAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace System.Management.Automation
{
  internal sealed class ArgumentTypeConverterAttribute : ArgumentTransformationAttribute
  {
    private Type[] _convertTypes;

    internal ArgumentTypeConverterAttribute(List<TypeLiteral> typeLiterals)
    {
      if (typeLiterals == null)
        return;
      this._convertTypes = new Type[typeLiterals.Count];
      int num = 0;
      foreach (TypeLiteral typeLiteral in typeLiterals)
        this._convertTypes[num++] = typeLiteral.Type;
    }

    internal ArgumentTypeConverterAttribute(params Type[] types) => this._convertTypes = types;

    public override object Transform(EngineIntrinsics engineIntrinsics, object inputData) => this.Transform(engineIntrinsics, inputData, false, false);

    internal object Transform(
      EngineIntrinsics engineIntrinsics,
      object inputData,
      bool bindingParameters,
      bool bindingScriptCmdlet)
    {
      if (this._convertTypes == null)
        return inputData;
      object valueToConvert = inputData;
      try
      {
        for (int index = 0; index < this._convertTypes.Length; ++index)
        {
          if (bindingParameters)
          {
            if (this._convertTypes[index].Equals(typeof (PSReference)))
            {
              if (!((!(valueToConvert is PSObject psObject) ? valueToConvert : psObject.BaseObject) is PSReference))
                throw new PSInvalidCastException("InvalidCastExceptionReferenceTypeExpected", (Exception) null, "ExtendedTypeSystem", "ReferenceTypeExpected", new object[0]);
            }
            else
            {
              if ((!(valueToConvert is PSObject psObject) ? valueToConvert : psObject.BaseObject) is PSReference psReference)
                valueToConvert = psReference.Value;
              if (bindingScriptCmdlet && this._convertTypes[index] == typeof (string))
              {
                object obj = PSObject.Base(valueToConvert);
                if (obj != null && obj.GetType().IsArray)
                  throw new PSInvalidCastException("InvalidCastFromAnyTypeToString", (Exception) null, "ExtendedTypeSystem", "InvalidCastCannotRetrieveString", new object[0]);
              }
            }
          }
          if (LanguagePrimitives.IsBoolOrSwitchParameterType(this._convertTypes[index]))
            ArgumentTypeConverterAttribute.CheckBoolValue(valueToConvert, this._convertTypes[index]);
          if (bindingScriptCmdlet)
          {
            ParameterCollectionTypeInformation collectionTypeInformation = new ParameterCollectionTypeInformation(this._convertTypes[index]);
            if (collectionTypeInformation.ParameterCollectionType != ParameterCollectionType.NotCollection && LanguagePrimitives.IsBoolOrSwitchParameterType(collectionTypeInformation.ElementType))
            {
              IList ilist = ParameterBinderBase.GetIList(valueToConvert);
              if (ilist != null)
              {
                foreach (object obj in (IEnumerable) ilist)
                  ArgumentTypeConverterAttribute.CheckBoolValue(obj, collectionTypeInformation.ElementType);
              }
              else
                ArgumentTypeConverterAttribute.CheckBoolValue(valueToConvert, collectionTypeInformation.ElementType);
            }
          }
          valueToConvert = LanguagePrimitives.ConvertTo(valueToConvert, this._convertTypes[index], (IFormatProvider) CultureInfo.InvariantCulture);
        }
      }
      catch (PSInvalidCastException ex)
      {
        throw new ArgumentTransformationMetadataException(ex.Message, (Exception) ex);
      }
      return valueToConvert;
    }

    private static void CheckBoolValue(object value, Type boolType)
    {
      if (value != null)
      {
        Type type = value.GetType();
        if (type == typeof (PSObject))
          type = ((PSObject) value).BaseObject.GetType();
        if (LanguagePrimitives.IsNumeric(Type.GetTypeCode(type)) || LanguagePrimitives.IsBoolOrSwitchParameterType(type))
          return;
        ArgumentTypeConverterAttribute.ThrowPSInvalidBooleanArgumentCastException(type, boolType);
      }
      else
      {
        if (boolType.IsGenericType && boolType.GetGenericTypeDefinition() == typeof (Nullable<>) || !LanguagePrimitives.IsBooleanType(boolType))
          return;
        ArgumentTypeConverterAttribute.ThrowPSInvalidBooleanArgumentCastException((Type) null, boolType);
      }
    }

    internal static void ThrowPSInvalidBooleanArgumentCastException(
      Type resultType,
      Type convertType)
    {
      throw new PSInvalidCastException("InvalidCastExceptionUnsupportedParameterType", (Exception) null, "ExtendedTypeSystem", "InvalidCastExceptionForBooleanArgumentValue", new object[2]
      {
        (object) resultType,
        (object) convertType
      });
    }
  }
}
