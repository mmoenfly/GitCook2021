// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DataRowAdapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Data;

namespace System.Management.Automation
{
  internal class DataRowAdapter : PropertyOnlyAdapter
  {
    protected override void DoAddAllProperties<T>(
      object obj,
      PSMemberInfoInternalCollection<T> members)
    {
      DataRow dataRow = (DataRow) obj;
      if (dataRow.Table == null || dataRow.Table.Columns == null)
        return;
      foreach (DataColumn column in (InternalDataCollectionBase) dataRow.Table.Columns)
        members.Add(new PSProperty(column.ColumnName, (Adapter) this, obj, (object) column.ColumnName) as T);
    }

    protected override PSProperty DoGetProperty(object obj, string propertyName)
    {
      DataRow dataRow = (DataRow) obj;
      if (!dataRow.Table.Columns.Contains(propertyName))
        return (PSProperty) null;
      string columnName = dataRow.Table.Columns[propertyName].ColumnName;
      return new PSProperty(columnName, (Adapter) this, obj, (object) columnName);
    }

    protected override string PropertyType(PSProperty property)
    {
      string adapterData = (string) property.adapterData;
      return ((DataRow) property.baseObject).Table.Columns[adapterData].DataType.FullName;
    }

    protected override bool PropertyIsSettable(PSProperty property)
    {
      string adapterData = (string) property.adapterData;
      return !((DataRow) property.baseObject).Table.Columns[adapterData].ReadOnly;
    }

    protected override bool PropertyIsGettable(PSProperty property) => true;

    protected override object PropertyGet(PSProperty property) => ((DataRow) property.baseObject)[(string) property.adapterData];

    protected override void PropertySet(
      PSProperty property,
      object setValue,
      bool convertIfPossible)
    {
      ((DataRow) property.baseObject)[(string) property.adapterData] = setValue;
    }
  }
}
