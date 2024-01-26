using BSDay15.Data;
using BSDay15.Models;
using BSDay15.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BSDay15.Controllers
{
    public class UserController : Controller
    {
        private readonly BookShopDbContext bookShopDbContext;

        public UserController(BookShopDbContext context)
        {
            bookShopDbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginViewModel userLoginViewModel)
        {
            if (ModelState.IsValid)
            {
                // Authenticate the user (you need to replace this with your actual authentication logic)
                var user = await AuthenticateUser(userLoginViewModel.UserEmail, userLoginViewModel.UserPassword);

                if (user != null)
                {
                    // Create claims for the authenticated user
                    var claims = new[]
                    {
                new System.Security.Claims.Claim(ClaimTypes.Name, user.UserName),
                // Add more claims as needed
            };

                    var claimsIdentity = new System.Security.Claims.ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Create authentication properties
                    var authProperties = new AuthenticationProperties
                    {
                        // Set additional properties as needed
                    };

                    // Sign in the user
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new System.Security.Claims.ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Redirect to the Index action of the Book controller upon successful login
                    return RedirectToAction("Index", "Book");
                }
            }

            // If authentication fails, return the login view with errors
            return View(userLoginViewModel);
        }

        private async Task<User> AuthenticateUser(string userEmail, string password)
        {
            // Replace this with your actual authentication logic using your data store
            var user = await bookShopDbContext.Users.FirstOrDefaultAsync(u =>
                u.UserEmail == userEmail && u.UserPassword == password);

            return user;
        }
        [HttpGet]
        public IActionResult SignUp()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(UserSignUpViewModel userSignUpViewModel)
        {
            if(ModelState.IsValid)
            {
                var singUp = new User()
                {
                    UserName = userSignUpViewModel.UserName,
                    UserEmail= userSignUpViewModel.UserEmail,
                    UserPassword= userSignUpViewModel.UserPassword
                };
                bookShopDbContext.Add(singUp);
                await bookShopDbContext.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View(userSignUpViewModel);
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect to the home page or any other desired page
            return RedirectToAction("Index", "Home");
        }
    }
}
