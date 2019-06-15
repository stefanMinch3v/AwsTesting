namespace WebAdvert.Web.Controllers
{
    using Amazon.AspNetCore.Identity.Cognito;
    using Amazon.Extensions.CognitoAuthentication;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Models.Accounts;
    using System.Linq;
    using System.Threading.Tasks;

    public class AccountsController : Controller
    {
        private readonly SignInManager<CognitoUser> signInManager;
        private readonly UserManager<CognitoUser> userManager;
        private readonly CognitoUserPool cognitoUserPool;

        public AccountsController(
            SignInManager<CognitoUser> signInManager,
            UserManager<CognitoUser> userManager,
            CognitoUserPool cognitoUserPool)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.cognitoUserPool = cognitoUserPool;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest();
            }

            var user = this.cognitoUserPool.GetUser(model.Email);

            user.Attributes.Add(CognitoAttribute.Email.AttributeName, model.Email);

            var result = await this.userManager.CreateAsync(user, model.Password).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return this.BadRequest(result.Errors.FirstOrDefault().Description);
            }

            return this.RedirectToAction(
                nameof(ConfirmEmail), new ConfirmEmailViewModel { Email = model.Email });
        }

        [HttpGet]
        public IActionResult ConfirmEmail(ConfirmEmailViewModel model)
        {
            return this.View(model);
        }

        [HttpPost]
        [ActionName(nameof(ConfirmEmail))]
        public async Task<IActionResult> ConfirmEmailPost(ConfirmEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest("Confirmation failed!");
            }

            var user = await this.userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);

            if (user == null)
            {
                return this.NotFound();
            }

            var result = await (this.userManager as CognitoUserManager<CognitoUser>)
                .ConfirmSignUpAsync(user, model.Code, true).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return this.BadRequest("Invalid code!");
            }

            return this.RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
