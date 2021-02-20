// Decompiled with JetBrains decompiler
// Type: Partial.PcId
// Assembly: PartialBeta, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9CF783A5-38FA-472F-8C51-9A2203433095
// Assembly location: E:\git\partialbits\PartialBeta.exe

using Microsoft.Win32;
using Partial.wslogger;
using System;
using System.IO;
using System.Management;
using System.Xml;

namespace Partial
{
  internal class PcId
  {
    private string sProduct;
    private string sSerialNumber;
    private string sUnid;
    private string sXmlDoc;
    private string sOffice;
    private string sDir;
    private string sCustomerName;
    private string sUserName;
    private XmlDocument xmldoc;
    public int IBatchNo;
    public bool bCust;
    public string sEmail;
    private StreamWriter sw;

    public PcId(string sVal, string snDir, string sval)
    {
      this.sUnid = sVal;
      this.sDir = snDir;
      this.sOffice = this.checkOffice();
      this.sEmail = sval;
      foreach (ManagementObject managementObject in new ManagementObjectSearcher("root\\cimv2", "SELECT Product, SerialNumber FROM Win32_BaseBoard").Get())
      {
        this.sProduct = Convert.ToString(managementObject["Product"]);
        this.sSerialNumber = Convert.ToString(managementObject["SerialNumber"]);
      }
      this.xmldoc = new XmlDocument();
      this.xmldoc.AppendChild((XmlNode) this.xmldoc.CreateXmlDeclaration("1.0", "UTF-8", "no"));
      XmlNode element1 = (XmlNode) this.xmldoc.CreateElement("PCID");
      this.xmldoc.AppendChild(element1);
      XmlNode element2 = (XmlNode) this.xmldoc.CreateElement("PC");
      XmlAttribute attribute1 = this.xmldoc.CreateAttribute("id");
      attribute1.Value = this.sSerialNumber;
      element2.Attributes.Append(attribute1);
      XmlAttribute attribute2 = this.xmldoc.CreateAttribute("unid");
      attribute2.Value = this.sUnid;
      element2.Attributes.Append(attribute2);
      XmlAttribute attribute3 = this.xmldoc.CreateAttribute("email");
      attribute3.Value = this.sEmail;
      element2.Attributes.Append(attribute3);
      XmlAttribute attribute4 = this.xmldoc.CreateAttribute("installtype");
      attribute4.Value = "partial";
      element2.Attributes.Append(attribute4);
      XmlAttribute attribute5 = this.xmldoc.CreateAttribute("product");
      attribute5.Value = this.sProduct;
      element2.Attributes.Append(attribute5);
      XmlAttribute attribute6 = this.xmldoc.CreateAttribute("os");
      attribute6.Value = Convert.ToString((object) Environment.OSVersion);
      element2.Attributes.Append(attribute6);
      XmlAttribute attribute7 = this.xmldoc.CreateAttribute("officeversion");
      attribute7.Value = Convert.ToString(this.sOffice);
      element2.Attributes.Append(attribute7);
      XmlAttribute attribute8 = this.xmldoc.CreateAttribute("targetdir");
      attribute8.Value = this.sDir;
      element2.Attributes.Append(attribute8);
      XmlAttribute attribute9 = this.xmldoc.CreateAttribute("systemname");
      attribute9.Value = Convert.ToString(Environment.MachineName);
      element2.Attributes.Append(attribute9);
      element1.AppendChild(element2);
      this.sXmlDoc = this.xmldoc.OuterXml;
    }

    public void setCust(string sCustval) => this.sCustomerName = sCustval;

    public void setUser(string sUserval) => this.sUserName = sUserval;

    public void setUnid(string sval) => this.sUnid = sval;

    public void LogEvent()
    {
      Ws_Logger wsLogger = new Ws_Logger();
      this.IBatchNo = -1;
      try
      {
        int iBatchid = int.Parse(wsLogger.FindNextId(this.sUnid));
        this.IBatchNo = iBatchid;
        wsLogger.Logger(this.sUnid, this.sXmlDoc, 99999, iBatchid);
      }
      catch (Exception ex)
      {
        throw ex;
      }
      finally
      {
        wsLogger.Dispose();
      }
    }

    public string PackXml(string sXml, string sUnid)
    {
      Ws_Logger wsLogger = new Ws_Logger();
      try
      {
        return wsLogger.RndCustXml(sXml, sUnid);
      }
      catch (Exception ex)
      {
        throw ex;
      }
      finally
      {
        wsLogger.Dispose();
      }
    }

    public string ChkInstall(string sUnid)
    {
      Ws_Logger wsLogger = new Ws_Logger();
      try
      {
        return !this.sEmail.Contains("cookconsulting") ? wsLogger.ChkInstall(sUnid) : "go";
      }
      catch (Exception ex)
      {
        throw ex;
      }
      finally
      {
        wsLogger.Dispose();
      }
    }

    public void Close()
    {
      try
      {
        this.sw.Flush();
        this.sw.Dispose();
      }
      catch
      {
      }
    }

    public void FileLog(string sUnid, string sMsg, int iStatus, int iBatch)
    {
    }

    public void CreateLogFile(string sPath)
    {
    }

    public void LogEvent(string sUnid, string sMsg, int iStatus, int iBatch)
    {
      Ws_Logger wsLogger = new Ws_Logger();
      if (iStatus == 10000)
        wsLogger.Logger(sUnid, this.sCustomerName + "&" + this.sUserName, iStatus, iBatch);
      else
        wsLogger.Logger(sUnid, sMsg, iStatus, iBatch);
      wsLogger.Dispose();
    }

    private string checkOffice()
    {
      string[] strArray = new string[2]
      {
        "Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\excel.exe",
        "Software\\Microsoft\\Windows\\CurrentVersion\\App Paths"
      };
      try
      {
        string upper = Convert.ToString((Registry.LocalMachine.OpenSubKey(strArray[0]) ?? Registry.CurrentUser.OpenSubKey(strArray[1])).GetValue("")).ToUpper();
        if (upper.Contains("14"))
          return "14";
        return upper.Contains("12") || upper.Contains("11") ? "12" : "Unknown Office Version";
      }
      catch
      {
        return "Unknown";
      }
    }
  }
}
