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
5) (addImagesTimeoutMiliseconds) optional field for a custom time limit for image upload, minimum time hardcoded at 1 sec.
