using BSDay15.Data;
using BSDay15.IService;
using BSDay15.Models;
using BSDay15.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BSDay15.Controllers
{
    [Authorize]
    public class AuthorController : Controller
    {
        private readonly BookShopDbContext bookShopDbContext;
        private readonly IAuthorService authorService;

        // Dependencies injection
        public AuthorController(BookShopDbContext context, IAuthorService author)
        {
            bookShopDbContext = context;
            authorService = author;
        }

        // GET: Index
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "List of all the authors";

            if (bookShopDbContext.Authors == null)
            {
                return NotFound();
            }
            var authors = authorService.GetAuthorsAsync();
            var authorsViewModel = authorService.AuthorToAuthorViewModelEnumerable(await authors);
            return View(authorsViewModel);
        }

        // GET: Author/Details/id
        public async Task<IActionResult> Details(string id)
        {
            ViewBag.Title = "Author";

            if (id == null || bookShopDbContext.Authors == null)
            {
                return NotFound();
            }

            var author = authorService.FindByIdAsync(id);

            if (author == null)
            {
                return NotFound();
            }

            var authorViewModel = authorService.AuthorToAuthorViewModel(await author);

            return View(authorViewModel);
        }

        // GET: Author/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Author/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuthorCreationViewModel authorCreationViewModel)
        {

            if (authorCreationViewModel is null)
            {
                throw new ArgumentNullException(nameof(authorCreationViewModel));
            }
            if (ModelState.IsValid)
            {
                var author = new Author()
                {
                    AuthorName = authorCreationViewModel.AuthorName,
                    AuthorEmail = authorCreationViewModel.AuthorEmail,
                    Description = authorCreationViewModel.Description,
                    PictureFormat = authorCreationViewModel.AuthorPhoto.ContentType
                };

                var memoryStream = new MemoryStream();
                authorCreationViewModel.AuthorPhoto.CopyTo(memoryStream);
                author.AuthorPhoto = memoryStream.ToArray();

                bookShopDbContext.Add(author);
                await bookShopDbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(authorCreationViewModel);
        }

        // GET: Author/Edit/id
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || bookShopDbContext.Authors == null)
            {
                return NotFound();
            }

            var author = await bookShopDbContext.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            var authorEditViewModel = new AuthorEditViewModel()
            {
                AuthorId = author.AuthorId,
                AuthorEmail = author.AuthorEmail,
                AuthorName = author.AuthorName,
                Description = author.Description,
            };

            // from byte array to formFile
            var stream = new MemoryStream(author.AuthorPhoto);
            IFormFile file = new FormFile(stream, 0, author.AuthorPhoto.Length, "name", "filename");
            authorEditViewModel.AuthorPhoto = file;

            return View(authorEditViewModel);
        }

        // POST: Author/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AuthorEditViewModel authorEditViewModel)
        {


            if (authorEditViewModel == null || id != authorEditViewModel.AuthorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var author = await bookShopDbContext.Authors.FindAsync(id);

                    if (author == null)
                    {
                        return NotFound();
                    }

                    // updating the author object 
                    author.AuthorName = authorEditViewModel.AuthorName;
                    author.AuthorEmail = authorEditViewModel.AuthorEmail;
                    author.Description = authorEditViewModel.Description;
                    author.PictureFormat = authorEditViewModel.AuthorPhoto.ContentType;

                    // from iformfile to byte array
                    var memoryStream = new MemoryStream();
                    authorEditViewModel.AuthorPhoto.CopyTo(memoryStream);
                    author.AuthorPhoto = memoryStream.ToArray();


                    bookShopDbContext.Update(author);
                    await bookShopDbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(authorEditViewModel.AuthorId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(authorEditViewModel);
        }


        // GET: Author/Delete/id
        public async Task<IActionResult> Delete(string id)
        {
            ViewBag.Title = "Deletion of Author";

            if (id == null || bookShopDbContext.Authors == null)
            {
                return NotFound();
            }

            var author = await authorService.FindByIdAsync(id); // author with books
            if (author == null)
            {
                return NotFound();
            }

            var authorViewModel = authorService.AuthorToAuthorViewModel(author);

            return View(authorViewModel);
        }

        // POST: Author/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (bookShopDbContext.Authors == null)
            {
                return Problem("Entity set 'BookShopDbContex.Authors'  is null.");
            }
            var author = await bookShopDbContext.Authors.FindAsync(id);
            if (author != null)
            {
                await authorService.DeleteAsync(author);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(string id)
        {
            return (bookShopDbContext.Authors?.Any(e => e.AuthorId == id)).GetValueOrDefault();
        }
    }   
}
