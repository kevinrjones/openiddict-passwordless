using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rsk.AspNetCore.Fido;
using Rsk.AspNetCore.Fido.Dtos;
using Rsk.AspNetCore.Fido.Models;
using Velusia.Server.Data;
using Velusia.Server.ViewModels.Fido;

namespace Velusia.Server.Controllers;

public class FidoController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IFidoAuthentication _fido;

    public FidoController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IFidoAuthentication fido)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _fido = fido;
    }
    public IActionResult StartRegistration(string returnUrl)
    {
        StartRegistration model = new()
        {
            ReturnUrl = returnUrl
        };
        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Register(StartRegistration model)
    {
        if (ModelState.IsValid)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                return BadRequest("A user with that email address already exists.");
            }

            FidoRegistrationChallenge challenge = await _fido.InitiateRegistration(model.Email, model.DeviceName);
            var register = new Register()
            {
                ReturnUrl = model.ReturnUrl,
                Challenge = challenge.ToBase64Dto()
            };

            return View(register);

        }
        
        return View("StartRegistration", model);
    }

    [HttpPost]
    public async Task<IActionResult> CompleteRegistration(
        [FromBody] Base64FidoRegistrationResponse registrationResponse)
    {
        IFidoRegistrationResult result = await _fido.CompleteRegistration(registrationResponse.ToFidoResponse());

        if (result.IsError)
        {
            return BadRequest(result.ErrorDescription);
        }
        
        ApplicationUser user = new()
        {
            UserName = result.UserId,
            Email = result.UserId,
        };

        IdentityResult identityResult = await _userManager.CreateAsync(user);

        if (identityResult.Succeeded)
        {
            return Ok();
        }

        return BadRequest(string.Join(',', identityResult.Errors.Select(e => e.Description)));

    }
    
    public ActionResult StartLogin(string? returnUrl = null)
    {
        StartLogin model = new()
        {
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartLogin(StartLogin model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                return BadRequest("A user with that email address does not exist.");
            }

            FidoAuthenticationChallenge challenge = await _fido.InitiateAuthentication(model.Email);
            var base64Challenge = challenge.ToBase64Dto();

            Login loginModel = new()
            {
                UserId = base64Challenge.UserId,
                Challenge = base64Challenge,
                RelyingPartyId = base64Challenge.RelyingPartyId,
                Keys = base64Challenge.Base64KeyIds,
                ReturnUrl = model.ReturnUrl
            };

            return View("Login", loginModel);
        }
        return View("StartLogin", model);
    }

    [HttpPost]
    public async Task<IActionResult> FidoCompleteLogin(
        [FromBody] Base64FidoAuthenticationResponse authenticationResponse)
    {
        var result = await _fido.CompleteAuthentication(authenticationResponse.ToFidoResponse());
        
        if (result.IsError) return BadRequest(result.ErrorDescription);

        if (result.IsSuccess)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(result.UserId);
            if (user == null)
            {
                return BadRequest("A user with that email address does not exist.");
            }

            await _signInManager.SignInAsync(user, false);

        }

        return Ok();
    }
}