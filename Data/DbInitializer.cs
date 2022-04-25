using Newtonsoft.Json.Linq;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Project.Data
{
    public class DbInitializer
    {
        static HttpClient httpClient;

        static string BASE_URL = "https://imdb-api.com/en/API/";
        static string API_KEY = "k_o1j7e1n9";

        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            getGenres(context);
            getMovies(context);
        }

        public static void getGenres(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            Genre[] genres = new Genre[]
            {
                new Genre{GenreID="ACT",GenreName="Action"},
                new Genre{GenreID="ADV",GenreName="Adventure"},
                new Genre{GenreID="ANI",GenreName="Animation"},
                new Genre{GenreID="BIO",GenreName="Biography"},
                new Genre{GenreID="COM",GenreName="Comedy"},
                new Genre{GenreID="CRI",GenreName="Crime"},
                new Genre{GenreID="DOC",GenreName="Documentary"},
                new Genre{GenreID="DRA",GenreName="Drama"},
                new Genre{GenreID="FAM",GenreName="Family"},
                new Genre{GenreID="FAN",GenreName="Fantasy"},
                new Genre{GenreID="FN",GenreName="Film Noir"},
                new Genre{GenreID="HIS",GenreName="History"},
                new Genre{GenreID="HOR",GenreName="Horror"},
                new Genre{GenreID="MUS",GenreName="Music"},
                new Genre{GenreID="MSS",GenreName="Musical"},
                new Genre{GenreID="MYS",GenreName="Mystery"},
                new Genre{GenreID="ROM",GenreName="Romance"},
                new Genre{GenreID="SciFi",GenreName="Sci-Fi"},
                new Genre{GenreID="SF",GenreName="Short"},
                new Genre{GenreID="SP",GenreName="Sport"},
                new Genre{GenreID="SH",GenreName="Superhero"},
                new Genre{GenreID="THR",GenreName="Thriller"},
                new Genre{GenreID="WAR",GenreName="War"},
                new Genre{GenreID="WES",GenreName="Western"},
            };
            try
            {
                if (!context.genres.Any())
                {
                    foreach (Genre g in genres)
                    {
                        context.genres.Add(g);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void getMovies(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            if (context.movies.Any())
            {
                return;
            }

            string uri = BASE_URL + "Top250Movies/" + API_KEY ;
            string responsebody = "";
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            //httpClient.DefaultRequestHeaders.Add("X-Api-Key", API_KEY);
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.BaseAddress = new Uri(uri);

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(uri).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    responsebody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }

                if (!responsebody.Equals(""))
                {
                    JObject parsedResponse = JObject.Parse(responsebody);
                    JArray movieslist = (JArray)parsedResponse["items"];

                    foreach(JObject mov in movieslist)
                    {
                        string mId = mov["id"].ToString().Replace("{", "").Replace("}", "");
                        string newurl = BASE_URL + "Wikipedia/" + API_KEY + "/" + mId;
                        string newmovie = "";
                        HttpResponseMessage response1 = httpClient.GetAsync(newurl).GetAwaiter().GetResult();
                        if (response1.IsSuccessStatusCode)
                        {
                            newmovie = response1.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        }
                        string aId = mov["id"].ToString().Replace("{", "").Replace("}", "");
                        string newurl2 = BASE_URL + "Title/" + API_KEY + "/" + aId;
                        string newactor = "";
                        HttpResponseMessage response2 = httpClient.GetAsync(newurl2).GetAwaiter().GetResult();
                        if (response2.IsSuccessStatusCode)
                        {
                            newactor = response2.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        }
                        if (!newmovie.Equals("") && !newactor.Equals(""))
                        {
                            JObject parsedResponse1 = JObject.Parse(newmovie);
                            JObject parsedResponse2 = JObject.Parse(newactor);
                            
                            Movie m = new Movie
                            {
                                ImdbId = mov["id"].ToString().Replace("{", "").Replace("}", ""),
                                Title = mov["title"].ToString().Replace("{", "").Replace("}", ""),
                                Year = int.Parse(mov["year"].ToString().Replace("{", "").Replace("}", "")),
                                Director = mov["crew"].ToString().Substring(0, (mov["crew"].ToString().IndexOf('('))-1),
                                URL = parsedResponse1["url"].ToString().Replace("{", "").Replace("}", ""),
                                imageURL = mov["image"].ToString().Replace("{", "").Replace("}", ""),
                                Description = parsedResponse2["plot"].ToString().Replace("{", "").Replace("}", ""),
                                ImdbRating = float.Parse(mov["imDbRating"].ToString().Replace("{", "").Replace("}", "")),
                            };
                            context.movies.Add(m);

                            JArray actorslist = (JArray)parsedResponse2["actorList"];
                            if (actorslist.Count != 0)
                            {
                                foreach (JObject actor in actorslist)
                                {
                                    Actor a = context.actors.Where(c => c.ActorID == (string)actor["id"]).FirstOrDefault();
                                    if (a == null)
                                    {
                                        a = new Actor
                                        {
                                            ActorID = (string)actor["id"],
                                            ActorName = (string)actor["name"]
                                        };
                                        context.actors.Add(a);
                                        context.SaveChanges();
                                    }

                                    Movie_Actor ma = new Movie_Actor
                                    {
                                        Actor = a,
                                        Movie = m
                                    };
                                    context.movie_actor.Add(ma);
                                }
                            }

                            string[] genres = ((string)parsedResponse2["genres"]).Split(",");
                            foreach (string g in genres)
                            {
                                Genre mg = context.genres.Where(c => c.GenreName == g).FirstOrDefault();
                                if (mg != null)
                                {
                                    Movie_Genre sp = new Movie_Genre()
                                    {
                                        Movie = m,
                                        Genre = mg
                                    };
                                    context.movie_genre.Add(sp);
                                    context.SaveChanges();
                                }
                            }

                        }
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
