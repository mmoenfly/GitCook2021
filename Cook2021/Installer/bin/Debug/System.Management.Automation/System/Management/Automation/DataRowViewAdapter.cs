// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DataRowViewAdapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Data;

namespace System.Management.Automation
{
  internal class DataRowViewAdapter : PropertyOnlyAdapter
  {
    protected override void DoAddAllProperties<T>(
      object obj,
      PSMemberInfoInternalCollection<T> members)
    {
      DataRowView dataRowView = (DataRowView) obj;
      if (dataRowView.Row == null || dataRowView.Row.Table == null || dataRowView.Row.Table.Columns == null)
        return;
      foreach (DataColumn column in (InternalDataCollectionBase) dataRowView.Row.Table.Columns)
        members.Add(new PSProperty(column.ColumnName, (Adapter) this, obj, (object) column.ColumnName) as T);
    }

    protected override PSProperty DoGetProperty(object obj, string propertyName)
    {
      DataRowView dataRowView = (DataRowView) obj;
      if (!dataRowView.Row.Table.Columns.Contains(propertyName))
        return (PSProperty) null;
      string columnName = dataRowView.Row.Table.Columns[propertyName].ColumnName;
      return new PSProperty(columnName, (Adapter) this, obj, (object) columnName);
    }

    protected override string PropertyType(PSProperty property)
    {
      string adapterData = (string) property.adapterData;
      return ((DataRowView) property.baseObject).Row.Table.Columns[adapterData].DataType.FullName;
    }

    protected override bool PropertyIsSettable(PSProperty property)
    {
      string adapterData = (string) property.adapterData;
      return !((DataRowView) property.baseObject).Row.Table.Columns[adapterData].ReadOnly;
    }

    protected override bool PropertyIsGettable(PSProperty property) => true;

    protected override object PropertyGet(PSProperty property) => ((DataRowView) property.baseObject)[(string) property.adapterData];

    protected override void PropertySet(
      PSProperty property,
      object setValue,
      bool convertIfPossible)
    {
      ((DataRowView) property.baseObject)[(string) property.adapterData] = setValue;
    }
  }
}
