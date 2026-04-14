Just Another Data Emancipacifier
==============
This is base project for website scrapping, it contains core code an plugin example 

Files needed to get started:
---------------
**data/products.txt** - put here all products to search for. Both trade id and product id are supported.
**JadeConfig.json** - various paths and settings:
1. path to the products.txt (prodPath)
2. path where the images will be stored (imgDir)
3. http backed address (backend)
4. links to various manufacturers (manufac name : manufac link).
5. (addImagesTimeoutMiliseconds) optional field for a custom time limit for image upload, minimum time hardcoded at 1 sec.

How to use your own plugin:
--
#### Using compiled dll
1. Grab JADE.dll from the main program
2. Either put it in the plugin project, or change HintPath in .csproj to point to it.
3. Compile plugin
4. put .dll file into the plugins directory of the main program
#### Using source code
1. Dowload JADE source code
2. Put your plugin's source code into Plugins directory
3. Compile main project
4. All compiled plugin's dlls will be moved to plugins directory of compiled main program
