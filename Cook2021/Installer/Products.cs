using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.Specialized; 

namespace Installer
{
    class Products
    {

        public string
             sProduct
            , sFileName
            , sUrl
            , sReadOnly
            , sMethodCall
            , sQueryEnd
            , sPassed
            , sVisible;
       
           
        public NameValueCollection Nvp; 


        struct sAttr
        {
            public string
            skeyname
            , sValue
            , sPassed;

        }
        public Products()
        {
        }
        public Products(string sProd, string sFile, string sRead)
        {
            sProduct = sProd;
            sFileName = sFile;
            sReadOnly = sRead; 
       
            Nvp = new NameValueCollection(); 

        }
    }
}
