# Create a new Sitecore Package (SPE cmdlet)
$pkg = New-Package "Brightcove Video Connect for Sitecore"

$pkg.Metadata.Version = "10.1.X-3.2"
$pkg.Metadata.Publisher = "Brightcove Inc."

$pkg.Metadata.Readme = "PATCHING INSTRUCTIONS:`nIf you have already installed the Brightcove connector and are now installing a patch`nthen please follow these instructions to ensure all changes inlcuded in the patch`nare properly installed.`n`n1. Select the overwrite option if prompted about item/file conflicts`n2. Delete and then re-create all pre-existing Brightcove account folders`n3. Delete and then re-create all pre-existing Brightcove DEF tenants"

# Get the Unicorn Configuration(s) we want to package
$configs = Get-UnicornConfiguration "Brightcove.*" 

# Pipe the configs into New-UnicornItemSource 
# to process them and add them to the package project
# (without -Project, this would emit the source object(s) 
#   which can be manually added with $pkg.Sources.Add())
$configs | New-UnicornItemSource -Project $pkg

$source = New-FileSource -Name "Configs" -Root "$AppPath\App_Config\Include\Brightcove"
$pkg.Sources.Add($source);

$source = New-FileSource -Name "Layouts" -Root "$AppPath\layouts\Brightcove"
$pkg.Sources.Add($source);

$source = New-FileSource -Name "Analytics Renderings" -Root "$AppPath\sitecore\shell\client\Applications\ExperienceAnalytics\Common\Layouts\Renderings\Brightcove"
$pkg.Sources.Add($source);

$source = New-FileSource -Name "Embed Media RTE Controls" -Root "$AppPath\sitecore\shell\Controls\Rich Text Editor\EmbedMedia"
$pkg.Sources.Add($source);

$source = New-FileSource -Name "Embed Media Link RTE Controls" -Root "$AppPath\sitecore\shell\Controls\Rich Text Editor\EmbedLink"
$pkg.Sources.Add($source);

$source = New-FileSource -Name "Brighcove Module" -Root "$AppPath\sitecore modules\Web\Brightcove"
$pkg.Sources.Add($source);

$source = New-FileSource -Name "Binaries" -Root "$AppPath\bin\Brightcove.Core.dll"
$pkg.Sources.Add($source);

$source = New-FileSource -Name "Binaries" -Root "$AppPath\bin\Brightcove.Web.dll"
$pkg.Sources.Add($source);

$source = New-FileSource -Name "Binaries" -Root "$AppPath\bin\Brightcove.DataExchangeFramework.dll"
$pkg.Sources.Add($source);

$source = New-FileSource -Name "Themes" -Root "$AppPath\sitecore\shell\Themes\Standard\Brightcove\"
$pkg.Sources.Add($source);

$source = New-FileSource -Name "Themes" -Root "$AppPath\sitecore\shell\Themes\Standard\Images\Brightcove\"
$pkg.Sources.Add($source);


# Export the package to a zip file on disk
Export-Package -Project $pkg -Path "C:\Code\Brightcove.Video.Connect.for.Sitecore-10.1.zip" -Zip