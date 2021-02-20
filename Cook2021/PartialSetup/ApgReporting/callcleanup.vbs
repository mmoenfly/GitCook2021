Dim temp
 temp = Property("CustomActionData") 
Set wshshell = CreateObject ("WScript.shell")
dim scmd as string 
scmd = " start " + temp + "uninstall.exe "
msgbox(scmd)
wshshell.Run (scmd )