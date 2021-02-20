

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Text;
using System.Xml;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Core;
using System.Diagnostics;
using System.Web;
using System.IO; 



using System.Reflection;


namespace Installer
{
    class DocDetails
    // Created By Michael Moen
    // 5 Jan 2011
    // Utility Class for working with the configuration methods
    {
        public string sTarget;
        private XmlDocument pDoc;
        private NameValueCollection nvpKeys;

       // const string sHost = sHost;
        public ArrayList
                    nProd
                  , cSheetColl;
        struct sTheProducts
        {
            public string
                sProduct
                , sReadonly
                , sFileName
               ;

            public sTheProducts(string sProd, string sFile, string sRead)
            {
                sProduct = sProd;
                sFileName = sFile;
                sReadonly = sRead;

            }
        }

        public void SetProducts(ArrayList inProds)
        {
            nProd = new ArrayList();
            nProd = inProds;
        }
        public string GetKeys(string sKey)
        {
            string sRet;
            sRet = nvpKeys.Get(sKey);
            return sRet;
        }
        public DocDetails(XmlDocument xmlprod, string sHost)
        {
            try
            {
                pDoc = xmlprod;
                nvpKeys = new NameValueCollection();
                //LoadPairs();
                cSheetColl = GetTheSheets(xmlprod, sHost);

                //      ProcessProducts(xmlprod ,  cSheetColl); 
            }
            catch (Exception ex)
            { throw ex; }
        }
        private ArrayList GetTheSheets(XmlDocument doc , string sHost)
        {
            XmlNode root;
            ArrayList aColl = new ArrayList();

            root = doc.DocumentElement;

            XmlNodeList nodes;

            nodes = root.SelectNodes("child::*");
            Products pd;
            pd = null;
            foreach (XmlNode n in nodes)
            {

                if (pd == null)
                {
                    string sRead;
                    try
                    {
                        sRead = n.Attributes["readonly"].Value;
                    }
                    catch (Exception ex)
                    { sRead = "N"; }

                   
                    pd = new Products(
                      n.Attributes["keyname"].Value
                      , n.Attributes["file"].Value
                      , sRead
                      );
                }

                // Must Set a Default for Processing

                try
                {
                    pd.sMethodCall = n.Attributes["procname"].Value;
                }
                catch (Exception ex)
                {
                    pd.sMethodCall = "Module1.SetSheet";
                }

                try
                {
                    pd.sVisible = n.Attributes["visible"].Value;
                }
                catch (Exception ex)
                {
                    pd.sVisible = "Y";
                }

                try
                {
                    pd.sPassed = n.Attributes["passed"].Value;
                }
                catch (Exception ex)
                {
                    pd.sPassed = "N";
                }

                // Must Have a QueryEnd
                try
                {
                    pd.sQueryEnd = n.Attributes["queryend"].Value;
                }
                catch (Exception ex)
                {
                    pd.sQueryEnd = "070111";
                }
            
                pd.sUrl = sHost + @"/Sheets/Version/" + pd.sQueryEnd + @"/" + HttpUtility.UrlPathEncode(pd.sFileName);



                //Debug.Print("att value={0} Attribute Name{1}", att.Value, att.Name); 
                //Debug.Print("Got a Product{0}", n.Attributes["Value"].Value);

                //     string sRemoteName = "http://localhost/sheets/" + n.Attributes["Value"].Value + ".xls";
                //     string sLocalName = @"C:\cci\" + n.Attributes["Value"].Value + ".xls";

                // add to sheet coll
                // go get the parms/methods for it.

                XmlNodeList nodelist2;
                nodelist2 = n.ChildNodes;
                foreach (XmlNode n2 in nodelist2)
                {
                    if (n2.Attributes["passed"].Value == "Y")
                        pd.Nvp.Add(n2.Attributes["keyname"].Value, n2.Attributes["passed"].Value);



                }
                aColl.Add(pd);

                // destroy it.

                pd = null;





            }
            return aColl;
        }

       

        private void LoadPairs()
        {
            XmlNodeList nodes;
            XmlNode root = pDoc.DocumentElement;

            nodes = root.SelectNodes("Customer/Parms");
            foreach (XmlNode n in nodes)
            {
                // Deserialize them 


                nvpKeys.Add(n.Attributes["keyName"].Value, n.Attributes["Value"].Value);

            }
            // All loaded
            // Can retrieve by Key Value
        }
        public void CallXlConfigure(Products pd, string sFileName, NameValueCollection nParms,string sQry, ref PcId pc)
        {

            Microsoft.Office.Interop.Excel.Workbook wb;
            Microsoft.Office.Interop.Excel.ApplicationClass xl;
            xl = new Microsoft.Office.Interop.Excel.ApplicationClass();

            xl.DisplayAlerts = false;

            //  sFileName = @"C:\cci\";  // + sp.sFileName; 

            // Build the call to setset the Excel Sheet 
            try
            {

   // 2 August Late open excel after parm binding

                //if (pd.sVisible == "Y") xl.Visible = true;
                //else xl.Visible = false;


                //xl.Interactive = false;
                ////Microsoft.Office.Core.MsoAutomationSecurity secAuto = (MsoAutomationSecurity)MsoAutomationSecurity.msoAutomationSecurityLow;

                ////xl.Application.AutomationSecurity = secAuto;



                //wb = xl.Workbooks.Open(

                //              sFileName
                //              , 2
                //              , false,
                //              5,
                //              "",
                //              "",
                //              true,
                //              Microsoft.Office.Interop.Excel.XlPlatform.xlWindows,
                //              "\t",
                //              false,
                //              false,
                //              0,
                //              true,
                //              1,
                //              0);

                int iSize = nParms.Count + 1;

                System.Threading.AutoResetEvent waiter = new System.Threading.AutoResetEvent(false);

                while (xl.Ready != true)
                { waiter.WaitOne(60 * 1000); };
                object[] oParms = new object[]  
                { 
                    pd.sMethodCall};
                //     ,"Hello World!"
                //     ,nvpKeys.Get("SystemNm")
                //     ,nvpKeys.Get("FinanceType")
                //     , null

                //  };
                int ix = 1;
                Array.Resize(ref oParms, iSize);
                foreach (string skey in nParms.Keys)
                {
                    string sVal = "";
                    sVal = nParms.Get(Convert.ToString(skey) );
                    if (sVal != null)
                    {
                        if (sVal.Contains(","))
                        {// is an array 
                            object[] sTarget;

                            sTarget = processstring(sVal);
                            oParms[ix] = sTarget;

                        }
                        else
                        {

                            oParms[ix] = nParms.Get(skey);

                            if (skey == "queryend")
                                oParms[ix] = sQry;

                            // oParms[ix] = nParms.Get(skey);
                        }

                    }
                    else
                    {
                        oParms[ix] = nParms.Get(skey) == null ? "" : nParms.Get(skey);

                        if (
                               skey == "specialind"
                            || skey == "budgetind"
                            || skey == "otherdepts"
                            || skey == "salariedcodes"
                            || skey == "salariedwithsupp"
                            || skey == "certifiedcodes"
                            || skey == "princodes"
                            || skey == "suppcodes"

                            )
                        { oParms[ix] = new object[] { "" }; }




                    }
                    ix += 1;

                }


                if (oParms[0].ToString().Length + oParms[1].ToString().Length + oParms[2].ToString().Length == 0)
                {
              //    pc.LogEvent(
                    throw new Exception("The parms are empty from cust.xml");
                }

                // 2 august 2012
                if (pd.sVisible == "Y") xl.Visible = true;
                else xl.Visible = false;


                xl.Interactive = false;
                //Microsoft.Office.Core.MsoAutomationSecurity secAuto = (MsoAutomationSecurity)MsoAutomationSecurity.msoAutomationSecurityLow;

                //xl.Application.AutomationSecurity = secAuto;



                wb = xl.Workbooks.Open(

                              sFileName
                              , 2
                              , false,
                              5,
                              "",
                              "",
                              true,
                              Microsoft.Office.Interop.Excel.XlPlatform.xlWindows,
                              "\t",
                              false,
                              false,
                              0,
                              true,
                              1,
                              0);

                // 2 august 2012




                // Sub SetSheet(ConnectionNm As String, SystemNm As String, SystemType As String, FinanceType As String, UserType As String, ProcPrefix As String, QueryEnd As String)

                // 
                //tcookcci: Added another tag called Maintenance in the XML 
                //(Y or N) - if they are not on Maintenance, it will be blank for now. 
                //    The current QueryEnd is 070109 - once we are ready to update, it will change to 070110

                try
                {
                    if (pd.sMethodCall.Length > 0 && pd.sMethodCall != "None")
                        RunMacro(xl, oParms);
                }
                catch (Exception ex)
                { xl.Quit(); throw ex; }

                Debug.Print(xl.Version);

  
                try
                {

                    Type type = xl.GetType();
                  

                    PropertyInfo prop = type.GetProperty("CheckCompatibility");
                    Console.WriteLine("Retrieved Xl Property");
                    prop.SetValue(xl, false, null);
                }
                catch { }

                string sFTarget = this.sTarget + pd.sProduct;

                if (String.Compare(xl.Version, "11.0") > 0)
                {
                    if (File.Exists(sFTarget + ".xlsm"))
                    {
                        FileInfo fi2 = new FileInfo(sFTarget + ".xlsm");
                        fi2.IsReadOnly = false;
                        fi2.Delete();
                    }



                    string sPth1 = sTarget + pd.sProduct + ".xlsm";
                    Console.WriteLine("Path to XL file is {0}", sPth1);

                    wb.SaveAs(sTarget + pd.sProduct + ".xlsm"
                      , 52, Type.Missing, Type.Missing, Type.Missing,
                        Type.Missing, XlSaveAsAccessMode.xlNoChange, XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                }
                else
                {
                    string sPth1 = sTarget + pd.sProduct + ".xlsm";
                    Console.WriteLine("Path to XL file is {0}", sPth1);

                    if (File.Exists(sFTarget + ".xls"))
                    {
                        FileInfo fi2 = new FileInfo(sFTarget + ".xls");
                        fi2.IsReadOnly = false;
                        fi2.Delete();
                    }



                    wb.SaveAs(sTarget + pd.sProduct + ".xls"
                   , XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, Type.Missing,
                     Type.Missing, XlSaveAsAccessMode.xlNoChange, XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                }

               
                xl.Quit();

            }
            catch (Exception ex)
            {

                if (xl != null)
                    xl.Quit();
                throw ex;
            }
            finally
            {
                try
                {
                    xl.Quit();
                }
                catch { }
            }

        }

        private object[] processstring(string sVal)
        {

            try
            {
                object[] sRes = new object[10];
                string[] sTarget;
                NameValueCollection nv = new NameValueCollection();

                sTarget = sVal.Split(',');
                foreach (string s in sTarget)
                {
                    string stest = nv.Get(s);
                    if (stest == "Y")
                    {
                    }
                    else
                        nv.Add(s, "Y");
                }

                Array.Resize(ref sRes, nv.Count);
                int ix = 0;
                foreach (string s in nv.AllKeys)
                { sRes[ix] = s; ix += 1; }

                return sRes;
            }
            catch { return (new object[1] { "" }); }
        }
        private void RunMacro(object oApp, object[] oRunArgs)
        {
            try
            {
                oApp.GetType().InvokeMember("Run",
                      System.Reflection.BindingFlags.Default |
                      System.Reflection.BindingFlags.InvokeMethod,
                      null, oApp, oRunArgs);
            }
            catch (Exception ex)
            { throw ex; }
        }


    }
}
