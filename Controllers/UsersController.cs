﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assetify.Data;
using Assetify.Models;

using Microsoft.AspNetCore.Http;
using Assetify.Service;
using System.Web.Helpers;
using SQLitePCL;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.Generic;

namespace Assetify.Controllers
{
    public class UsersController : Controller
    {
        private readonly AssetifyContext _context;

        public UsersController(AssetifyContext context)
        {
            _context = context;
        }


        public ActionResult Login(string? message, string? returnUrl)
        {
            var userContext = UserContextService.GetUserContext(HttpContext);
            if (userContext.sessionID != null)
                return RedirectToAction("Logout");
            ViewBag.Message = TempData["LoginMessage"] != null ? TempData["LoginMessage"] : message;
            TempData["ReturnUrl"] = TempData["ReturnUrl"] != null ? TempData["ReturnUrl"] : returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult SignIn(String Email, String Password)
        {

            foreach (var u in _context.Users)
            {
                if (u.Email == Email && (Crypto.VerifyHashedPassword(u.Password.ToString(), Password.ToString())))
                {
                    if (u.ProfileImgPath != null)
                        HttpContext.Session.SetString("ProfileImg", u.ProfileImgPath);
                    if (u.IsAdmin)
                        HttpContext.Session.SetString("AdminIDSession", u.UserID.ToString());

                    HttpContext.Session.SetString("UserIDSession", u.UserID.ToString());
                    HttpContext.Session.SetString("name", u.FirstName.ToString());
                    ViewBag.Login = true;
                    if (TempData["ReturnUrl"] != null)
                        return Redirect(TempData["ReturnUrl"].ToString());

                    return RedirectToAction("Index", "Home");
                }
            }

            TempData["LoginMessage"] = "Login failed, name or password is incorrect!";
            TempData["ReturnUrl"] = TempData["ReturnUrl"].ToString();

            return RedirectToAction("Login");
        }

        public ActionResult Logout()
        {
            var userContext = UserContextService.GetUserContext(HttpContext);
            userContext.sessionID = null;
            userContext.sessionID = null;
            userContext.isAdmin = false;
            HttpContext.Session.Clear();

            TempData["ReturnUrl"] = null;
            TempData["LoginMessage"] = "You just logged out :)";
            return RedirectToAction("Login", "Users");
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var userContext = UserContextService.GetUserContext(HttpContext);
            if (!(userContext.isAdmin))
            {
                TempData["ReturnUrl"] = Request.GetDisplayUrl().ToString();
                TempData["LoginMessage"] = "You have to be an Admin to see all users, please login with admin credentials";
                return RedirectToAction("Login", "Users");
            }

            List<User> allUsers = await _context.Users.Include(u => u.Assets).ToListAsync();

            foreach (User u in allUsers)
            {
                u.NumOfFavorites = u.Assets.Where(ua => ua.Action == ActionType.LIKE).Count();
                u.NumOfPublish = u.Assets.Where(ua => ua.Action == ActionType.PUBLISH).Count();
            }

            return View(new UserIndex() { users = allUsers, userSearch = new UserSearch() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IndexSearch([FromForm] UserSearch userSearch)
        {
            var filteredUsers = _context.Users.AsQueryable();
            if (userSearch.Email != null) filteredUsers = filteredUsers.Where(x => x.Email.Contains(userSearch.Email));
            if (userSearch.FirstName != null) filteredUsers = filteredUsers.Where(x => x.FirstName.Contains(userSearch.FirstName));
            if (userSearch.LastName != null) filteredUsers = filteredUsers.Where(x => x.LastName.Contains(userSearch.LastName));
            var users = await filteredUsers.Include(u => u.Assets).ToListAsync();


            return View("Index", new UserIndex() { users = users, userSearch = userSearch });
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserID,Email,Password,FirstName,LastName,Phone,IsVerified,ProfileImgPath,LastSeenFavorite,LastSeenMessages")] User user, IFormFile file)
        {
            UserContext userContext = UserContextService.GetUserContext(HttpContext);
            user.Password = Crypto.HashPassword(user.Password);

            var emailExist = isEmailExist(user.Email);
            if (emailExist) ModelState.AddModelError("Email", $"Email {user.Email} is already in use.");
            if (ModelState.IsValid && !emailExist)
            {
                if (file != null)
                    user.ProfileImgPath = await FileUploader.UploadFile(file);
                _context.Add(user);
                await _context.SaveChangesAsync();
                if (userContext.isAdmin)
                    return RedirectToAction("Index", "Users");
                TempData["LoginMessage"] = "Now that you have an account, please login! :)";
                return RedirectToAction("Login", "Users");
            }
            return View(user);
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyEmail(string email)
        {
            var emailExist = isEmailExist(email);
            if (emailExist)
            {
                return Json($"Email {email} is already in use.");
            }

            return Json(true);
        }

        public IActionResult EditMyProfile()
        {
            UserContext userContext = UserContextService.GetUserContext(HttpContext);
            if (userContext.sessionID == null)
                return RedirectToAction("Login");

            return RedirectToAction("Edit", "Users", new { id = userContext.sessionID });
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            UserContext userContext = UserContextService.GetUserContext(HttpContext);
            //Check that this is an Admin or the user signed in
            if (!userContext.isAdmin && id.ToString() != userContext.sessionID)
            {
                return RedirectToAction("EditMyProfile"); // TODO: no permission error
            }
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.isAdmin = userContext.isAdmin;
            ViewBag.IsEdit = true;
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserID,Email,FirstName,LastName,Phone,IsVerified,ProfileImgPath,LastSeenFavorite,LastSeenMessages")] UserWithoutPassword user)
        {
            if (!UserExists(id))
            {
                TempData["UserNotFound"] = "User not found";
                return RedirectToAction("Index", "Users"); 
            }
            UserContext userContext = UserContextService.GetUserContext(HttpContext);
            var loggedInUser = _context.Users.First(x => x.UserID == int.Parse(userContext.sessionID));
            if (loggedInUser.UserID != id || loggedInUser.UserID != user.UserID || loggedInUser.Email != user.Email)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    loggedInUser.FirstName = user.FirstName;
                    loggedInUser.LastName = user.LastName;
                    loggedInUser.Phone = user.Phone;
                    _context.Update(loggedInUser);
                    await _context.SaveChangesAsync();
                    UserContextService.UpdateNameInContext(HttpContext, loggedInUser.FirstName);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(loggedInUser.UserID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if (UserContextService.GetUserContext(HttpContext).isAdmin)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public ActionResult AdminDashboard()
        {
            UserContext userContext = UserContextService.GetUserContext(HttpContext);
            if (!userContext.isAdmin)
            {
                TempData["LoginMessage"] = "Only admins can see the Admin dashboard :) Login as Admin!";
                return RedirectToAction("Login", "Users"); 
            }
            ViewData["AdminName"] = userContext.name.ToString();
            return View();

        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }

        private bool isEmailExist(string email)
        {
            return _context.Users.Where(x => x.Email == email).Count() > 0;
        }
    }
}
