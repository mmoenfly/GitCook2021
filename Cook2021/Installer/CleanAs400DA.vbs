 
 

temp = "C:\AS400DA"
 ' Folder Path
on error resume next 
Set objFSO = CreateObject("Scripting.FileSystemObject")
if objFSO.FolderExists("C:\as400da") Then
	objFSO.DeleteFile("C:\AS400DA\*.xls"),true
	objFSO.DeleteFile("C:\AS400DA\*.xlsm"),true
        
End If


on error resume next

 

temp = Property("CustomActionData")   ' Folder Path
if error.number <> 0 then temp = "c:\as400da\" 


err.clear
Set objShell = CreateObject("WScript.Shell")
objDesktop = objShell.SpecialFolders("Desktop")
Set oShell = CreateObject( "WScript.Shell" )  
sRoot=oShell.ExpandEnvironmentStrings("%SystemRoot%")  

Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFolder = objFSO.GetFolder(temp)  

objFSO.DeleteFile(objDeskTop + "\AS400 SpreadSheets\*.lnk"),true

 if not objFSO.FolderExists(objDeskTop  &"\AS400 SpreadSheets") Then _
      objFSO.CreateFolder(objDeskTop  &"\AS400 SpreadSheets" ) 

' if Not objFSO.FolderExists(objDeskTop  &"\CCI SpreadSheets") Then _
'      objFSO.CreateFolder(objDeskTop  & "\CCI SpreadSheets" ) 
 
iCount = 0 
 
Set colItems = objFolder.Files

For Each objItem in colItems
    
	 pos = InStrRev(objItem.Name,".",-1) 
        File_Name = Left(objItem.Name, pos - 1)
        sArr = split(objItem.Name,".")
        sExt = sArr(1)
        IF sExt = "lnk"  Then
             objFSO.DeleteFile(objItem.Name)
            
            if err.number <> 0 Then iCount = iCount + 1
            Set objLink = Nothing
      End if 
   
Next
 
 
if iCount > 0 Then MsgBox("Your shortcuts did not create. Please contact support at " + "supportdesk@cookconsulting.net or call 800-425-0720")
 