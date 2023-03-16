# UE4LocalizationsTool
simple tool to edit unreal engine 4 text files.

By: Amr Shaheen
<hr>

## Command Lines
### for export single file use:
```
UE4localizationsTool.exe  export <(Locres/Uasset/Umap) FilePath>  <Options>
Example:
UE4localizationsTool.exe export Actions.uasset
```
### for import single file use:
```
UE4localizationsTool.exe  import <(txt) FilePath>  <Options>
Example:
UE4localizationsTool.exe import Actions.uasset.txt
```
### for import single file without rename it use:
```
UE4localizationsTool.exe  -import <(txt) FilePath>  <Options>
Example:
UE4localizationsTool.exe -import Actions.uasset.txt
```

### for export many files from folder use:
```
UE4localizationsTool.exe  exportall  <Folder> <TxtFile> <Options>
Example:
UE4localizationsTool.exe exportall Actions text.txt
```
### for import many files in folder use:
```
UE4localizationsTool.exe  importall  <Folder> <TxtFile>  <Options>
Example:
UE4localizationsTool.exe importall Actions text.txt
```
### for import many files in folder without rename files use:
```
UE4localizationsTool.exe  -importall  <Folder> <TxtFile>  <Options>
Example:
UE4localizationsTool.exe -importall Actions text.txt
```

### Options: (Remember to apply the same OPTIONS when importing)

#### To use last filter you applied before in GUI use: (apply only in name table)
```
-f or -filter 
Example:
UE4localizationsTool.exe export Actions.uasset -filter
```

#### To export file without including name table use:
```
-nn or -NoName
Example:
UE4localizationsTool.exe export Actions.uasset -NoName
```
#### To use method 2 use:(trying to catch text without using ue4 asset structure (for uasset and umap only))
```
-m2 or -method2
Examplemethod2
UE4localizationsTool.exe export Actions.uasset -method2
UE4localizationsTool.exe export Actions.uasset -method2 -NoName -filter
```