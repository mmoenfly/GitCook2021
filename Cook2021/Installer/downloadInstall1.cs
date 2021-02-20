// Decompiled with JetBrains decompiler
// Type: Installer.DownloadInstall
// Assembly: Installer, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FB83BBFE-8B22-4C6D-B1D0-39D6C50033CE
// Assembly location: E:\Downloads\Installer.exe
using Installer.wslogger;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Xml;


namespace Installer
{
    internal class DownloadInstall
    {
        private DocDetails dt;
        private ArrayList nProd;
        private ArrayList nMethColl;
        private NameValueCollection nMethods;
        private static string sHost = "https://spinstalls.app-garden.com";
        private static string sTarget = "C:\\CCI\\";
        private static string sUnid,SDominoUserid,SDominoPwd,sXml,sAns,sDeskTopFolderName,sUnidEmail,sIconFile;

        private static string sCurrentFile;
        private static string sEmail;
        private static int iBatchCl;
        private static int iLogCount = 0;
        private static int iDotCount = 0;
        private static bool bWebLog = true;
        private static bool bError;
        private static Newtonsoft.Json.Linq.JObject Jsonobj, Jsonobj2;
        private static JArray sDeleteFolders;

        private static void Main(string[] args)
        {






            var sDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DownloadInstall.bError = false;
            int num1 = 0;
            Console.WriteLine("Full Installer Begins. {0}", (object)Assembly.GetExecutingAssembly().GetName().Version);
            string oldValue = "\"";
            Console.WriteLine("The Arg {0} is {1}:", 0, args[0]);
            Console.WriteLine("The Arg {0} is {1}:", 1, args[1]);
            Console.WriteLine("The Arg {0} is {1}:", 2, args[2]);
            Console.WriteLine("The Arg {0} is {1}:", 3, args[3]);
            Console.WriteLine("The Dir is {0} :",sDir);

            string fullName = Assembly.GetExecutingAssembly().FullName;

          

            Newtonsoft.Json.Linq.JObject Jsonobj   ;
            try
            {
                string path1 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                path1 += "\temp";
                new FileInfo(path1 + "\\creds.json").IsReadOnly = false;
                System.IO.File.Delete(path1 + "\\creds.json");
            }
            catch
            {
            }
            finally
            {
                string path1 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                path1 +=  "\\temp";
                Console.Write(path1);
                DownloadInstall.DownLoadASheet("https://spreporting.app-garden.com" + "/Xml/creds.json", path1 + "\\creds.json");


            }

            try
            {
                string path1 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                path1 += "\\temp";
                Jsonobj = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(path1 +"\\creds.json"));
                sHost = (string) Jsonobj["server"];
                SDominoUserid = (string)Jsonobj["key1"];
                SDominoPwd = (string)Jsonobj["key2"];
                sDeskTopFolderName = (string)Jsonobj["DesktopFolderName"];
                sDeleteFolders = (JArray)Jsonobj["DeleteFolders"];
                sIconFile = (string)Jsonobj["iconFile"];
                FileInfo fi2 = new FileInfo(path1 +"\\creds.json");
                 fi2.Delete();
            }
            catch (Exception ex)
            {
                SDominoUserid = "spadmin";
                SDominoPwd = "Gar2020@*ss";
                sHost = args[0];

            }


            try
            {
                FileInfo fileInfo4 = new FileInfo(DownloadInstall.sTarget + "CleanDirs.vbs");
                if (fileInfo4.Exists)
                    fileInfo4.Delete();
                DownloadInstall.DownLoadASheet(DownloadInstall.sHost + "/Xml/CleanDirs.vbs", DownloadInstall.sTarget + "/CleanDirs.vbs");
                string str6 = "  " + DownloadInstall.sTarget + "CleanDirs.vbs ";
                foreach (JValue itm in sDeleteFolders)
                {
                    try
                    {
                        Process process1 = new Process();
                        process1.StartInfo.FileName = "wscript.exe";
                        process1.StartInfo.Arguments = DownloadInstall.sTarget + "\\CleanDirs.vbs " + "\"" + itm.ToString() + "\"";
                        process1.StartInfo.UseShellExecute = true;
                        process1.StartInfo.WorkingDirectory = DownloadInstall.sTarget;
                        process1.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        process1.Start();
                        process1.WaitForExit();
                        process1.Close();


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            finally
            {

                FileInfo fileInfo4 = new FileInfo(DownloadInstall.sTarget + "\\CleanDirs.vbs");
                if (fileInfo4.Exists)
                    fileInfo4.Delete();
            }


            try
            {
                //DownloadInstall.sHost = (string)Jsonobj["server"];
                Console.WriteLine("The sHOST is {0}  :", sHost);
                DownloadInstall.sUnid = args[1];
                DownloadInstall.sEmail = args[2];
                DownloadInstall.sTarget = args[3];
                DownloadInstall.sTarget = DownloadInstall.sTarget.Replace(oldValue, "");
                Console.WriteLine("Email is {0}", (object)DownloadInstall.sEmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0}", ex.Message);
                DownloadInstall.sTarget = DownloadInstall.sTarget.Replace(oldValue, "");
                DownloadInstall.sHost = "https://spreporting.app-garden.com";
                Console.WriteLine("Error - Parameter Signature Invalid " + ex.Message);
                if (!Directory.Exists(DownloadInstall.sTarget))
                {
                    num1 = 0;
                    bool flag = false;
                    while (num1 <= 2 && !flag)
                    {


                        Console.WriteLine("The Arg {0} is {1}:", 0, args[0]);
                        Console.WriteLine("The Arg {0} is {1}:", 1, args[1]);
                        Console.WriteLine("The Arg {0} is {1}:", 2, args[2]);
                        Console.WriteLine("The Arg {0} is {1}:", 3, args[3]);
                        Console.WriteLine("The SHOST is {0}  :", sHost);
                        Console.WriteLine("Problem with Target Location");
                        Console.WriteLine("The target {0} cannot be Validated Please Reenter:", (object)DownloadInstall.sTarget);
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
                Console.WriteLine("The email address {0} cannot be Validated Please Reenter:", (object)DownloadInstall.sEmail);
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
                    if (!DownloadInstall.sEmail.Contains("testing") && pc.ChkInstall(DownloadInstall.sUnid) == "stop")
                    {
                        Console.WriteLine("This install is terminated - This license code has been previously installed");
                        DownloadInstall.bError = true;
                        goto label_165;
                    }
                    else
                    {
                        pc.LogEvent();
                        DownloadInstall.iBatchCl = pc.IBatchNo;
                        DownloadInstall.bWebLog = true;
                        if (DownloadInstall.iBatchCl == -1)
                        {
                            DownloadInstall.bWebLog = false;
                            pc.CreateLogFile(DownloadInstall.sTarget);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DownloadInstall.bWebLog = false;
                }
                pc.LogEvent(DownloadInstall.sUnid, "Full Installer is running Batch ID -> " + pc.IBatchNo.ToString() + " for UNID -> " + DownloadInstall.sUnid, 0, pc.IBatchNo);
                pc.LogEvent(DownloadInstall.sUnid, "Assembly Name is " + fullName, 0, pc.IBatchNo);
                for (; num1 <= 2 && DownloadInstall.sUnid.Length != 32; ++num1)
                {
                    Console.WriteLine("Problem with the Customer License Code - Check your Cut and Paste.");
                    Console.WriteLine("The unid {0} cannot be Validated Please Reenter:", (object)DownloadInstall.sUnid);
                    DownloadInstall.sUnid = Console.ReadLine();
                    Console.WriteLine("");
                    DownloadInstall.sUnid = DownloadInstall.sUnid.Trim();
                }
                if (DownloadInstall.sUnid.Length != 32)
                {
                    Console.WriteLine("The unid {0} cannot be Validated.", (object)DownloadInstall.sUnid);
                    DownloadInstall.bError = true;
                }
                else
                {
                    Console.WriteLine("Installer is running Batch ID -> {0} for UNID -> {1}.", (object)DownloadInstall.iBatchCl, (object)DownloadInstall.sUnid);
                    if (!DownloadInstall.sTarget.EndsWith("\\"))
                        DownloadInstall.sTarget += "\\";
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
                        foreach (FileInfo file in new DirectoryInfo(DownloadInstall.sTarget).GetFiles())
                        {
                            if (file.Extension == ".xls" || file.Extension == ".xlsm" || file.Extension == ".lnk")
                            {
                                file.IsReadOnly = false;
                                file.Delete();
                            }
                        }
                        string path1 = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\AS400 SpreadSheets";
                        try
                        {
                            foreach (FileInfo file in new DirectoryInfo(path1).GetFiles())
                            {
                                if (file.Extension == ".lnk" || file.Extension == ".xls" || file.Extension == ".xlsm")
                                {
                                    file.IsReadOnly = false;
                                    file.Delete();
                                }
                            }
                            try
                            {
                                Directory.Delete(path1);
                            }
                            catch
                            {
                            }
                        }
                        catch
                        {
                        }
                        string path2 = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + sDeskTopFolderName;
                        try
                        {
                            DirectoryInfo directoryInfo = new DirectoryInfo(path2);
                            if (directoryInfo.Exists)
                            {
                                foreach (FileInfo file in directoryInfo.GetFiles())
                                {
                                    if (file.Extension == ".lnk")
                                    {
                                        file.IsReadOnly = false;
                                        file.Delete();
                                    }
                                }
                                try
                                {
                                    Directory.CreateDirectory(path2);
                                    Thread.Sleep(100);
                                }
                                catch
                                {
                                }
                            }
                            else
                                Directory.CreateDirectory(path2);
                        }
                        catch
                        {
                        }
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
                        dws.GetFullCustomersService customersService = new dws.GetFullCustomersService();
                        NetworkCredential networkCredential = new NetworkCredential(SDominoUserid, SDominoPwd, "");
                        customersService.Credentials = (ICredentials)networkCredential;
                        sXml = customersService.GETFULLCUSTOMERS("changeor.nsf", "XMLOUTPUT", DownloadInstall.sUnid);
                        try
                        {
                            new FileInfo(DownloadInstall.sTarget + "Cust.xml").IsReadOnly = false;
                            System.IO.File.Delete(DownloadInstall.sTarget + "Cust.xml");
                        }
                        catch
                        {
                        }
                        StreamWriter streamWriter = new StreamWriter(DownloadInstall.sTarget + "Cust.Xml", false);
                        string sMsg = pc.PackXml(sXml, DownloadInstall.sUnid);
                        pc.LogEvent(DownloadInstall.sUnid, sMsg, 0, pc.IBatchNo);
                        streamWriter.Write(sMsg);
                        streamWriter.Close();
                        streamWriter.Dispose();
                        if (DownloadInstall.bWebLog)
                            pc.LogEvent(DownloadInstall.sUnid, "File Download Complete on " + DownloadInstall.sTarget + "Cust.Xml", 0, pc.IBatchNo);
                        else
                            pc.FileLog(DownloadInstall.sUnid, "File Download Complete on " + DownloadInstall.sTarget + "Cust.Xml", 0, pc.IBatchNo);
                    }
                    catch (Exception ex)
                    {
                        int num2 = DownloadInstall.bError ? 1 : 0;
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
                        goto label_165;
                    }
                    FileInfo fileInfo1 = new FileInfo(DownloadInstall.sTarget + "Products.Xml");
                    if (System.IO.File.Exists(DownloadInstall.sTarget + "Products.Xml") && fileInfo1.Length == 0L)
                    {
                        if (DownloadInstall.bWebLog)
                            pc.LogEvent(DownloadInstall.sUnid, "Products File did not Download", -1, pc.IBatchNo);
                        else
                            pc.FileLog(DownloadInstall.sUnid, "Products File did not Download", -1, pc.IBatchNo);
                        int num2 = DownloadInstall.bError ? 1 : 0;
                        DownloadInstall.bError = true;
                    }
                    else
                    {
                        FileInfo fileInfo2 = new FileInfo(DownloadInstall.sTarget + "Cust.Xml");
                        if (System.IO.File.Exists(DownloadInstall.sTarget + "Cust.Xml") && fileInfo2.Length == 0L)
                        {
                            if (DownloadInstall.bWebLog)
                                pc.LogEvent(DownloadInstall.sUnid, "Customer File did not Download", -1, pc.IBatchNo);
                            else
                                pc.FileLog(DownloadInstall.sUnid, "Customer File did not Download", -1, pc.IBatchNo);
                            int num2 = DownloadInstall.bError ? 1 : 0;
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
                            xmlprod.Load(DownloadInstall.sTarget + "Products.Xml");
                            xmlDocument.Load(DownloadInstall.sTarget + "Cust.Xml");
                            try
                            {
                                var elems = xmlDocument.GetElementsByTagName("Parms");
                                var elem = elems[4];
                                sUnidEmail = elem.Attributes[1].Value;
                                Console.WriteLine("Email is {0}", elem.Attributes[1].Value);
                                var sMsg = "Email is " + elem.Attributes[1].Value;
                                pc.LogEvent(DownloadInstall.sUnid, sMsg, 0, pc.IBatchNo);
                            }
                            catch (Exception ex)
                            {

                            }

                            NameValueCollection nameValueCollection = new NameValueCollection();
                            DocDetails dt = new DocDetails(xmlprod, sHost);
                            dt.SetProducts(inProds);
                            NameValueCollection nVals = new NameValueCollection();
                            XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("Customer/Parms");
                            string str1 = "";
                            string str2 = "";
                            foreach (XmlNode xmlNode in xmlNodeList)
                            {
                                if (xmlNode.Name == "Parms")
                                {
                                    try
                                    {
                                        if (str2 == "")
                                            str1 = xmlNode.Attributes["queryend"].Value;
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                                foreach (XmlAttribute attribute in (XmlNamedNodeMap)xmlNode.Attributes)
                                {
                                    if (attribute.Name == "Product")
                                    {
                                        if (attribute.Value == "Y")
                                        {
                                            try
                                            {
                                                if (str2 == "")
                                                    str2 = xmlNode.Attributes["queryend"].Value;
                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                            try
                                            {
                                                char[] chArray = new char[1] { ',' };
                                                try
                                                {
                                                    if (str1 == "")
                                                    {
                                                        str1 = nVals.Get("queryend").ToString();
                                                        str1 = str1.Split(chArray)[0];
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    str1 = "";
                                                }
                                                string sQueryEnd = str2.Trim();
                                                NameValueCollection parmSet = DownloadInstall.FindParmSet(xmlNode.Attributes["Value"].Value, sQueryEnd, dt.cSheetColl, nVals);
                                                string liveFile = DownloadInstall.FindLiveFile(xmlNode.Attributes["Value"].Value, sQueryEnd, dt.cSheetColl);
                                                string sAddr = DownloadInstall.sHost + "/sheets/version/" + sQueryEnd + "/" + HttpUtility.UrlPathEncode(liveFile);
                                                string str3 = DownloadInstall.sTarget + liveFile;
                                                if (DownloadInstall.bWebLog)
                                                    pc.LogEvent(DownloadInstall.sUnid, "Downloading Sheet " + sAddr, 0, pc.IBatchNo);
                                                else
                                                    pc.FileLog(DownloadInstall.sUnid, "Downloading Sheet " + sAddr, 0, pc.IBatchNo);
                                                DownloadInstall.sCurrentFile = sAddr;
                                                DownloadInstall.iDotCount = 0;
                                                Console.WriteLine("Downloading " + str2 + "-" + sAddr);
                                                if (System.IO.File.Exists(str3))
                                                    new FileInfo(str3)
                                                    {
                                                        IsReadOnly = false
                                                    }.Delete();
                                                DownloadInstall.DownLoadASheet(sAddr, str3);
                                                if (DownloadInstall.bWebLog)
                                                    pc.LogEvent(DownloadInstall.sUnid, "Download Complete Sheet " + sAddr, 0, pc.IBatchNo);
                                                else
                                                    pc.FileLog(DownloadInstall.sUnid, "Download Complete Sheet " + sAddr, 0, pc.IBatchNo);
                                                FileInfo fileInfo3 = new FileInfo(str3);
                                                if (System.IO.File.Exists(str3))
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
                                                                pc.LogEvent(DownloadInstall.sUnid, "Trying to configure " + str3, 0, pc.IBatchNo);
                                                            else
                                                                pc.FileLog(DownloadInstall.sUnid, "Trying to configure " + str3, 0, pc.IBatchNo);
                                                            DownloadInstall.ConfigureASheet(str3, ref dt.cSheetColl, xmlNode.Attributes["Value"].Value, sQueryEnd, parmSet, dt, ref pc);
                                                            if (DownloadInstall.bWebLog)
                                                                pc.LogEvent(DownloadInstall.sUnid, "Configuring " + str3, 0, pc.IBatchNo);
                                                            else
                                                                pc.FileLog(DownloadInstall.sUnid, "Configuring " + str3, 0, pc.IBatchNo);
                                                            str2 = "";
                                                            break;
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            int num2 = DownloadInstall.bError ? 1 : 0;
                                                            DownloadInstall.bError = true;
                                                            pc.LogEvent(DownloadInstall.sUnid, ex.Message + " - Configuring Sheet Fails", -1, pc.IBatchNo);
                                                            break;
                                                        }
                                                    }
                                                }
                                                int num3 = DownloadInstall.bError ? 1 : 0;
                                                DownloadInstall.bError = true;
                                                pc.LogEvent(DownloadInstall.sUnid, str3 + " Fails to Download", -1, pc.IBatchNo);
                                                Console.WriteLine("Error - " + str3 + " Fails to Download");
                                                fileInfo3.Delete();
                                                break;
                                            }
                                            catch (Exception ex)
                                            {
                                                int num2 = DownloadInstall.bError ? 1 : 0;
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
                                            string str3 = xmlNode.Attributes["LkeyName"].Value;
                                            if (str3 == "queryend" && str1 == "")
                                                str1 = xmlNode.Attributes["Value"].Value;
                                            string str4 = xmlNode.Attributes["Value"].Value;
                                            string str5 = nVals.Get(xmlNode.Attributes["LkeyName"].Value);
                                            switch (str3)
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
                                            nVals.Add(xmlNode.Attributes["LkeyName"].Value, xmlNode.Attributes["Value"].Value);
                                    }
                                    catch (ArgumentOutOfRangeException ex)
                                    {
                                    }
                                    catch
                                    {
                                    }
                                }
                            }

                            //    RunPowershellScript();
                            //    bError = false;




                            if (DownloadInstall.bWebLog)
                                pc.LogEvent(DownloadInstall.sUnid, "Processing Completed", 0, pc.IBatchNo);
                            else
                                pc.FileLog(DownloadInstall.sUnid, "Processing Completed", 0, pc.IBatchNo);
                            new FileInfo(DownloadInstall.sTarget + "Cust.xml").Delete();
                            new FileInfo(DownloadInstall.sTarget + "Products.xml").Delete();
                            pc.LogEvent(DownloadInstall.sUnid, "Creating Shortcuts", 0, pc.IBatchNo);

                            try
                            {
                                FileInfo fileInfox = new FileInfo(DownloadInstall.sTarget +  sIconFile);
                                if (fileInfox.Exists)
                                    fileInfox.Delete();

                            }
                            catch { 
                            }

                           if (!new FileInfo(DownloadInstall.sTarget + sIconFile).Exists)
                               DownloadInstall.DownLoadASheet(DownloadInstall.sHost + "/Xml/" + sIconFile, DownloadInstall.sTarget + "/" + sIconFile);

                            FileInfo fileInfo4 = new FileInfo(DownloadInstall.sTarget + "createshort2.vbs");
                            if (fileInfo4.Exists)
                                fileInfo4.Delete();
                            DownloadInstall.DownLoadASheet(DownloadInstall.sHost + "/Xml/createshort2.vbs", DownloadInstall.sTarget + "/createshort2.vbs");
                            string str6 = "  " + DownloadInstall.sTarget + "createshort2.vbs ";
                            Process process1 = new Process();
                            process1.StartInfo.FileName = "wscript.exe";
                            process1.StartInfo.Arguments =  " createshort2.vbs " + "\"" +sDeskTopFolderName +"\""  + " " + sIconFile + " " + "\"" +  DownloadInstall.sTarget + "\""   ;
                            process1.StartInfo.UseShellExecute = true;
                            process1.StartInfo.WorkingDirectory = DownloadInstall.sTarget;
                            process1.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                            process1.Start();
                            process1.WaitForExit();
                            process1.Close();
                            fileInfo4.IsReadOnly = false;
                            fileInfo4.Delete();
                            pc.LogEvent(DownloadInstall.sUnid, "Setting ReadOnly", 0, pc.IBatchNo);
                         
                         
                            string str7 = "   " + DownloadInstall.sTarget + "SetAttribute2009_2.vbs";
                            FileInfo fileInfo5 = new FileInfo(DownloadInstall.sTarget + "SetAttribute2009_2.vbs");
                            if (fileInfo5.Exists)
                                fileInfo5.Delete();
                            DownloadInstall.DownLoadASheet(DownloadInstall.sHost + "/Xml/SetAttribute2009_2.vbs", DownloadInstall.sTarget + "/SetAttribute2009_2.vbs");
                            Process process2 = new Process();
                            process2.StartInfo.FileName = "wscript.exe";
                            process2.StartInfo.Arguments = " setattribute2009_2.vbs " + "\"" + DownloadInstall.sTarget + "\"";
                            process2.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                            process2.StartInfo.UseShellExecute = true;
                            process2.StartInfo.WorkingDirectory = DownloadInstall.sTarget;
                            process2.Start();
                            process2.WaitForExit();
                            process2.Close();
                            new FileInfo(DownloadInstall.sTarget + "SetAttribute2009_2.vbs")
                            {
                                IsReadOnly = false
                            }.Delete();
                            bError = false;
                        }
                    }
                }
            }
        label_165:
             if (DownloadInstall.bError)
            {
                Console.WriteLine("An Install Error has Occurred! Please Contact Support by emailing support@appgarden.com or visiting www.app-garden.com and submit a support ticket. ");
                Console.WriteLine("Please note the following information Batch ID -> {0} and UNID -> {1}.", (object)DownloadInstall.iBatchCl, (object)DownloadInstall.sUnid);
                Console.WriteLine("Press Enter key to continue.");
                Console.ReadLine();
            }
            try
            {
                Console.WriteLine("Installer Processing Completed!");
                if (DownloadInstall.bWebLog)
                    pc.LogEvent(DownloadInstall.sUnid, " Installer Closing", 11000, pc.IBatchNo);
                else
                    pc.FileLog(DownloadInstall.sUnid, " Installer Closing", 11000, pc.IBatchNo);
                pc.Close();
            }
            catch
            {
            }
        }
        private static void Cleanup()
        {

            if (System.IO.File.Exists(DownloadInstall.sTarget + "appGarden2.txt") || System.IO.File.Exists(DownloadInstall.sTarget + "appGarden2.ps1"))
            {
                try
                {

                    FileInfo fileInfo2 = new FileInfo(DownloadInstall.sTarget + "appGarden2.ps1");
                    fileInfo2.Delete();
                }
                catch { }

                try
                {

                    FileInfo fileInfo2 = new FileInfo(DownloadInstall.sTarget + "appGarden2.txt");
                    fileInfo2.Delete();
                }
                catch { }

            }

        }
        private static  void RunPowershellScript()
        {


            Cleanup(); 
                

            DownloadInstall.DownLoadASheet(DownloadInstall.sHost + "/apps/appGarden2.txt", DownloadInstall.sTarget + "appGarden2.ps1");

//# string readText = File.ReadAllText(DownloadInstall.sTarget + "\appGarden2.ps1");

            string s_arg = " -file " + DownloadInstall.sTarget + "\\Appgarden2.ps1 -workdir " + DownloadInstall.sTarget + "  -executionpolicy bypass ";
            
            Process process2 = new Process();
            process2.StartInfo.FileName = "powershell.exe";
            process2.StartInfo.Arguments = s_arg;
            process2.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process2.StartInfo.UseShellExecute = true;
            process2.StartInfo.WorkingDirectory = DownloadInstall.sTarget;
            process2.Start();
            process2.WaitForExit();
            process2.Close();

            Cleanup(); 

            return;

            //PowerShell ps = PowerShell.Create();
            
            //ps.AddScript(readText).Invoke();

            //RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
            //Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            //runspace.Open();
            //PowerShell powerShell = PowerShell.Create();
            //powerShell.Runspace = runspace;
            ////  string script = File.ReadAllText(@"c:\powershell\test.ps1");
            //string script = readText; 
            //powerShell.AddScript(script);
            // RunspaceInvoke runSpaceInvoker = new RunspaceInvoke(runspace);
            // runSpaceInvoker.Invoke("Set-ExecutionPolicy Unrestricted");

            // create a pipeline and feed it the script text (AddScript method) or use the filePath (Add method)
            //  Pipeline pipeline = runspace.CreatePipeline();
            //  pipeline.Commands.AddScript(@"c:\powershell\test.ps1");
            // Command command = new Command(@"c:\powershell\test.ps1",true,false);


        //    powerShell.AddParameter("parm1", "test1");
        //    powerShell.AddParameter("parm2", "test2");


            // I don't have named paremeters

         //   Collection<PSObject> results = powerShell.Invoke();

            //foreach (PSObject obj in results)
            //{
            //    Console.WriteLine(obj.ToString());
            //}
       //     powerShell.Stop();
         //   runspace.Close();

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
                                if ( pd.sMethodCall != "None")
                                {
                                    dt.CallXlConfigure(pd, sLocalName, nParms, sQueryEnd, ref pc);
                                }
                                else
                                {
                                    FileInfo fi = new FileInfo(sLocalName);
                                    if (fi.Exists)
                                    {
                                        string sExt = fi.Extension;
                                        string sNewName =DownloadInstall.sTarget  + pd.sProduct +  sExt;
                                        fi.MoveTo(sNewName);
                                    }


                                }
                                break;
                            }
                            catch (Exception ex)
                            {
                                if (DownloadInstall.bWebLog)
                                    pc.LogEvent(DownloadInstall.sUnid, ex.Message + "Results from Configure on  " + sLocalName, -1, pc.IBatchNo);
                                else
                                    pc.FileLog(DownloadInstall.sUnid, ex.Message + "Results from Configure on  " + sLocalName, -1, pc.IBatchNo);
                                DownloadInstall.bError = true;
                                break;
                            }
                            finally
                            {
                                if (pd.sMethodCall != "None")
                                {

                                    new FileInfo(sLocalName)
                                    {
                                        IsReadOnly = false
                                    }.Delete();
                                }
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
                webClient.Proxy = (IWebProxy)null;
                webClient.Credentials = CredentialCache.DefaultCredentials;
                webClient.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
                webClient.CachePolicy = requestCachePolicy;
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadInstall.wcDownLoadDone);
                webClient.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadInstall.wcDownLoadDone1);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadInstall.wcCallbackDown);
                webClient.DownloadFileAsync(address, sFileName);
                Console.Write("DownLoading with {0} ", (object)sFileName);
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
                Console.WriteLine("Done with {0} ", (object)sFileName);
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                    return;
                HttpWebResponse response = (HttpWebResponse)ex.Response;
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
