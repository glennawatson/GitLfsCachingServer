// <copyright file="ManageController.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Server.Caching.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using GitLfs.Server.Caching.Models;
    using GitLfs.Server.Caching.Models.ManageViewModels;
    using GitLfs.Server.Caching.Services;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http.Authentication;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    [Authorize]
    public class ManageController : Controller
    {
        private readonly IEmailSender emailSender;

        private readonly string externalCookieScheme;

        private readonly ILogger logger;

        private readonly SignInManager<ApplicationUser> signInManager;

        private readonly ISmsSender smsSender;

        private readonly UserManager<ApplicationUser> userManager;

        public ManageController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<IdentityCookieOptions> identityCookieOptions,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            this.emailSender = emailSender;
            this.smsSender = smsSender;
            this.logger = loggerFactory.CreateLogger<ManageController>();
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,

            AddLoginSuccess,

            ChangePasswordSuccess,

            SetTwoFactorSuccess,

            SetPasswordSuccess,

            RemoveLoginSuccess,

            RemovePhoneSuccess,

            Error
        }

        // GET: /Manage/AddPhoneNumber
        public IActionResult AddPhoneNumber()
        {
            return this.View();
        }

        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            // Generate the token and send it
            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            string code = await this.userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
            await this.smsSender.SendSmsAsync(model.PhoneNumber, "Your security code is: " + code);
            return this.RedirectToAction(nameof(VerifyPhoneNumber), new { model.PhoneNumber });
        }

        // GET: /Manage/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return this.View();
        }

        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user != null)
            {
                IdentityResult result =
                    await this.userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, false);
                    this.logger.LogInformation(3, "User changed their password successfully.");
                    return this.RedirectToAction(
                        nameof(this.Index),
                        new { Message = ManageMessageId.ChangePasswordSuccess });
                }

                this.AddErrors(result);
                return this.View(model);
            }

            return this.RedirectToAction(nameof(this.Index), new { Message = ManageMessageId.Error });
        }

        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactorAuthentication()
        {
            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user != null)
            {
                await this.userManager.SetTwoFactorEnabledAsync(user, false);
                await this.signInManager.SignInAsync(user, false);
                this.logger.LogInformation(2, "User disabled two-factor authentication.");
            }

            return this.RedirectToAction(nameof(this.Index), "Manage");
        }

        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user != null)
            {
                await this.userManager.SetTwoFactorEnabledAsync(user, true);
                await this.signInManager.SignInAsync(user, false);
                this.logger.LogInformation(1, "User enabled two-factor authentication.");
            }

            return this.RedirectToAction(nameof(this.Index), "Manage");
        }

        // GET: /Manage/Index
        [HttpGet]
        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            this.ViewData["StatusMessage"] = message == ManageMessageId.ChangePasswordSuccess
                                                 ? "Your password has been changed."
                                                 : message == ManageMessageId.SetPasswordSuccess
                                                     ? "Your password has been set."
                                                     : message == ManageMessageId.SetTwoFactorSuccess
                                                         ? "Your two-factor authentication provider has been set."
                                                         : message == ManageMessageId.Error
                                                             ? "An error has occurred."
                                                             : message == ManageMessageId.AddPhoneSuccess
                                                                 ? "Your phone number was added."
                                                                 : message == ManageMessageId.RemovePhoneSuccess
                                                                     ? "Your phone number was removed."
                                                                     : string.Empty;

            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            var model = new IndexViewModel
                            {
                                HasPassword = await this.userManager.HasPasswordAsync(user),
                                PhoneNumber = await this.userManager.GetPhoneNumberAsync(user),
                                TwoFactor = await this.userManager.GetTwoFactorEnabledAsync(user),
                                Logins = await this.userManager.GetLoginsAsync(user),
                                BrowserRemembered =
                                    await this.signInManager.IsTwoFactorClientRememberedAsync(user)
                            };
            return this.View(model);
        }

        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkLogin(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await this.HttpContext.Authentication.SignOutAsync(this.externalCookieScheme);

            // Request a redirect to the external login provider to link a login for the current user
            string redirectUrl = this.Url.Action(nameof(this.LinkLoginCallback), "Manage");
            AuthenticationProperties properties = this.signInManager.ConfigureExternalAuthenticationProperties(
                provider,
                redirectUrl,
                this.userManager.GetUserId(this.User));
            return this.Challenge(properties, provider);
        }

        // GET: /Manage/LinkLoginCallback
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            ExternalLoginInfo info =
                await this.signInManager.GetExternalLoginInfoAsync(await this.userManager.GetUserIdAsync(user));
            if (info == null)
            {
                return this.RedirectToAction(nameof(this.ManageLogins), new { Message = ManageMessageId.Error });
            }

            IdentityResult result = await this.userManager.AddLoginAsync(user, info);
            var message = ManageMessageId.Error;
            if (result.Succeeded)
            {
                message = ManageMessageId.AddLoginSuccess;

                // Clear the existing external cookie to ensure a clean login process
                await this.HttpContext.Authentication.SignOutAsync(this.externalCookieScheme);
            }

            return this.RedirectToAction(nameof(this.ManageLogins), new { Message = message });
        }

        // GET: /Manage/ManageLogins
        [HttpGet]
        public async Task<IActionResult> ManageLogins(ManageMessageId? message = null)
        {
            this.ViewData["StatusMessage"] = message == ManageMessageId.RemoveLoginSuccess
                                                 ? "The external login was removed."
                                                 : message == ManageMessageId.AddLoginSuccess
                                                     ? "The external login was added."
                                                     : message == ManageMessageId.Error
                                                         ? "An error has occurred."
                                                         : string.Empty;
            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            IList<UserLoginInfo> userLogins = await this.userManager.GetLoginsAsync(user);
            List<AuthenticationDescription> otherLogins = this.signInManager.GetExternalAuthenticationSchemes()
                .Where(auth => userLogins.All(ul => auth.AuthenticationScheme != ul.LoginProvider)).ToList();
            this.ViewData["ShowRemoveButton"] = user.PasswordHash != null || userLogins.Count > 1;
            return this.View(new ManageLoginsViewModel { CurrentLogins = userLogins, OtherLogins = otherLogins });
        }

        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
        {
            ManageMessageId? message = ManageMessageId.Error;
            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user != null)
            {
                IdentityResult result =
                    await this.userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
                if (result.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, false);
                    message = ManageMessageId.RemoveLoginSuccess;
                }
            }

            return this.RedirectToAction(nameof(this.ManageLogins), new { Message = message });
        }

        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePhoneNumber()
        {
            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user != null)
            {
                IdentityResult result = await this.userManager.SetPhoneNumberAsync(user, null);
                if (result.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, false);
                    return this.RedirectToAction(
                        nameof(this.Index),
                        new { Message = ManageMessageId.RemovePhoneSuccess });
                }
            }

            return this.RedirectToAction(nameof(this.Index), new { Message = ManageMessageId.Error });
        }

        // GET: /Manage/SetPassword
        [HttpGet]
        public IActionResult SetPassword()
        {
            return this.View();
        }

        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user != null)
            {
                IdentityResult result = await this.userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, false);
                    return this.RedirectToAction(
                        nameof(this.Index),
                        new { Message = ManageMessageId.SetPasswordSuccess });
                }

                this.AddErrors(result);
                return this.View(model);
            }

            return this.RedirectToAction(nameof(this.Index), new { Message = ManageMessageId.Error });
        }

        // GET: /Manage/VerifyPhoneNumber
        [HttpGet]
        public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            string code = await this.userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);

            // Send an SMS to verify the phone number
            return phoneNumber == null
                       ? this.View("Error")
                       : this.View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user != null)
            {
                IdentityResult result =
                    await this.userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.Code);
                if (result.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, false);
                    return this.RedirectToAction(nameof(this.Index), new { Message = ManageMessageId.AddPhoneSuccess });
                }
            }

            // If we got this far, something failed, redisplay the form
            this.ModelState.AddModelError(string.Empty, "Failed to verify phone number");
            return this.View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return this.userManager.GetUserAsync(this.HttpContext.User);
        }
    }
}