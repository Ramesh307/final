using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManageMentSystem.Models;
using SchoolManageMentSystem.Services.AccountServices;

namespace SchoolManageMentSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SchoolDbContext _schoolDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAccount _account;
       
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager ,
            RoleManager<IdentityRole> roleManager, SchoolDbContext schoolDbContext,IAccount account)
        {
            _account=account;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager  = roleManager;
            _schoolDbContext = schoolDbContext;
        }
        public async Task <IActionResult> Index()
        {
            ApplicationUser currentuser=await _userManager.GetUserAsync(HttpContext.User);
            string currentRole=_account.getroleId(currentuser);
            // Sometimes redirect to Index rather than Admindashboard
            if (currentRole== "Admin")
            {
                return Redirect("Account/Admindashboard") ;
            }
            else if(currentRole == "Student" || currentRole == "Guardian")
            {
                return Redirect("Home/Index");
            }
            else       
            {
                // Teacher Dashbooard
                return Redirect("Account/Teacherboard");
            }
           
            

        }
        [Authorize (Roles = "Admin")]
        public IActionResult UserView()
        {
            List<UserViewModel> users = (from u in _userManager.Users
                                         select new UserViewModel
                                         {
                                             Id = u.Id,
                                             Email = u.Email,
                                             UserName = u.UserName,
                                             PhoneNumber = u.PhoneNumber,
                                         }).ToList();
            return View(users);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Admindashboard()
        {
            return View();
        }
        [Authorize(Roles = "Teacher")]
        public IActionResult Teacherboard()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
       
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            ApplicationUser User = new ApplicationUser
            {

                Email = model.Email,
                UserName = model.Email,
                DepartmentId = 2,
                PhoneNumber = model.PhoneNumber
            };
            IdentityResult result = await _userManager.CreateAsync(User, model.Password);
            if (result.Succeeded)
            {
                TempData["ResponseMessage"] = " Successfully Registered";
                TempData["ResponseValue"] = "1";
                return RedirectToAction("Register");
            }
            else
            {
                TempData["ResponseMessage"] = " Register Faild!";
                TempData["ResponseValue"] = "0";
                return View(model);
            }
            
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult>Login(LoginModel model)
        {
            ApplicationUser user =await _userManager.FindByEmailAsync(model.Email);
            if (user!=null)
            {
                bool Result=await _userManager.CheckPasswordAsync(user, model.Password);
                if (Result)
                {
                    await _signInManager.SignInAsync(user, Result);
                    return RedirectToAction("Index");
                }
                else RedirectToAction("Index");
                return View(model);
            }
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AssignRole()
        {
            List < SelectListItem> roles= _roleManager.Roles.Select(x=>new SelectListItem { Text=x.Name,Value=x.Name}).ToList();
            List <SelectListItem> users=_userManager.Users.Select(x=>new SelectListItem { Text=x.UserName, Value=x.Id}).ToList();
            ViewBag.Roles = roles;
            ViewBag.Users = users;
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AssignRole(RoleModel model)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(model.UserId);
           await _userManager.AddToRoleAsync(user, model.RoleName);
            return RedirectToAction("AssignRole");

        }
        public async Task<IActionResult> StudentView()
        {
            //2ed id
            // srole = 4
            
            IList<ApplicationUser> User = await _userManager.GetUsersInRoleAsync("Student");
            List<ApplicationUser> studentUser = User.ToList();
            return View(studentUser);


        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);    
            return View(user);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult>Edit(ApplicationUser userModel)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(userModel.Email);
            user.Email = userModel.Email;
            user.PhoneNumber = userModel.PhoneNumber;
            user.UserName = userModel.UserName;
            await _userManager.UpdateAsync(user);
            return View("Edit");
            
        }
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return Redirect("/Home/Index");
        }
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            
           

            return View(user);
        }
     
        




    }
}
