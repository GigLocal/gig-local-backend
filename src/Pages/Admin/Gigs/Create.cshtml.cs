﻿using GigLocal.Data;
using GigLocal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GigLocal.Pages.Admin.Gigs
{
    public class CreateModel : PageModel
    {
        private readonly GigContext _context;
        private readonly ILogger<CreateModel> _logger;
        public IEnumerable<SelectListItem> Artists { get; set; }
        public IEnumerable<SelectListItem> Venues { get; set; }

        [BindProperty]
        public GigCreateModel Gig { get; set; }

        public class GigCreateModel
        {
            [Required]
            [Display(Name = "Artist")]
            public string ArtistID { get; set; }

            [Required]
            [Display(Name = "Venue")]
            public string VenueID { get; set; }

            [Required]
            [FutureDate(ErrorMessage = "The date must be in the future.")]
            [Display(Name = "Date and time")]
            public DateTime Date { get; set; }

            [Display(Name = "Ticket price")]
            public Decimal TicketPrice { get; set; }

            [Display(Name = "Ticket website")]
            public string TicketWebsite { get; set; }
        }

        public CreateModel(GigContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await PopulateSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                return Page();
            }

            var foundArtist = await _context.Artists
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(a => a.ID == int.Parse(Gig.ArtistID));
            if (foundArtist == null)
            {
                _logger.LogWarning("Artist {Artist} not found", Gig.ArtistID);
                await PopulateSelectListsAsync();
                return Page();
            }
            var foundVenue = await _context.Venues
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync(v => v.ID == int.Parse(Gig.VenueID));
            if (foundVenue == null)
            {
                _logger.LogWarning("Venue {Venue} not found", Gig.VenueID);
                await PopulateSelectListsAsync();
                return Page();
            }

            try
            {
                var newGig = new Gig
                {
                    ArtistID = foundArtist.ID,
                    VenueID = foundVenue.ID,
                    Date = Gig.Date,
                    TicketPrice = Gig.TicketPrice,
                    TicketWebsite = Gig.TicketWebsite
                };
                _context.Gigs.Add(newGig);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex.Message);
            }

            await PopulateSelectListsAsync();
            return Page();
        }

        public async Task PopulateSelectListsAsync()
        {
            Artists = (await _context.Artists.Select(a => new {Name = a.Name, ID = a.ID})
                                             .OrderBy(a => a.Name)
                                             .ToListAsync())
                                             .Select(a => new SelectListItem(a.Name, a.ID.ToString()));

            Venues = (await _context.Venues.Select(v => new {Name = v.Name, ID = v.ID})
                                           .OrderBy(v => v.Name)
                                           .ToListAsync())
                                           .Select(v => new SelectListItem(v.Name, v.ID.ToString()));
        }
    }
}
