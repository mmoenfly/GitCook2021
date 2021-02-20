// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.HostDefaultData
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Host;

namespace System.Management.Automation.Remoting
{
  internal class HostDefaultData
  {
    private Dictionary<HostDefaultDataId, object> data;

    private HostDefaultData() => this.data = new Dictionary<HostDefaultDataId, object>();

    internal object this[HostDefaultDataId id] => this.GetValue(id);

    internal bool HasValue(HostDefaultDataId id) => this.data.ContainsKey(id);

    internal void SetValue(HostDefaultDataId id, object dataValue) => this.data[id] = dataValue;

    internal object GetValue(HostDefaultDataId id) => this.data.ContainsKey(id) ? this.data[id] : (object) null;

    internal static HostDefaultData Create(PSHostRawUserInterface hostRawUI)
    {
      if (hostRawUI == null)
        return (HostDefaultData) null;
      HostDefaultData hostDefaultData = new HostDefaultData();
      try
      {
        hostDefaultData.SetValue(HostDefaultDataId.ForegroundColor, (object) hostRawUI.ForegroundColor);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      try
      {
        hostDefaultData.SetValue(HostDefaultDataId.BackgroundColor, (object) hostRawUI.BackgroundColor);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      try
      {
        hostDefaultData.SetValue(HostDefaultDataId.CursorPosition, (object) hostRawUI.CursorPosition);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      try
      {
        hostDefaultData.SetValue(HostDefaultDataId.WindowPosition, (object) hostRawUI.WindowPosition);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      try
      {
        hostDefaultData.SetValue(HostDefaultDataId.CursorSize, (object) hostRawUI.CursorSize);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      try
      {
        hostDefaultData.SetValue(HostDefaultDataId.BufferSize, (object) hostRawUI.BufferSize);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      try
      {
        hostDefaultData.SetValue(HostDefaultDataId.WindowSize, (object) hostRawUI.WindowSize);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      try
      {
        hostDefaultData.SetValue(HostDefaultDataId.MaxWindowSize, (object) hostRawUI.MaxWindowSize);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      try
      {
        hostDefaultData.SetValue(HostDefaultDataId.MaxPhysicalWindowSize, (object) hostRawUI.MaxPhysicalWindowSize);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      try
      {
        hostDefaultData.SetValue(HostDefaultDataId.WindowTitle, (object) hostRawUI.WindowTitle);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      return hostDefaultData;
    }
  }
}
