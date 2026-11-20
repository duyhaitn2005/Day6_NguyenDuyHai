using Day6_NDH.Data;
using Day6_NDH.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Day6_NDH.Controllers;

public class MoviesController : Controller
{
    private readonly ApplicationDbContext _context;

    public MoviesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var movies = await _context.Movies
            .Include(movie => movie.Genre)
            .OrderBy(movie => movie.Title)
            .ToListAsync();
        return View(movies);
    }

    public async Task<IActionResult> Create()
    {
        await LoadGenresAsync();
        return View(new Movie { ReleaseDate = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,ReleaseDate,Duration,GenreId")] Movie movie)
    {
        await ValidateGenreAsync(movie.GenreId);
        if (!ModelState.IsValid)
        {
            await LoadGenresAsync(movie.GenreId);
            return View(movie);
        }

        _context.Add(movie);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var movie = await _context.Movies.FindAsync(id.Value);
        if (movie is null)
        {
            return NotFound();
        }

        await LoadGenresAsync(movie.GenreId);
        return View(movie);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Duration,GenreId")] Movie movie)
    {
        if (id != movie.Id)
        {
            return NotFound();
        }

        await ValidateGenreAsync(movie.GenreId);
        if (!ModelState.IsValid)
        {
            await LoadGenresAsync(movie.GenreId);
            return View(movie);
        }

        try
        {
            _context.Entry(movie).Property(item => item.Title).IsModified = true;
            _context.Entry(movie).Property(item => item.ReleaseDate).IsModified = true;
            _context.Entry(movie).Property(item => item.Duration).IsModified = true;
            _context.Entry(movie).Property(item => item.GenreId).IsModified = true;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Movies.AnyAsync(item => item.Id == movie.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var movie = await _context.Movies
            .Include(item => item.Genre)
            .FirstOrDefaultAsync(item => item.Id == id.Value);
        return movie is null ? NotFound() : View(movie);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadGenresAsync(int? selectedGenreId = null)
    {
        var genres = await _context.Genres.OrderBy(genre => genre.Name).ToListAsync();
        ViewData["GenreId"] = new SelectList(genres, "Id", "Name", selectedGenreId);
    }

    private async Task ValidateGenreAsync(int genreId)
    {
        if (!await _context.Genres.AnyAsync(genre => genre.Id == genreId))
        {
            ModelState.AddModelError(nameof(Movie.GenreId), "Vui lòng chọn thể loại hợp lệ.");
        }
    }
}
