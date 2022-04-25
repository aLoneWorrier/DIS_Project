using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class Movie
    {
        [Key]
        public int MovieID { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Director { get; set; }
        public string URL { get; set; }
        public string imageURL { get; set; }
        public string Description { get; set; }
        public float ImdbRating { get; set; }
        public ICollection<Movie_Genre> Genres { get; set; }
        public ICollection<Movie_Actor> Actors { get; set; }
    }

    public class Genre
    {
        [Key]
        public string GenreID { get; set; }
        public string GenreName { get; set; }
        public ICollection<Movie_Genre> Movies { get; set; }
    }

    public class Movie_Genre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public Movie Movie { get; set; }
        public Genre Genre { get; set; }
    }

    public class Actor
    {
        [Key]
        public int ID { get; set; }
        public string ActorID { get; set; }
        public string ActorName { get; set; }
        public ICollection<Movie_Actor> Movies { get; set; }
    }

    public class Movie_Actor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public Movie Movie { get; set; }
        public Actor Actor { get; set; }
    }

    // This is a View Model
    public class CreateMovie
    {
        [Key]
        public int MovieID { get; set; }
        [Required]
        public string ImdbId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public string Director { get; set; }
        [Required]
        [Url]
        public string URL { get; set; }
        [Required]
        [Url]
        public string imageURL { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public float ImdbRating { get; set; }
        [Required]
        public ICollection<string> GenreNames { get; set; }
        [Required]
        public ICollection<string> ActorNames { get; set; }
    }
}
