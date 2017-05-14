// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountController.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using GitLfs.Server.Caching.Models;
    using GitLfs.Server.Caching.Models.AccountViewModels;
    using GitLfs.Server.Caching.Services;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http.Authentication;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

    [Authorize]
    public class AccountController : Controller
    {
        private readonly IEmailSender _emailSender;

        private readonly string _externalCookieScheme;

        private readonly ILogger _logger;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ISmsSender _smsSender;

        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<IdentityCookieOptions> identityCookieOptions,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            this._emailSender = emailSender;
            this._smsSender = smsSender;
            this._logger = loggerFactory.CreateLogger<AccountController>();
        }

        // GET /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return this.View();
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return this.View("Error");
            }

            ApplicationUser user = await this._userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return this.View("Error");
            }

            IdentityResult result = await this._userManager.ConfirmEmailAsync(user, code);
            return this.View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            string redirectUrl = this.Url.Action(
                nameof(this.ExternalLoginCallback),
                "Account",
                new { ReturnUrl = returnUrl });
            AuthenticationProperties properties =
                this._signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return this.Challenge(properties, provider);
        }

        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                this.ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return this.View(nameof(Login));
            }

            ExternalLoginInfo info = await this._signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return this.RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            SignInResult result =
                await this._signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            if (result.Succeeded)
            {
                this._logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                return this.RedirectToLocal(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                return this.RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
            }

            if (result.IsLockedOut)
            {
                return this.View("Lockout");
            }

            // If the user does not have an account, then ask the user to create an account.
            this.ViewData["ReturnUrl"] = returnUrl;
            this.ViewData["LoginProvider"] = info.LoginProvider;
            string email = info.Principal.FindFirstValue(ClaimTypes.Email);
            return this.View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
        }

        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(
            ExternalLoginConfirmationViewModel model,
            string returnUrl = null)
        {
            if (this.ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                ExternalLoginInfo info = await this._signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return this.View("ExternalLoginFailure");
                }

                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                IdentityResult result = await this._userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await this._userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await this._signInManager.SignInAsync(user, false);
                        this._logger.LogInformation(
                            6,
                            "User created an account using {Name} provider.",
                            info.LoginProvider);
                        return this.RedirectToLocal(returnUrl);
                    }
                }

                this.AddErrors(result);
            }

            this.ViewData["ReturnUrl"] = returnUrl;
            return this.View(model);
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return this.View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                ApplicationUser user = await this._userManager.FindByEmailAsync(model.Email);
                if (user == null || !await this._userManager.IsEmailConfirmedAsync(user))
                {
                    return this.View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                // var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                // var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                // await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                // $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                // return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return this.View();
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await this.HttpContext.Authentication.SignOutAsync(this._externalCookieScheme);

            this.ViewData["ReturnUrl"] = returnUrl;
            return this.View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            this.ViewData["ReturnUrl"] = returnUrl;
            if (this.ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                SignInResult result =
                    await this._signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    this._logger.LogInformation(1, "User logged in.");
                    return this.RedirectToLocal(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return this.RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, model.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    this._logger.LogWarning(2, "User account locked out.");
                    return this.View("Lockout");
                }

                this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return this.View(model);
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await this._signInManager.SignOutAsync();
            this._logger.LogInformation(4, "User logged out.");
            return this.RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            this.ViewData["ReturnUrl"] = returnUrl;
            return this.View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            this.ViewData["ReturnUrl"] = returnUrl;
            if (this.ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                IdentityResult result = await this._userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    // var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    // var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    // await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                    // $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
                    await this._signInManager.SignInAsync(user, false);
                    this._logger.LogInformation(3, "User created a new account with password.");
                    return this.RedirectToLocal(returnUrl);
                }

                this.AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? this.View("Error") : this.View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            ApplicationUser user = await this._userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return this.RedirectToAction(nameof(this.ResetPasswordConfirmation), "Account");
            }

            IdentityResult result = await this._userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return this.RedirectToAction(nameof(this.ResetPasswordConfirmation), "Account");
            }

            this.AddErrors(result);
            return this.View();
        }

        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return this.View();
        }

        // GET: /Account/SendCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        {
            ApplicationUser user = await this._signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            IList<string> userFactors = await this._userManager.GetValidTwoFactorProvidersAsync(user);
            List<SelectListItem> factorOptions = userFactors
                .Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return this.View(
                new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(SendCodeViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            ApplicationUser user = await this._signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            // Generate the token and send it
            string code = await this._userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                return this.View("Error");
            }

            string message = "Your security code is: " + code;
            if (model.SelectedProvider == "Email")
            {
                await this._emailSender.SendEmailAsync(
                    await this._userManager.GetEmailAsync(user),
                    "Security Code",
                    message);
            }
            else if (model.SelectedProvider == "Phone")
            {
                await this._smsSender.SendSmsAsync(await this._userManager.GetPhoneNumberAsync(user), message);
            }

            return this.RedirectToAction(
                nameof(VerifyCode),
                new { Provider = model.SelectedProvider, model.ReturnUrl, model.RememberMe });
        }

        // GET: /Account/VerifyCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            ApplicationUser user = await this._signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            return this.View(
                new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            SignInResult result = await this._signInManager.TwoFactorSignInAsync(
                                      model.Provider,
                                      model.Code,
                                      model.RememberMe,
                                      model.RememberBrowser);
            if (result.Succeeded)
            {
                return this.RedirectToLocal(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                this._logger.LogWarning(7, "User account locked out.");
                return this.View("Lockout");
            }

            this.ModelState.AddModelError(string.Empty, "Invalid code.");
            return this.View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (this.Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}