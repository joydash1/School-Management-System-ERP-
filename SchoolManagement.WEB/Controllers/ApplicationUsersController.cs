using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.DataAccess.UnitOfWork;
using SchoolManagement.Domain.DTOs.AuthenticationAndAuthorizationDtos;
using SchoolManagement.Domain.Entities.Authentication;
using SchoolManagement.Domain.Interfaces.AuthenticationAndAuthorization;
using SchoolManagement.WEB.ViewModels;
using System.Security.Claims;

namespace SchoolManagement.WEB.Controllers
{
    public class ApplicationUsersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUsers> _userManager;
        private readonly SignInManager<ApplicationUsers> _signInManager;
        private readonly ILogger<ApplicationUsers> _logger;
        private readonly IAuthenticationRepository _authenticationRepository;

        public ApplicationUsersController(IUnitOfWork unitOfWork, UserManager<ApplicationUsers> userManager, SignInManager<ApplicationUsers> signInManager, ILogger<ApplicationUsers> logger, IAuthenticationRepository authenticationRepository)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _authenticationRepository = authenticationRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Registration", model);
            }

            try
            {
                var registerDto = new RegisterDto
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth,
                    Address = model.Address,
                    Role = model.Role
                };

                // Call the repository method
                var result = await _authenticationRepository.RegisterAsync(registerDto);

                if (result.IsSuccess)
                {
                    // Sign in the user automatically after registration
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    await _signInManager.SignInAsync(user!, isPersistent: false);

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(RegistrationSuccess), new { email = model.Email });
                }

                // Add errors to ModelState
                foreach (var error in result.Errors ?? new List<string>())
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View("Registration", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                return View("Registration", model);
            }
        }

        [HttpGet]
        public IActionResult RegistrationSuccess(string email)
        {
            ViewBag.Email = email;
            return View();
        }
    }
}