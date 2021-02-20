Option Explicit
Dim objShell, objDesktop, objLink,objFSO,objFolder,colItems,objItem,oShell
Dim strAppPath, strWorkDir, strIconPath,pos,File_Name, sRoot, sExt, sArr
dim temp, iCount

 
on error resume next

err.clear

' Flags for the options parameter
Const BIF_returnonlyfsdirs   = &H0001
Const BIF_dontgobelowdomain  = &H0002
Const BIF_statustext         = &H0004
Const BIF_returnfsancestors  = &H0008
Const BIF_editbox            = &H0010
Const BIF_validate           = &H0020
Const BIF_browseforcomputer  = &H1000
Const BIF_browseforprinter   = &H2000
Const BIF_browseincludefiles = &H4000

Dim wsh, objDlg, objF

' Get Application object of the Windows shell.
Set objDlg = WScript.CreateObject("Shell.Application")

' Use the BrowseForFolder method.
' For instance: Set objF = objDlg.BrowseForFolder _
'     (&H0, "Select the Install Folder", &H10, "C:\AS400DA")

Set objF = objDlg.BrowseForFolder (&H0, _
    "Select install folder - Default c:\as400da", _
    BIF_editbox + BIF_returnonlyfsdirs )

' Here we use the first method to detect the result.
If IsValue(objF) Then 
    temp = objF.Title
 
End If
 
if right(temp,1) <> "\" then temp = temp + "\"

if error.number <> 0 then temp = "c:\as400da\"
err.clear
Set objShell = CreateObject("WScript.Shell")
objDesktop = objShell.SpecialFolders("Desktop")
Set oShell = CreateObject( "WScript.Shell" )  
sRoot=oShell.ExpandEnvironmentStrings("%SystemRoot%")  

Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFolder = objFSO.GetFolder(temp)  

 if not objFSO.FolderExists(objDeskTop  &"\AS400 SpreadSheets") Then _
      objFSO.CreateFolder(objDeskTop  &"\AS400 SpreadSheets" ) 

' if Not objFSO.FolderExists(objDeskTop  &"\CCI SpreadSheets") Then _
'      objFSO.CreateFolder(objDeskTop  & "\CCI SpreadSheets" ) 
 
iCount = 0 
 
Set colItems = objFolder.Files

For Each objItem in colItems
     If objitem.Name <> "installlog.txt" Then
	 pos = InStrRev(objItem.Name,".",-1) 
     File_Name = Left(objItem.Name, pos - 1)
     sArr = split(objItem.Name,".")
     sExt = sArr(1)
        IF sExt = "xlsm" or sExt = "xls" Then

            
            Set objLink = objShell.CreateShortcut(objDesktop & "\AS400 SpreadSheets\"  & File_Name &  ".lnk")

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


if iCount > 0 Then MsgBox("Your shortcuts did not create. Please contact support at " + "supportdesk@cookconsulting.net or call 800-425-0720 or download "+ " http://ccisupportsite.com/xml/createshort1_1.vbs and execute the script")

Msgbox("Shortcuts successfully created!")

Function IsValue(obj)
    ' Check whether the value has been returned.
    Dim tmp
    On Error Resume Next
    tmp = " " & obj
    If Err <> 0 Then
        IsValue = False
    Else
        IsValue = True
    End If
    On Error GoTo 0
End Function

'*** End

 
