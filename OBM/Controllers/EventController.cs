﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Diagnostics;
using System.Web.Mvc;
using Newtonsoft.Json;
using OBM.DAL;
using OBM.Models;
using OBM.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.IO;
using static MoreLinq.Extensions.MaxByExtension;
using static MoreLinq.Extensions.MinByExtension;
using Microsoft.Ajax.Utilities;

namespace OBM.Controllers
{

    public class EventsController : Controller
    {

        private EventContext db = new EventContext();

        // GET: Events
        public ActionResult Index()
        {
            var eventViewList = new List<EventViewModel>();
            foreach (var i in db.Events.ToList())
            {
                if (i.Public == true)
                    eventViewList.Add(new EventViewModel(i, HttpContext.GetOwinContext().Get<ApplicationUserManager>().FindById(@i.OrganizerID).UserName));
            }
            return View(eventViewList);
        }

        public ActionResult Manage(int? id)
        {
            if (id == null)
            {
                throw new HttpException(400, "Bad Request");
            }
            var eventView = new EventViewModel(db.Events.Find(id), HttpContext.GetOwinContext().Get<ApplicationUserManager>().FindById(db.Events.Find(id).OrganizerID).UserName);
            if (eventView == null)
            {
                throw new HttpException(404, "Page not Found");
            }
            if ((Request.IsAuthenticated && (User.Identity.GetUserId() == eventView.OrganizerID)))
            {
                ViewBag.Access = true;
            }
            else
                ViewBag.Access = false;

            return View(eventView);
        }

        [HttpGet]
        public String PublicEvents()
        {
            var eventsDb = db.Events.Where(x => x.Public).Select(x=>new { x.EventID, x.EventName, x.Description, x.Location, url=("/Events/Details/"+x.EventID)}).ToList();
            var jsonResponse = new JArray();
            var mylist = new List<Object>();

            foreach(var tempevent in eventsDb)
            {
                var organizerId = db.Events.Find(tempevent.EventID).OrganizerID;
                var organizerName = db.AspNetUsers.Find(organizerId).UserName;

                var tempJson = JObject.FromObject(tempevent);
                tempJson.Add("UserName", organizerName);

                jsonResponse.Add(tempJson);
                
                mylist.Add(new { tempevent.EventID, tempevent.EventName, tempevent.Description, tempevent.Location, UserName = organizerName, url = ("/Events/Details/" + tempevent.EventID) });
            }


            Debug.WriteLine(jsonResponse);


            
            var result=JsonConvert.SerializeObject(mylist);


            result.Replace('[', '{');
            result.Replace(']', '}');
            return result;
        }



        // GET: Events/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                throw new HttpException(400, "Bad Request");
            }
            var eventView = new EventViewModel(db.Events.Find(id), HttpContext.GetOwinContext().Get<ApplicationUserManager>().FindById(db.Events.Find(id).OrganizerID).UserName);
            if (eventView == null)
            {
                throw new HttpException(404, "Page not Found");
            }
            if ((eventView.Public == true) || (Request.IsAuthenticated && (User.Identity.GetUserId() == eventView.OrganizerID)))
            {
                ViewBag.Access = true;
            }
            else
                ViewBag.Access = false;

            return View(eventView);
        }

        // GET: Events/Create
        public ActionResult Create()
        {
            ViewBag.OrganizerID = User.Identity.GetUserId();
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EventID,OrganizerID,EventName,Description,Location,Public")] Event @event)
        {
            if (ModelState.IsValid)
            {
                db.Events.Add(@event);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            ViewBag.OrganizerID = User.Identity.GetUserId();
            return View(@event);
        }

        // GET: Events/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                throw new HttpException(400, "Bad Request");
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                throw new HttpException(404, "Page not Found");
            }
            if ((Request.IsAuthenticated && (User.Identity.GetUserId() == @event.OrganizerID)))
            {
                ViewBag.Access = true;
            }
            else
                ViewBag.Access = false;
            ViewBag.OrganizerID = User.Identity.GetUserId();
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EventID,OrganizerID,EventName,Description,Location,Public")] Event @event)
        {
            if (ModelState.IsValid)
            {
                db.Entry(@event).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            if ((Request.IsAuthenticated && (User.Identity.GetUserId() == @event.OrganizerID)))
            {
                ViewBag.Access = true;
            }
            else
                ViewBag.Access = false;
            return View(@event);
        }

        // GET: Events/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                throw new HttpException(400, "Bad Request");
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                throw new HttpException(404, "Page not Found");
            }
            if ((Request.IsAuthenticated && (User.Identity.GetUserId() == @event.OrganizerID)))
            {
                ViewBag.Access = true;
            }
            else
                ViewBag.Access = false;
            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Event @event = db.Events.Find(id);
            db.Events.Remove(@event);
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

        public ActionResult ResponsiveEvents(String search)
        {
            ViewBag.search = search;
            return View();
        }

        [HttpGet]
        public JsonResult EventList(String location, String name)   //ONCE AN EVENT DATE IS PROPERLY IMPLEMENTED IN THE DATABASE, IT WILL BE USED TO SORT THE RESULTS OF THESE QUERIES (.Orderby(x=>x.DateTime))
        {
            Debug.WriteLine(location, name);

            List<Event> eventList = new List<Event>();
            JArray TournamentList = new JArray();

            Debug.WriteLine("LOCATION IS: " + location);
            Debug.WriteLine("NAME IS: " + name);
            if (location != null&&location!="")// if we know the location of event, get the events at the location
            {
                foreach (var i in db.Events.Where(p => p.Public && p.Location.Contains(location)).ToList())
                {
                    eventList.Add(i);
                }
            }

            else// if the event field is null, or empty, return all events
            {
                foreach (var i in db.Events.Where(p => p.Public).ToList())
                {
                    eventList.Add(i);
                }
            }

            //NOW THE SEARCH BY NAME
            if (name != null && name != "")
            {
                var eventListTemp =new List<Event>(eventList);
                foreach (var i in eventListTemp)
                {
                    if(!i.EventName.Contains(name))
                    {
                        eventList.Remove(i);
                    }
                }
            }


    
            return Json (JsonConvert.SerializeObject(eventList, Formatting.Indented, new JsonSerializerSettings()
            {
            }), JsonRequestBehavior.AllowGet);

        }

        public JsonResult TournamentList(int? id)
        {
            List<Tournament> TournamentList = new List<Tournament>();

            foreach (var i in db.Tournaments.Where(x=>x.EventID==id).ToList())
            {
                TournamentList.Add(i);
            }

            return Json(JsonConvert.SerializeObject(TournamentList, Formatting.Indented), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult NewTournament(int? id)
        {
            if (id == null)
            {
                throw new HttpException(400, "Bad Request");
            }
            else if (db.Events.Find(id).OrganizerID == User.Identity.GetUserId())
            {
                ViewBag.Access = true;
                var challongURL = Request.QueryString["search"];
                //var api_key = Request.QueryString["api_key"];
                var api_key = HttpContext.GetOwinContext().Get<ApplicationUserManager>().FindById(User.Identity.GetUserId()).ApiKey;
                ViewBag.Found = api_key;
                ViewBag.api_key = api_key;
                ViewBag.Success = "";

                if (challongURL == string.Empty)
                {
                    ViewBag.Success = "No url entered.";
                }
                else if (api_key == null || api_key == string.Empty)
                {
                    ViewBag.Success = "No api key associated with your account. Don't have one? You can find yours ";
                    ViewBag.Link1 = "https://challonge.com/settings/developer";
                    ViewBag.Link2 = "here.";
                }
                else if (challongURL != null && api_key != null)
                {
                    if (!Uri.IsWellFormedUriString(challongURL, UriKind.Absolute))
                    {
                        ViewBag.Success = "Invalid URL";
                    }
                    else
                    {
                        var searchSegments = new Uri(challongURL).Segments;
                        if (searchSegments != null)
                        {
                            string urlEnd = searchSegments[searchSegments.Length - 1];
                            string tournamentRoute = @"https://api.challonge.com/v1/tournaments/" + urlEnd + ".json?api_key=" + api_key;
                            //ViewBag.Found = participantsRoute;
                            string responseTournament = "";
                            ViewBag.Found = api_key;
                            try
                            {
                                HttpWebRequest requestTournaments = (HttpWebRequest)WebRequest.Create(tournamentRoute);
                                requestTournaments.Method = "Get";
                                requestTournaments.Headers.Add("api_key", api_key);
                                HttpWebResponse response1 = (HttpWebResponse)requestTournaments.GetResponse();
                                using (System.IO.StreamReader sr = new System.IO.StreamReader(response1.GetResponseStream()))
                                {
                                    responseTournament = sr.ReadToEnd();
                                }

                                JObject jsonTournament = JObject.Parse(responseTournament);
                                Tournament newTournament = new Tournament
                                {
                                    TournamentName = (string)jsonTournament["tournament"]["name"],
                                    EventID = id ?? default(int),
                                    Description = (string)jsonTournament["tournament"]["description"],
                                    Game = (string)jsonTournament["tournament"]["game_name"],
                                    ApiId = (int)jsonTournament["tournament"]["id"],
                                    UrlString = (string)jsonTournament["tournament"]["url"],
                                    IsTeams = (bool)jsonTournament["tournament"]["teams"],
                                    IsStarted = false
                                };

                                if (ModelState.IsValid)
                                {
                                    EventContext DB = new EventContext();
                                    DB.Tournaments.Add(newTournament);
                                    DB.SaveChanges();
                                    ViewBag.Success = "Tournament was added ";
                                    ViewBag.Link1 = "/Events/Tournament/" + newTournament.TournamentID;
                                    ViewBag.Link2 = "here.";
                                    //Add link here to event page with tournament showing
                                }
                                else
                                {
                                    ViewBag.Success = "Tournament was not saved";
                                }
                            }
                            catch
                            {
                                ViewBag.Success = "Unable to get tournament data. Please review and re-enter the URL and Api Key above.";
                            }
                        }
                    }
                }
            }
            else
                ViewBag.Access = false;

            return View();
        }

        [HttpGet]
        public ActionResult Tournament(int? id)
        {
            if (id == null)
            {
                throw new HttpException(404, "Page not Found");
            }

            try
            {
                Tournament found = db.Tournaments.Find(id);
                TournamentViewModel tour = new TournamentViewModel(found, HttpContext.GetOwinContext().Get<ApplicationUserManager>().FindById(db.Events.Find(found.EventID).OrganizerID).UserName);
            
                if (tour == null)
                {
                    throw new HttpException(404, "Page not Found");
                }
                if ((tour.Public == true) || (Request.IsAuthenticated && (User.Identity.GetUserId() == db.Events.Find(found.EventID).OrganizerID)))
                {
                    ViewBag.Access = true;
                }
                else
                {
                    ViewBag.Access = false;
                }
                return View(tour);

            }
            catch 
            {
                throw new HttpException(404, "Page not Found");
            }
        }



        [HttpPost]
        public ActionResult Tournament(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Tournament found = db.Tournaments.Find(id);
                    db.Tournaments.Remove(found);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                throw new HttpException(401, "Tournament does not exist");
            }
        }


        [HttpGet]
        public ActionResult Competitor(int? id)
        {
            if (id == null)
            {
                throw new HttpException(404, "Page not Found");
            }

            try
            {
                Competitor comp = db.Competitors.Find(id);
                Event even = db.Events.Find(comp.EventID);
                if (comp == null)
                {
                    throw new HttpException(404, "Page not Found");
                }
                if ((even.Public == true) || (Request.IsAuthenticated && (User.Identity.GetUserId() == even.OrganizerID)))
                {
                    ViewBag.Access = true;
                }
                else
                {
                    ViewBag.Access = false;
                }
                return View(comp);
            }
            catch
            {
                throw new HttpException(404, "Page not Found");
            }
        }

        [HttpPost]
        public JsonResult Competitor(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var found = db.Competitors.Find(id);
                    if (found.BusyState == "a")
                    {
                        found.BusyState = "b";
                    }
                    else
                    {
                        found.BusyState = "a";
                    }
                    db.SaveChanges();
                }
                return Json(new { success = true, responseText = "Successful Post" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, responseText = "Bad Post Request" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Matches(int? id)
        {
            List<Tournament> tournList = db.Tournaments.Where(x => x.EventID == id).ToList();
            List<Match> matchList = new List<Match>();
            foreach (var tourn in tournList)
            {
                List<Match> temp = db.Matches.Where(x => x.TournamentID == tourn.TournamentID).ToList();
                matchList.AddRange(temp);
            }

            List<MatchViewModel> matchVM = new List<MatchViewModel>();
            foreach (var model in matchList)
            {
                matchVM.Add(new MatchViewModel(model));
            }

            List<MatchViewModel> sortedList = matchVM.OrderBy(x => x.Status).ThenBy(y => y.Time).ToList();

            return Json(sortedList, JsonRequestBehavior.AllowGet);
        }

        public void CompetitorUpdate(int? id)
        {
            string api_key = HttpContext.GetOwinContext().Get<ApplicationUserManager>().FindById(db.Events.Find(id).OrganizerID).ApiKey;
            foreach (var i in db.Tournaments.Where(p => p.EventID == id).ToList())
            {
                if(i.IsStarted == false)
                {
                    string uri = "https://api.challonge.com/v1/tournaments/" + i.ApiId + "/participants.json?api_key=" + api_key;
                    string participantsData = SendRequest(uri);
                    var participantsObject = JToken.Parse(participantsData);
                    foreach (var p in participantsObject)
                    {
                        Boolean InDB = false;
                        var participant = (string)p["participant"]["name"];
                        foreach (var c in db.Competitors.Where(x => x.EventID == id))
                        {
                            if (c.CompetitorName == participant)
                            {
                                InDB = true;
                                break;
                            }
                        }
                        if (InDB == false)
                        {
                            Competitor newCompetitor = new Competitor
                            {
                                CompetitorName = participant,
                                EventID = id ?? default(int),
                                BusyState = null
                            };
                            db.Competitors.Add(newCompetitor);
                            db.SaveChanges();//Move to save only once?
                        }

                    }
                    uri = "https://api.challonge.com/v1/tournaments/" + i.ApiId + ".json?api_key=" + api_key;
                    string startData = SendRequest(uri);
                    var startObject = JToken.Parse(startData);
                    if (startObject["tournament"]["started_at"].ToString() != "")
                    {
                        MatchSetup(i.TournamentID, id, participantsObject);
                        i.IsStarted = true;
                        db.SaveChanges();
                    }
                }   
            }
        }

        public void MatchSetup(int? tid, int? eid, JToken participantsObject)
        {
            string api_key = HttpContext.GetOwinContext().Get<ApplicationUserManager>().FindById(db.Events.Find(eid).OrganizerID).ApiKey;
            string uri = "https://api.challonge.com/v1/tournaments/" + db.Tournaments.Find(tid).ApiId + "/matches.json?api_key=" + api_key;
            string matchData = SendRequest(uri);
            var matchObject = JToken.Parse(matchData);
            foreach (var m in matchObject)
            {
                Match newMatch = new Match
                {
                    TournamentID = tid ?? default(int),
                    Identifier = (string)m["match"]["identifier"],
                    Round = (int?)m["match"]["round"],
                    ApiID = (int)m["match"]["id"],
                    UpdatedAt = DateTime.Parse((string)m["match"]["updated_at"]),
                    Competitor1ID = null,
                    Competitor2ID = null,
                    PrereqMatch1ID = null,
                    PrereqMatch2ID = null,
                };
                if (m["match"]["started_at"].ToString() != "")
                {
                    newMatch.Time = DateTime.Parse((string)m["match"]["started_at"]);
                    Console.WriteLine(newMatch.Time);
                }
                if (m["match"]["player1_id"].ToString() != "")
                {
                    string temp = (string)participantsObject.Where(x => (int)x["participant"]["id"] == (int)m["match"]["player1_id"]).First()["participant"]["name"];
                    newMatch.Competitor1ID = db.Competitors.Where(x => x.EventID == eid).Where(x => x.CompetitorName == temp).First().CompetitorID;
                }
                if (m["match"]["player2_id"].ToString() != "")
                {
                    string temp = (string)participantsObject.Where(x => (int)x["participant"]["id"] == (int)m["match"]["player2_id"]).First()["participant"]["name"];
                    newMatch.Competitor2ID = db.Competitors.Where(x => x.EventID == eid).Where(x => x.CompetitorName == temp).First().CompetitorID;
                }
                if (m["match"]["player1_prereq_match_id"].ToString() != "")
                {
                    int prereqID = (int)m["match"]["player1_prereq_match_id"];
                    newMatch.PrereqMatch1ID = db.Matches.Where(x => x.TournamentID == tid).Where(x => x.ApiID == prereqID).First().MatchID;
                }
                if (m["match"]["player2_prereq_match_id"].ToString() != "")
                {
                    int prereqID = (int)m["match"]["player2_prereq_match_id"];
                    newMatch.PrereqMatch2ID = db.Matches.Where(x => x.TournamentID == tid).Where(x => x.ApiID == prereqID).First().MatchID;
                }
                db.Matches.Add(newMatch);
                db.SaveChanges();
            }
        }

        public JsonResult CompetitorList(int? id)
        {
            CompetitorUpdate(id);

            var j = 0;
            string compStr = "<table class=\"table table-bordered table-striped\"><tr><th>Competitors</th></tr>";
            foreach (var i in db.Competitors.Where(p => p.EventID == id).ToList().OrderBy(p => p.CompetitorName))
            {
                var state = "";
                var col = "";
                if (i.BusyState == "b")
                {
                    col = "danger";
                    state = "b";
                }
                else
                {
                    col = "success";
                    state = "a";
                }
                compStr += "<tr><td>" + "<button id=\"busyState-" + j + "\" type=\"submit\" class=\"btn btn-outline-" + col + "\" value=\"" 
                              + state + "\" onclick=\"sharedFunction(" + i.CompetitorID + ")\">" + state + "</button>" + i.CompetitorName + "</td></tr>";
            }
            compStr += "</table>";
            var data = new
            {
                compTable = compStr
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MatchList(int? id)
        {
            string matchStr = "<h4 align=\"left\">Brackets</h4>";

            foreach(var t in db.Tournaments.Where(x =>x.EventID == id).ToList()) 
            {
                var matchList = db.Matches.Where(x => x.TournamentID == t.TournamentID).ToList();
                matchStr += "<div class =\"card\" style = \"background-color:lightgrey\"> <h5 align=\"left\"><a href=\"/Events/Tournament/" + t.TournamentID + "\">" + t.TournamentName + "</a></h5><div>";
                if (matchList.Any())
                {
                    var GFinal = (int)matchList.MaxBy(x => x.Round).First().Round;
                    var LFinal = (int)matchList.MinBy(x => x.Round).First().Round;
                    foreach (var m in matchList)
                    {
                        matchStr += "<table class=\"table table-bordered\" style=\"display: inline-block; border: solid; border-color:black; width:350px\">";
                        matchStr += "<tr style=\"height:30px\"><td width=\"20%\">";
                        matchStr += m.Identifier;
                        matchStr += "</td><td width=\"55%\">";

                        if (m.Competitor1ID != null)
                            matchStr += db.Competitors.Find(m.Competitor1ID).CompetitorName;
                        else
                        {
                            if ((m.Round > 0) || ((m.Round < 0) && (db.Matches.Find(m.PrereqMatch1ID).Round < 0)))
                                matchStr += "Winner of ";
                            else
                                matchStr += "Loser of ";
                            matchStr += db.Matches.Find(m.PrereqMatch1ID).Identifier;
                        }

                        matchStr += "</td><td width=\"25%\">";


                       

                        if (m.Score1 == null)
                            matchStr += "<button id=" + m.ApiID + "\" style=\"width: 100 % \" onclick=StartMatch("+ JsonConvert.SerializeObject(m)+") >Start</button>";
                        else
                        {
                            //location.href='<%: Url.Action("Action", "Controller") %>'
                        }
                        matchStr += m.Score1;
                        matchStr += "</td></tr>";




                        matchStr += "<tr><td>";
                        if ((m.Round > 0) && (m.Round < (GFinal - 3)))
                            matchStr += "W" + m.Round;
                        else if ((m.Round < 0) && (m.Round > LFinal + 2))
                            matchStr += "L" + Math.Abs((int)m.Round);
                        else if (m.Round == GFinal)
                        {
                            if ((m.PrereqMatch1ID != null) && (db.Matches.Find(m.PrereqMatch1ID).Round == GFinal))
                                matchStr += "GFR";
                            else
                                matchStr += "GF";
                        }
                        else if (m.Round == GFinal - 1)
                            matchStr += "WF";
                        else if (m.Round == GFinal - 2)
                            matchStr += "WSF";
                        else if (m.Round == GFinal - 3)
                            matchStr += "WQF";
                        else if (m.Round == LFinal)
                            matchStr += "LF";
                        else if (m.Round == LFinal + 1)
                            matchStr += "LSF";
                        else if (m.Round == LFinal + 2)
                            matchStr += "LQF";
                        matchStr += "</td><td>";

                        if (m.Competitor2ID != null)
                            matchStr += db.Competitors.Find(m.Competitor2ID).CompetitorName;
                        else
                        {
                            if ((m.Round > 0) || ((m.Round < 0) && (db.Matches.Find(m.PrereqMatch2ID).Round < 0)))
                                matchStr += "Winner of ";
                            else
                                matchStr += "Loser of ";
                            matchStr += db.Matches.Find(m.PrereqMatch2ID).Identifier;
                        }

                        matchStr += "</td><td>";

                        if (m.Score2 == null)
                            matchStr += "<button id=sub" + m.ApiID + "\" style=\"width: 100 % \" onclick=SubmitScore(" + JsonConvert.SerializeObject(m) + ")>Submit</button>";
                        else
                            matchStr += m.Score2;
                        matchStr += "</td></tr></table>";
                        matchStr += "<div style = \"display: inline-block; width: 5px\"></div>";
                    }
                    matchStr += "</div></div></br>";
                }
            }

            var data = new
            {
                matchTable = matchStr
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public String MatchDetails(int matchApiId, int? tournamentApiId, String apiKey)    //takes a match from our db and finds its competitors apikeys
        {
            //get comp apikeys via match details  GET https://api.challonge.com/v1/tournaments/{tournament}/matches/{match_id}.{json|xml}

            Debug.WriteLine("Keys for match lookup are:" + tournamentApiId + "   " + matchApiId);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.challonge.com/v1/tournaments/" + tournamentApiId + "/matches/" + matchApiId + ".json?api_key="+apiKey);
            httpWebRequest.Method = "GET";
            Debug.WriteLine("https://api.challonge.com/v1/tournaments/" + tournamentApiId + "/matches/" + matchApiId + ".json?api_key="+apiKey);
            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Debug.WriteLine("OPPS POOPS"+httpResponse);

                    return result;

                }

            }

            catch (System.Net.WebException e)
            {
                Debug.WriteLine(e);
            }




            return "";
        }


        [HttpPost]
        public void SubmitScore()
        {
            Debug.WriteLine("Score POSTING"+ Request.Params["Competitor1ID"]);

            var matchId = Int32.Parse(Request.Params["MatchID"]);
            var matchApiId = db.Matches.Where(x => x.MatchID == matchId).First().ApiID;


            var tournamentId = Int32.Parse(Request.Params["TournamentID"]);
            var tournamentApiId = db.Tournaments.Where(x => x.TournamentID == tournamentId).First().ApiId;
            var userid = HttpContext.User.Identity.GetUserId();
            var apiKey = db.AspNetUsers.Where(x => x.Id == userid).First().ApiKey;



            //PUT Help https://api.challonge.com/v1/tournaments/{tournament}/matches/{match_id}.{json|xml}
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.challonge.com/v1/tournaments/" + tournamentApiId + "/matches/" + matchApiId + ".json");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PUT";

            var matchDetails = JObject.Parse(MatchDetails(matchApiId, tournamentApiId, apiKey));
            Debug.WriteLine("CHALLONGE MATCH DETAILS: "+matchDetails);

            JObject match = new JObject();
            match.Add("scores_csv",Request.Params["scoreCsv"]);

            int score1 = Int32.Parse(Request.Params["score1"].Substring(1, Request.Params["score1"].Length-1));
            int score2 = Int32.Parse(Request.Params["score2"].Substring(1, Request.Params["score1"].Length-1));

            //Debug.WriteLine(Request.Params["score1"] + " " + Request.Params["score2"]);



           

            if (score1 > score2)
            {
                Debug.WriteLine("player 1 is winner: " + matchDetails["match"]["player1_id"]);
                match.Add("winner_id", matchDetails["match"]["player1_id"]);

            }

            if (score1 < score2)
            {
                Debug.WriteLine("player 2 is winner: " + matchDetails["match"]["player2_id"]);

                match.Add("winner_id", matchDetails["match"]["player2_id"]);

            }

            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine(matchDetails["match"]["player1_id"]);
            Debug.WriteLine("");
            Debug.WriteLine("");
            JObject myJson = new JObject();
            myJson.Add("match_id", matchApiId);
            myJson.Add("tournament", tournamentApiId);
            myJson.Add("api_key", apiKey);
            myJson.Add("match", match);
            myJson.Add("state", "complete");

            Debug.WriteLine("NOW SENDING!!!!:   "+myJson);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {


                streamWriter.Write(myJson);
            }

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();



                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    //Debug.WriteLine(result);

                }
            }

            catch (System.Net.WebException e)
            {


            }

        }



        [HttpPost]
        public void StartMatch()
        {
            //FETCH NEEDED VALUES
            var matchId = Int32.Parse(Request.Params["MatchID"]);
            var matchApiId = db.Matches.Where(x => x.MatchID == matchId).First().ApiID;
            var tournamentId = Int32.Parse(Request.Params["TournamentID"]);
            var tournamentApiId = db.Tournaments.Where(x => x.TournamentID == tournamentId).First().ApiId;
            var userid = HttpContext.User.Identity.GetUserId();
            var apiKey = db.AspNetUsers.Where(x => x.Id ==userid ).First().ApiKey;

            //PREPARE REQUEST
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.challonge.com/v1/tournaments/" + tournamentApiId + "/matches/" + matchApiId + "/mark_as_underway.json");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";


            //BUILD REQUEST DATA
            JObject myJson= new JObject();
            myJson.Add("match_id", matchApiId);
            myJson.Add("tournament", tournamentApiId);
            myJson.Add("api_key", apiKey);

            //WRITE DATA TO REQUEST
            Debug.WriteLine(myJson);
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(myJson);
            }
            //TRY SENDING THE REQUEST
            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
                Debug.WriteLine(httpResponse);
            }

            catch (System.Net.WebException e)
            {
            }
            var eventId = db.Tournaments.Where(y => y.TournamentID == tournamentId).First().EventID;
        }

        public void MatchUpdate(int? id)
        {
            string api_key = HttpContext.GetOwinContext().Get<ApplicationUserManager>().FindById(db.Events.Find(id).OrganizerID).ApiKey;
            foreach (var t in db.Tournaments.Where(p => p.EventID == id).ToList())
            {
                if (t.IsStarted == true)
                {
                    string uriPart = "https://api.challonge.com/v1/tournaments/" + t.ApiId + "/participants.json?api_key=" + api_key;
                    string participantsData = SendRequest(uriPart);
                    var participantsObject = JToken.Parse(participantsData);

                    string uriMatch = "https://api.challonge.com/v1/tournaments/" + t.ApiId + "/matches.json?api_key=" + api_key;
                    string chalMatchData = SendRequest(uriMatch);
                    var chalMatchObject = JToken.Parse(chalMatchData);

                    var matchList = db.Matches.Where(x => x.TournamentID == t.TournamentID).ToList();

                    foreach (var m in chalMatchObject)
                    {
                        int matchID = (int)m["match"]["id"];
                        var temp = db.Matches.Where(x => x.ApiID == matchID).First();
                        var chalUpdatedTime = DateTime.Parse((string)m["match"]["updated_at"]);
                       // if (temp.UpdatedAt != chalUpdatedTime)
                        {
                            temp.UpdatedAt = chalUpdatedTime;

                            if (m["match"]["player1_id"].ToString() != "")
                            {
                                string tempPart1 = (string)participantsObject.Where(x => (int)x["participant"]["id"] == (int)m["match"]["player1_id"]).First()["participant"]["name"];
                                temp.Competitor1ID = db.Competitors.Where(x => x.EventID == id).Where(x => x.CompetitorName == tempPart1).First().CompetitorID;
                            }
                            else
                            {
                                temp.Competitor1ID = null;
                            }
                            if (m["match"]["player2_id"].ToString() != "")
                            {
                                string tempPart2 = (string)participantsObject.Where(x => (int)x["participant"]["id"] == (int)m["match"]["player2_id"]).First()["participant"]["name"];
                                temp.Competitor2ID = db.Competitors.Where(x => x.EventID == id).Where(x => x.CompetitorName == tempPart2).First().CompetitorID;
                            }
                            else
                            {
                                temp.Competitor2ID = null;
                            }
                            if (m["match"]["scores-csv"] == null)
                            {
                                temp.Score1 = null;
                                temp.Score2 = null;
                            }
                            else
                            {
                                string chalScore = (string)m["match"]["scores-csv"];
                                int firstHyph = chalScore.IndexOf('-');
                                if (firstHyph != 0)
                                {
                                    temp.Score1 = Int32.Parse(chalScore.Substring(0, chalScore.IndexOf('-')));
                                    temp.Score2 = Int32.Parse(chalScore.Substring(chalScore.IndexOf('-')+1));
                                }
                                else
                                {
                                    int secondHyph = chalScore.IndexOf('-', 1);
                                    temp.Score1 = Int32.Parse(chalScore.Substring(0, secondHyph));
                                    temp.Score2 = Int32.Parse(chalScore.Substring(secondHyph+1));
                                }
                            }
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        private string SendRequest(string uri)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Accept = "application/json";

            string jsonString = null;
            // TODO: You should handle exceptions here
            try
            {
                using (WebResponse response = request.GetResponse())
                {

                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream);
                    jsonString = reader.ReadToEnd();
                    reader.Close();
                    stream.Close();
                }
                
            }
            catch (System.Net.WebException e)
            {


            }
            return jsonString;
        }
    }
}
