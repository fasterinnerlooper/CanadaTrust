param($installPath, $toolsPath, $package, $project)

# Set AdSettings.xml to BuildAction Content
$item = $project.ProjectItems | where-object {$_.Name -eq "Ads\AdSettings.xml"} 
if($item)
{
  $item.Properties.Item("BuildAction").Value = [int]2
}
# Set WP-Unified-Ad-HowTo.txt to BuildAction None
$item = $project.ProjectItems | where-object {$_.Name -eq "WP-Unified-Ad-HowTo.txt"} 
if($item)
{
  $item.Properties.Item("BuildAction").Value = [int]0
}
# Set UnifiedAd.xsd to BuildAction None
$item = $project.ProjectItems | where-object {$_.Name -eq "Ads\UnifiedAd.xsd"} 
if($item)
{
  $item.Properties.Item("BuildAction").Value = [int]0
}