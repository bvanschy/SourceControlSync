Param(
  [string]$smtpUserName,
  [string]$smtpPassword,
  [string]$filename
)

$fullPath = Resolve-Path $filename
$xml = [xml](get-content $fullPath)
 
$root = $xml.get_DocumentElement();
 
$root."system.net".mailSettings.smtp.network.userName = $smtpUserName
$root."system.net".mailSettings.smtp.network.password = $smtpPassword
 
$xml.Save($fullPath)
