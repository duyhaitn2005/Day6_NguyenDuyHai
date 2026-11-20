using Day6_NDH.Data;
using Day6_NDH.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Day6_NDH.Controllers;

public class GenresController : Controller
{
    private readonly ApplicationDbContext _context;

    public GenresController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Genres
            .Include(genre => genre.Movies)
            .OrderBy(genre => genre.Name)
            .ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name")] Genre genre)
    {
        if (await _context.Genres.AnyAsync(item => item.Name == genre.Name))
        {
            ModelState.AddModelError(nameof(genre.Name), "Tên thể loại này đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            return View(genre);
        }

        _context.Add(genre);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var genre = await _context.Genres.FindAsync(id.Value);
        return genre is null ? NotFound() : View(genre);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Genre genre)
    {
        if (id != genre.Id)
        {
            return NotFound();
        }

        if (await _context.Genres.AnyAsync(item => item.Name == genre.Name && item.Id != genre.Id))
        {
            ModelState.AddModelError(nameof(genre.Name), "Tên thể loại này đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            return View(genre);
        }

        try
        {
            _context.Update(genre);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Genres.AnyAsync(item => item.Id == genre.Id))
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

        var genre = await _context.Genres
            .Include(item => item.Movies)
            .FirstOrDefaultAsync(item => item.Id == id.Value);

        return genre is null ? NotFound() : View(genre);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var genre = await _context.Genres
            .Include(item => item.Movies)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (genre is null)
        {
            return NotFound();
        }

        if (genre.Movies.Count > 0)
        {
            TempData["Error"] = "Không thể xóa thể loại đang có phim.";
            return RedirectToAction(nameof(Index));
        }

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
