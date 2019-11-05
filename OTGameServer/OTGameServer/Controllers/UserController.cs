using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
//using NetcodeIO.NET;
using Newtonsoft.Json;
using TGame.Entities;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TGame.Controllers
{
    [Route("api/[controller]/[action]")]
    public class UserController : Controller
    {
        private readonly GameContext gameContext;
        private readonly UserManager<User> userManager;

        public UserController(UserManager<User> userManager, GameContext gameContext)
        {
            this.userManager = userManager;
            this.gameContext = gameContext;
        }

        // POST: /user/auth
        [HttpPost]
        public async Task<IActionResult> Auth([FromBody]AuthModel model)
        {
            if (!ModelState.IsValid
                || string.IsNullOrWhiteSpace(model.Email) 
                || string.IsNullOrWhiteSpace(model.Password)
               )
                return BadRequest(ModelState);
            
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
                return new OkObjectResult(JsonConvert.SerializeObject(new { result = "Invalid email or password" }));

            var response = new
            {
                result = "OK",
                displayName = user.UserName,
                auth_token = GenerateToken(user)
            };

            return new OkObjectResult(JsonConvert.SerializeObject(response));
        }

        // GET: /user/register
        [HttpPost]
        public async Task<IActionResult> Register([FromBody]RegistrationModel model)
        {
            if (!ModelState.IsValid
                || string.IsNullOrWhiteSpace(model.Email)
                || string.IsNullOrWhiteSpace(model.Password)
            )
                return BadRequest(ModelState);

            var user = new User
            {
                Email = model.Email,
                UserName = model.UserName,
                Hero = new Hero{
                    MapId = 0,
                    BaseStats = new Stats{
                        Health = 40
                    },
                    Level = 1,
                    Location = new Point(32, 36)
                    //Location = new Vector2(0, -3.6f)
                }
            };
            //user.Hero = Hero.CreateNew(user, gameContext);

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return new OkObjectResult(JsonConvert.SerializeObject(new { result = "Could not create new account! <br>" + string.Join("<br>", result.Errors) }));

            return new OkObjectResult(new
            {
                result = "OK",
                displayName = user.UserName,
                auth_token = GenerateToken(user)
            });
        }

        string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = new JwtSecurityToken(
                issuer: "commentFaire",
                audience: "commentFaire",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: new SigningCredentials(Startup.signingKey, SecurityAlgorithms.HmacSha256)
            );

            return tokenHandler.WriteToken(token);
        }
        
    }

    
    public class AuthModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    
    public class RegistrationModel
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
