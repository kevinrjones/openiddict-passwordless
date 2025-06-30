using System.Collections.Generic;
using Rsk.AspNetCore.Fido.Dtos;

namespace Velusia.Server.ViewModels.Fido;

public class Login
{
    public string RelyingPartyId { get; set; }
    public Base64FidoAuthenticationChallenge Challenge { get; set; }
    public string ReturnUrl { get; set; }
    public string UserId { get; set; }
    public List<string> Keys { get; set; }
}