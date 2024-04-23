using AccountsTransactions_BusinessObjects.Helpers;
using AccountsTransactions_BusinessObjects.ResponseObjects;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels;
using AccountsTransactions_BusinessObjects.ServiceModels.ValidateModels;
using AccountsTransactions_BusinessObjects.Services.Interface;
using AccountsTransactions_DataAccess.Models;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.Services.Implement
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly SignInManager<User> _signInManager;
		private readonly IConfiguration _configuration;
		#region Validator
		private readonly LoginModelValidator _loginValidator;
		private readonly RegisterModelValidator _registerValidator;
		private readonly ResetPasswordModelValidator _resetPassValidator;
		private readonly SendCodeByEmailModelValidator _sendCodeValidator;
		private readonly ResetPasswordByEmailModelValidator _resetPassByEmailValidator;
		#endregion
		public AuthService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration,
			SignInManager<User> signInManager)
        {
            _userManager = userManager;
			_roleManager = roleManager;
			_configuration = configuration;
			_signInManager = signInManager;

			_loginValidator = new LoginModelValidator();
			_registerValidator = new RegisterModelValidator();
			_resetPassValidator = new ResetPasswordModelValidator();
			_sendCodeValidator = new SendCodeByEmailModelValidator();
			_resetPassByEmailValidator = new ResetPasswordByEmailModelValidator();
        }
		#region Login
		public async Task<ResponseObject<UserModelResponse>> LoginEmailAsync(LoginModel model)
		{
			var result = new ResponseObject<UserModelResponse>();
			var validationResult = _loginValidator.Validate(model);
			if (!validationResult.IsValid)
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - ", error);
				return result;
			}
			// Check userExist by email
			var userExistByEmail = await _userManager.FindByEmailAsync(model.Email);
			// Check userExist by username
			var userExistByUsername = await _userManager.FindByNameAsync(model.Email);
			if (userExistByEmail == null && userExistByUsername == null)
			{
				result.StatusCode = 400;
				result.Message = "User not exist!";
				return result;
			}
			var userExist = userExistByEmail ?? userExistByUsername;
			if (userExist != null)
			{
				//check Login
				var resultLogin = await _signInManager.CheckPasswordSignInAsync(userExist, model.Password, false);
				if (resultLogin.Succeeded)
				{
					//check email confirm
					if (!userExist.EmailConfirmed)
					{
						var token = await _userManager.GenerateEmailConfirmationTokenAsync(userExist);
						result.StatusCode = 405;
						result.Message = Base64UrlHelper.EncodeTokenToBase64(token);
						return result;
					}
					//Success -> Reset AccessFailedCount
					userExist.AccessFailedCount = 0;
					await _userManager.UpdateAsync(userExist);

					//Token
					var tokens = await GenerateToken(userExist);
					if (tokens != null)
					{
						result.StatusCode = 200;
						result.Message = tokens;
						return result;
					}
					else
					{
						result.StatusCode = 400;
						result.Message = "Create token fail!";
						return result;
					}
				}
				else if (resultLogin.IsLockedOut)
				{
					result.StatusCode = 400;
					result.Message = "Account locked! Please contact administrator!";
					return result;
				}
				else
				{
					//Login fail -> increase AccessFailedCount
					userExist.AccessFailedCount++;
					//check user login fail more than 5 time
					if (userExist.AccessFailedCount >= 5)
					{
						//temporary account lock
						userExist.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10); //lock 10 minute
						await _userManager.SetLockoutEndDateAsync(userExist, userExist.LockoutEnd);
						result.StatusCode = 400;
						result.Message = "Try agian after 10 minute!";
						return result;
					}
					else
					{
						await _userManager.UpdateAsync(userExist);
						result.StatusCode = 400;
						result.Message = "Wrong password!";
						return result;
					}
				}
			}
			else
			{
				result.StatusCode = 400;
				result.Message = "User not exist!";
				return result;
			}
		}
		private async Task<string> GenerateToken(User user)
		{
			//create list claim
			var authClaims = new List<Claim>
				{
					new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
				};
			if (!string.IsNullOrEmpty(user.Email))
			{
				authClaims.Add(new Claim(ClaimTypes.Email, user.Email));
			}
			//Get Roles user
			var userRoles = await _userManager.GetRolesAsync(user);
			//add roles into claims
			authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
			//check and add email into list Claim
			var signingKey = _configuration["JWT:IssuerSigningKey"];
			if (!string.IsNullOrEmpty(signingKey))
			{
				//create key
				var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
				//create JWT
				var token = new JwtSecurityToken(
					issuer: _configuration["JWT:ValidIssuer"],
					audience: _configuration["JWT:ValidAudience"],
					claims: authClaims,
					expires: DateTime.Now.AddHours(2),
					signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha512Signature)
				);

				//string JWT
				return new JwtSecurityTokenHandler().WriteToken(token).ToString();
			}
			return "";
		}
		public async Task<ResponseObject<BaseUserModelResponse>> ConfirmEmailAsync(ConfirmMailModel model)
		{
			var result = new ResponseObject<BaseUserModelResponse>();
			var userExist = await _userManager.FindByEmailAsync(model.Email);
			if (userExist != null)
			{
				var decordToken = Base64UrlHelper.DecodeTokenFromURL(model.Token);
				var resultConfirm = await _userManager.ConfirmEmailAsync(userExist, decordToken);
				if (resultConfirm.Succeeded)
				{
					result.StatusCode = 200;
					result.Message = "Email verified successfully";
					return result;
				}
			}
			result.StatusCode = 400;
			result.Message = "This user Doesn't exist";
			return result;
		}
		#endregion
		#region Register
		public async Task<ResponseObject<UserModelResponse>> RegisterEmailAsync(RegisterModel model)
		{
			var result = new ResponseObject<UserModelResponse>();
			var validationResult = _registerValidator.Validate(model);
			if (!validationResult.IsValid)
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - ", error);
				return result;
			}
			//check confirm password
			if (!string.Equals(model.Password, model.ConfirmPassword))
			{
				result.StatusCode = 400;
				result.Message = "Password and confirm password does not match!";
				return result;
			}
			//check user exists
			var userExists = await _userManager.FindByEmailAsync(model.Email);
			if (userExists != null)
			{
				result.StatusCode = 404;
				result.Message = "Email not exist!";
				return result;
			}
			//create user
			var newUser = new User()
			{
				Email = model.Email,
				UserName = model.Email,
				FirstName = model.FirstName,
				LastName = model.LastName,
				PhoneNumber = model.PhoneNumber,
				Gender = model.Gender,
				DateOfBirth = model.Dob
			};
			//check create user
			var resultUser = await _userManager.CreateAsync(newUser);
			if (resultUser.Succeeded)
			{
				//add password
				await _userManager.AddPasswordAsync(newUser, model.Password);
				//set role customer
				await _userManager.AddToRoleAsync(newUser, "customer");
				result.StatusCode = 200;
				result.Message = "Register successfully.";
				return result;
			}
			else
			{
				result.StatusCode = 400;
				result.Message = "Failed to register";
				return result;
			}
		}
		#endregion
		#region Reset password
		public async Task<ResponseObject<BaseUserModelResponse>> ResetPasswordAsync(ResetPasswordModelRequest model)
		{
			var result = new ResponseObject<BaseUserModelResponse>();
			var validationResult = _resetPassValidator.Validate(model);
			if (!validationResult.IsValid)
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - ", error);
				return result;
			}
			if (!string.IsNullOrEmpty(model.Id))
			{
				//check userExist
				var userExist = await _userManager.FindByIdAsync(model.Id);
				if (userExist == null)
				{
					result.StatusCode = 400;
					result.Message = "User not found!";
					return result;
				}
				//update new password
				//check confirmPassword
				if (!string.Equals(model.NewPassword, model.ConfirmPassword))
				{
					result.StatusCode = 400;
					result.Message = "NewPassword and confirm password does not match!";
					return result;
				}
				var updateResult = await _userManager.ChangePasswordAsync(userExist, model.OldPassword, model.NewPassword);
				if (updateResult.Succeeded)
				{
					result.StatusCode = 200;
					result.Message = "Password reset successfully.";
					return result;
				}
				else
				{
					result.StatusCode = 500;
					string errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
					result.Message = "Failed to reset password! Errors: " + errors;
					return result;
				}
			}
			result.StatusCode = 400;
			result.Message = "UserName is required!";
			return result;
		}
		public async Task<ResponseObject<BaseUserModelResponse>> ResetPasswordEmailAsync(ResetPasswordByEmailModelRequest model)
		{
			var result = new ResponseObject<BaseUserModelResponse>();
			var validationResult = _resetPassByEmailValidator.Validate(model);
			if (!validationResult.IsValid)
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - ", error);
				return result;
			}
			//check email exist and email confirm
			var userExist = await _userManager.FindByEmailAsync(model.Email);
			if (userExist == null)
			{
				result.StatusCode = 400;
				result.Message = "User not found!";
				return result;
			}
			if (userExist.EmailConfirmed == false)
			{
				result.StatusCode = 405;
				result.Message = "Email not confirm!";
				return result;
			}
			//check code reset
			if (userExist.Code != null && userExist.Code.Equals(model.CodeReset))
			{
				//check confirmPassword
				if (!string.Equals(model.NewPassword, model.ConfirmPassword))
				{
					result.StatusCode = 400;
					result.Message = "NewPassword and confirm password does not match!";
					return result;
				}
				var token = await _userManager.GeneratePasswordResetTokenAsync(userExist);
				var updateResult = await _userManager.ResetPasswordAsync(userExist, token, model.NewPassword);
				if (updateResult.Succeeded)
				{
					BackgroundJob.Schedule(() => ResetCode(userExist.Id), TimeSpan.FromMicroseconds(2));
					result.StatusCode = 200;
					result.Message = "Password reset successfully.";
					return result;
				}
				else
				{
					result.StatusCode = 500;
					string errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
					result.Message = "Failed to reset password! Errors: " + errors;
					return result;
				}
			}
			result.StatusCode = 400;
			result.Message = "Code reset does not match. Please check email!";
			return result;
		}

		public async Task<ResponseObject<BaseUserModelResponse>> SendCodeResetEmailAsync(SendCodeByEmailModelRequest model)
		{
			var result = new ResponseObject<BaseUserModelResponse>();
			var validationResult = _sendCodeValidator.Validate(model);
			if (!validationResult.IsValid)
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - ", error);
				return result;
			}
			var userExist = await _userManager.FindByEmailAsync(model.Email);
			if (userExist == null)
			{
				result.StatusCode = 400;
				result.Message = "User not Exist!";
				return result;
			}
			//send Code and save to database
			var codeReset = GenerateRandomCode(6);
			userExist.Code = codeReset;
			//hangfire after 10 minutes reset code = null
			BackgroundJob.Schedule(() => ResetCode(userExist.Id), TimeSpan.FromMinutes(2));
			var updateResult = await _userManager.UpdateAsync(userExist);
			if (updateResult.Succeeded)
			{
				result.StatusCode = 200;
				result.Message = codeReset;
				return result;
			}
			else
			{
				result.StatusCode = 500;
				result.Message = "Failed update code reset!";
				return result;
			}
		}
		public async Task ResetCode(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user != null)
			{
				user.Code = null;
				await _userManager.UpdateAsync(user);
			}
		}
		private string GenerateRandomCode(int length)
		{
			var random = new Random();
			const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			string randomChars = new string(Enumerable.Repeat(chars, length)
				.Select(s => s[random.Next(s.Length)]).ToArray());
			return "WMK-" + randomChars;
		}

		#endregion
	}
}
