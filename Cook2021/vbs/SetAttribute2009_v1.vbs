on error resume next  
 


'temp = Property("CustomActionData") 
 
 ' Folder Path
Set objFSO = CreateObject("Scripting.FileSystemObject")
temp = objFSO.GetAbsolutePathName(".")
temp = objShell.CurrentDirectory 


Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFolder = objFSO.GetFolder(temp)  
Set colItems = objFolder.Files
For Each objItem in colItems 
   nmw = objItem.Name 


Select Case nmw
  
     Case "PayrollSettings.xls"
           objItem.Attributes = 32
           
     case "PayrollSettings.xlsm"
           objItem.Attributes = 32
           
     Case "AbsenceSettings.xls"
          objItem.Attributes = 32
     Case "AbsenceSettings.xlsm" 
          objItem.Attributes = 32
    Case Else
         objItem.Attributes = 33
   End Select
  
  If objitem.Name = "installlog.txt" Then       ' fixing the file name
   objItem.Attributes = 32 
  End If
  
  
  
Next
 