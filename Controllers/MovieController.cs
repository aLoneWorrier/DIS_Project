using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        public MovieController(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public IActionResult Model()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (Genre g in dbContext.genres)
            {
                dict.Add(g.GenreID, g.GenreName);
            }
            ViewBag.adict = dict;
            return View();
        }

        [HttpPost]
        public JsonResult Model(string id)
        {
            List<object> chartTable = new List<object>();
            List<float> ratingList = dbContext.movies.Select(m => m.ImdbRating).Distinct().ToList();
            List<int> mcount = new List<int>();
            string genreName = dbContext.genres.Where(g => g.GenreID == id).Select(g => g.GenreName).FirstOrDefault();
            foreach (float r in ratingList)
            {
                int movieCount = 0;
                if (id == "All")
                {
                    movieCount = dbContext.movie_genre
                    .Where(mg => mg.Movie.ImdbRating == r)
                    .Select(mg => mg.Movie)
                    .Count();
                }
                else
                {
                    movieCount = dbContext.movie_genre
                    .Where(mg => mg.Movie.ImdbRating == r)
                    .Select(mg => mg.Genre)
                    .Where(mg => mg.GenreName == genreName)
                    .Count();
                }
                mcount.Add(movieCount);
            }
            chartTable.Add(ratingList);
            chartTable.Add(mcount);
            chartTable.Add(genreName);
            return Json(chartTable);
        }

        public async Task<IActionResult> Search(string movieName, string genre, string directorName)
        {
            movieName = (movieName == null) ? "" : movieName;
            genre = (genre == null) ? "" : genre;
            directorName = (directorName == null) ? "" : directorName;


            List<Movie> movieList = new List<Movie>();
            if (movieName == "" && genre == "" && directorName == "")
            {
                movieList = await dbContext.movies.Include(m => m.Genres).ToListAsync();
            }
            else
            {
                movieList = await dbContext.movies
                            .Include(m => m.Genres)
                            .Include(m => m.Actors)
                            .Where(m => m.Genres.Any(g => g.Genre.GenreName.Contains(genre)))
                            .Where(m => m.Title.Contains(movieName))
                            .Where(m => m.Director.Contains(directorName))
                            .ToListAsync();
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (Genre g in dbContext.genres)
            {
                dict.Add(g.GenreID,g.GenreName);
            }
            ViewBag.genreDict = dict;
            return View(movieList);
        }

        public async Task<IActionResult> Create()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (Genre g in dbContext.genres)
            {
                dict.Add(g.GenreID, g.GenreName);
            }
            List<string> actorName = await dbContext.actors.Select(a => a.ActorName).ToListAsync();
            ViewBag.actorNames = actorName;
            ViewBag.genreDict = dict;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ImdbId,Title,Year,Director,URL,imageURL,Description,ImdbRating,GenreNames,ActorNames")] CreateMovie newm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Movie newMovie = new Movie()
                    {
                        ImdbId = newm.ImdbId,
                        Title = newm.Title,
                        Year = newm.Year,
                        Director = newm.Director,
                        URL = newm.URL,
                        imageURL = newm.imageURL,
                        Description = newm.Description,
                        ImdbRating = newm.ImdbRating
                    };
                    dbContext.movies.Add(newMovie);
                    if (newm.GenreNames != null)
                    {
                        foreach (string genre in newm.GenreNames)
                        {
                            Genre gen = dbContext.genres.Where(g => g.GenreID == genre).FirstOrDefault();
                            dbContext.movie_genre.Add(new Movie_Genre()
                            {
                                Movie = newMovie,
                                Genre = gen
                            });
                        }
                    }
                    if (newm.ActorNames != null)
                    {
                        foreach (string str in newm.ActorNames)
                        {
                            Actor a = dbContext.actors.Where(a => a.ActorName == str).FirstOrDefault();
                            dbContext.movie_actor.Add(new Movie_Actor()
                            {
                                Movie = newMovie,
                                Actor = a
                            });
                        }
                    }
                    await dbContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Thanks), new { message = "Thanks for helping us to grow our database!" });
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (Genre g in dbContext.genres)
            {
                dict.Add(g.GenreID, g.GenreName);
            }
            List<string> actorName = await dbContext.actors.Select(a => a.ActorName).ToListAsync();
            ViewBag.actorNames = actorName;
            ViewBag.genreDict = dict;
            return View(newm);
        }

        public IActionResult Thanks(string message)
        {
            ViewBag.dispMsg = message;
            return View();
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var m = await dbContext.movies
                .Include(m => m.Genres)
                    .ThenInclude(g => g.Genre)
                .Include(m => m.Actors)
                    .ThenInclude(a => a.Actor)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ImdbId.Equals(id));

            if (m == null)
            {
                return NotFound();
            }
            ViewData["Title"] = "Details: " + m.Title;
            return View(m);
        }

        public async Task<IActionResult> Edit(string id)
        {
            Movie movieToUpdate = dbContext.movies.Where(m => m.ImdbId == id).FirstOrDefault();
            List<string> mg = dbContext.movie_genre.Where(g => g.Movie == movieToUpdate).Select(g => g.Genre.GenreName).ToList();
            List<string> ma = dbContext.movie_actor.Where(a => a.Movie == movieToUpdate).Select(a => a.Actor.ActorID).ToList();

            CreateMovie cm_edit = new CreateMovie()
            {
                ImdbId = movieToUpdate.ImdbId,
                Title = movieToUpdate.Title,
                Year = movieToUpdate.Year,
                Director = movieToUpdate.Director,
                URL = movieToUpdate.URL,
                imageURL = movieToUpdate.imageURL,
                Description = movieToUpdate.Description,
                ImdbRating = movieToUpdate.ImdbRating,
                GenreNames = mg,
                ActorNames = ma
            };

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (Genre g in dbContext.genres)
            {
                dict.Add(g.GenreID, g.GenreName);
            }
            List<string> actorName = await dbContext.actors.Select(a => a.ActorName).ToListAsync();
            ViewBag.actorNames = actorName;
            ViewBag.genreDict = dict;

            return View(cm_edit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ImdbId,Title,Year,Director,URL,imageURL,Description,ImdbRating,GenreNames,ActorNames")] CreateMovie modifiedm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Movie mToBeUpdated = dbContext.movies
                        .Include(m => m.Genres)
                        .Include(m => m.Actors)
                        .Where(m => m.ImdbId == id)
                        .FirstOrDefault();

                    mToBeUpdated.ImdbId = modifiedm.ImdbId;
                    mToBeUpdated.Title = modifiedm.Title;
                    mToBeUpdated.Year = modifiedm.Year;
                    mToBeUpdated.Director = modifiedm.Director;
                    mToBeUpdated.URL = modifiedm.URL;
                    mToBeUpdated.imageURL = modifiedm.imageURL;
                    mToBeUpdated.Description = modifiedm.Description;
                    mToBeUpdated.ImdbRating = modifiedm.ImdbRating;

                    mToBeUpdated.Genres.Clear();
                    if (modifiedm.GenreNames != null)
                    {
                        foreach (string genre in modifiedm.GenreNames)
                        {
                            Genre gen = dbContext.genres.Where(g => g.GenreID == genre).FirstOrDefault();
                            dbContext.movie_genre.Add(new Movie_Genre()
                            {
                                Movie = mToBeUpdated,
                                Genre = gen
                            });
                        }
                    }

                    mToBeUpdated.Actors.Clear();
                    if (modifiedm.ActorNames != null)
                    {
                        foreach (string str in modifiedm.ActorNames)
                        {
                            Actor a = dbContext.actors.Where(a => a.ActorName == str).FirstOrDefault();
                            dbContext.movie_actor.Add(new Movie_Actor()
                            {
                                Movie = mToBeUpdated,
                                Actor = a
                            });
                        }
                    }

                    dbContext.Update(mToBeUpdated);
                    await dbContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Thanks), new { message = "Thanks for adding a movie and keeping our database up-to-date!" });
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (Genre g in dbContext.genres)
            {
                dict.Add(g.GenreID, g.GenreName);
            }
            List<string> actorName = await dbContext.actors.Select(a => a.ActorName).ToListAsync();
            ViewBag.actorNames = actorName;
            ViewBag.genreDict = dict;
            return View(modifiedm);
        }

        public async Task<IActionResult> Delete(string id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            Movie m = await dbContext.movies
                .Include(m => m.Genres)
                .Include(m => m.Actors)
                .Where(m => m.ImdbId == id)
                .FirstOrDefaultAsync();

            if (m == null)
            {
                return NotFound();
            }
            ViewData["Title"] = "Delete: " + m.MovieID;
            if (m == null)
            {
                return RedirectToAction(nameof(Search));
            }

            try
            {
                dbContext.movies.Remove(m);
                await dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Thanks), new { message = "Thanks! The movie has been deleted." });
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

    }
}
