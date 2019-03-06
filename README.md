# Unity-Build-Bridge
The Unity Build Bridge is an editor extension for the Unity3D game engine, I developed for my daily work at [VRTX Labs GmbH](www.vrtxlabs.com).\
It is used to automate and streamline several steps of my mobile-app development workflow and takes some repetetive tasks and actions off my list during development.

## Requirements
This package is developed for use on Windows platform and is untested in a macOS or Linux environment. Several functions are based on specific windows executables and will require adjustments to work on macOS or Linux

The build bridge for Android requires an Android SDK to be installed and setup to work with your installed Unity version.

The build bridge for iOS requires the [iOS Build Environment](https://www.pmbaty.com/iosbuildenv) to be installed and configured to work on your machine (refer to the tool-website for details on this)

To use the package with your Unity version you need at least Unity 2018.1 or newer. It should work with older versions, although the internal build API of Unity has changed since and changes need to be applied. \
If you point out problems, I'll be glad to add them for backward compatibility.\
\
Beside loading it as a unity package manager package, you can import the source into your project directly (especially in older versions of Unity3D).

## Remarks
Unfortunately I didn't have the time to do a more complete documentation and implementing more features (for the sake of completeness) and this state is all I can provide for now.\
\
If you decide to give it a try and stumble across problems and errors, leave an issue here on github.
If you try to use it on Linux or macOS and provide me with more information to make it more cross-platform compatible in several points, I'll appreciate it.
