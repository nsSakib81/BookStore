using BSDay15.Data;
using BSDay15.Models;
using BSDay15.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BSDay15.Controllers
{

        [Authorize]    
        public class BookController : Controller
        {
            private readonly BookShopDbContext bookShopDbContext;

            public BookController(BookShopDbContext contex)
            {
                bookShopDbContext = contex;
            }


            // GET: Books
            public async Task<IActionResult> Index()
            {
                ViewBag.Title = "List of the books";
                var booksWithAuthors = await bookShopDbContext.Books.Include(b => b.Author).ToListAsync();
                if (booksWithAuthors == null)
                {
                    return NotFound();
                }

                var booksWithAuthorsViewModel = new List<BookIndexPageViewModel>();
                foreach (var book in booksWithAuthors)
                {
                    var bookWithAuthorViewModel = new BookIndexPageViewModel()
                    {
                        BookId = book.BookId,
                        Author = book.Author,
                        Title = book.Title,
                        PictureFormat = book.PictureFormat,
                        Description = book.Description,
                        Genre = book.Genre,
                        ISBN = book.ISBN,
                        publicationDate = book.publicationDate,
                        Language = book.Language,
                        CoverPhoto = Convert.ToBase64String(book.CoverPhoto),
                        AuthorId = book.AuthorId,
                        StockAmount = book.StockAmount
                        
                    };
                    booksWithAuthorsViewModel.Add(bookWithAuthorViewModel);
                }

                return View(booksWithAuthorsViewModel);
            }

        // GET: Book/Details/id
        
            public IActionResult borrowDetails(string id)
        {
            var book=bookShopDbContext.Books.Find(id);
            return View(book);
        }
        
            public async Task<IActionResult> Details(string id)
            {
                ViewBag.Title = "Book Details";
                if (id == null || bookShopDbContext.Books == null)
                {
                    return NotFound();
                }
                

                var book = await bookShopDbContext.Books
             
                    .Include(b => b.Author)
                    
                    .FirstOrDefaultAsync(m => m.BookId == id);
                if (book == null)
                {
                    return NotFound();
                }

                var bookDetailsViewModel = new BookDetailsPageViewModel()
                {
                    BookId = book.BookId,
                    Author = book.Author,
                    ISBN = book.ISBN,
                    PictureFormat = book.PictureFormat,
                    AuthorId = book.AuthorId,
                    Genre = book.Genre,
                    Title = book.Title,
                    publicationDate = book.publicationDate,
                    Language = book.Language,
                    Description = book.Description,
                    StockAmount= book.StockAmount,
                    CoverPhoto = Convert.ToBase64String(book.CoverPhoto)
                    
                };
                var selectedBook=await bookShopDbContext.Books.FindAsync(id);
                if(selectedBook !=null && selectedBook.StockAmount > 0)
                {
                    selectedBook.StockAmount--;
                    await bookShopDbContext.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Book borrowed successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Sorry, no books available for borrowing.";
                }



                return View(bookDetailsViewModel);
            }

            // GET: Book/Create
            public async Task<IActionResult> Create()
            {
                ViewBag.Title = "Add the Book Details";
                var authors = await bookShopDbContext.Authors.ToListAsync();
                ViewBag.Authors = authors;
                return View();
            }

            // POST: Book/Create
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(BookCreationViewModel bookCreationViewModel)
            {
                if (bookCreationViewModel == null)
                {
                    return NotFound();
                }
                if (ModelState.IsValid)
                {
                    // convert viewModel object to book model object
                    var book = new Book()
                    {
                        Title = bookCreationViewModel.Title,
                        Genre = bookCreationViewModel.Genre,
                        Description = bookCreationViewModel.Description,
                        ISBN = bookCreationViewModel.ISBN,
                        Language = bookCreationViewModel.Language,
                        publicationDate = bookCreationViewModel.publicationDate,
                        StockAmount= bookCreationViewModel.StockAmount,
                        AuthorId = bookCreationViewModel.AuthorId,
                        PictureFormat = bookCreationViewModel.CoverPhoto.ContentType
                    };
                    // converting the coverPhoto from FormFile to byte array
                    var memoryStream = new MemoryStream();
                    bookCreationViewModel.CoverPhoto.CopyTo(memoryStream);
                    book.CoverPhoto = memoryStream.ToArray();

                    // getting the author of the book 
                    var authorOfTheBook = await bookShopDbContext.Authors.FindAsync(bookCreationViewModel.AuthorId);
                    if (authorOfTheBook == null)
                    {
                        return Problem("Author of the book not found in the 'BookShopDbContext.Author' entity");
                    }
                    book.Author = authorOfTheBook;

                    // adding to the context
                    bookShopDbContext.Add(book);
                    await bookShopDbContext.SaveChangesAsync(); // adding to the database
                    return RedirectToAction(nameof(Index));
                }
                // if Model state not valid 
                var authors = await bookShopDbContext.Authors.ToListAsync();
                ViewBag.Authors = authors;
                return View(bookCreationViewModel); // resending the bookCreationViewModel so the user might not reenter all the fields
            }

            // GET: Book/Edit/id
            public async Task<IActionResult> Edit(string id)
            {
                ViewBag.Title = "Updation of Book Details";
                if (id == null || bookShopDbContext.Books == null)
                {
                    return NotFound();
                }

                var book = await bookShopDbContext.Books.FindAsync(id);
                if (book == null)
                {
                    return NotFound();
                }
                var authors = bookShopDbContext.Authors.ToList();
                ViewBag.Authors = authors;
                // from Book model object to BookEditViewModel object
                var bookEditViewModel = new BookEditViewModel()
                {
                    AuthorId = book.AuthorId,
                    BookId = book.BookId,
                    Genre = book.Genre,
                    ISBN = book.ISBN,
                    Language = book.Language,
                    Title = book.Title,
                    StockAmount = book.StockAmount,
                    Description = book.Description,
                    publicationDate = book.publicationDate
                    
                };

                var stream = new MemoryStream(book.CoverPhoto);
                IFormFile formFile = new FormFile(stream, 0, book.CoverPhoto.Length, "name", "fileName");
                bookEditViewModel.CoverPhoto = formFile;

                return View(bookEditViewModel);
            }

            // POST: Books/Edit/id
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(string id, BookEditViewModel bookEditViewModel)
            {
                if (id != bookEditViewModel.BookId) // ensuring security 
                {
                    return NotFound();
                }
                if (ModelState.IsValid)
                {
                    try
                    {
                        var book = new Book()
                        {
                            Title = bookEditViewModel.Title,
                            Genre = bookEditViewModel.Genre,
                            Description = bookEditViewModel.Description,
                            ISBN = bookEditViewModel.ISBN,
                            Language = bookEditViewModel.Language,
                            publicationDate = bookEditViewModel.publicationDate,
                            StockAmount = bookEditViewModel.StockAmount,
                            AuthorId = bookEditViewModel.AuthorId,
                            PictureFormat = bookEditViewModel.CoverPhoto.ContentType
                        };
                        // converting the coverPhoto from FormFile to byte array
                        var memoryStream = new MemoryStream();
                        bookEditViewModel.CoverPhoto.CopyTo(memoryStream);
                        book.CoverPhoto = memoryStream.ToArray();

                        // getting the author of the book 
                        var authorOfTheBook = await bookShopDbContext.Authors.FindAsync(bookEditViewModel.AuthorId);
                        if (authorOfTheBook == null)
                        {
                            return Problem("Author of the book not found in the 'BookShopDbContext.Author' entity");
                        }
                        book.Author = authorOfTheBook;

                        // adding to the context
                        bookShopDbContext.Add(book);
                        await bookShopDbContext.SaveChangesAsync(); // adding to the database
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!BookExists(bookEditViewModel.BookId))
                        {
                            if (!BookExists(bookEditViewModel.BookId))
                            {
                                return NotFound();
                            }
                            else
                            {
                                throw new Exception("Error from the BookController Post Edit method!");
                            }
                        }
                        return RedirectToAction(nameof(Index));
                    }
                }
                var authors = await bookShopDbContext.Authors.ToListAsync();
                ViewBag.Authors = authors;
                return View(bookEditViewModel);
            }

            // GET: Book/Delete/id
            public async Task<IActionResult> Delete(string id)
            {
                ViewBag.Title = "Detetion of Book";
                if (id == null || bookShopDbContext.Books == null)
                {
                    return NotFound();
                }

                var book = await bookShopDbContext.Books
                    .Include(b => b.Author)
                    .FirstOrDefaultAsync(m => m.BookId == id);
                if (book == null)
                {
                    return NotFound();
                }


                var bookDeletePageViewModel = new BookDeletePageViewModel()
                {
                    AuthorId = book.AuthorId,
                    BookId = book.BookId,
                    PictureFormat = book.PictureFormat,
                    Author = book.Author,
                    Description = book.Description,
                    Genre = book.Genre,
                    CoverPhoto = Convert.ToBase64String(book.CoverPhoto),
                    ISBN = book.ISBN,
                    Language = book.Language,
                    StockAmount = book.StockAmount,
                    publicationDate = book.publicationDate,
                    Title = book.Title
                };



                return View(bookDeletePageViewModel);
            }

        // POST: Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (bookShopDbContext.Books == null)
            {
                return Problem("Entity set 'BookShopDbContex.Books'  is null.");
            }

            var book = await bookShopDbContext.Books.FindAsync(id);
            if (book != null)
            {
                bookShopDbContext.Books.Remove(book);
            }

            await bookShopDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

            private bool BookExists(string id)
            {
                return (bookShopDbContext.Books?.Any(b => b.BookId == id)).GetValueOrDefault();
            }



        
        

        

    }
    
}
