using BSDay15.Models;
using Microsoft.EntityFrameworkCore;

namespace BSDay15.Data
{
   
        public class BookShopDbContext : DbContext
        {
            public BookShopDbContext(DbContextOptions optons) : base(optons) { }


            public DbSet<Book> Books { get; set; }
            public DbSet<Author> Authors { get; set; }

            public DbSet<User> Users { get; set; }
        }
    
}
