﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OBM.DAL;
using OBM.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using OBM.Models.ViewModels;
using OBM.Models.API;
using OBM.Controllers;

namespace OBM.Controllers
{

    public class TournamentsController : Controller
    {
        private static readonly HttpClient client = new HttpClient();
        private EventContext db = new EventContext();

        // GET: Tournaments
        public ActionResult Index()
        {
            return View(db.Tournaments.ToList());
        }

        // GET: Tournaments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tournament tournament = db.Tournaments.Find(id);
            if (tournament == null)
            {
                return HttpNotFound();
            }
            return View(tournament);
        }

        // GET: Tournaments/Create
        public ActionResult Create()
        {           //FETCH NEEDED USER DATA
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var userid = HttpContext.User.Identity.GetUserId();
                var UserEventsList = db.Events.Where(x => x.OrganizerID == userid);
                ViewBag.UserEventsList = UserEventsList;

                var userApiKey = db.AspNetUsers.Where(x => x.Id == userid).First().ApiKey;
                if (userApiKey == null)
                    userApiKey = "Please_Link_an_API_key_to_your_Account";
                Debug.WriteLine(userApiKey);
                ViewBag.ApiKey = userApiKey;
            }


            return View();
        }

        // POST: Tournaments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TournamentID,TournamentName,EventID,Description,Game,ApiId,UrlString,IsTeams")] Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                db.Tournaments.Add(tournament);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tournament);
        }

        // GET: Tournaments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tournament tournament = db.Tournaments.Find(id);
            if (tournament == null)
            {
                return HttpNotFound();
            }
            return View(tournament);
        }

        // POST: Tournaments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TournamentID,TournamentName,EventID,Description,Game,ApiId,UrlString,IsTeams")] Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tournament).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tournament);
        }

        // GET: Tournaments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tournament tournament = db.Tournaments.Find(id);
            if (tournament == null)
            {
                return HttpNotFound();
            }
            return View(tournament);
        }

        // POST: Tournaments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tournament tournament = db.Tournaments.Find(id);
            db.Tournaments.Remove(tournament);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        [HttpPost]
        public string ChallongeCreate()
        {
            //PARSE VALUES
            var userId = User.Identity.GetUserId();
            var myobject = new {name = Request.Params["name"], description = Request.Params["description"], game = Request.Params["game"], url = Request.Params["myURL"], event_id = Request.Params["event_id"], Private = Request.Params["private"], ranked_by = Request.Params["ranked_by"], open_signup = Request.Params["open_signup"], pts_for_bye = Request.Params["pts_for_bye"], signup_cap = Request.Params["signup_cap"], start_at = Request.Params["start_at"], checkin_duration = Request.Params["checkin_duration"], tournament_type = Request.Params["tournament_type"] };
            var myJSON = new JObject();
            var mytournament = new JObject();

<<<<<<< HEAD
            var myJSON = JObject.FromObject(myobject);



            return ChallongePost(myJSON).ToString();
=======
            mytournament=JObject.FromObject(myobject);
            

           var apikey = db.AspNetUsers.Where(x => x.Id == userId).First().ApiKey;
            myJSON.Add("api_key",apikey);
            myJSON.Add("tournament", JObject.FromObject(myobject));
            mytournament.Add("api_key", apikey);
            Debug.WriteLine("mytournament is: "+ mytournament);
            return ChallongePost(mytournament).ToString();
>>>>>>> dev

        }



        public JObject ChallongePost(JObject myJSON)
        {
            myJSON.Add("private", myJSON["Private"]);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.challonge.com/v1/tournaments.json");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";


            Debug.WriteLine("api_key is : " + myJSON["api_key"]);
            Debug.WriteLine("url is : " + myJSON["url"]);
            Debug.WriteLine("name is : " + myJSON["name"]);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {


                streamWriter.Write(myJSON);
            }

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();



                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
<<<<<<< HEAD
                    var result = streamReader.ReadToEnd();
                    Debug.WriteLine(result);
                    Tournament resultTournament = new Tournament();


                    resultTournament.EventID = (int)myJSON["event_id"];
                    resultTournament.TournamentName = (string)myJSON["name"];
                    resultTournament.UrlString = (string)myJSON["url"];
                    resultTournament.Description = (string)myJSON["description"];
                    resultTournament.Game = (string)myJSON["game_name"];
                    resultTournament.IsStarted = false;
                    //resultTournament.Subdomain = (string)myJSON["subdomain"];
                    //resultTournament.ApiId = (string)myJSON["api_key"];


                    Create(resultTournament);
                    return JObject.Parse(result);
                }//EXCEPTION HAnDlINGSGINJFDIGSF
=======
                var result = streamReader.ReadToEnd();
                Tournament resultTournament = new Tournament();


                resultTournament.EventID = (int)myJSON["event_id"];
                resultTournament.TournamentName = (string)myJSON["name"];
                resultTournament.UrlString = (string)myJSON["url"];
                resultTournament.Description = (string)myJSON["description"];
                resultTournament.Game = (string)myJSON["game_name"];
                if((string)myJSON["state"]=="underway") 
                        resultTournament.IsStarted = true;
                else 
                        resultTournament.IsStarted = false;
                    //resultTournament.Subdomain = (string)myJSON["subdomain"];

                    resultTournament.ApiId = (int)JObject.Parse(result)["tournament"]["id"];


                    Create(resultTournament);
                return JObject.Parse(result);
                 }//EXCEPTION HAnDlINGSGINJFDIGSF
>>>>>>> dev

            }
            catch (System.Net.WebException e)
            {
                return JObject.FromObject(new { error = "webException", message = "please ensure that you have not already create a tournament with this url, and that the URL, name, and event_id fields are accurate." });
            }

        }

        [HttpGet]
        public JArray getTournamentsJson()
        {

            var tournaments = JArray.FromObject(db.Tournaments);
            Debug.WriteLine(tournaments);
            return tournaments;
        }
<<<<<<< HEAD
=======

        [HttpGet]
        public String PublicTournaments()
        {
            var tournamentsDb = db.Tournaments.Select(x => new { x.TournamentID, x.TournamentName, x.Game, x.Description, url = ("/Tournament/Details/" + x.TournamentID) }).ToList();
            var jsonResponse = new JArray();
            var mylist = new List<Object>();

            foreach (var temptourney in tournamentsDb)
            {
                jsonResponse.Add(JObject.FromObject(temptourney));
                mylist.Add(temptourney);
            }


            Debug.WriteLine(jsonResponse);



            var result = JsonConvert.SerializeObject(mylist);


            result.Replace('[', '{');
            result.Replace(']', '}');
            return result;
        }
>>>>>>> dev
    }
}
