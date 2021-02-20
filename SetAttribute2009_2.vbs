on error resume next  
dim Args, fWorkDir 

Set objShell = CreateObject("WScript.Shell")
set Args = Wscript.Arguments
set fWorkDir = Args(0)
'temp = Property("CustomActionData") 
 
 ' Folder Path
 
'msgbox("temp=" & Args(0) )
set fWorkDir = Args(0)
Set objFSO = CreateObject("Scripting.FileSystemObject")
'Msgbox("built objFSO")
Set objFolder = objFSO.GetFolder(Args(0))  
'Msgbox("Got Folder contents")
Set colItems = objFolder.Files
'msgbox("files = " &  objFolder.Files.Count )
For Each objItem in colItems 
   nmw = objItem.Name 

'Msgbox("File is " & objItem.name) 

objItem.Attributes = 1

if instr( nmw, "PayrollSettings") >  0 then 
   objItem.Attributes = 32
  ' Msgbox("Hit payroll")
End if 

if instr( nmw , "AbsenceSettings") >  0 then 
    objItem.Attributes = 32
    'Msgbox("Hit absence")
End if 


'Select Case nmw
'  
'     Case "PayrollSettings.xls"
'           objItem.Attributes = 32
           
 '    case "PayrollSettings.xlsm"
 '          objItem.Attributes = 32
           
 '    Case "AbsenceSettings.xls"
 '         objItem.Attributes = 32
 '    Case "AbsenceSettings.xlsm" 
 '         objItem.Attributes = 32
  '  Case Else
  '       objItem.Attributes = 1
  ' End Select
  
  If objitem.Name = "installlog.txt" Then       ' fixing the file name
   objItem.Attributes = 32 
  End If
  
  
  
Next  
 
