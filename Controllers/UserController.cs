//using DPUruNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RVMSService.Helpers;
using RVMSService.Models;
using RVMSService.Services;
using System.Diagnostics.Eventing.Reader;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;

namespace RVMSService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;
        private readonly IAuditTrailService _auditTrail;

        private const int IdentifyThreshold = 21474;


        public UserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration, ILogger<UserController> logger,
            IAuditTrailService auditTrail)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _auditTrail = auditTrail;
        }

        // DELETE: api/User/{username}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{userName}")]
        public async Task<IActionResult> DeleteUser(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // Optional: Prevent self-deletion
            if (User?.Identity?.Name == user.UserName)
                return BadRequest(new { message = "You cannot delete your own account." });

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                return Ok(new { message = $"User {userName} deleted successfully." });

            return BadRequest(result.Errors);
        }

        // POST: api/User/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                BioID = model.BioID
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
                return Ok(new { message = "User registered successfully." });

            return BadRequest(result.Errors);
        }


        // POST: api/User/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //var result = await _signInManager.PasswordSignInAsync(
            //    model.UserName,
            //    model.Password,
            //    isPersistent: false,
            //    lockoutOnFailure: false);

            //if (result.Succeeded)
            //    return Ok(new { message = "Login successful." });


            //string secretKey = SettingsConfig["JwtKey"]?.ToString();
            //string issuer = SettingsConfig["JwtIssuer"]?.ToString();

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid username or password." });

            // Update last login time
            user.LastLoginTime = DateTime.Now;
            await _userManager.UpdateAsync(user);

            // Generate JWT token
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"] ?? "your_secret_key_here"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtIssuer"] ?? "authcheck",
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            //return Ok(new { user = user.UserName, roles = roles.FirstOrDefault(), token = tokenString });
            return Ok(new
            {
                token = tokenString,
                userName = user.UserName,
                roles = roles.FirstOrDefault()

            });
        }


        // POST: api/User/change-password
        [Authorize(Roles = "Admin, Operator")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] UserLoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            ApplicationUser user = null;

            //Check if admin
            if (User.IsInRole("Admin"))
            {
                if (string.IsNullOrWhiteSpace(model.UserName))
                    return BadRequest(new { message = "UserName is required for admin password change." });

                user = await _userManager.FindByNameAsync(model.UserName);
                if (user == null)
                    return NotFound(new { message = "User not found." });
            }
            else if (User.IsInRole("Operator"))
            {
                // Operators can only change their own password
                user = await _userManager.FindByNameAsync(model.UserName);
                if (user == null)
                    return Unauthorized(new { message = "User not found." });

                // If username is provided and does not match, block the request
                if (!string.IsNullOrEmpty(model.UserName) && !string.Equals(user.UserName, model.UserName, StringComparison.OrdinalIgnoreCase))
                    return Forbid();
            }
            else
            {
                return Forbid();
            }


            //var user = await _userManager.FindByNameAsync(model.UserName);
            //if (user == null)
            //    return Unauthorized(new { message = "User not found." });

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

            if (result.Succeeded)
                return Ok(new { message = "Password changed successfully." });

            return BadRequest(result.Errors);
        }


        [Authorize(Roles = "Admin")] // Only admins can assign roles
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // Create role if it doesn't exist
            var roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roleManager.RoleExistsAsync(model.Role))
                await roleManager.CreateAsync(new IdentityRole(model.Role));

            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (result.Succeeded)
                return Ok(new { message = $"Role '{model.Role}' assigned to user '{model.UserName}'." });

            return BadRequest(result.Errors);
        }

        // GET: api/User/list
        [Authorize(Roles = "Admin")]
        [HttpGet("list")]
        public async Task<IActionResult> ListUsers()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<UserModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var isLockedOut = await _userManager.IsLockedOutAsync(user);
                userList.Add(new UserModel
                {
                    Id = Guid.Parse(user.Id),
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = string.Join(", ", roles),
                    FullName = user.FullName,
                    LastLoginTime = user.LastLoginTime,
                    IsLockedOut = isLockedOut,
                    BioID = user.BioID
                });
            }

            return Ok(userList);
        }

        // POST: api/User/logout
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // For JWT, logout is handled client-side by removing the token.
            // Optionally, you can implement token blacklisting here if needed.
            return Ok(new { message = "Logout successful. Please remove the token on the client." });
        }

        // POST: api/User/lockout/{username}
        [Authorize(Roles = "Admin")]
        [HttpPost("lockout/{userName}")]
        public async Task<IActionResult> LockoutUser(string userName, [FromBody] int lockoutMinutes = 30)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var lockoutEnd = DateTimeOffset.UtcNow.AddYears(50);
            var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

            if (result.Succeeded)
                return Ok(new { message = $"User {userName} locked out until {lockoutEnd}." });

            return BadRequest(result.Errors);
        }

        // POST: api/User/unlock/{username}
        [Authorize(Roles = "Admin")]
        [HttpPost("unlock/{userName}")]
        public async Task<IActionResult> UnlockUser(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // Reset lockout end date
            var result = await _userManager.SetLockoutEndDateAsync(user, null);

            if (result.Succeeded)
            {
                // Reset access failed count
                await _userManager.ResetAccessFailedCountAsync(user);
                return Ok(new { message = $"User {userName} unlocked successfully." });
            }

            return BadRequest(result.Errors);
        }

        //POST : api/User/update/{operatorModel}
        [Authorize(Roles = "Admin")]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateOperator([FromBody] UserModel operatorModel)
        {
            var user = await _userManager.FindByIdAsync(operatorModel.Id.ToString());
            if (user == null)
                return BadRequest(new { message = "User not found." });
            user.FullName = operatorModel.FullName;
            user.UserName = operatorModel.UserName;
            user.Email = operatorModel.Email;
            user.BioID = operatorModel.BioID;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)//update data success
            {
                //update role
                var roleresult = await _userManager.GetRolesAsync(user);
                if (!roleresult.Contains(operatorModel.Role))
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, roleresult);
                    if (removeResult.Succeeded)
                    {
                        var addResult = await _userManager.AddToRoleAsync(user, operatorModel.Role);
                        if (addResult.Succeeded)
                        {
                            return Ok(new { message = "User updated successfully." });
                        }
                        else
                        {
                            return BadRequest(addResult.Errors);
                        }
                    }
                    else
                    {
                        return BadRequest(removeResult.Errors);
                    }
                }
                else
                {
                    return Ok(new { message = "User updated successfully." });
                }

            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        // POST:650
        // Client sends a DPUruNet verification Fmd serialized as byte[] (base64 in JSON).
        [HttpPost("Fingerlogin")]
        //public async Task<IActionResult> FingerLogin([FromBody] byte[] fmdBytes)
        public async Task<IActionResult> FingerLogin([FromBody] DOTUserModel dotUserModel)
        {
            var fmdBytes = dotUserModel.User?.BioID;
            if (fmdBytes == null || fmdBytes.Length == 0)
                return BadRequest(new { message = "Fingerprint data is required." });

            //// 1. Reconstruct the verification Fmd from raw bytes
            //Fmd verificationFmd;

            // 1. Deserialize the verification FMD sent by the client.
            byte[] verificationFmd;

            try
            {
                //verificationFmd = Fmd.DeserializeXml(Encoding.UTF8.GetString(fmdBytes));
                verificationFmd = FmdSerializer.Deserialize(fmdBytes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid fingerprint data received");
                return BadRequest(new { message = "Invalid fingerprint data format." });
            }

            // 2. Load all users that have an enrolled fingerprint template
            var candidates = _userManager.Users
                .Where(u => u.BioID != null)
                .ToList();

            if (candidates.Count == 0)
                return Unauthorized(new { message = "No enrolled fingerprints in the system." });

            // 3. Deserialize each stored enrollment Fmd and build arrays for Identify
            //var enrolledFmds = new List<Fmd>();

            var enrolledFmds = new List<byte[]>();
            var candidateUsers = new List<ApplicationUser>();

            foreach (var candidate in candidates)
            {
                try
                {
                    //var fmd = Fmd.DeserializeXml(Encoding.UTF8.GetString(candidate.BioID!));
                    var fmd = FmdSerializer.Deserialize(candidate.BioID!);
                    enrolledFmds.Add(fmd);
                    candidateUsers.Add(candidate);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping user {UserId}: corrupt BioID data", candidate.Id);
                }
            }

            if (enrolledFmds.Count == 0)
                return Unauthorized(new { message = "No valid enrolled fingerprints found." });

            // 4. Use DPUruNet Comparison.Identify for fuzzy matching
            //    (same threshold as DPUruHelper: 21474 ≈ 1/100,000 FAR)
            //var identifyResult = Comparison.Identify(
            //    verificationFmd,
            //    0,
            //    enrolledFmds.ToArray(),
            //    IdentifyThreshold,
            //    enrolledFmds.Count);

            //if (identifyResult.ResultCode != Constants.ResultCode.DP_SUCCESS
            //    || identifyResult.Indexes == null
            //    || identifyResult.Indexes.Length == 0
            //    || identifyResult.Indexes[0].Length == 0)
            //{
            //    _logger.LogInformation("Fingerprint login failed: no match found");
            //    return Unauthorized(new { message = "Fingerprint not recognized." });
            //}

            // 4. Identify via native SDK (dpfp_identify).
            //    Pin each enrolled FMD to get stable native pointers.
            uint count = (uint)enrolledFmds.Count;
            var handles = enrolledFmds.Select(f => GCHandle.Alloc(f, GCHandleType.Pinned)).ToArray();
            int matchIndex = -1;

            try
            {
                var ptrs = handles.Select(h => h.AddrOfPinnedObject()).ToArray();
                var sizes = enrolledFmds.Select(f => (uint)f.Length).ToArray();
                var indices = new uint[count];

                int rc = DpfpddNative.dpfp_identify(
                    verificationFmd, (uint)verificationFmd.Length,
                    0,
                    ptrs, sizes, count,
                    IdentifyThreshold,
                    out uint resultCount,
                    indices);

                if (rc != DpfpddNative.DPFPDD_SUCCESS || resultCount == 0)
                {
                    _logger.LogInformation("Fingerprint login failed: no match found");
                    return Unauthorized(new { message = "Fingerprint not recognized." });
                }

                matchIndex = (int)indices[0];
            }
            finally
            {
                foreach (var h in handles) h.Free();
            }






            // 5. Get the matched user
            //int matchIndex = identifyResult.Indexes[0][0];
            var user = candidateUsers[matchIndex];

            // 6. Check lockout
            if (await _userManager.IsLockedOutAsync(user))
                return Unauthorized(new { message = "Account is locked out." });

            // 7. Update last login time
            user.LastLoginTime = DateTime.Now;
            await _userManager.UpdateAsync(user);

            // 8. Generate JWT token (same logic as Login)
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"] ?? "your_secret_key_here"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtIssuer"] ?? "authcheck",
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Fingerprint login successful for user {UserName}", user.UserName);

            AuditTrailModel audit = new AuditTrailModel
            {
                UserName = user.UserName,
                //Description = "Fingerprint Login",
                Timestamp = dotUserModel.audit?.Timestamp ?? DateTime.Now,

                Location = dotUserModel.audit?.Location ?? "Unknown",

                Description = $"User {user.UserName} logged in using fingerprint authentication."
            };

            // Save audit trail
            await _auditTrail.RecordAsync(audit);

            return Ok(new
            {
                token = tokenString,
                userName = user.UserName,
                roles = roles.FirstOrDefault()
            });
        }
    }
}