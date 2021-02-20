Option Explicit
Dim objShell, objDesktop, objLink,objFSO,objFolder,colItems,objItem,oShell, objShCutPath
Dim strAppPath, strWorkDir, strIconPath,pos,File_Name, sRoot, sExt, sArr
dim temp, iCount, Args, fDeskFld, fIconfile, fWorkDir

' Msgbox "Got to Script"   
on error Goto 0 

err.clear
Set objShell = CreateObject("WScript.Shell")
set Args = Wscript.Arguments
fDeskFld = Args(0) 
fIconFile = Args( 1) 
fWorkDir = Args ( 2)  
'MsgBox("Deskfolder=" & fDeskFld)
'MsgBox("fIconFile ="& fIconFile)
'MsgBox("fWorkDir ="& fWorkDir)


'temp = Property("CustomActionData")   ' Folder Path
'temp = Wscript.Arguments(0) 
Set objFSO = CreateObject("Scripting.FileSystemObject")
temp = objFSO.GetAbsolutePathName(".")
temp = objShell.CurrentDirectory 
 
 
err.clear
 
 
Set objShell = CreateObject("WScript.Shell")
objDesktop = objShell.SpecialFolders("Desktop")
Set oShell = CreateObject( "WScript.Shell" )  
sRoot=oShell.ExpandEnvironmentStrings("%SystemRoot%")  

Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFolder = objFSO.GetFolder(FWorkDir)  
iCount = 0 


if   objFSO.FolderExists(objDeskTop  & "\"  & fDeskfld &"\")  Then
Else
	        objFSO.CreateFolder(objDeskTop  &"\" & fDeskfld &"\")  
	End if        
objShCutPath = objDeskTop  & "\" & fDeskFld & "\"
 
Set colItems = objFolder.Files

For Each objItem in colItems
     If objitem.Name <> "installlog.txt" Then
	 pos = InStrRev(objItem.Name,".",-1) 
     File_Name = Left(objItem.Name, pos - 1)
     sArr = split(objItem.Name,".")
     File_name =  sArr(0)
     sExt = sArr(1)
  '   Msgbox(File_Name)
  '   MsgBox(sExt)
        IF sExt = "xlsm" or sExt = "xls" or sExt = "xlsx" Then

            
            Set objLink = objShell.CreateShortcut(objShCutPath & File_Name &  ".lnk")

          

            strAppPath = fWorkDir + "\" + objItem.Name
            strIconPath = fWorkDir + "\" + fIconFile
        '      MsgBox("apppath = " & fWorkDir + "\" + objItem.Name)
        '    MsgBox("strIconpath = " & fWorkDir + "\" +  fIconfile)
              
         '   MsgBox("iconfile = " & fWorkDir + "\" & fIconfile)
       '  msgbox( "iconlocation = "& fWorkDir & "\" & fIconfile)
            objLink.IconLocation = fWorkDir & "\" & fIconfile
            objLink.TargetPath = strAppPath
            objLink.Description = "App Garden Reporting Sheet"
            objLink.WindowStyle = 3
            objLink.WorkingDirectory = fWorkDir
            objLink.Hotkey = 0 


          
            objLink.Save
            if err.number <> 0 Then iCount = iCount + 1
            Set objLink = Nothing
      End if 
  End If
Next
' MsgBox "Done!" 
 
if iCount > 0 Then MsgBox("Your shortcuts did not create. Please contact support at " + "helpdesk@app-garden.com")
 
