// Decompiled with JetBrains decompiler
// Type: Partial.DownloadInstall
// Assembly: PartialBeta, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9CF783A5-38FA-472F-8C51-9A2203433095
// Assembly location: E:\git\partialbits\PartialBeta.exe

using Partial.WSCustomer;
using Partial.wslogger;
using Partial.WSPartials;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;

namespace Partial
{
  internal class DownloadInstall
  {
    private DocDetails dt;
    private ArrayList nProd;
    private ArrayList nMethColl;
    private NameValueCollection nMethods;
    private static string sHost = "http://ccisupportsite.com";
    private static string sTarget = "C:\\CCI\\";
    private static string sUnid;
    private static string sCurrentFile;
    private static string sEmail;
    private static int iBatchCl;
    private static int iLogCount = 0;
    private static int iDotCount = 0;
    private static bool bWebLog = true;
    private static bool bError;

    private static void Main(string[] args)
    {
      DownloadInstall.bError = false;
      int num1 = 0;
      short num2 = 0;
      Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      Console.WriteLine("Installer Begins. {0}", (object) Assembly.GetExecutingAssembly().GetName().Version);
      string fullName = Assembly.GetExecutingAssembly().FullName;
      try
      {
        string oldValue = "\"";
        DownloadInstall.sHost = args[0];
        DownloadInstall.sUnid = args[1];
        DownloadInstall.sUnid = DownloadInstall.sUnid.Trim();
        DownloadInstall.sEmail = args[2];
        DownloadInstall.sTarget = args[3];
        DownloadInstall.sTarget = DownloadInstall.sTarget.Replace(oldValue, "");
        DownloadInstall.sTarget = DownloadInstall.sTarget.Replace("$", "");
        if (!DownloadInstall.sTarget.Trim().EndsWith("\\"))
          DownloadInstall.sTarget += "\\";
      }
      catch (Exception ex)
      {
        DownloadInstall.sHost = "http://ccisupportsite.com";
        Console.WriteLine("Error - Parameter Signature Invalid " + ex.Message);
        if (!Directory.Exists(DownloadInstall.sTarget))
        {
          num1 = 0;
          bool flag = false;
          while (num1 <= 2 && !flag)
          {
            Console.WriteLine("Problem with Target Location");
            Console.WriteLine("The target {0} cannot be Validated Please Reenter:", (object) DownloadInstall.sTarget);
            DownloadInstall.sTarget = Console.ReadLine();
            Console.WriteLine("");
            DownloadInstall.sTarget = DownloadInstall.sTarget.Trim();
            ++num1;
            if (Directory.Exists(DownloadInstall.sTarget))
              flag = true;
          }
          if (num1 > 2 && !flag)
            DownloadInstall.bError = true;
        }
        Console.WriteLine("Problem with Email Address");
        Console.WriteLine("The email address {0} cannot be Validated Please Reenter:", (object) DownloadInstall.sEmail);
        DownloadInstall.sEmail = Console.ReadLine();
        Console.WriteLine("");
        DownloadInstall.sEmail = DownloadInstall.sEmail.Trim();
      }
      DownloadInstall.iBatchCl = -999;
      PcId pc = new PcId(DownloadInstall.sUnid, DownloadInstall.sTarget, DownloadInstall.sEmail);
      pc.sEmail = DownloadInstall.sEmail;
      if (!DownloadInstall.bError)
      {
        try
        {
          DownloadInstall.sEmail.Contains("cookconsulting.net");
          pc.LogEvent();
          DownloadInstall.iBatchCl = pc.IBatchNo;
          DownloadInstall.bWebLog = true;
          if (DownloadInstall.iBatchCl == -1)
          {
            DownloadInstall.bWebLog = false;
            pc.CreateLogFile(DownloadInstall.sTarget);
          }
        }
        catch (Exception ex)
        {
          DownloadInstall.bWebLog = false;
          pc.CreateLogFile(DownloadInstall.sTarget);
        }
        pc.LogEvent(DownloadInstall.sUnid, "Installer is running Batch ID -> " + pc.IBatchNo.ToString() + " for UNID -> " + DownloadInstall.sUnid, 0, pc.IBatchNo);
        pc.LogEvent(DownloadInstall.sUnid, "Assembly Name is " + fullName, 0, pc.IBatchNo);
        for (; num1 <= 2 && DownloadInstall.sUnid.Length != 32; ++num1)
        {
          Console.Write("Problem with the Customer License Code Check your Cut and Past");
          Console.Write("The unid {0} cannot be Validated Please Reenter:", (object) DownloadInstall.sUnid);
          DownloadInstall.sUnid = Console.ReadLine();
          Console.WriteLine("");
          DownloadInstall.sUnid = DownloadInstall.sUnid.Trim();
        }
        if (DownloadInstall.sUnid.Length != 32)
        {
          Console.WriteLine("The unid {0} cannot be Validated.", (object) DownloadInstall.sUnid);
          DownloadInstall.bError = true;
        }
        else
        {
          Console.WriteLine("Installer is running Batch ID -> {0} for UNID -> {1}.", (object) DownloadInstall.iBatchCl, (object) DownloadInstall.sUnid);
          ArrayList inProds = new ArrayList();
          try
          {
            if (!Directory.Exists(DownloadInstall.sTarget))
              Directory.CreateDirectory(DownloadInstall.sTarget);
          }
          catch (Exception ex)
          {
          }
          try
          {
            new FileInfo(DownloadInstall.sTarget + "Products.xml").IsReadOnly = false;
            System.IO.File.Delete(DownloadInstall.sTarget + "Products.xml");
          }
          catch
          {
          }
          try
          {
            new FileInfo(DownloadInstall.sTarget + "Cust.xml").IsReadOnly = false;
            System.IO.File.Delete(DownloadInstall.sTarget + "Cust.xml");
          }
          catch
          {
          }
          try
          {
            DownloadInstall.DownLoadASheet(DownloadInstall.sHost + "/Xml/Products.xml", DownloadInstall.sTarget + "Products.Xml");
            if (DownloadInstall.bWebLog)
              pc.LogEvent(DownloadInstall.sUnid, "File Download Complete on " + DownloadInstall.sTarget + "Products.Xml", 0, pc.IBatchNo);
            else
              pc.FileLog(DownloadInstall.sUnid, "File Download Complete on " + DownloadInstall.sTarget + "Products.Xml", 0, pc.IBatchNo);
            GetCustomersPartialService customersPartialService = new GetCustomersPartialService();
            NetworkCredential networkCredential = new NetworkCredential("mmoen", "bobbob1", "");
            customersPartialService.Credentials = (ICredentials) networkCredential;
            string sXml = customersPartialService.GETCUSTOMERSPARTIAL("changeor.nsf", "XMLOUTPUT", DownloadInstall.sUnid);
            try
            {
              new FileInfo(DownloadInstall.sTarget + "Cust.xml").IsReadOnly = false;
              System.IO.File.Delete(DownloadInstall.sTarget + "Cust.xml");
            }
            catch
            {
            }
            StreamWriter streamWriter = new StreamWriter(DownloadInstall.sTarget + "Cust.Xml", false);
            string str = pc.PackXml(sXml, DownloadInstall.sUnid);
            streamWriter.Write(str);
            streamWriter.Close();
            streamWriter.Dispose();
            if (DownloadInstall.bWebLog)
              pc.LogEvent(DownloadInstall.sUnid, "File Download Complete on " + DownloadInstall.sTarget + "Cust.Xml", 0, pc.IBatchNo);
            else
              pc.FileLog(DownloadInstall.sUnid, "File Download Complete on " + DownloadInstall.sTarget + "Cust.Xml", 0, pc.IBatchNo);
          }
          catch (Exception ex)
          {
            int num3 = DownloadInstall.bError ? 1 : 0;
            DownloadInstall.bError = true;
            if (DownloadInstall.bWebLog)
              pc.LogEvent(DownloadInstall.sUnid, ex.Message + " - Program Abort", -1, pc.IBatchNo);
            else
              pc.FileLog(DownloadInstall.sUnid, ex.Message + " - Program Abort", -1, pc.IBatchNo);
            if (DownloadInstall.bWebLog)
              pc.LogEvent(DownloadInstall.sUnid, ex.Message + "Program Abort", 11000, pc.IBatchNo);
            else
              pc.FileLog(DownloadInstall.sUnid, ex.Message + "Program Abort", 11000, pc.IBatchNo);
            try
            {
              pc.Close();
            }
            catch
            {
            }
            Console.WriteLine("Program Stopping - " + ex.Message);
            goto label_133;
          }
          FileInfo fileInfo1 = new FileInfo(DownloadInstall.sTarget + "\\Products.Xml");
          if (System.IO.File.Exists(DownloadInstall.sTarget + "\\Products.Xml") && fileInfo1.Length == 0L)
          {
            if (DownloadInstall.bWebLog)
              pc.LogEvent(DownloadInstall.sUnid, "Products File did not Download", -1, pc.IBatchNo);
            else
              pc.FileLog(DownloadInstall.sUnid, "Products File did not Download", -1, pc.IBatchNo);
            int num3 = DownloadInstall.bError ? 1 : 0;
            DownloadInstall.bError = true;
          }
          else
          {
            FileInfo fileInfo2 = new FileInfo(DownloadInstall.sTarget + "\\Cust.Xml");
            if (System.IO.File.Exists(DownloadInstall.sTarget + "\\Cust.Xml") && fileInfo2.Length == 0L)
            {
              if (DownloadInstall.bWebLog)
                pc.LogEvent(DownloadInstall.sUnid, "Customer File did not Download", -1, pc.IBatchNo);
              else
                pc.FileLog(DownloadInstall.sUnid, "Customer File did not Download", -1, pc.IBatchNo);
              int num3 = DownloadInstall.bError ? 1 : 0;
              DownloadInstall.bError = true;
            }
            else
            {
              if (DownloadInstall.bWebLog)
                pc.LogEvent(DownloadInstall.sUnid, "File Download Complete ", 0, pc.IBatchNo);
              else
                pc.FileLog(DownloadInstall.sUnid, "File Download Complete ", 0, pc.IBatchNo);
              XmlDocument xmlDocument = new XmlDocument();
              XmlDocument xmlprod = new XmlDocument();
              xmlprod.Load(DownloadInstall.sTarget + "\\Products.Xml");
              UTF8Encoding utF8Encoding = new UTF8Encoding();
              StreamReader streamReader = new StreamReader(DownloadInstall.sTarget + "\\Cust.Xml");
              streamReader.ReadToEnd();
              streamReader.Close();
              streamReader.Dispose();
              xmlDocument.Load(DownloadInstall.sTarget + "\\Cust.Xml");
              NameValueCollection nameValueCollection = new NameValueCollection();
              DocDetails dt = new DocDetails(xmlprod);
              dt.SetProducts(inProds);
              NameValueCollection nVals = new NameValueCollection();
              XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("Customer/Parms");
              string str1 = "";
              foreach (XmlNode xmlNode in xmlNodeList)
              {
                if (xmlNode.Name == "Parms")
                {
                  try
                  {
                    if (str1 == "")
                      str1 = xmlNode.Attributes["queryend"].Value;
                  }
                  catch (Exception ex)
                  {
                  }
                }
                foreach (XmlAttribute attribute in (XmlNamedNodeMap) xmlNode.Attributes)
                {
                  if (attribute.Name == "Product")
                  {
                    if (attribute.Value == "Y")
                    {
                      ++num2;
                      try
                      {
                        if (str1 == "")
                          str1 = xmlNode.Attributes["queryend"].Value;
                      }
                      catch (Exception ex)
                      {
                      }
                      try
                      {
                        string sQueryEnd = str1;
                        NameValueCollection parmSet = DownloadInstall.FindParmSet(xmlNode.Attributes["Value"].Value, sQueryEnd, dt.cSheetColl, nVals);
                        string liveFile = DownloadInstall.FindLiveFile(xmlNode.Attributes["Value"].Value, sQueryEnd, dt.cSheetColl);
                        string sAddr = DownloadInstall.sHost + "/sheets/version/" + str1 + "/" + HttpUtility.UrlPathEncode(liveFile);
                        string str2 = DownloadInstall.sTarget + liveFile;
                        if (DownloadInstall.bWebLog)
                          pc.LogEvent(DownloadInstall.sUnid, "Downloading Sheet " + sAddr, 0, pc.IBatchNo);
                        else
                          pc.FileLog(DownloadInstall.sUnid, "Downloading Sheet " + sAddr, 0, pc.IBatchNo);
                        DownloadInstall.sCurrentFile = sAddr;
                        DownloadInstall.iDotCount = 0;
                        if (System.IO.File.Exists(str2))
                          new FileInfo(str2)
                          {
                            IsReadOnly = false
                          }.Delete();
                        DownloadInstall.DownLoadASheet(sAddr, str2);
                        if (DownloadInstall.bWebLog)
                          pc.LogEvent(DownloadInstall.sUnid, "Download Complete Sheet " + sAddr, 0, pc.IBatchNo);
                        else
                          pc.FileLog(DownloadInstall.sUnid, "Download Complete Sheet " + sAddr, 0, pc.IBatchNo);
                        FileInfo fileInfo3 = new FileInfo(str2);
                        if (System.IO.File.Exists(str2))
                        {
                          if (fileInfo3.Length > 0L)
                          {
                            try
                            {
                              ++DownloadInstall.iLogCount;
                              if (!pc.bCust)
                              {
                                pc.LogEvent(DownloadInstall.sUnid, "", 10000, pc.IBatchNo);
                                pc.bCust = true;
                              }
                              if (DownloadInstall.bWebLog)
                                pc.LogEvent(DownloadInstall.sUnid, "Trying to configure " + str2, 0, pc.IBatchNo);
                              else
                                pc.FileLog(DownloadInstall.sUnid, "Trying to configure " + str2, 0, pc.IBatchNo);
                              DownloadInstall.ConfigureASheet(str2, ref dt.cSheetColl, xmlNode.Attributes["Value"].Value, sQueryEnd, parmSet, dt, ref pc);
                              if (DownloadInstall.bWebLog)
                                pc.LogEvent(DownloadInstall.sUnid, "Configuring " + str2, 0, pc.IBatchNo);
                              else
                                pc.FileLog(DownloadInstall.sUnid, "Configuring " + str2, 0, pc.IBatchNo);
                              str1 = "";
                              break;
                            }
                            catch (Exception ex)
                            {
                              int num3 = DownloadInstall.bError ? 1 : 0;
                              DownloadInstall.bError = true;
                              pc.LogEvent(DownloadInstall.sUnid, ex.Message + " - Configuring Sheet Fails", -1, pc.IBatchNo);
                              break;
                            }
                          }
                        }
                        int num4 = DownloadInstall.bError ? 1 : 0;
                        DownloadInstall.bError = true;
                        pc.LogEvent(DownloadInstall.sUnid, str2 + " Fails to Download", -1, pc.IBatchNo);
                        Console.WriteLine("Error - " + str2 + " Fails to Download");
                        fileInfo3.Delete();
                        break;
                      }
                      catch (Exception ex)
                      {
                        int num3 = DownloadInstall.bError ? 1 : 0;
                        DownloadInstall.bError = true;
                        if (DownloadInstall.bWebLog)
                          pc.LogEvent(DownloadInstall.sUnid, ex.Message + " - Downloading Sheet", -1, pc.IBatchNo);
                        else
                          pc.FileLog(DownloadInstall.sUnid, ex.Message + " - Downloading Sheet", -1, pc.IBatchNo);
                        Console.WriteLine("Error - " + ex.Message);
                        continue;
                      }
                    }
                  }
                  try
                  {
                    if (nVals.Get(xmlNode.Attributes["LkeyName"].Value) != null)
                    {
                      string str2 = xmlNode.Attributes["LkeyName"].Value;
                      if (str2 == "queryend")
                      {
                        string str3 = xmlNode.Attributes["Value"].Value;
                      }
                      string str4 = xmlNode.Attributes["Value"].Value;
                      string str5 = nVals.Get(xmlNode.Attributes["LkeyName"].Value);
                      switch (str2)
                      {
                        case "suppcodes":
                          nVals.Remove(xmlNode.Attributes["LkeyName"].Value);
                          nVals.Add(xmlNode.Attributes["LkeyName"].Value, str5 + "," + str4);
                          continue;
                        case "princodes":
                          nVals.Remove(xmlNode.Attributes["LkeyName"].Value);
                          nVals.Add(xmlNode.Attributes["LkeyName"].Value, str5 + "," + str4);
                          continue;
                        case "certifiedcodes":
                          nVals.Remove(xmlNode.Attributes["LkeyName"].Value);
                          nVals.Add(xmlNode.Attributes["LkeyName"].Value, str5 + "," + str4);
                          continue;
                        case "salariedwithsupp":
                          nVals.Remove(xmlNode.Attributes["LkeyName"].Value);
                          nVals.Add(xmlNode.Attributes["LkeyName"].Value, str5 + "," + str4);
                          continue;
                        case "salariedcodes":
                          nVals.Remove(xmlNode.Attributes["LkeyName"].Value);
                          nVals.Add(xmlNode.Attributes["LkeyName"].Value, str5 + "," + str4);
                          continue;
                        case "otherdepts":
                          nVals.Remove(xmlNode.Attributes["LkeyName"].Value);
                          nVals.Add(xmlNode.Attributes["LkeyName"].Value, str5 + "," + str4);
                          continue;
                        case "budgetind":
                          nVals.Remove(xmlNode.Attributes["LkeyName"].Value);
                          nVals.Add(xmlNode.Attributes["LkeyName"].Value, str5 + "," + str4);
                          continue;
                        case "specialind":
                          nVals.Remove(xmlNode.Attributes["LkeyName"].Value);
                          nVals.Add(xmlNode.Attributes["LkeyName"].Value, str5 + "," + str4);
                          continue;
                        case "username":
                          pc.setUser(xmlNode.Attributes["Value"].Value);
                          continue;
                        case "customername":
                          pc.setCust(xmlNode.Attributes["Value"].Value);
                          continue;
                        default:
                          continue;
                      }
                    }
                    else
                      nVals.Add(xmlNode.Attributes["LkeyName"].Value.ToString(), xmlNode.Attributes["Value"].Value.ToString());
                  }
                  catch (ArgumentOutOfRangeException ex)
                  {
                  }
                  catch
                  {
                  }
                }
              }
              if (DownloadInstall.bWebLog)
                pc.LogEvent(DownloadInstall.sUnid, "Processing Completed", 0, pc.IBatchNo);
              else
                pc.FileLog(DownloadInstall.sUnid, "Processing Completed", 0, pc.IBatchNo);
              pc.Close();
              new FileInfo(DownloadInstall.sTarget + "Cust.xml").Delete();
              new FileInfo(DownloadInstall.sTarget + "Products.xml").Delete();
              pc.LogEvent(DownloadInstall.sUnid, "Creating Shortcuts", 0, pc.IBatchNo);
              FileInfo fileInfo4 = new FileInfo(DownloadInstall.sTarget + "createshort1_1v2.vbs");
              if (fileInfo4.Exists)
                fileInfo4.Delete();
              DownloadInstall.DownLoadASheet(DownloadInstall.sHost + "/Xml/createshort1_1v2.vbs", DownloadInstall.sTarget + "/createshort1_1v2.vbs");
              string str6 = "  " + DownloadInstall.sTarget + "createshort1_1v2.vbs ";
              Process process1 = new Process();
              process1.StartInfo.FileName = "wscript.exe";
              process1.StartInfo.Arguments = "  \"" + DownloadInstall.sTarget + "createshort1_1v2.vbs\"";
              process1.StartInfo.UseShellExecute = true;
              process1.StartInfo.WorkingDirectory = DownloadInstall.sTarget;
              process1.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
              process1.Start();
              process1.WaitForExit();
              process1.Close();
              fileInfo4.IsReadOnly = false;
              fileInfo4.Delete();
              pc.LogEvent(DownloadInstall.sUnid, "Setting ReadOnly", 0, pc.IBatchNo);
              FileInfo fileInfo5 = new FileInfo(DownloadInstall.sTarget + "SetAttribute2009_v1.vbs");
              if (fileInfo5.Exists)
                fileInfo5.Delete();
              DownloadInstall.DownLoadASheet(DownloadInstall.sHost + "/Xml/SetAttribute2009_v1.vbs", DownloadInstall.sTarget + "/SetAttribute2009_v1.vbs");
              if (!new FileInfo(DownloadInstall.sTarget + "excelcci.ico").Exists)
                DownloadInstall.DownLoadASheet(DownloadInstall.sHost + "/Xml/excelcci.ico", DownloadInstall.sTarget + "/excelcci.ico");
              string str7 = "   " + DownloadInstall.sTarget + "SetAttribute2009_v1.vbs";
              Process process2 = new Process();
              process2.StartInfo.FileName = "wscript.exe";
              process2.StartInfo.Arguments = "   \"" + DownloadInstall.sTarget + "SetAttribute2009_v1.vbs\"";
              process2.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
              process2.StartInfo.UseShellExecute = true;
              process2.StartInfo.WorkingDirectory = DownloadInstall.sTarget;
              process2.Start();
              process2.WaitForExit();
              process2.Close();
              new FileInfo(DownloadInstall.sTarget + "SetAttribute2009_v1.vbs")
              {
                IsReadOnly = false
              }.Delete();
            }
          }
        }
      }
label_133:
      if (num2 == (short) 0)
      {
        DownloadInstall.bError = true;
        Console.WriteLine("No Sheets were retrieved from WebService - Check Domino Sheet Selections");
      }
      if (DownloadInstall.bError)
      {
        Console.WriteLine("An Install Error has Occurred! Please Contact Support desk @ 800 425 0720 or email supportdesk@cookconsulting.net");
        Console.WriteLine("Please note the following information Batch ID -> {0} and UNID -> {1}.", (object) DownloadInstall.iBatchCl, (object) DownloadInstall.sUnid);
        Console.WriteLine("Press Enter key to continue.");
        Console.ReadLine();
      }
      if (!DownloadInstall.bError)
      {
        try
        {
          RemovePartialFlagsService partialFlagsService = new RemovePartialFlagsService();
          NetworkCredential networkCredential = new NetworkCredential("mmoen", "bobbob1", "");
          partialFlagsService.Credentials = (ICredentials) networkCredential;
          if (!DownloadInstall.sEmail.Contains("donotreset@cookconsulting.net"))
            partialFlagsService.REMOVEPARTIALFLAGS("Changeor.nsf", "XMLOUTPUT", DownloadInstall.sUnid);
        }
        catch
        {
        }
      }
      Console.WriteLine("Installer Processing Completed!");
      if (DownloadInstall.bWebLog)
        pc.LogEvent(DownloadInstall.sUnid, " Installer Closing", 11000, pc.IBatchNo);
      else
        pc.FileLog(DownloadInstall.sUnid, " Installer Closing", 11000, pc.IBatchNo);
    }

    private static NameValueCollection FindParmSet(
      string sProductName,
      string sQueryEnd,
      ArrayList allProducts,
      NameValueCollection nVals)
    {
      try
      {
        foreach (string key in nVals.Keys)
          ;
        foreach (Products allProduct in allProducts)
        {
          if (allProduct.sProduct.Trim() == sProductName.Trim() && allProduct.sQueryEnd.Trim() == sQueryEnd.Trim())
          {
            NameValueCollection nameValueCollection = new NameValueCollection();
            foreach (string allKey in allProduct.Nvp.AllKeys)
            {
              if (allProduct.Nvp.Get(allKey) == "Y")
                nameValueCollection.Add(allKey, nVals.Get(allKey));
            }
            return nameValueCollection;
          }
        }
        throw new Exception("File :" + sProductName + " is not found in Products.xml for parameters");
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private static string FindLiveFile(
      string sProductName,
      string sQueryEnd,
      ArrayList allProducts)
    {
      try
      {
        foreach (Products allProduct in allProducts)
        {
          if (allProduct.sProduct.Trim() == sProductName.Trim() && allProduct.sQueryEnd.Trim() == sQueryEnd.Trim())
            return allProduct.sFileName;
        }
        throw new Exception("File :" + sProductName + " is not found in Products.xml");
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private static void ConfigureASheet(
      string sLocalName,
      ref ArrayList allProducts,
      string sProductName,
      string sQueryEnd,
      NameValueCollection nParms,
      DocDetails dt,
      ref PcId pc)
    {
      try
      {
        foreach (Products pd in allProducts)
        {
          if (pd.sProduct.Trim() == sProductName.Trim())
          {
            if (pd.sQueryEnd.Trim() == sQueryEnd.Trim())
            {
              try
              {
                pc.LogEvent(DownloadInstall.sUnid, "At dt.CallXlConfigure for " + sLocalName, 0, pc.IBatchNo);
                dt.sTarget = DownloadInstall.sTarget;
                dt.CallXlConfigure(pd, sLocalName, nParms, sQueryEnd);
                break;
              }
              catch (Exception ex)
              {
                if (DownloadInstall.bWebLog)
                {
                  pc.LogEvent(DownloadInstall.sUnid, ex.Message + "Results from Configure on  " + sLocalName, -1, pc.IBatchNo);
                  break;
                }
                pc.FileLog(DownloadInstall.sUnid, ex.Message + "Results from Configure on  " + sLocalName, -1, pc.IBatchNo);
                break;
              }
              finally
              {
                new FileInfo(sLocalName)
                {
                  IsReadOnly = false
                }.Delete();
                if (System.IO.File.Exists(sLocalName))
                  throw new IOException();
                if (DownloadInstall.bWebLog)
                  pc.LogEvent(DownloadInstall.sUnid, "Success Results w/Delete from Configure on  " + sLocalName, 0, pc.IBatchNo);
                else
                  pc.FileLog(DownloadInstall.sUnid, "Success Results w/Delete from Configure on  " + sLocalName, 0, pc.IBatchNo);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private static void LoadMethods(ref NameValueCollection nV, XmlDocument xdoc, string sUnid)
    {
      try
      {
        foreach (XmlNode selectNode in xdoc.SelectNodes("/Sheets/Parms"))
          ;
      }
      catch (Exception ex)
      {
      }
    }

    private static void DownLoadASheet(string sAddr, string sFileName)
    {
      try
      {
        if (System.IO.File.Exists(sFileName))
          new FileInfo(sFileName) { IsReadOnly = false }.Delete();
      }
      catch
      {
      }
      RequestCachePolicy requestCachePolicy = new RequestCachePolicy(RequestCacheLevel.Reload);
      string uriString = sAddr;
      AutoResetEvent autoResetEvent = new AutoResetEvent(false);
      Uri address = new Uri(uriString);
      try
      {
        WebClient webClient = new WebClient();
        webClient.Proxy = (IWebProxy) null;
        webClient.Credentials = CredentialCache.DefaultCredentials;
        webClient.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
        webClient.CachePolicy = requestCachePolicy;
        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadInstall.wcDownLoadDone);
        webClient.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadInstall.wcDownLoadDone1);
        webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadInstall.wcCallbackDown);
        webClient.DownloadFileAsync(address, sFileName);
        Console.Write("DownLoading with {0} ", (object) sFileName);
        while (webClient.IsBusy)
        {
          try
          {
            autoResetEvent.WaitOne(100);
          }
          catch
          {
          }
        }
        Console.WriteLine("Done with {0} ", (object) sFileName);
      }
      catch (WebException ex)
      {
        if (ex.Response == null)
          return;
        HttpWebResponse response = (HttpWebResponse) ex.Response;
        new Ws_Logger().Logger(DownloadInstall.sUnid, response.StatusDescription, -1, DownloadInstall.iBatchCl);
        Console.WriteLine(response.StatusDescription + "  Download Error -" + ex.Message);
        if (response.StatusCode != HttpStatusCode.OK)
        {
          Console.WriteLine(ex.Status.ToString() + "  Download Error -" + ex.Message);
          Console.ReadLine();
          throw ex;
        }
      }
      catch (Exception ex)
      {
        new Ws_Logger().Logger(DownloadInstall.sUnid, ex.Message, -1, DownloadInstall.iBatchCl);
        Console.WriteLine(ex.Message);
        throw ex;
      }
    }

    private static void wcDownLoadDone(object sender, AsyncCompletedEventArgs e) => Console.WriteLine("File Download AsyncComplete Complete");

    private static void wcDownLoadDone1(object sender, DownloadDataCompletedEventArgs e) => Console.WriteLine("File Download DoneComplete Complete");

    private static void wcCallbackDown(object sender, DownloadProgressChangedEventArgs e)
    {
      if (DownloadInstall.iDotCount % 50 == 0)
        Console.Write(".");
      ++DownloadInstall.iDotCount;
    }

    private struct sTheMethods
    {
      public string sSheet;
      public string sPassed;
      public string sKeyName;
      public string sParent;

      public sTheMethods(string nSheet, string nPassed, string nKeyed, string nParent)
      {
        this.sSheet = nSheet;
        this.sPassed = nPassed;
        this.sKeyName = nKeyed;
        this.sParent = nParent;
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
