using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AGOLCatalogApp.Controllers
{
  public class AGOLCatController : Controller
  {
    
    // GET: /AGOLCat/
    [HttpGet]
    //public ActionResult Foo(string id)
    public ActionResult Foo(string info)
    {
      string[] split = { "||||" };
      string[]val = info.Split(split,StringSplitOptions.RemoveEmptyEntries);
      string id = val[0];
      string orgId = val[1];
      var  person = "umm, hi " +id;
      //ViewBag.userToken = id;

      //
      myModel.userToken = id;
      myModel.orgID = orgId;
      myModel.org = "http://" + val[2] + "." + val[3];
      Session["userToken"] = id;
      Session["userOrgId"] = orgId;
      Session["userOrg"] = myModel.org;
      //return RedirectToAction("AGOLCat", "AGOLCat");
      return Json(person, JsonRequestBehavior.AllowGet);

      

    }

    private AGOLCat.Models.AGOLCatalogModel myModel = new AGOLCat.Models.AGOLCatalogModel();

    public ActionResult AGOLCat()
    {
      if (Session["userToken"] != null)
      { 
        string id = Session["userToken"].ToString();
        string orgId = Session["userOrgId"].ToString();
        string org = Session["userOrg"].ToString();
        myModel.userToken = id;
        myModel.userAuthenticated = "";
        myModel.orgID = orgId;
        myModel.org = org;
      }

      
      return View(myModel);
    }

    public ActionResult Login()
    {

      return View();
      
    }

    public ActionResult Logout()
    {
      
      Session["userToken"]=null;
      Session["userOrgId"]=null;
      Session["userOrg"]=null;

      Session.Clear();
      
      myModel.userToken = null;
      myModel.userAuthenticated = "disabled";
      myModel.orgID = null;
      myModel.org = null;

      return View();

    }

    //public ActionResult Refresh(string userName, string password, string org, string query, bool bIncludeThumbnails, bool bIncludeSize)
    public ActionResult Refresh(string query, bool bIncludeThumbnails, bool bIncludeSize)
    {

      AGOLCat.Models.AGOLCatalogModel pModel = myModel;// new AGOLCat.Models.AGOLCatalogModel();
      pModel.userToken = Session["userToken"].ToString();
      pModel.userOrgId = Session["userOrgId"].ToString();
      pModel.org = Session["userOrg"].ToString();
      //pModel.userName = userName;
      //pModel.password = password;
      //pModel.org = org;

      pModel.bIncludeThumbnails = bIncludeThumbnails;
      pModel.bIncludeSize = bIncludeSize;
      pModel.query = query;

      this.myModel = pModel;
      this.Session.Add("myModel", pModel);
      ViewBag.myModel = pModel;

      return View("AGOLCat", pModel);

    }

    private void writeReportHeader(StreamWriter writer)
    {

      try
      {
        string s = "";

        s += "id,";
        s += "owner,";
        s += "created,";
        s += "modified,";
        //s += "guid,";
        s += "name,";
        s += "title,";
        s += "type,";
        s += "typeKeywords,";
        s += "description,";
        s += "tags,";
        s += "snippet,";
        s += "thumbnail,";
        //s += "documentation,";
        s += "extent,";
        s += "spatialReference,";
        s += "accessInformation,";
        s += "licenseInfo,";
        s += "culture,";
        //s += "properties,";
        s += "url,";
        s += "access,";
        s += "size,";
        //s += "appCategories,";
        //s += "industries,";
        //s += "languages,";
        //s += "largeThumbnail,";
        //s += "banner,";
        //s += "screenshots,";
        s += "listed,";
        s += "numComments,";
        s += "numRatings,";
        s += "avgRating,";
        s += "numViews,";


        s += "itemURL";

        writer.WriteLine(s);
      }
      catch (Exception ex)
      {


      }

    }

    private void writeReportRecord(StreamWriter writer, AGOLCat.Models.Result r)
    {

      try
      {
        string s = "";

        s += getResultValue(r.id) + ",";
        s += getResultValue(r.owner) + ",";

        s += getResultValue(r.created) + ",";


        s += getResultValue(r.modified) + ",";


        //s += getResultValue(r.guid) + ",";
        s += getResultValue(r.name) + ",";
        s += getResultValue(r.title) + ",";
        s += getResultValue(r.type) + ",";

        string sKeyWords = "";
        foreach (string sKW in r.typeKeywords)
        {
          sKeyWords += sKW + ",";
        }
        if (sKeyWords.Length > 0)
          sKeyWords = sKeyWords.Remove(sKeyWords.LastIndexOf(","));

        s += getResultValue(sKeyWords) + ",";
        s += getResultValue(r.description) + ",";

        string sTags = "";
        foreach (string sKW in r.tags)
        {
          sTags += sKW + ",";
        }
        if (sTags.Length > 0)
          sTags = sTags.Remove(sTags.LastIndexOf(","));

        s += getResultValue(sTags) + ",";


        s += getResultValue(r.snippet) + ",";
        s += getResultValue(r.thumbnail) + ",";
        //s += getResultValue(r.documentation) + ",";

       
        s += "" + ",";
        //s += getResultValue(r.extent) + ",";

        s += getResultValue(r.spatialReference) + ",";
        s += getResultValue(r.accessInformation) + ",";
        s += getResultValue(r.licenseInfo) + ",";
        s += getResultValue(r.culture) + ",";
        //s += getResultValue(r.properties) + ",";
        s += getResultValue(r.url) + ",";
        s += getResultValue(r.access) + ",";
        s += getResultValue(r.size) + ",";
        //s += getResultValue(r.appCategories) + ",";
        //s += getResultValue(r.industries) + ",";
        //s += getResultValue(r.languages) + ",";
        //s += getResultValue(r.largeThumbnail) + ",";
        //s += getResultValue(r.banner) + ",";
        //s += getResultValue(r.screenshots) + ",";
        s += getResultValue(r.listed) + ",";
        s += getResultValue(r.numComments) + ",";
        s += getResultValue(r.numRatings) + ",";
        s += getResultValue(r.avgRating) + ",";
        s += getResultValue(r.numViews) + ",";


        s += getResultValue(r.itemURL);


        writer.WriteLine(s);
      }
      catch (Exception ex)
      {


      }
    }

    public DateTime ConvertFromUnixTimestamp(double timestamp)
    {
      timestamp = timestamp / 1000;
      DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
      return origin.AddSeconds(timestamp);
    }

    public string getResultValueDate(object oVal)
    {

      return ConvertFromUnixTimestamp(Convert.ToDouble(oVal)).ToShortDateString();

    }

    public string getResultValue(object oVal)
    {

      if (oVal == null) return "";

      try
      {

        string sResult = oVal.ToString();

        if (sResult.IndexOf(",") > 0)
        {
          sResult = sResult.Replace("\"", "\"\"");
          return "\"" + sResult.ToString() + "\"";

        }
        else
        {
          return sResult.ToString();
        }
      }
      catch
      {
        return "";
      }
    }

    public FileResult Download()
    {

      if (0 == 1)
      {

        MemoryStream ms = new MemoryStream();

        byte[] ba;
        using (StreamWriter writer = new StreamWriter(ms))
        {
          writer.WriteLine("hi");
          writer.Flush();
          ba = new byte[ms.Length];
          ms.Position = 0;
          ms.Read(ba, 0, (int)ms.Length);


        }

        return File(ba, "application/txt", "AGOLCatResults.txt");

      }
      else
      {

        if (this.Session["myModel"] != null)
        {
          AGOLCat.Models.AGOLCatalogModel pModel = this.Session["myModel"] as AGOLCat.Models.AGOLCatalogModel;

          MemoryStream ms = new MemoryStream();

          byte[] ba;
          using (StreamWriter writer = new StreamWriter(ms))
          {
            writeReportHeader(writer);

            int iCounter = 0;
            foreach (AGOLCat.Models.Result r in pModel.AGOLCatalogItems)
            {
              writeReportRecord(writer, r);
              iCounter++;

              System.Diagnostics.Debug.WriteLine("writing record " + iCounter.ToString());
            }

            writer.Flush();
            ba = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(ba, 0, (int)ms.Length);

          }

          return File(ba, "application/txt", "AGOLCatResults.csv");

        }

      }

      return null;

    }
  }
}
