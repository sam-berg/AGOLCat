AGOLCat
=======

ArcGIS Online Catalog
The AGOLCat application will query the content of all users within an organization and return a list of all items meeting the criteria which are accessible to the supplied credential.

The result can be downloaded as a CSV file for further processing.

For example, an administrator of an organization can use this to generate a spreadsheet referencing all items in the organization, sorted by file size.

To use:
* Enter the organization URL
* Enter a username and password to use for the query authentication
* Optionally enter a query.  If left blank, all items will be returned.
* If you would like to see image thumbnails on the query result web page, check "Include Thumbnails".  
* If you would like to see the file size value for each item, check "Include Size".  This option requires an additional query for EACH result item so the option is set to "false" by default.

Choose "Refresh" to generate the report.

On the resulting page, choose "Download results" to generate and download a CSV file with the resulting items.

For more information on possible queries, click here.

The web application was written using ASP.NET MVC4 and C#.

Try AGOLCat:
http://bosdemo2.esri.com/apps/agolcat

Note: Included here are the significant files to insert and configure into a Visual Studio MVC 4 Web Application (Razor syntax).  I did not want to include the core application files.  Please configure these into a standard application template.
