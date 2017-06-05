# Psd2Unity Importer

A Photoshop importer tool for the Unity game editor that simplifies layer import and layout rebuilding in Unity.

This is a rewrite of an [earlier importer](https://github.com/ChemiKhazi/UnityPsdImporter) to use the more compatible [NtreevSoft PSD Parser](https://github.com/NtreevSoft/psd-parser) library as well as make the project more maintainable.

## Installation

Get the latest UnityPackage from the [GitHub releases](https://github.com/ChemiKhazi/PsdToUnityImporter/releases) page. The `PsdToUnity` directory can be moved anywhere afterwards.

## Usage

Documentation is WIP but the [basic workflow](https://github.com/ChemiKhazi/UnityPsdImporter#usage) from the old PSD importer still applies.

## Caveats

* Only raster layers are supported as of now. For best results, flatten layers in Photoshop before importing.

## Open source used
* [NtreevSoft PSD Parser](https://github.com/NtreevSoft/psd-parser)
* [@LotteMakesStuff](https://github.com/LotteMakesStuff)'s [EditorCoroutineRunner](https://gist.github.com/LotteMakesStuff/16b5f2fc108f9a0201950c797d53cfbf)
* [FullSerializer](https://github.com/jacobdufault/fullserializer)
