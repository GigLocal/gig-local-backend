﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GigLocal.Data;
using GigLocal.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using GigLocal.Services;

namespace GigLocal.Pages.Admin.Artists
{
    public class CreateModel : PageModel
    {
        private readonly GigContext _context;
        private readonly IStorageService _storageService;

        [BindProperty]
        public ArtistCreateModel Artist { get; set; }

        public class ArtistCreateModel
        {
            [Required]
            [StringLength(50)]
            public string Name { get; set; }

            [Required]
            public string Description { get; set; }

            [Required]
            public string Genre { get; set; }

            [Required]
            public string Website { get; set; }

            [Display(Name = "Image")]
            public IFormFile FormFile { get; set; }
        }

        public CreateModel(GigContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: put into a re-useable class to use in Edit.cshtml.cs
            var newArtist = new Artist
            {
                Name = Artist.Name,
                Description = Artist.Description,
                Genre = Artist.Genre,
                Website = Artist.Website,
            };

            _context.Artists.Add(newArtist);
            await _context.SaveChangesAsync();

            if (Artist.FormFile?.Length > 0)
            {
                using var memStream = new MemoryStream();
                var fileBytes = Artist.FormFile.CopyToAsync(memStream);

                var imageUrl = await _storageService.UploadAsync("public", $"artists/{newArtist.ID}",  memStream);

                newArtist.ImageUrl = imageUrl;
                await _context.SaveChangesAsync();
            }
            //////// End re-usable

            return RedirectToPage("./Index");
        }
    }
}
