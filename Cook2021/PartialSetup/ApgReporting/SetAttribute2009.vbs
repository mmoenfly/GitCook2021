on error resume next  
 


temp = Property("CustomActionData") 
 ' Folder Path
 
 
Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFolder = objFSO.GetFolder(temp)  
Set colItems = objFolder.Files
For Each objItem in colItems
  If objitem.Name = "PayrollSettings.xls" or objitem.Name =  "PayrollSettings.xlsm" Then       ' fixing the file name
   	objItem.Attributes = 32 
  Else 
	 If objitem.Name = "AbsenceSettings.xls"  or objitem.Name = "AbsenceSettings.xlsm" Then
		objItem.Attributes = 32 
	Else
  		objItem.Attributes = 33 
	End If
  End If
  If objitem.Name = "installlog.txt" Then       ' fixing the file name
   objItem.Attributes = 32 
  End If
Next
 