using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_api.Data;
using web_api.Models;

namespace web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesApiController : ControllerBase
    {
        private readonly WebAPIDbContext _context;

        public QuotesApiController(WebAPIDbContext context)
        {
            _context = context;
        }

        // GET: api/quotesapi?page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetQuotes([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            IQueryable<Quote> query = _context.Quotes
                .Include(q => q.TagAssignments)
                    .ThenInclude(ta => ta.Tag);

            // We also want to consider a situation where we may need all of the quotes at once,
            // hence '-1' in pagesize will be our flag
            if (pageSize != -1)
                //query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Models.Quote, Models.Tag>)query.Skip((page - 1) * pageSize).Take(pageSize);
                query = query.Skip((page - 1) * pageSize).Take(pageSize);


            var quotes = await query.ToListAsync();
            return Ok(quotes);
        }

        // GET: api/quotesapi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuote(int id)
        {
            var quote = await _context.Quotes
                .Include(q => q.TagAssignments)
                    .ThenInclude(ta => ta.Tag)
                .FirstOrDefaultAsync(q => q.QuoteId == id);

            if (quote == null)
                return NotFound();

            return Ok(quote);
        }

        // GET: api/quotesapi/top?count=10
        [HttpGet("top")]
        public async Task<IActionResult> GetTopQuotes([FromQuery] int count = 10)
        {
            var quotes = await _context.Quotes
                .OrderByDescending(q => q.Likes)
                .Take(count)
                .ToListAsync();

            return Ok(quotes);
        }
        // GET: api/quotesapi/quotes_by_tag/{tagName}
        [HttpGet("quotes_by_tag/{tagName}")]
        public async Task<IActionResult> GetQuotesByTag(string tagName)
        {
            var quotes = await _context.Quotes
                .Include(q => q.TagAssignments)
                    .ThenInclude(ta => ta.Tag)
                .Where(q => q.TagAssignments.Any(ta => ta.Tag.Name.ToLower() == tagName.ToLower()))
                .ToListAsync();

            return Ok(quotes);
        }

        // GET: api/quotesapi/tags
        [HttpGet("tags")]
        public async Task<IActionResult> GetTags()
        {
            var tags = await _context.Tags.ToListAsync();
            return Ok(tags);
        }

        // POST: api/quotesapi
        [HttpPost]
        public async Task<IActionResult> AddQuote([FromBody] Models.Quote quote)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            quote.Likes = 0;


            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuote), new { id = quote.QuoteId }, quote);
        }

        // POST: api/quotesapi/{id}/tags
        // Expects a JSON body with { "name": "tagname" }
        [HttpPost("{id}/tags")]
        public async Task<IActionResult> AddTagToQuote(int id, [FromBody] Models.Tag tag)
        {
            var quote = await _context.Quotes
                .Include(q => q.TagAssignments)
                    .ThenInclude(ta => ta.Tag)
                .FirstOrDefaultAsync(q => q.QuoteId == id);
            if (quote == null)
                return NotFound();

            // Check if the tag already exists in the database
            var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == tag.Name.ToLower());
            if (existingTag == null)
            {
                // Create the tag if it doesn't exist
                existingTag = new Models.Tag { Name = tag.Name };
                _context.Tags.Add(existingTag);
                await _context.SaveChangesAsync();
            }

            // Check if the quote already has this tag assigned
            if (quote.TagAssignments.Any(ta => ta.TagId == existingTag.TagId))
                return BadRequest("Tag already assigned to quote.");

            // Create a new tag assignment
            var tagAssignment = new Models.TagAssignment
            {
                QuoteId = quote.QuoteId,
                TagId = existingTag.TagId
            };

            _context.Add(tagAssignment);
            await _context.SaveChangesAsync();

            return Ok(quote);
        }


        // POST: api/quotesapi/{id}/like
        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeQuote(int id)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote == null)
                return NotFound();

            quote.Likes++;
            await _context.SaveChangesAsync();

            return Ok(quote);
        }

        // PUT: api/quotesapi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuote(int id, [FromBody] Models.Quote updatedQuote)
        {
            if (id != updatedQuote.QuoteId)
                return BadRequest();

            _context.Entry(updatedQuote).State = EntityState.Modified;

            if (!_context.Quotes.Any(q => q.QuoteId == id))
                return NotFound();

            await _context.SaveChangesAsync();

            return NoContent();
        }



        // PATCH: api/quotesapi/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchQuote(int id, [FromBody] Dictionary<string, object> updates)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote == null)
                return NotFound();

            foreach (var update in updates)
            {
                switch (update.Key.ToLower())
                {
                    case "content":
                        quote.Content = update.Value.ToString();
                        break;
                    case "author":
                        quote.Author = update.Value.ToString();
                        break;
                    default:
                        return BadRequest("Invalid field: " + update.Key);
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }



        // DELETE: api/quotesapi/{id}/tags/{tagId}
        // Removes a tag from a quote.
        [HttpDelete("{id}/tags/{tagId}")]
        public async Task<IActionResult> RemoveTagFromQuote(int id, int tagId)
        {
            var quote = await _context.Quotes
                .Include(q => q.TagAssignments)
                .FirstOrDefaultAsync(q => q.QuoteId == id);
            if (quote == null)
                return NotFound();

            var tagAssignment = quote.TagAssignments.FirstOrDefault(ta => ta.TagId == tagId);
            if (tagAssignment == null)
                return NotFound("Tag assignment not found.");

            _context.Remove(tagAssignment);
            await _context.SaveChangesAsync();

            return Ok(quote);
        }

    }
}
