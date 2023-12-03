# Fright: Unity Find & Replace
Quickly search your Unity project for nearly anything.

Originally designed as a tool to find asset references within your project, this project has increased in scope to search your project for nearly anything, and perform replacements to those searches.

## Purpose
The purpose of this tool is to make it clearer where in your project any particular asset is being used, as well as providing a mechanism for replacing any references to that asset.

| Assets | Regex | Settings |
| --- | --- | --- |
| ![Reference Window Asset Panel Preview](https://github.com/StaticPtr/Fright-Unity-Reference-Find-Replace/blob/master/Readme%20Assets/ReferencesWindow-Assets.png?raw=true) | ![Reference Window Regular Expression Panel Preview](https://github.com/StaticPtr/Fright-Unity-Reference-Find-Replace/blob/master/Readme%20Assets/ReferencesWindow-Regex.png?raw=true) | ![Reference Window Settings Panel Preview](https://github.com/StaticPtr/Fright-Unity-Reference-Find-Replace/blob/master/Readme%20Assets/ReferencesWindow-Settings.png?raw=true) |

## Setup
Download the [latest .unitypackage](https://github.com/StaticPtr/Fright-Unity-Reference-Find-Replace/releases/download/v1.4.0/Fright.Find.and.Replace.v1.4.0.unitypackage) and import it into your project

## Usage
### Searching your project
1) Open the References window by clicking on the `Window/Find And Replace` menu item
![Reference Window Preview](https://github.com/StaticPtr/Fright-Unity-Reference-Find-Replace/blob/master/Readme%20Assets/HowTo-MainMenu.png?raw=true)
2) Pick an asset to search for, or enter a regular expression in the Regex panel
3) Click on the **Find** button to start scanning your project. _Please note that this may take a couple minutes depending on the size of your project, and the speed of your storage._
4) A list of files that match your search will be displayed when the search is complete.

### Quick searching for assets
1) Select one or more assets from the Project view that you would like to find references to
2) Right click on the selection, and click on `Find References` from the dropdown
![Reference Window Preview](https://github.com/StaticPtr/Fright-Unity-Reference-Find-Replace/blob/master/Readme%20Assets/HowTo-ContextMenu.png?raw=true)
3) The references window will open automatically and begin searching your project for the selected assets

### Replacing
1) Perform a search using the above instructions
2) If you are using the Assets panel, provide an asset to replace any found references to.
<br>If you are using the Regex panel, provide a string to replace any matches to the search regex with.
3) Click on the **Replace** button. _**Please note that this is permanent**_
