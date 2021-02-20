﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace Installer.dws {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="DominoSoapBinding", Namespace="urn:DefaultNamespace")]
    public partial class GetFullCustomersService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback GETFULLCUSTOMERSOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public GetFullCustomersService() {
            this.Url = "http://spinstalls.app-garden.com:80/cciweb.nsf/getfullcustomers?OpenWebService";
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event GETFULLCUSTOMERSCompletedEventHandler GETFULLCUSTOMERSCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("GETFULLCUSTOMERS", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Bare)]
        [return: System.Xml.Serialization.XmlElementAttribute("GETFULLCUSTOMERSReturn", Namespace="urn:DefaultNamespace")]
        public string GETFULLCUSTOMERS([System.Xml.Serialization.XmlElementAttribute(Namespace="urn:DefaultNamespace")] string DBNAME, [System.Xml.Serialization.XmlElementAttribute(Namespace="urn:DefaultNamespace")] string VIEWNAME, [System.Xml.Serialization.XmlElementAttribute(Namespace="urn:DefaultNamespace")] string MYUNID) {
            object[] results = this.Invoke("GETFULLCUSTOMERS", new object[] {
                        DBNAME,
                        VIEWNAME,
                        MYUNID});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GETFULLCUSTOMERSAsync(string DBNAME, string VIEWNAME, string MYUNID) {
            this.GETFULLCUSTOMERSAsync(DBNAME, VIEWNAME, MYUNID, null);
        }
        
        /// <remarks/>
        public void GETFULLCUSTOMERSAsync(string DBNAME, string VIEWNAME, string MYUNID, object userState) {
            if ((this.GETFULLCUSTOMERSOperationCompleted == null)) {
                this.GETFULLCUSTOMERSOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGETFULLCUSTOMERSOperationCompleted);
            }
            this.InvokeAsync("GETFULLCUSTOMERS", new object[] {
                        DBNAME,
                        VIEWNAME,
                        MYUNID}, this.GETFULLCUSTOMERSOperationCompleted, userState);
        }
        
        private void OnGETFULLCUSTOMERSOperationCompleted(object arg) {
            if ((this.GETFULLCUSTOMERSCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GETFULLCUSTOMERSCompleted(this, new GETFULLCUSTOMERSCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    public delegate void GETFULLCUSTOMERSCompletedEventHandler(object sender, GETFULLCUSTOMERSCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GETFULLCUSTOMERSCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GETFULLCUSTOMERSCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591