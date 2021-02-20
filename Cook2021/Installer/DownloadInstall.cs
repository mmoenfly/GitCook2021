// Start of the Beta  15 May 2012 


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Net.Cache;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Collections;
using System.Web;
//add 15 May 2012
using System.Reflection;
using System.Configuration;

namespace Installer
{
    class DownloadInstall
    {

        DocDetails dt;
        private ArrayList nProd, nMethColl;
        private NameValueCollection nMethods;
        private static string sHost = @"http://ccisupportsite.com", sTarget = @"C:\CCI\", sUnid, sCurrentFile, sEmail, SxmlRet;
        private static int iBatchCl, iLogCount = 0, iDotCount = 0;
        private static bool bWebLog = true, bError;

        struct sTheMethods
        {
            public string sSheet;
            public string sPassed;
            public string sKeyName;
            public string sParent;

            public sTheMethods(string nSheet, string nPassed, string nKeyed, string nParent)
            {
                sSheet = nSheet;
                sPassed = nPassed;
                sKeyName = nKeyed;
                sParent = nParent;
            }
        }
        struct sTheProducts
        {
            public string sProduct;
            public string sReadonly;
            public string sFileName;
            public sTheProducts(string sProd, string sFile, string sRead)
            {
                sProduct = sProd;
                sFileName = sFile;
                sReadonly = sRead;
            }
        }

        static void Main(string[] args)
        {

            

            /* Arguments
             * 0 host
             * 1 target directory
             * 2 unid
             * 
             */
            bError = false;


            
            int iCnt = 0;
            Console.WriteLine("Installer Begins. {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            string     sQuote = "\"";
            
            string sCmdLine = "";
            try
            {
               

                sHost = args[0];
                sUnid = args[1];
                sEmail = args[2];
                sTarget = args[3];
                sTarget = sTarget.Replace(sQuote, "");

                Console.WriteLine("Email is {0}", sEmail);
                string currentAssemblyDirectoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


            }
                
            catch (Exception ex)
            {
                sTarget = sTarget.Replace(sQuote, "");
                sHost = @"http://ccisupportsite.com";
                //      sTarget = @"c:\As400da\";
                //       sUnid = "4621E8E7157912B185257829006ECFC0";
                //      sEmail = "mmoen@test.com";
                Console.WriteLine("Error - Parameter Signature Invalid " + ex.Message);
                // throw ex;

                // fix the install directory 
                if (!Directory.Exists(sTarget))
                {

                    iCnt = 0;
                    bool bValid = false;
                    while (iCnt <= 2 && !bValid)
                    {
                        Console.WriteLine("Problem with Target Location");
                        Console.WriteLine("The target {0} cannot be Validated Please Reenter:", sTarget);
                        sTarget = Console.ReadLine();
                        Console.WriteLine("");
                        sTarget = sTarget.Trim();
                        iCnt += 1;

                        if (Directory.Exists(sTarget)) bValid = true;
                    }
                    if (iCnt > 2 && !bValid)
                        bError = true;

                }


                // fix the email address 
               
                Console.WriteLine("Problem with Email Address");
                Console.WriteLine("The email address {0} cannot be Validated Please Reenter:", sEmail);
                sEmail = Console.ReadLine();
                Console.WriteLine("");
                sEmail = sEmail.Trim();
            }



            iBatchCl = -999;
            // Lets Id the box
             PcId  pc = new PcId(sUnid, sTarget, sEmail);
            pc.sEmail = sEmail;

            if (bError) goto Done;


            // 21 Jan 2011 - Added support for godaddy webservice and Id on box 
            try
            {
                if (sEmail.Contains(@"cookconsulting.net"))
                {
                     
                    goto Next;
                }
                
                if (pc.ChkInstall(sUnid) == "stop")
                {
                    Console.WriteLine("This install is terminated - This license code has been previously installed");
                    bError = true;
                    goto Done;
                }
            Next:
                pc.LogEvent();
                iBatchCl = pc.IBatchNo;
                bWebLog = true;
                if (iBatchCl == -1)
                {
                    // logging is not available
                    bWebLog = false;
                    pc.CreateLogFile(sTarget);
                }

            }
            catch (Exception ex)
            {
                bWebLog = false;
             //   pc.CreateLogFile(sTarget);
            }
           
            //sUnid = sUnid.Trim();

            while (iCnt <= 2 && sUnid.Length != 32)
            {
                Console.Write("Problem with the Customer License Code Check your Cut and Past");
                Console.Write("The unid {0} cannot be Validated Please Reenter:", sUnid);
                sUnid = Console.ReadLine();
                Console.WriteLine("");
                sUnid = sUnid.Trim();
                iCnt += 1;
            }
            if (sUnid.Length != 32)
            {
                Console.WriteLine("The unid {0} cannot be Validated.", sUnid);
                bError = true;
                goto Done;
            }

            Console.WriteLine("Installer is running Batch ID -> {0} for UNID -> {1}.", iBatchCl, sUnid);

            // Removed
            // 31 Jan 2012

            if (sTarget.EndsWith("\\"))
            { }
            else
                sTarget += "\\";

            ArrayList nProd = new ArrayList();

            //if (args.GetUpperBound(0) > 0)
            //    sUnid = args[0];

            // make sure our work directory exists

            FileInfo fi;
            try
            {
                if (!Directory.Exists(sTarget)) Directory.CreateDirectory(sTarget);
            }
            catch (Exception ex) { }

            ;
            // 23 Juin 2011
            // MJ Moen
            // added to replace kill switch in script
            try
            {
                DirectoryInfo di = new DirectoryInfo(sTarget);
                foreach (FileInfo tfi in di.GetFiles())
                {
                    if (tfi.Extension == ".xls" || tfi.Extension == ".xlsm")
                    {
                        tfi.IsReadOnly = false;
                        tfi.Delete();
                    }
                }

                di = new DirectoryInfo(@"c:\as400da\");

                foreach (FileInfo tfi in di.GetFiles())
                {
                    if (tfi.Extension == ".xls" || tfi.Extension == ".xlsm")
                    {
                        tfi.IsReadOnly = false;
                        tfi.Delete();
                    }
                }
                string sDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\AS400 SpreadSheets";
                di = new DirectoryInfo(sDesktop);
                foreach (FileInfo tfi in di.GetFiles())
                {
                    if (tfi.Extension == ".lnk")
                    {
                        tfi.IsReadOnly = false;
                        tfi.Delete();
                    }
                }
                // Clear CCI Sheets Short Cuts
                // 31 Jan 2012
                 sDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\CCI SpreadSheets";
                di = new DirectoryInfo(sDesktop);
                foreach (FileInfo tfi in di.GetFiles())
                {
                    if (tfi.Extension == ".lnk")
                    {
                        tfi.IsReadOnly = false;
                        tfi.Delete();
                    }
                }
                di = new DirectoryInfo(sTarget);

                foreach (FileInfo tfi in di.GetFiles())
                {
                    if (tfi.Extension == ".lnk")
                    {
                        tfi.IsReadOnly = false;
                        tfi.Delete();
                    }
                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }

            try
            {
                fi = new FileInfo(sTarget + @"Products.xml");
                fi.IsReadOnly = false;
                File.Delete(sTarget + @"Products.xml");

            }
            catch { }


            try
            {
                fi = new FileInfo(sTarget + @"Cust.xml");
                fi.IsReadOnly = false;
                File.Delete(sTarget + @"Cust.xml");

            }
            catch { }



            try
            {
                string sFileName = "";
                sFileName = sHost + @"/Xml/Products.xml";


                DownLoadASheet(sFileName, sTarget + @"Products.Xml");

            

                if (bWebLog)
                    pc.LogEvent(sUnid, "File Download Complete on "
                       + sTarget + @"Products.Xml", 0, pc.IBatchNo);
                else
                    pc.FileLog(sUnid, "File Download Complete on "
                        + sTarget + @"Products.Xml", 0, pc.IBatchNo);


                dws.GetFullCustomersService customersService = new dws.GetFullCustomersService();
               NetworkCredential  networkCredential = new NetworkCredential("mmoen", "bobbob1", "");
              
                customersService.Credentials = (ICredentials)networkCredential;
                try
                {
                  SxmlRet  = customersService.GETFULLCUSTOMERS("changeor.nsf", "XMLOUTPUT", sUnid);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message  ); 

                }


                sFileName = sHost + @"/Xml/" + sUnid + @"/Cust.Xml";
                DownLoadASheet(sFileName, sTarget + @"Cust.Xml");

                if (bWebLog)
                    pc.LogEvent(sUnid, "File Download Complete on "
                   + sTarget + @"Cust.Xml", 0, pc.IBatchNo);
                else
                    pc.FileLog(sUnid, "File Download Complete on "
                           + sTarget + @"Cust.Xml", 0, pc.IBatchNo);





            }
            catch (Exception ex)
            {

                bError = bError || true;

                if (bWebLog)
                    pc.LogEvent(sUnid, ex.Message + " - Program Abort", -1, pc.IBatchNo);
                else
                    pc.FileLog(sUnid, ex.Message + " - Program Abort", -1, pc.IBatchNo);

                if (bWebLog)
                    pc.LogEvent(sUnid, ex.Message + "Program Abort", 11000, pc.IBatchNo);
                else
                    pc.FileLog(sUnid, ex.Message + "Program Abort", 11000, pc.IBatchNo);

                // test
                try
                {
                    pc.Close();
                }
                catch { }

                Console.WriteLine("Program Stopping - " + ex.Message);
                goto Done;


            }


            fi = new FileInfo(sTarget + @"Products.Xml");
            if (File.Exists(sTarget + @"Products.Xml") && fi.Length == 0)
            {

                if (bWebLog)
                    pc.LogEvent(sUnid, "Products File did not Download", -1, pc.IBatchNo);
                else
                    pc.FileLog(sUnid, "Products File did not Download", -1, pc.IBatchNo);

                bError = bError || true;

                goto Done;




            }

            fi = new FileInfo(sTarget + @"Cust.Xml");

            if (File.Exists(sTarget + @"Cust.Xml") && fi.Length == 0)
            {

                if (bWebLog)
                    pc.LogEvent(sUnid, "Customer File did not Download", -1, pc.IBatchNo);
                else
                    pc.FileLog(sUnid, "Customer File did not Download", -1, pc.IBatchNo);
                bError = bError || true;

                goto Done;





            }

            if (bWebLog)
                pc.LogEvent(sUnid, "File Download Complete ", 0, pc.IBatchNo);
            else
                pc.FileLog(sUnid, "File Download Complete ", 0, pc.IBatchNo);
            // Process the xmlDoc

            XmlDocument xmldoc = new XmlDocument();
            XmlDocument xmldocProducts = new XmlDocument();

            xmldocProducts.Load(sTarget + @"Products.Xml");


            System.Text.UTF8Encoding str = new System.Text.UTF8Encoding();

            string sContents;
            StreamReader sr = new StreamReader(sTarget + @"Cust.Xml");
            sContents = sr.ReadToEnd();

            sr.Close();
            sr.Dispose();

            byte[] b = Convert.FromBase64String(sContents);


            sContents = System.Text.Encoding.UTF8.GetString(b);

          
            StreamWriter sw = new StreamWriter(sTarget + @"Cust.Xml", false);

            sw.Write(sContents);
            sw.Close();
            sw.Dispose();

            if (bWebLog)
                pc.LogEvent(sUnid,sContents, 0, pc.IBatchNo);
            else
                pc.FileLog(sUnid,sContents, 0, pc.IBatchNo);


            xmldoc.Load(sTarget + @"Cust.Xml");

            NameValueCollection nMethods = new NameValueCollection();
            //       LoadMethods(ref nMethods, xmldocMethods); 

            //  Call the Config Clas
            DocDetails dt = new DocDetails(xmldocProducts);

            dt.SetProducts(nProd);

            NameValueCollection nParms = new NameValueCollection();

            XmlNode root;

            root = xmldoc.DocumentElement;

            XmlNodeList nodes;


            //  nodes = root.SelectNodes("Customer/Parms[@Sheet='Y']");
            nodes = root.SelectNodes("Customer/Parms");
            // Build all the nodes, mark which are sheets, which are parms.
            // Match it by queryand and sheet to determine the parms.

            string sQryP = "", sQry = "";
            foreach (XmlNode n in nodes)
            {

                if (n.Name == "Parms")
                {
                    try
                    {

                        if (sQry == "")
                            sQryP = n.Attributes["queryend"].Value;




                    }
                    catch (Exception ex)
                    { }
                }




                foreach (XmlAttribute att in n.Attributes)
                {
                    Debug.Print("att value={0} Attribute Name{1}", att.Value, att.Name);

                    if (att.Name == "Product" && att.Value == "Y")
                    {
                        Debug.Print("Got a Product{0}", n.Attributes["Value"].Value);

                        //     string sRemoteName = "http://localhost/sheets/" + n.Attributes["Value"].Value + ".xls";
                        //     string sLocalName = @"C:\cci\" + n.Attributes["Value"].Value + ".xls";


                        //try to get the attribute based queryend 
                        try
                        {

                            if (sQry == "")
                                sQry = n.Attributes["queryend"].Value;




                        }
                        catch (Exception ex)
                        { }


                        try
                        {
                            char[] delimiterChars = { ',' };
                            string[] sqrysplit;
                            try
                            {
                                if (sQryP == "")
                                {
                                    sQryP = nParms.Get("queryend").ToString();
                                    sqrysplit = sQryP.Split(delimiterChars);
                                    sQryP = sqrysplit[0];
                                }
                            }
                            catch (Exception ex)
                            {
                                sQryP = @"070110";
                            }


                            string sQueryEnd;
                            // logic is there is a node queryend attribute use it
                            // else use the file queryend

                            // 16 July 2012
                            // Resolve the correct queryend only from cust.xml 
                            //if (sQry == "")
                            //    sQueryEnd = sQryP;
                            //else
                                sQueryEnd = sQry; 


                        



                            NameValueCollection tParms = FindParmSet(n.Attributes["Value"].Value, sQueryEnd, dt.cSheetColl, nParms);
                            string sFile = FindLiveFile(n.Attributes["Value"].Value, sQueryEnd, dt.cSheetColl);
                            string sRemoteName = sHost + @"/sheets/version/" + sQueryEnd + @"/" + HttpUtility.UrlPathEncode(sFile);
                            string sLocalName = sTarget + sFile;

                            if (bWebLog)
                                pc.LogEvent(sUnid, "Downloading Sheet " + sRemoteName, 0, pc.IBatchNo);
                            else
                                pc.FileLog(sUnid, "Downloading Sheet " + sRemoteName, 0, pc.IBatchNo);

                            sCurrentFile = sRemoteName;
                            iDotCount = 0;

                            if (File.Exists(sLocalName))
                            {
                                FileInfo fi2 = new FileInfo(sLocalName);
                                fi2.IsReadOnly = false;
                                fi2.Delete();
                            }

                            DownLoadASheet(sRemoteName, sLocalName);

                            if (bWebLog)
                                pc.LogEvent(sUnid, "Download Complete Sheet " + sRemoteName, 0, pc.IBatchNo);
                            else
                                pc.FileLog(sUnid, "Download Complete Sheet " + sRemoteName, 0, pc.IBatchNo);

                            

                            bool bProcess = false;

                            fi = new FileInfo(sLocalName);





                            if (File.Exists(sLocalName) && fi.Length > 0)
                            {
                                try
                                {
                                    // inc the logger 
                                    iLogCount += 1;

                                    if (!pc.bCust)
                                    {
                                        pc.LogEvent(sUnid, "", 10000, pc.IBatchNo);
                                        pc.bCust = true;
                                    }
                                    if (bWebLog)
                                        pc.LogEvent(sUnid, "Trying to configure " + sLocalName, 0, pc.IBatchNo);
                                    else
                                        pc.FileLog(sUnid, "Trying to configure " + sLocalName, 0, pc.IBatchNo);


                                    ConfigureASheet(sLocalName, ref  dt.cSheetColl, n.Attributes["Value"].Value, sQueryEnd, tParms, dt, ref pc);

                                    if (bWebLog)
                                        pc.LogEvent(sUnid, "Configuring " + sLocalName, 0, pc.IBatchNo);
                                    else
                                        pc.FileLog(sUnid, "Configuring " + sLocalName, 0, pc.IBatchNo);


                               
                                    sQry = "";
                                }

                                catch (Exception ex)
                                {
                                    bError = bError || true;

                                    pc.LogEvent(sUnid, ex.Message + " - Configuring Sheet Fails", -1, pc.IBatchNo);
                                }

                            }
                            else
                            { fi.Delete(); }
                            break;
                        }
                        catch (Exception ex)
                        {
                            bError = bError || true;

                            if (bWebLog)
                                pc.LogEvent(sUnid, ex.Message + " - Downloading Sheet", -1, pc.IBatchNo);
                            else
                                pc.FileLog(sUnid, ex.Message + " - Downloading Sheet", -1, pc.IBatchNo);

                            Console.WriteLine("Error - " + ex.Message);

                        }

                    }
                    else
                    { // it is not a sheet
                        try
                        {
                            if (nParms.Get(n.Attributes["LkeyName"].Value) != null)
                            {//skip it 
                                string sKeyName = n.Attributes["LkeyName"].Value;
                                Debug.Print("Key name is {0}", sKeyName); 
                              
                                if (sKeyName == "queryend"  && sQryP =="")
                                    sQryP = n.Attributes["Value"].Value;



                                string sKeyValue = n.Attributes["Value"].Value;
                                string sCurrentVal = nParms.Get(n.Attributes["LkeyName"].Value);
                                Debug.Print("Atrib {0} ", n.Attributes["Value"].Value);
                                //       if (sKeyValue == sCurrentVal) goto Lp1; 
                                switch (sKeyName)
                                {

                                    //salariedcodes
                                    //salariedwithsupp
                                    //certifiedcodes
                                    //princodes
                                    //suppcodes


                                    case "suppcodes":
                                        nParms.Remove(n.Attributes["LkeyName"].Value);
                                        nParms.Add(n.Attributes["LkeyName"].Value
                                           , sCurrentVal
                                           + "," + sKeyValue);
                                        break;
                                    case "princodes":
                                        nParms.Remove(n.Attributes["LkeyName"].Value);
                                        nParms.Add(n.Attributes["LkeyName"].Value
                                           , sCurrentVal
                                           + "," + sKeyValue);
                                        break;
                                    case "certifiedcodes":
                                        nParms.Remove(n.Attributes["LkeyName"].Value);
                                        nParms.Add(n.Attributes["LkeyName"].Value
                                           , sCurrentVal
                                           + "," + sKeyValue);
                                        break;

                                    case "salariedwithsupp":
                                        nParms.Remove(n.Attributes["LkeyName"].Value);
                                        nParms.Add(n.Attributes["LkeyName"].Value
                                           , sCurrentVal
                                           + "," + sKeyValue);
                                        break;
                                    case "salariedcodes":
                                        nParms.Remove(n.Attributes["LkeyName"].Value);
                                        nParms.Add(n.Attributes["LkeyName"].Value
                                           , sCurrentVal
                                           + "," + sKeyValue);
                                        break;
                                    case "otherdepts":
                                        nParms.Remove(n.Attributes["LkeyName"].Value);
                                        nParms.Add(n.Attributes["LkeyName"].Value
                                            , sCurrentVal
                                            + "," + sKeyValue);
                                        break;
                                    case "budgetind":
                                        nParms.Remove(n.Attributes["LkeyName"].Value);
                                        nParms.Add(n.Attributes["LkeyName"].Value
                                           , sCurrentVal
                                           + "," + sKeyValue);
                                        break;
                                    case "specialind":
                                        nParms.Remove(n.Attributes["LkeyName"].Value);
                                        nParms.Add(n.Attributes["LkeyName"].Value
                                           , sCurrentVal
                                           + "," + sKeyValue);
                                        break;
                                    case "username":
                                        pc.setUser(n.Attributes["Value"].Value);
                                        break;
                                    case "customername":
                                        pc.setCust(n.Attributes["Value"].Value);
                                        break;
                                    default:
                                        break;
                                }
                                goto Lp1;
                            }

                            else
                            { nParms.Add(n.Attributes["LkeyName"].Value, n.Attributes["Value"].Value); }
                        }
                        catch (ArgumentOutOfRangeException ex)
                        { // is missing
                        }
                        catch
                        { }


                    }

                Lp1: ;
                };
            }

            // Calling Configuration Class 
            //  dt = new DocDetails(xmldocProducts );
            //  dt.SetProducts(nProd);
            //   dt.CallBudgetMethod(new sTheProducts()); 
            if (bWebLog)
                pc.LogEvent(sUnid, "Processing Completed", 0, pc.IBatchNo);
            else
                pc.FileLog(sUnid, "Processing Completed", 0, pc.IBatchNo);
            pc.Close();

            FileInfo
             fi1 = new FileInfo(sTarget + "Cust.xml");
            fi1.Delete();
            fi1 = new FileInfo(sTarget + "Products.xml");
            fi1.Delete();


        Done:
            if (bError)
            {
                Console.WriteLine("An Install Error has Occurred! Please Contact Support desk @ 800 425 0720 or email supportdesk@cookconsulting.net");
                Console.WriteLine("Please note the following information Batch ID -> {0} and UNID -> {1}.", iBatchCl, sUnid);
                Console.WriteLine("Press Enter key to continue.");
                Console.ReadLine();
            }



            Console.WriteLine("Installer Processing Completed!");

            if (bWebLog)
                pc.LogEvent(sUnid, " Installer Closing", 11000, pc.IBatchNo);
            else
                pc.FileLog(sUnid, " Installer Closing", 11000, pc.IBatchNo);
            return;

        }

        private static NameValueCollection FindParmSet(string sProductName
            , string sQueryEnd
            , ArrayList allProducts
            , NameValueCollection nVals)
        {
            try
            {

                foreach (string s in nVals.Keys)
                { Debug.Print("Keys={0},Vals={1}", s, nVals.Get(s)); }
                string sRet = "";
                foreach (object o in allProducts)
                {
                    Products pd = (Products)o;

                    Debug.Print("Product is {0}", pd.sProduct);
                    if (pd.sProduct.Trim() == sProductName.Trim() && pd.sQueryEnd.Trim() == sQueryEnd.Trim())
                    {  // Got It
                        // Marshall the Parms

                        NameValueCollection nvParms = new NameValueCollection();
                        foreach (string sk in pd.Nvp.AllKeys)
                        {
                            Debug.Print("Value = {0} {1}   {2}", sk, pd.Nvp.Get(sk), nVals.Get(sk));
                            if (pd.Nvp.Get(sk) == "Y")
                            { nvParms.Add(sk, nVals.Get(sk)); }
                        }
                        return nvParms;

                        break;
                    }


                }
                throw new Exception("File :" + sProductName + " is not found in Products.xml for parameters");
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string FindLiveFile(string sProductName
            , string sQueryEnd
            , ArrayList allProducts)
        {
            try
            {
                string sRet = "";
                foreach (object o in allProducts)
                {
                    Products pd = (Products)o;
                    Debug.Print("Product is {0}", pd.sProduct);
                    if (pd.sProduct.Trim() == sProductName.Trim() && pd.sQueryEnd.Trim() == sQueryEnd.Trim())
                    {  // Got It
                        // Marshall the Parms


                        return pd.sFileName;

                        break;
                    }


                }
                throw new Exception("File :" + sProductName + " is not found in Products.xml");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private static void ConfigureASheet(
            string sLocalName
            , ref ArrayList allProducts
            , string sProductName
            , string sQueryEnd
            , NameValueCollection nParms
            , DocDetails dt
            , ref PcId pc
            )
        {
            /*
             * Sub to Call config on the Sheet with the Method 
             *  
             *
             */
            try
            {
                foreach (object o in allProducts)
                {
                    Products pd = (Products)o;
                    if (pd.sProduct.Trim() == sProductName.Trim() && pd.sQueryEnd.Trim() == sQueryEnd.Trim())
                    {  // Got It
                        // Marshall the Parms
                        try
                        {
                            pc.LogEvent(sUnid
                                , "At dt.CallXlConfigure for " + sLocalName
                                , 0
                                , pc.IBatchNo);
                            dt.sTarget = sTarget;
                            dt.CallXlConfigure(pd, sLocalName, nParms,sQueryEnd, ref pc);
                        }
                        catch (Exception ex)
                        {
                            if (bWebLog)
                                pc.LogEvent(sUnid
                                   , ex.Message + "Results from Configure on  " + sLocalName
                                   , -1
                                   , pc.IBatchNo);
                            else
                                pc.FileLog(sUnid
                             , ex.Message + "Results from Configure on  " + sLocalName
                             , -1
                             , pc.IBatchNo);
                            bError = true;
                        }
                        finally
                        {

                            FileInfo fi = new FileInfo(sLocalName);
                            
                            fi.IsReadOnly = false;
                            
                            fi.Delete();
                            
                            if (bWebLog)
                                pc.LogEvent(sUnid
                                   , "Success Results w/Delete from Configure on  " + sLocalName
                                   , 0
                                   , pc.IBatchNo);
                            else
                                pc.FileLog(sUnid
                             , "Success Results w/Delete from Configure on  " + sLocalName
                             , 0
                             , pc.IBatchNo);
                        }
                        break;
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
                XmlNodeList nodelist;
                nodelist = xdoc.SelectNodes("/Sheets/Parms");
                foreach (XmlNode n in nodelist)
                {


                }

            }
            catch (Exception ex) { }

        }

        private static void DownLoadASheet(string sAddr, string sFileName)
        {


            try
            {
                if (File.Exists(sFileName))
                {
                    FileInfo fi = new FileInfo(sFileName);
                    
                    fi.IsReadOnly = false;

                    fi.Delete();
                }
            }
            catch { }


            RequestCachePolicy policy = new RequestCachePolicy(RequestCacheLevel.Reload);

            string address = sAddr;
            //"http://localhost/Sheets/" + sFileName + ".xls";

            System.Threading.AutoResetEvent waiter = new System.Threading.AutoResetEvent(false);
            Uri uri = new Uri(address);
            try
            {
                WebClient wc = new WebClient();
                wc.Proxy = null;
                wc.Credentials = CredentialCache.DefaultCredentials;
                wc.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
                wc.CachePolicy = policy;


                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wcDownLoadDone);
                wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(wcDownLoadDone1);
                wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wcCallbackDown);

                wc.DownloadFileAsync(uri, sFileName);
                Console.Write("DownLoading with {0} ", sFileName);
                while (wc.IsBusy)
                {

                    try
                    {
                        waiter.WaitOne(100);
                    }
                    catch { }
                }

                try
                {
                    Debug.Print("Done");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " Error in Download");
                }


                Console.WriteLine("Done with {0} ", sFileName);
                // Got the File
                // Download the Files 

            }

            catch (WebException ex)
            {



                if (ex.Response is WebResponse)
                {
                    HttpWebResponse o = (HttpWebResponse)ex.Response;
                    Installer.wslogger.Ws_Logger ws = new Installer.wslogger.Ws_Logger();
                    ws.Logger(sUnid, o.StatusDescription, -1, iBatchCl);
                    Console.WriteLine(o.StatusDescription + "  Download Error -" + ex.Message);
                    if (o.StatusCode == HttpStatusCode.OK)
                    { }
                    else
                    {
                        Console.WriteLine(ex.Status + "  Download Error -" + ex.Message);
                        Console.ReadLine();
                        throw ex;
                    }
                }

            }
            catch (Exception ex)
            {
                Installer.wslogger.Ws_Logger ws = new Installer.wslogger.Ws_Logger();
                ws.Logger(sUnid, ex.Message, -1, iBatchCl);

                Console.WriteLine(ex.Message);
                throw ex;
            }

        }
        private static void wcDownLoadDone(object sender, AsyncCompletedEventArgs e)
        {
            // if (e.Error != null)
            //{
            // //    Console.WriteLine("  Error Downloading Async Completed Handler - Message - {0}", e.Error.Message);
            // //    Console.WriteLine(e.Error.StackTrace.ToString());
            // //    Console.WriteLine(e.Error.InnerException.ToString());
            //     Installer.wslogger.WS_Logger ws = new Installer.wslogger.WS_Logger();
            //   ws.Logger(sUnid, sCurrentFile + " - " + e.Error.Message, -1, iBatchCl);
            // //    //    throw new Exception("Downloading Error"); 
            // //       throw   new Exception(e.Error.Message ) ;
            // //    //   Console.ReadLine(); 

            // }

            Console.WriteLine("File Download AsyncComplete Complete");
        }
        private static void wcDownLoadDone1(object sender, DownloadDataCompletedEventArgs e)
        {
            //if (e.Error != null)
            //    {
            //        //Console.WriteLine("Status Code is {0}", 1);
            //        //Console.WriteLine("  Error Downloading Data Completed Handler  - Message - {0}", e.Error.Message);
            //        //Console.WriteLine(e.Error.StackTrace.ToString());
            //        //Console.WriteLine(e.Error.InnerException.ToString());
            //         Installer.wslogger.WS_Logger ws = new Installer.wslogger.WS_Logger();
            //         ws.Logger(sUnid, sCurrentFile + " - " + e.Error.Message, -1, iBatchCl);
            //        //throw new Exception("Downloading Error");
            //     //   throw new Exception(e.Error.Message);
            //      //  Console.ReadLine(); 

            //  }
            Console.WriteLine("File Download DoneComplete Complete");
        }
        private static void wcCallbackDown(object sender, DownloadProgressChangedEventArgs e)
        {
            if ((iDotCount % 50) == 0)
                Console.Write(".");

            iDotCount += 1;
        }

    }
}



