// Decompiled with JetBrains decompiler
// Type: Partial.DocDetails
// Assembly: PartialBeta, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9CF783A5-38FA-472F-8C51-9A2203433095
// Assembly location: E:\git\partialbits\PartialBeta.exe

using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Xml;

namespace Partial
{
  internal class DocDetails
  {
    private const string sHost = "http://ccisupportsite.com";
    public string sTarget;
    private XmlDocument pDoc;
    private NameValueCollection nvpKeys;
    public ArrayList nProd;
    public ArrayList cSheetColl;

    public void SetProducts(ArrayList inProds)
    {
      this.nProd = new ArrayList();
      this.nProd = inProds;
    }

    public string GetKeys(string sKey) => this.nvpKeys.Get(sKey);

    public DocDetails(XmlDocument xmlprod)
    {
      try
      {
        this.pDoc = xmlprod;
        this.nvpKeys = new NameValueCollection();
        this.cSheetColl = this.GetTheSheets(xmlprod);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private ArrayList GetTheSheets(XmlDocument doc)
    {
      ArrayList arrayList = new ArrayList();
      XmlNodeList xmlNodeList = doc.DocumentElement.SelectNodes("child::*");
      Products products = (Products) null;
      foreach (XmlNode xmlNode in xmlNodeList)
      {
        if (products == null)
        {
          string sRead;
          try
          {
            sRead = xmlNode.Attributes["readonly"].Value;
          }
          catch (Exception ex)
          {
            sRead = "N";
          }
          products = new Products(xmlNode.Attributes["keyname"].Value, xmlNode.Attributes["file"].Value, sRead);
        }
        try
        {
          products.sMethodCall = xmlNode.Attributes["procname"].Value;
        }
        catch (Exception ex)
        {
          products.sMethodCall = "Module1.SetSheet";
        }
        try
        {
          products.sVisible = xmlNode.Attributes["visible"].Value;
        }
        catch (Exception ex)
        {
          products.sVisible = "Y";
        }
        try
        {
          products.sPassed = xmlNode.Attributes["passed"].Value;
        }
        catch (Exception ex)
        {
          products.sPassed = "N";
        }
        try
        {
          products.sQueryEnd = xmlNode.Attributes["queryend"].Value;
        }
        catch (Exception ex)
        {
          products.sQueryEnd = "070111";
        }
        products.sUrl = "http://ccisupportsite.com/Sheets/Version/" + products.sQueryEnd + "/" + HttpUtility.UrlPathEncode(products.sFileName);
        foreach (XmlNode childNode in xmlNode.ChildNodes)
        {
          if (childNode.Attributes["passed"].Value == "Y")
            products.Nvp.Add(childNode.Attributes["keyname"].Value, childNode.Attributes["passed"].Value);
        }
        arrayList.Add((object) products);
        products = (Products) null;
      }
      return arrayList;
    }

    private void LoadPairs()
    {
      foreach (XmlNode selectNode in this.pDoc.DocumentElement.SelectNodes("Customer/Parms"))
        this.nvpKeys.Add(selectNode.Attributes["keyName"].Value, selectNode.Attributes["Value"].Value);
    }

    public void CallXlConfigure(
      Products pd,
      string sFileName,
      NameValueCollection nParms,
      string sQry)
    {
      ApplicationClass applicationClass = new ApplicationClass();
      applicationClass.DisplayAlerts = false;
      try
      {
        applicationClass.Visible = pd.sVisible == "Y";
        applicationClass.Interactive = false;
        Workbook workbook = applicationClass.Workbooks.Open(sFileName, (object) 2, (object) false, (object) 5, (object) "", (object) "", (object) true, (object) XlPlatform.xlWindows, (object) "\t", (object) false, (object) false, (object) 0, (object) true, (object) 1, (object) 0);
        int newSize = nParms.Count + 1;
        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        while (!applicationClass.Ready)
          autoResetEvent.WaitOne(60000);
        object[] array = new object[1]
        {
          (object) pd.sMethodCall
        };
        int index = 1;
        Array.Resize<object>(ref array, newSize);
        foreach (string key in nParms.Keys)
        {
          string sVal = nParms.Get(key);
          if (sVal != null)
          {
            if (sVal.Contains(","))
            {
              object[] objArray = this.processstring(sVal);
              array[index] = (object) objArray;
            }
            else
            {
              array[index] = (object) nParms.Get(key);
              if (key == "queryend")
                array[index] = (object) sQry;
            }
          }
          else
          {
            array[index] = nParms.Get(key) == null ? (object) "" : (object) nParms.Get(key);
            if (key == "specialind" || key == "budgetind" || (key == "otherdepts" || key == "salariedcodes") || (key == "salariedwithsupp" || key == "certifiedcodes" || (key == "princodes" || key == "suppcodes")))
              array[index] = (object) new object[1]
              {
                (object) ""
              };
          }
          ++index;
        }
        try
        {
          if (pd.sMethodCall.Length > 0)
          {
            if (pd.sMethodCall != "None")
              this.RunMacro((object) applicationClass, array);
          }
        }
        catch (Exception ex)
        {
          applicationClass.Quit();
          throw ex;
        }
        try
        {
          applicationClass.GetType().GetProperty("CheckCompatibility").SetValue((object) applicationClass, (object) false, (object[]) null);
        }
        catch
        {
        }
        string str = this.sTarget + pd.sProduct;
        if (string.Compare(applicationClass.Version, "11.0") > 0)
        {
          if (File.Exists(str + ".xlsm"))
            new FileInfo(str + ".xlsm")
            {
              IsReadOnly = false
            }.Delete();
          workbook.SaveAs((object) (this.sTarget + pd.sProduct + ".xlsm"), (object) 52, Type.Missing, Type.Missing, Type.Missing, Type.Missing, ConflictResolution: ((object) XlSaveConflictResolution.xlLocalSessionChanges), AddToMru: Type.Missing, TextCodepage: Type.Missing, TextVisualLayout: Type.Missing, Local: Type.Missing);
        }
        else
        {
          if (File.Exists(str + ".xls"))
            new FileInfo(str + ".xls")
            {
              IsReadOnly = false
            }.Delete();
          workbook.SaveAs((object) (this.sTarget + pd.sProduct + ".xls"), (object) XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, Type.Missing, Type.Missing, ConflictResolution: ((object) XlSaveConflictResolution.xlLocalSessionChanges), AddToMru: Type.Missing, TextCodepage: Type.Missing, TextVisualLayout: Type.Missing, Local: Type.Missing);
        }
        applicationClass.Quit();
      }
      catch (Exception ex)
      {
        applicationClass?.Quit();
        throw ex;
      }
      finally
      {
        try
        {
          applicationClass.Quit();
        }
        catch
        {
        }
      }
    }

    private object[] processstring(string sVal)
    {
      try
      {
        object[] array = new object[10];
        NameValueCollection nameValueCollection = new NameValueCollection();
        string str = sVal;
        char[] chArray = new char[1]{ ',' };
        foreach (string name in str.Split(chArray))
        {
          if (!(nameValueCollection.Get(name) == "Y"))
            nameValueCollection.Add(name, "Y");
        }
        Array.Resize<object>(ref array, nameValueCollection.Count);
        int index = 0;
        foreach (string allKey in nameValueCollection.AllKeys)
        {
          array[index] = (object) allKey;
          ++index;
        }
        return array;
      }
      catch
      {
        return new object[1]{ (object) "" };
      }
    }

    private void RunMacro(object oApp, object[] oRunArgs)
    {
      try
      {
        oApp.GetType().InvokeMember("Run", BindingFlags.InvokeMethod, (Binder) null, oApp, oRunArgs);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private struct sTheProducts
    {
      public string sProduct;
      public string sReadonly;
      public string sFileName;

      public sTheProducts(string sProd, string sFile, string sRead)
      {
        this.sProduct = sProd;
        this.sFileName = sFile;
        this.sReadonly = sRead;
      }
    }
  }
}
