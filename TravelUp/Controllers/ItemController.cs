using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelUp.Application.Models;
using TravelUp.DataAccess;
using TravelUp.Domain.Entities;
using TravelUp.Models;

namespace TravelUp.Application.Controllers;

[Route("Item")]
public class ItemController : Controller
{
    private readonly AppDbContext dbContext;

    public ItemController(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await dbContext.Items
            .AsNoTracking()
            .Select(x => new ItemVM
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
            })
            .ToListAsync();

        return View(items);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] CreateItemVM model)
    {
        if (model == null)
        {
            return Json(new JsonResponse { Success = false, Errors = new List<string> { "Model is null" } });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new JsonResponse { Success = false, Errors = errors });
        }

        var item = new Item
        {
            Name = model.Name,
            Description = model.Description,
        };

        dbContext.Add(item);
        await dbContext.SaveChangesAsync();

        return Json(new JsonResponse
        {
            Success = true,
            Item = new ItemVM
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description
            }
        });
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await dbContext.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
        {
            return NotFound();
        }

        var model = new CreateItemVM
        {
            Name = item.Name,
            Description = item.Description
        };

        return View(model);
    }

    [HttpPost("Edit/{id}")]
    public async Task<IActionResult> Edit(int id, [FromBody] CreateItemVM model)
    {
        if (id <= 0 || model == null)
        {
            return Json(new JsonResponse { Success = false, Errors = new List<string> { "Invalid request" } });
        }

        var item = await dbContext.Items.FindAsync(id);
        if (item == null)
        {
            return Json(new JsonResponse { Success = false, Errors = new List<string> { "Item not found" } });
        }

        item.Name = model.Name;
        item.Description = model.Description;

        dbContext.Update(item);
        await dbContext.SaveChangesAsync();

        return Json(new JsonResponse
        {
            Success = true,
            Item = new ItemVM
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description
            }
        });
    }

    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await dbContext.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
        {
            return NotFound();
        }

        var model = new ItemVM
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description
        };

        return View(model);
    }

    [HttpPost("DeleteConfirmed/{id}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await dbContext.Items.FindAsync(id);

        if (item == null)
        {
            return Json(new JsonResponse { Success = false, Errors = new List<string> { "Item not found" } });
        }

        item.IsDeleted = true; // Mark as deleted instead of removing
        dbContext.Update(item);
        await dbContext.SaveChangesAsync();

        return Json(new JsonResponse { Success = true });
    }
}
