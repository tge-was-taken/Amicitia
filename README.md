# Amicitia
Editor for file formats used in Atlus' Persona games

# Solution structure

## Amicitia
This is the gui front-end of AtlusLibSharp. It aims to provide an easy to access way to edit or create content supported by the library.

## AmicitiaLibrary
This is the main file format parsing library, providing methods and classes representing the binary file structures in a managed and object-oriented fashion.

## Commandline tools
These are command line front-ends for certain functionalities of AmicitiaLibrary. They also provide examples of how to use the library.

# Basic guide on how to import custom RMD models

1. Open up any RMD model.
2. Navigate the tree and export the clump you wish to replace (for character models there's usually only one)
3. Right click on the clump and select export. Select Assimp Supported Model and make sure the extension is set to .dae
4. Import the file into 3ds Max or any other 3d modeler, and skin your custom model over the imported bones.
5. Export your custom model to FBX 2011 ASCII format
6. Right click on the clump and select Replace. Select the FBX file you just exported.
7. Your model will now have been imported over the original.
8. If you need to add custom textures, remove the original textures from the Textures node, right click on the Textures node, select Add and select all of your custom model's textures.
