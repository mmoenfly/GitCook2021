Option Explicit
Dim objShell, objDesktop, objLink,objFSO,objFolder,colItems,objItem,oShell, objShCutPath
Dim strAppPath, strWorkDir, strIconPath,pos,File_Name, sRoot, sExt, sArr
dim temp, iCount

' Msgbox "Got to Script"   
on error Goto 0 

err.clear
Set objShell = CreateObject("WScript.Shell")


'temp = Property("CustomActionData")   ' Folder Path
'temp = Wscript.Arguments(0) 
Set objFSO = CreateObject("Scripting.FileSystemObject")
temp = objFSO.GetAbsolutePathName(".")
temp = objShell.CurrentDirectory 
'Wscript.Echo temp
'msgbox temp 
'if error.number <> 0 then temp = "c:\as400da\"
err.clear
'Set objShell = CreateObject("WScript.Shell")
'objDesktop = objShell.SpecialFolders("Desktop")
'Set oShell = CreateObject( "WScript.Shell" )  
'sRoot=oShell.ExpandEnvironmentStrings("%SystemRoot%")  

'Set objFSO = CreateObject("Scripting.FileSystemObject")
'Set objFolder = objFSO.GetFolder(temp)  



'' Determine which folder on the desktop exists - if neither exists, create CCI Spreadsheets
'If objFSO.FolderExists(objDeskTop  &"\CCI SpreadSheets") Then 
'	objShCutPath = "\CCI Spreadsheets\"
'        Else
'	if objFSO.FolderExists(objDeskTop  &"\AS400 SpreadSheets") Then
'	        'objFSO.DeleteFolder(objDeskTop  &"\AS400 SpreadSheets")  
'    		objShCutPath = "\CCI Spreadsheets\"
'	Else
'		objFSO.CreateFolder(objDeskTop  & "\CCI SpreadSheets" )
'		objShCutPath = "\CCI Spreadsheets\" 
'	End If
'End If
'If err.number <> 0 then 0=0 
' 
Set objShell = CreateObject("WScript.Shell")
objDesktop = objShell.SpecialFolders("Desktop")
Set oShell = CreateObject( "WScript.Shell" )  
sRoot=oShell.ExpandEnvironmentStrings("%SystemRoot%")  

Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFolder = objFSO.GetFolder(temp)  
iCount = 0 


if   objFSO.FolderExists(objDeskTop  &"\CCI SpreadSheets\")  Then
Else
	        objFSO.CreateFolder(objDeskTop  &"\CCI SpreadSheets\")  
	End if        
objShCutPath = "\CCI SpreadSheets\"
 
'objShCutPath = "\CCI Spreadsheets\"
 
Set colItems = objFolder.Files

For Each objItem in colItems
     If objitem.Name <> "installlog.txt" Then
	 pos = InStrRev(objItem.Name,".",-1) 
     File_Name = Left(objItem.Name, pos - 1)
     sArr = split(objItem.Name,".")
     sExt = sArr(1)
        IF sExt = "xlsm" or sExt = "xls" Then

            
            Set objLink = objShell.CreateShortcut(objDesktop & objShCutPath  & File_Name &  ".lnk")

            strAppPath = "%SystemRoot%\notepad.exe"

            strAppPath = temp + "\" + objItem.Name
            strIconPath = temp + "\" + objItem.Name
            objLink.IconLocation = temp + "\excelcci.ico"
            objLink.TargetPath = strAppPath
            objLink.WindowStyle = 3
            objLink.WorkingDirectory = sRoot
            objLink.Save
            if err.number <> 0 Then iCount = iCount + 1
            Set objLink = Nothing
      End if 
  End If
Next
' MsgBox "Done!" 
 
if iCount > 0 Then MsgBox("Your shortcuts did not create. Please contact support at " + "supportdesk@cookconsulting.net or call 800-425-0720")
 
