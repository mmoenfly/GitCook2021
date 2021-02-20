using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Management;
using System.Collections.Specialized;
using System.Xml;
using System.IO;
using Microsoft.Win32;

namespace Installer
{
    class PcId
    {
        private string sProduct, sSerialNumber, sUnid, sXmlDoc, sOffice, sDir, sCustomerName, sUserName;
        private XmlDocument xmldoc;
        public int IBatchNo = 0;
        public Boolean bCust = false;
        public string sEmail;
        private StreamWriter sw;


        public PcId(string sVal, string snDir, string sEmail)
        {
            sUnid = sVal;
            sDir = snDir;
            sOffice = checkOffice();
           // sEmail = this.sEmail; 
            ManagementObjectSearcher searcher =
                   new ManagementObjectSearcher(@"root\cimv2", "SELECT Product, SerialNumber FROM Win32_BaseBoard");

            ManagementObjectCollection information = searcher.Get();
            foreach (ManagementObject mo in information)
            {
                sProduct = Convert.ToString(mo["Product"]);
                sSerialNumber = Convert.ToString(mo["SerialNumber"]);
            }

            // Build an xml to send to logger

            xmldoc = new XmlDocument();

            XmlNode docnode = xmldoc.CreateXmlDeclaration(@"1.0", @"UTF-8", @"no");

            xmldoc.AppendChild(docnode);


            XmlNode root = xmldoc.CreateElement("PCID");
            xmldoc.AppendChild(root);

            XmlNode PC = xmldoc.CreateElement("PC");
            XmlAttribute idattrib = xmldoc.CreateAttribute("id");
            idattrib.Value = sSerialNumber;
            PC.Attributes.Append(idattrib);

            idattrib = xmldoc.CreateAttribute("unid");
            idattrib.Value = sUnid;
            PC.Attributes.Append(idattrib);

            idattrib = xmldoc.CreateAttribute("installtype");
            idattrib.Value = "full";
            PC.Attributes.Append(idattrib);

            idattrib = xmldoc.CreateAttribute("email");
            idattrib.Value = sEmail;
            PC.Attributes.Append(idattrib);


            idattrib = xmldoc.CreateAttribute("product");
            idattrib.Value = sProduct;
            PC.Attributes.Append(idattrib);


            idattrib = xmldoc.CreateAttribute("os");
            idattrib.Value = Convert.ToString(Environment.OSVersion);
            PC.Attributes.Append(idattrib);

            idattrib = xmldoc.CreateAttribute("officeversion");
            idattrib.Value = Convert.ToString(sOffice);
            PC.Attributes.Append(idattrib);

            idattrib = xmldoc.CreateAttribute("targetdir");
            idattrib.Value = sDir;
            PC.Attributes.Append(idattrib);

            idattrib = xmldoc.CreateAttribute("systemname");
            idattrib.Value = Convert.ToString(Environment.MachineName);
            PC.Attributes.Append(idattrib);
            root.AppendChild(PC);

            //   xmldoc.AppendChild(PC);
            //     StringWriter sw = new StringWriter();
            //XmlTextWriter xw = new XmlTextWriter(sw);
            //  xmldoc.Save(xw);


            sXmlDoc = xmldoc.OuterXml;

            //    xmldoc.Save(

            //XmlNode nameNode = doc.CreateElement("Name");
            //nameNode.AppendChild(doc.CreateTextNode("Java"));
            //productNode.AppendChild(nameNode);
            //XmlNode priceNode = doc.CreateElement("Price");
            //priceNode.AppendChild(doc.CreateTextNode("Free"));
            //productNode.AppendChild(priceNode);

            //// Create and add another product node.
            //productNode = doc.CreateElement("product");
            //productAttribute = doc.CreateAttribute("id");
            //productAttribute.Value = "02";
            //productNode.Attributes.Append(productAttribute);
            //productsNode.AppendChild(productNode);
            //nameNode = doc.CreateElement("Name");
            //nameNode.AppendChild(doc.CreateTextNode("C#"));
            //productNode.AppendChild(nameNode);
            //priceNode = doc.CreateElement("Price");
            //priceNode.AppendChild(doc.CreateTextNode("Free"));
            //productNode.AppendChild(priceNode);

        }
        public void setCust(string sCustval)
        {
            sCustomerName = sCustval;

        }
        public void setUser(string sUserval)
        {
            sUserName = sUserval;

        }
        public void setUnid(string sval)
        {
            sUnid = sval;
        }
        public void LogEvent()
        {
            Installer.wslogger.Ws_Logger ws = new Installer.wslogger.Ws_Logger();
            // ws.Timeout = 20000;
            IBatchNo = -1;
            try
            {
                int iBatch = int.Parse(ws.FindNextId(sUnid));
                IBatchNo = iBatch;
// 15 May 2012 
// skip logging the cook stuff 
                //if (sEmail.Contains(@"cookconsulting"))
                //{ }
                //else
                    ws.Logger(sUnid, sXmlDoc, 99999, iBatch);


                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ws.Dispose();
            }


        }
        public string PackXml(string sXml, string sUnid)
        {
            string sOut = "";

            Installer.wslogger.Ws_Logger ws = new Installer.wslogger.Ws_Logger();
            // ws.Timeout = 20000;

            try
            {
                sOut = ws.RndCustXml(sXml, sUnid);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ws.Dispose();
            }
            return sOut;

        }


        public string ChkInstall(string sUnid)
        {
            Installer.wslogger.Ws_Logger ws = new Installer.wslogger.Ws_Logger();
            // ws.Timeout = 20000;

            try
            {
                string sRes = ""; 
                if (sEmail.Contains(@"cookconsulting"))
                    sRes = "go";
                else
                    sRes = ws.ChkInstall(sUnid);

                return sRes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ws.Dispose();
            }


        }

        public void Close()
        {
            try
            {
                sw.Flush();
                sw.Dispose();
            }
            catch
            { }
        }

        public void FileLog(string sUnid, string sMsg, int iStatus, int iBatch)
        {



        }
        public void CreateLogFile(string sPath)
        {
            try
            {
                sw = new StreamWriter(sPath + "Log.txt");
            }
            catch (Exception ex)
            { throw ex; }
        }

        public void LogEvent(string sUnid, string sMsg, int iStatus, int iBatch)
        {
            // chg per version II 17 Feb 2021 
            //if (sEmail.Contains(@"testing"))
            //{ }

            //else
            {
                Installer.wslogger.Ws_Logger ws = new Installer.wslogger.Ws_Logger();

                if (iStatus == 10000)
                    ws.Logger(sUnid, sCustomerName + "&" + sUserName, iStatus, iBatch);
                else
                    ws.Logger(sUnid, sMsg, iStatus, iBatch);

                ws.Dispose();
            }
        }
        private string checkOffice()
        {

            string sOffice = "";

            string[] sPaths = new string[]  
                {@"Software\Microsoft\Windows\CurrentVersion\App Paths\excel.exe"
            ,@"Software\Microsoft\Windows\CurrentVersion\App Paths"};
            try
            {
                RegistryKey pRegKey = Registry.LocalMachine;

                pRegKey = pRegKey.OpenSubKey(sPaths[0]);
                if (pRegKey == null)
                {
                    pRegKey = Registry.CurrentUser;
                    pRegKey = pRegKey.OpenSubKey(sPaths[1]);
                }
                sOffice = Convert.ToString(pRegKey.GetValue(""));
                sOffice = sOffice.ToUpper();
                if (sOffice.Contains("14"))
                    return "14";

                if (sOffice.Contains("12"))
                    return "12";

                if (sOffice.Contains("11"))
                    return "12";


                return "Unknown Office Version";

            }

            catch
            { return "Unknown"; }
        }


    }
}
