$esc = "$([char]27)"
$reset = $esc + "[0m"
$green = $esc + "[92m"
$red = $esc + "[91m"

$error.clear()

gacutil /i .\bin\Debug\Win10VirtualDesktopDeskBand.dll
 ($? ? $green + "success" : $red + "FAILED") + $reset #gosh i love powershell =)
if (!$?) { exit }

echo `r
regasm .\bin\Debug\Win10VirtualDesktopDeskBand.dll
($? ? $green + "success" : $red + "FAILED") + $reset #gosh i love powershell =)
if (!$?) { exit }

taskkill /f /im explorer.exe 
start explorer