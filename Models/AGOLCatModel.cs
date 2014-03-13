using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

namespace AGOLCat.Models
{
  public class AGOLCatalogModel
  {

    public string userName = null;
    public string password = null;
    public string org = "";
    public string query = null;
    public bool bIncludeThumbnails = true;
    public bool bIncludeSize = true;

    public int resultCount = 0;

    string portalURL = "https://www.arcgis.com";
    string searchURL = "http://esrinortheast.maps.arcgis.com/sharing/rest";
    string viewURL = "http://esrinortheast.maps.arcgis.com/home/item.html?id=";
    string orgID = "";
  
    string sUserName = "";
    string sPWD = "";

    string token = "";
    string sFullSearch ="";// "-type:" + "\"" + "Code Attachment" + "\"" + " -type:" + "\"" + "Featured Items" + "\"" + " -type:" + "\"" + "Windows Viewer Add In" + "\"" + " -type:" + "\"" + "Windows Viewer Configuration" + "\"" + " -type:" + "\"" + "Code Attachment" + "\"" ;
    
    List<Result> Results = new List<Result>();


    public List<Result> AGOLCatalogItems
    {
      get
      {

        if (this.userName != null && this.userName!="") this.sUserName = this.userName;
        if (this.password != null && this.password!="") this.sPWD = this.password;

        if (this.org == null || this.org == "") return Results;

        this.token = generateToken();

        //get orgid
        this.orgID = getOrgID();

        if (this.orgID == null || this.orgID == "") return Results;

        if (this.org != null && this.org!="")
        {
          this.searchURL = this.org + "/sharing/rest";
          this.viewURL = this.org + "/home/item.html?id=";
        }
        
   
        List<Result> pList = new List<Result>();

        
        if (this.token == null || this.token == "") return Results;


        int nextRecord = -1;
        int totalRecords = -1;
        int num = -1;
        int start = -1;
        List<Result> allResults = new List<Result>();

        string sQuery = getCatalogQuery2(1, 100);//get first batch

        HttpWebRequest request = WebRequest.CreateHttp(sQuery);
        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        {
          Stream responseStream = response.GetResponseStream();

          DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(CatalogResult));
          CatalogResult t = (CatalogResult)ser.ReadObject(responseStream);

          nextRecord = t.nextStart;
          totalRecords = t.total;
          num = t.num;
          start = t.start;

          #region iterate through
          foreach (Result r in t.results)
          {
            if (this.bIncludeThumbnails || 0==0)
            {
              if (r.thumbnail == null)
              {
                //sbtest r.myThumbnailURL = "http://static.arcgis.com/images/desktopapp.png";
              }
              else
              {
                r.myThumbnailURL = searchURL + "/content/items/" + r.id + "/info/" + r.thumbnail + "?token=" + this.token;
              }
            }

            r.itemURL = this.viewURL + r.id;

            r.created = this.ConvertFromUnixTimestamp(r.created);
        
            r.modified = this.ConvertFromUnixTimestamp(r.modified);


            
            if (r.size == -1) r.size = 0;

            r.size = getSize(r);// r.size;
            r.myRowID = allResults.Count + 1;
            allResults.Add(r);

          }
          #endregion


        }

        //if more records exist
        if (nextRecord > 0)
        {
          while (nextRecord > 0)
          {

            sQuery = getCatalogQuery2(nextRecord, 100);//get next batch

            request = WebRequest.CreateHttp(sQuery);
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
              Stream responseStream = response.GetResponseStream();

              DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(CatalogResult));
              CatalogResult t = (CatalogResult)ser.ReadObject(responseStream);

              #region iterate through
              foreach (Result r in t.results)
              {
                if (this.bIncludeThumbnails || 0==0)
                {
                  if (r.thumbnail == null)
                  {
                    r.myThumbnailURL = "http://static.arcgis.com/images/desktopapp.png";
                  }
                  else
                  {
                    r.myThumbnailURL = searchURL + "/content/items/" + r.id + "/info/" + r.thumbnail + "?token=" + this.token;
                  }
                }
                if (r.size == -1) r.size = 0;

                if (r.id == "f02848c17cfb4fa080d9aa15e112c5ba")
                {
                  string sdb = "";
                }

                r.created = this.ConvertFromUnixTimestamp(r.created);
                r.modified = this.ConvertFromUnixTimestamp(r.modified);


                r.size = getSize(r);// r.size;
                r.itemURL = this.viewURL + r.id;
                r.myRowID = allResults.Count + 1;
                allResults.Add(r);

              }
              #endregion


              nextRecord = t.nextStart;
              totalRecords = t.total;
              num = t.num;
              start = t.start;
            }

          }
        }
        //end if

        this.Results = allResults;
        //this.resultCount = allResults.Count;
        return (allResults);

      }
    }

    public string ConvertFromUnixTimestamp(object timestamp)
    {
      try
      {
        double ts = Convert.ToDouble(timestamp);
        ts = ts / 1000;
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return origin.AddSeconds(ts).ToShortDateString();
      }
      catch
      {

      }
      return "";
    }

    private string getOrgID()
    {

      string sURL = this.org + "/sharing/rest/portals/self?f=json";

      sURL += "&token=" + this.token;

      string sResult="";

      WebRequest request = WebRequest.Create(sURL);

      using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
      {


        Stream responseStream = copyStream(response.GetResponseStream());

        StreamReader reader = new StreamReader(responseStream);


        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SelfObject));
        SelfObject t = (SelfObject)ser.ReadObject(responseStream);

        sResult = t.id;
      }

      return sResult;
    }

    private long getSize(Result r)
    {

      if (!bIncludeSize) return 0;

      long result = 0;
      string sURL = this.searchURL + "/content/items/" + r.id + "?f=json&token=" + this.token;

      WebRequest request = WebRequest.Create(sURL);

      using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
      {

        Stream responseStream = copyStream(response.GetResponseStream());

        StreamReader reader = new StreamReader(responseStream);

        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Result));
        Result t = (Result)ser.ReadObject(responseStream);
        result = t.size;
        if (result > 0)
        { result = result / 1024; }
        else
        { result = 0; }

      }

      return result;



    }

    private string getCatalogQuery2(int start, int num)
    {
      string sQuery = null;
      if (this.query != null && this.query != "")
      {
        sQuery = this.query;
      }
      else
      {
        sQuery = this.sFullSearch;
      }


      string sCatalogQuery = this.searchURL + "/search?q=" + sQuery;
      if (this.orgID != null && this.orgID != "") sCatalogQuery += " orgid:" + this.orgID;
      sCatalogQuery += "&f=json&num="+num + "&start=" + start;
      if (this.token != null && this.token != "") sCatalogQuery += "&token=" + this.token;

      //string s = this.searchURL + "/search?q=" + sQuery + " orgid:" + this.orgID + "&f=json&start=" + start + "&num=" + num + "&token=" + this.token;

      return sCatalogQuery;

    }

    private string generateToken()
    {

      string result = "";
      string sURL = this.portalURL + "/sharing/rest/generateToken?";

      WebRequest request = WebRequest.Create(sURL);

      string st = "username=" + this.sUserName + "&";
      st += "password=" + this.sPWD + "&";
      st += "referer=" + "http" + "&";
      st += "expiration=60&f=json";
      byte[] byteArray = Encoding.UTF8.GetBytes(st);

      request.Method = "POST";
      request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

      Stream dataStream = request.GetRequestStream();
      dataStream.Write(byteArray, 0, byteArray.Length);
      dataStream.Close();

      using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
      {

        Stream responseStream = copyStream(response.GetResponseStream());

        StreamReader reader = new StreamReader(responseStream);

        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(TokenParams));
        TokenParams t = (TokenParams)ser.ReadObject(responseStream);
        result = t.token;
      }

      return result;
    }

    private static Stream copyStream(Stream st)
    {
      const int readSize = 256;
      byte[] buffer = new byte[readSize];
      MemoryStream ms = new MemoryStream();

      int count = st.Read(buffer, 0, readSize);
      while (count > 0)
      {
        ms.Write(buffer, 0, count);
        count = st.Read(buffer, 0, readSize);
      }
      ms.Position = 0;
      st.Close();
      return ms;

    }
  }

  public class AGOLCatalogItem
  {
    public string Name { get; set; }

  }

  public class TokenParams
  {
    public string token { get; set; }

  }

  public class Result
  {
    public string id { get; set; }
    public string owner { get; set; }
    public object created { get; set; }
    public object modified { get; set; }
    public string guid { get; set; }
    public string name { get; set; }
    public string title { get; set; }
    public string type { get; set; }
    public List<string> typeKeywords { get; set; }
    public string description { get; set; }
    public List<string> tags { get; set; }
    public string snippet { get; set; }
    public string thumbnail { get; set; }
    public object documentation { get; set; }
    public List<object> extent { get; set; }
    public string spatialReference { get; set; }
    public string accessInformation { get; set; }
    public string licenseInfo { get; set; }
    public string culture { get; set; }
    public object properties { get; set; }
    public string url { get; set; }
    public string access { get; set; }
//    public int size { get; set; }
    public long size { get; set; }
    public List<object> appCategories { get; set; }
    public List<object> industries { get; set; }
    public List<object> languages { get; set; }
    public object largeThumbnail { get; set; }
    public object banner { get; set; }
    public List<object> screenshots { get; set; }
    public bool listed { get; set; }
    public int numComments { get; set; }
    public int numRatings { get; set; }
    public double avgRating { get; set; }
    public int numViews { get; set; }

    public string myThumbnailURL { get; set; }
    
    public int myRowID { get; set; }
    public string itemURL { get; set; }


  }

  public class CatalogResult
  {
    public string query { get; set; }
    public int total { get; set; }
    public int start { get; set; }
    public int num { get; set; }
    public int nextStart { get; set; }
    public List<Result> results { get; set; }
  }


  public class Layer
  {
    public int id { get; set; }
    public string name { get; set; }
    public int parentLayerId { get; set; }
    public bool defaultVisibility { get; set; }
    public object subLayerIds { get; set; }
    public int minScale { get; set; }
    public int maxScale { get; set; }
  }

  public class SpatialReference
  {
    public int wkid { get; set; }
  }

  public class Origin
  {
    public double x { get; set; }
    public double y { get; set; }
  }

  public class SpatialReference2
  {
    public int wkid { get; set; }
  }

  public class Lod
  {
    public int level { get; set; }
    public double resolution { get; set; }
    public double scale { get; set; }
  }

  public class TileInfo
  {
    public int rows { get; set; }
    public int cols { get; set; }
    public int dpi { get; set; }
    public string format { get; set; }
    public int compressionQuality { get; set; }
    public Origin origin { get; set; }
    public SpatialReference2 spatialReference { get; set; }
    public List<Lod> lods { get; set; }
  }

  public class SpatialReference3
  {
    public int wkid { get; set; }
  }

  public class InitialExtent
  {
    public double xmin { get; set; }
    public double ymin { get; set; }
    public double xmax { get; set; }
    public double ymax { get; set; }
    public SpatialReference3 spatialReference { get; set; }
  }

  public class SpatialReference4
  {
    public int wkid { get; set; }
  }

  public class FullExtent
  {
    public double xmin { get; set; }
    public double ymin { get; set; }
    public double xmax { get; set; }
    public double ymax { get; set; }
    public SpatialReference4 spatialReference { get; set; }
  }

  public class DocumentInfo
  {
    public string Title { get; set; }
    public string Author { get; set; }
    public string Comments { get; set; }
    public string Subject { get; set; }
    public string Category { get; set; }
    public string Keywords { get; set; }
    public string Credits { get; set; }
  }

  public class ResourceInfo
  {
    public double currentVersion { get; set; }
    public string serviceDescription { get; set; }
    public string mapName { get; set; }
    public string description { get; set; }
    public string copyrightText { get; set; }
    public List<Layer> layers { get; set; }
    public List<object> tables { get; set; }
    public SpatialReference spatialReference { get; set; }
    public bool singleFusedMapCache { get; set; }
    public TileInfo tileInfo { get; set; }
    public InitialExtent initialExtent { get; set; }
    public FullExtent fullExtent { get; set; }
    public string units { get; set; }
    public string supportedImageFormatTypes { get; set; }
    public DocumentInfo documentInfo { get; set; }
    public string capabilities { get; set; }
  }

  public class BaseMapLayer
  {
    public string id { get; set; }
    public int opacity { get; set; }
    public bool visibility { get; set; }
    public string url { get; set; }
    public ResourceInfo resourceInfo { get; set; }
  }

  public class DefaultBasemap
  {
    public string id { get; set; }
    public string title { get; set; }
    public List<BaseMapLayer> baseMapLayers { get; set; }
    public List<object> operationalLayers { get; set; }
  }

  public class SpatialReference5
  {
    public int wkid { get; set; }
  }

  public class DefaultExtent
  {
    public string type { get; set; }
    public double xmin { get; set; }
    public double ymin { get; set; }
    public double xmax { get; set; }
    public double ymax { get; set; }
    public SpatialReference5 spatialReference { get; set; }
  }

  public class FeaturedGroup
  {
    public string owner { get; set; }
    public string title { get; set; }
  }

  public class M
  {
    public string __invalid_name__120000460 { get; set; }
    public string __invalid_name__120000814 { get; set; }
    public string __invalid_name__120000461 { get; set; }
    public string __invalid_name__120000464 { get; set; }
    public string __invalid_name__120000474 { get; set; }
    public string __invalid_name__120000467 { get; set; }
    public string __invalid_name__120000468 { get; set; }
    public string __invalid_name__120000470 { get; set; }
    public string __invalid_name__120000473 { get; set; }
    public string __invalid_name__120000456 { get; set; }
    public string __invalid_name__120000454 { get; set; }
    public string __invalid_name__120000455 { get; set; }
    public string __invalid_name__120000503 { get; set; }
    public string __invalid_name__120000516 { get; set; }
    public string __invalid_name__120000466 { get; set; }
    public string __invalid_name__120000471 { get; set; }
    public string __invalid_name__120000469 { get; set; }
    public string __invalid_name__120000463 { get; set; }
    public string __invalid_name__120000465 { get; set; }
    public string __invalid_name__120000815 { get; set; }
    public string __invalid_name__120000816 { get; set; }
    public string __invalid_name__120000817 { get; set; }
  }

  public class HelpMap
  {
    public string v { get; set; }
    public M m { get; set; }
  }

  public class AsyncClosestFacility
  {
    public string url { get; set; }
  }

  public class AsyncLocationAllocation
  {
    public string url { get; set; }
  }

  public class AsyncServiceArea
  {
    public string url { get; set; }
  }

  public class AsyncVRP
  {
    public string url { get; set; }
  }

  public class ClosestFacility
  {
    public string url { get; set; }
  }

  public class Elevation
  {
    public string url { get; set; }
  }

  public class ElevationSync
  {
    public string url { get; set; }
  }

  public class Geocode
  {
    public string url { get; set; }
    public string northLat { get; set; }
    public string southLat { get; set; }
    public string eastLon { get; set; }
    public string westLon { get; set; }
    public string name { get; set; }
    public bool suggest { get; set; }
    public string singleLineFieldName { get; set; }
    public string placeholder { get; set; }
    public int? zoomScale { get; set; }
    public bool? placefinding { get; set; }
  }

  public class Geometry
  {
    public string url { get; set; }
  }

  public class Hydrology
  {
    public string url { get; set; }
  }

  public class PrintTask
  {
    public string url { get; set; }
  }

  public class Route
  {
    public string url { get; set; }
  }

  public class ServiceArea
  {
    public string url { get; set; }
  }

  public class SyncVRP
  {
    public string url { get; set; }
  }

  public class Traffic
  {
    public string url { get; set; }
  }

  public class Analysis
  {
    public string url { get; set; }
  }

  public class Geoenrichment
  {
    public string url { get; set; }
  }

  public class HelperServices
  {
    public AsyncClosestFacility asyncClosestFacility { get; set; }
    public AsyncLocationAllocation asyncLocationAllocation { get; set; }
    public AsyncServiceArea asyncServiceArea { get; set; }
    public AsyncVRP asyncVRP { get; set; }
    public ClosestFacility closestFacility { get; set; }
    public Elevation elevation { get; set; }
    public ElevationSync elevationSync { get; set; }
    public List<Geocode> geocode { get; set; }
    public Geometry geometry { get; set; }
    public Hydrology hydrology { get; set; }
    public PrintTask printTask { get; set; }
    public Route route { get; set; }
    public ServiceArea serviceArea { get; set; }
    public SyncVRP syncVRP { get; set; }
    public Traffic traffic { get; set; }
    public Analysis analysis { get; set; }
    public Geoenrichment geoenrichment { get; set; }
  }

  public class Links
  {
  }

  public class PortalProperties
  {
    public Links links { get; set; }
  }

  public class RotatorPanel
  {
    public string id { get; set; }
    public string innerHTML { get; set; }
  }

  public class SelfObject
  {
    public string access { get; set; }
    public bool allSSL { get; set; }
    public string basemapGalleryGroupQuery { get; set; }
    public bool canShareBingPublic { get; set; }
    public string colorSetsGroupQuery { get; set; }
    public bool commentsEnabled { get; set; }
    public string culture { get; set; }
    public string customBaseUrl { get; set; }
    public DefaultBasemap defaultBasemap { get; set; }
    public DefaultExtent defaultExtent { get; set; }
    public string description { get; set; }
    public List<FeaturedGroup> featuredGroups { get; set; }
    public string featuredGroupsId { get; set; }
    public string featuredItemsGroupQuery { get; set; }
    public string galleryTemplatesGroupQuery { get; set; }
    public string helpBase { get; set; }
    public HelpMap helpMap { get; set; }
    public HelperServices helperServices { get; set; }
    public string homePageFeaturedContent { get; set; }
    public int homePageFeaturedContentCount { get; set; }
    public int httpPort { get; set; }
    public int httpsPort { get; set; }
    public string id { get; set; }
    public string ipCntryCode { get; set; }
    public bool isPortal { get; set; }
    public string layerTemplatesGroupQuery { get; set; }
    public string name { get; set; }
    public string portalHostname { get; set; }
    public string portalMode { get; set; }
    public string portalName { get; set; }
    public PortalProperties portalProperties { get; set; }
    public object portalThumbnail { get; set; }
    public string region { get; set; }
    public List<RotatorPanel> rotatorPanels { get; set; }
    public bool showHomePageDescription { get; set; }
    public string staticImagesUrl { get; set; }
    public bool supportsHostedServices { get; set; }
    public bool supportsOAuth { get; set; }
    public string symbolSetsGroupQuery { get; set; }
    public string templatesGroupQuery { get; set; }
    public string thumbnail { get; set; }
    public string units { get; set; }
    public string urlKey { get; set; }
  }













}