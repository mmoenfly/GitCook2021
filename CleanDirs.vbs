Dim Args
set Args = Wscript.Arguments

temp = Args(0)

'Msgbox(temp)
 ' Folder Path
on error resume next 
Set objFSO = CreateObject("Scripting.FileSystemObject")
if objFSO.FolderExists(temp) Then

       ' msgbox ("Deleting " & temp &"\*.xls") 
	objFSO.DeleteFile(temp &"\*.xls"),true
        objFSO.DeleteFile(temp &"\*.xlsm"),true
	 
End If


on error resume next
 