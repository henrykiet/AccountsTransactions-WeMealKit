using AccountsTransactions_BusinessObjects.ResponseObjects;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels;
using AccountsTransactions_BusinessObjects.ServiceModels.ValidateModels;
using AccountsTransactions_BusinessObjects.Services.Interface;
using AccountsTransactions_DataAccess.Enums;
using AccountsTransactions_DataAccess.Models;
using AccountsTransactions_DataAccess.Repository.Interface;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.Services.Implement
{
	public class UserService : IUserService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		#region Validator
		private readonly CreateUserModelValidator _createValidator;
		private readonly UpdateModelValidator _updateValidator;
		private readonly IdUserModelValidator _idUserValidator;
		private readonly AddRoleUserModelValidator _addRoleValidator;
		private readonly ChangeRoleUserModelValidator _changeRoleValidator;
		#endregion
		public UserService(IUnitOfWork unitOfWork , IMapper mapper ,
			UserManager<User> userManager , RoleManager<IdentityRole> roleManager)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_userManager = userManager;
			_roleManager = roleManager;

			_createValidator = new CreateUserModelValidator();
			_updateValidator = new UpdateModelValidator();
			_idUserValidator = new IdUserModelValidator();
			_addRoleValidator = new AddRoleUserModelValidator();
			_changeRoleValidator = new ChangeRoleUserModelValidator();
		}

		public async Task<ResponseObject<AllUserResponseModel>> GetAllUsers()
		{
			var result = new ResponseObject<AllUserResponseModel>();
			var users = await _userManager.Users.ToListAsync();
			if ( users != null && users.Count() > 0)
			{
				var usersModel = _mapper.Map<List<AllUserResponseModel>>(users);
				foreach ( var u in usersModel )
				{
					u.Role = await GetUserRole(u.ID);
				}
				result.StatusCode = 200;
				result.Message = "List User";
				result.List = usersModel;
				return result;
			}
			else
			{
				result.StatusCode = 404;
				result.Message = "Don't have user!";
				return result;
			}
		}
		public async Task<ResponseObject<UserModelResponse?>> GetUserByIdAsync(string id)
		{
			var result = new ResponseObject<UserModelResponse?>();
			var user = await _userManager.FindByIdAsync(id);
			if ( user != null )
			{
				if ( user.LockoutEnd != null )
				{
					result.StatusCode = 300;
					result.Message = "User UnActive!";
					return result;
				}
				var userModel = _mapper.Map<UserModelResponse>(user);
				userModel.Role = await GetUserRole(user.Id);
				result.StatusCode = 200;
				result.Message = "User";
				result.Data = userModel;
				return result;
			}
			else
			{
				result.StatusCode = 404;
				result.Message = "User not found!";
				return result;
			}
		}
		public async Task<ResponseObject<UserModelResponse?>> GetUserAsync(string email)
		{
			var result = new ResponseObject<UserModelResponse?>();
			var usernameExist = await _userManager.FindByNameAsync(email);
			var emailExist = await _userManager.FindByEmailAsync(email);
			var userExist = usernameExist ?? emailExist;
			if ( userExist != null )
			{
				if ( userExist.LockoutEnd != null )
				{
					result.StatusCode = 300;
					result.Message = "User UnActive!";
					return result;
				}
				var userModel = _mapper.Map<UserModelResponse>(userExist);
				userModel.Role = await GetUserRole(userExist.Id);
				result.StatusCode = 200;
				result.Message = "User";
				result.Data = userModel;
				return result;
			}
			else
			{
				result.StatusCode = 404;
				result.Message = "User not found!";
				return result;
			}
		}
		public async Task<ResponseObject<UserModelResponse>> GetProfileByTokenAsync(string token)
		{
			var result = new ResponseObject<UserModelResponse>();
			try
			{
				//read token
				var handler = new JwtSecurityTokenHandler();
				var tokenString = handler.ReadToken(token) as JwtSecurityToken;
				if ( tokenString != null )
				{
					//get user id from token
					var userIdClaim = tokenString.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);
					if ( userIdClaim != null )
					{
						var userId = userIdClaim.Value;
						var user = await _userManager.FindByIdAsync(userId);
						if ( user == null )
						{
							result.StatusCode = 404;
							result.Message = "User not found!";
							return result;
						}
						else
						{
							var rolesClaim = tokenString.Claims.Where(claim => claim.Type == ClaimTypes.Role).Select(claim => claim.Value);
							var userModel = new UserModelResponse
							{
								ID = Guid.Parse(user.Id) ,
								Email = user.Email ?? "" ,
								UserName = user.UserName ?? "" ,
								FirstName = user.FirstName ,
								LastName = user.LastName ,
								DateOfBirth = user.DateOfBirth ,
								PhoneNumber = user.PhoneNumber ?? "" ,
								Address = user.Address ?? "" ,
								Role = string.Join(", " , rolesClaim) ,
								Gender = user.Gender ,
							};
							result.StatusCode = 200;
							result.Message = "User Profile";
							result.Data = userModel;
							return result;
						}
					}
					else
					{
						result.StatusCode = 401;
						result.Message = "Token not have userId!";
						return result;
					}
				}
				else
				{
					result.StatusCode = 402;
					result.Message = "Token invalid!";
					return result;
				}
			}
			catch ( Exception ex )
			{
				result.StatusCode = 500;
				result.Message = "Error when processing: " + ex.Message;
				return result;
			}
		}
		private async Task<string> GetUserRole(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if ( user != null )
			{
				var roles = await _userManager.GetRolesAsync(user);
				return string.Join("," , roles);
			}
			else
			{
				return string.Empty;
			}
		}
		public async Task<ResponseObject<BaseUserModelResponse>> CreateUserAsync(CreateUserModelRequest model)
		{
			var result = new ResponseObject<BaseUserModelResponse>();
			//check validate
			var validationResult = _createValidator.Validate(model);
			if ( !validationResult.IsValid )
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - " , error);
				return result;
			}
			//check email exist
			var emailExist = await _userManager.FindByEmailAsync(model.Email);
			if ( emailExist != null )
			{
				result.StatusCode = 402;
				result.Message = "Email have exist!";
				return result;
			}
			//check role exist
			var roleExist = await _roleManager.RoleExistsAsync(model.Role);
			if ( roleExist == false )
			{
				result.StatusCode = 404;
				result.Message = "Role doesn't exist!";
				return result;
			}
			var newUser = _mapper.Map<User>(model);
			newUser.UserName = model.Email;
			var createResult = await _userManager.CreateAsync(newUser);
			if ( createResult.Succeeded )
			{
				//setup password
				var defaultPassword = "User123@";
				await _userManager.AddPasswordAsync(newUser , defaultPassword);
				// Gán vai trò cho người dùng
				await _userManager.AddToRoleAsync(newUser , model.Role);
				result.StatusCode = 200;
				result.Message = "Created user (" + newUser.Email + ") successfully with password (" + defaultPassword + ").";
				//result.Data = new CreateUserModelResponse { Id = newUser.Id, UserName = newUser.UserName };
				return result;
			}
			else
			{
				result.StatusCode = 500;
				result.Message = "Failed to create new User!";
				return result;
			}
		}
		public async Task<ResponseObject<BaseUserModelResponse>> UpdateUserAsync(UpdateUserModelRequest model)
		{
			var result = new ResponseObject<BaseUserModelResponse>();
			var validationResult = _updateValidator.Validate(model);
			if ( !validationResult.IsValid )
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - " , error);
				return result;
			}
			//check user exists
			var userExist = await _userManager.FindByIdAsync(model.Id);
			if ( userExist == null )
			{
				result.StatusCode = 404;
				result.Message = "User not found!";
				return result;
			}
			if ( !string.IsNullOrEmpty(model.Email) )
			{
				//check email exist
				var emailExist = await _userManager.FindByEmailAsync(model.Email);
				if ( emailExist != null )
				{
					result.StatusCode = 402;
					result.Message = "Email have exist!";
					return result;
				}
				userExist.Email = model.Email;
				userExist.EmailConfirmed = false;
			}
			if ( !string.IsNullOrEmpty(model.UserName) )
			{
				//check username exist
				var usernameExist = await _userManager.FindByNameAsync(model.UserName);
				if ( usernameExist != null )
				{
					result.StatusCode = 402;
					result.Message = "Username have exist!";
					return result;
				}
				userExist.UserName = model.UserName;
			}
			if ( !string.IsNullOrEmpty(model.Phone) )
			{
				userExist.PhoneNumber = model.Phone;
				userExist.PhoneNumberConfirmed = false;
			}
			if ( !string.IsNullOrEmpty(model.FirstName) )
				userExist.FirstName = model.FirstName;
			if ( !string.IsNullOrEmpty(model.LastName) )
				userExist.LastName = model.LastName;
			if ( model.DateOfBirth != null )
				userExist.DateOfBirth = model.DateOfBirth;
			if ( !string.IsNullOrEmpty(model.Address) )
				userExist.Address = model.Address;
			if ( model.Gender != null )
				userExist.Gender = (UserGender)model.Gender;
			var updateResult = await _userManager.UpdateAsync(userExist);
			if ( updateResult.Succeeded )
			{
				result.StatusCode = 200;
				result.Message = "Updated user (" + userExist.UserName + ") successfully.";
				// result.Data = new UpdateUserModelResponse { Id = updateUser.Id, UserName = updateUser.UserName };
				return result;
			}
			else
			{
				result.StatusCode = 500;
				result.Message = "Failed to update User!";
				return result;
			}
		}
		public async Task<ResponseObject<BaseUserModelResponse>> DeleteUserAsync(IdUserModelRequest model)
		{
			var result = new ResponseObject<BaseUserModelResponse>();
			var validationResult = _idUserValidator.Validate(model);
			if ( !validationResult.IsValid )
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - " , error);
				return result;
			}
			//check user exists
			var userExist = await _userManager.FindByIdAsync(model.Id);
			if ( userExist == null )
			{
				result.StatusCode = 404;
				result.Message = "User not found!";
				return result;
			}
			//If user have order trust change status
			var orderExist = await _unitOfWork.UserRepository.GetOrderExistInUserAsync(userExist.Id);
			if ( orderExist )
			{
				userExist.LockoutEnd = DateTimeOffset.MaxValue;
				var updateResult = await _userManager.UpdateAsync(userExist);
				if ( updateResult.Succeeded )
				{
					result.StatusCode = 200;
					result.Message = "User (" + userExist.Email + ")have order trust change status (UnActive) successfully.";
					return result;
				}
				else
				{
					result.StatusCode = 500;
					result.Message = "Failed to Change User status to UnActive!";
					return result;
				}
			}
			else
			{
				var deleteResult = await _userManager.DeleteAsync(userExist);
				if ( deleteResult.Succeeded )
				{
					result.StatusCode = 200;
					result.Message = "User (" + userExist.Email + ") deleted successfully.";
					// result.Data = new DeleteUserModelResponse { Id = userExist.Id, UserName = userExist.UserName };
					return result;
				}
				else
				{
					result.StatusCode = 500;
					result.Message = "Failed to delete User!";
					return result;
				}
			}
		}
		public async Task<ResponseObject<BaseUserModelResponse>> ChangeRoleAsync(ChangeRoleUserModelRequest model)
		{
			var result = new ResponseObject<BaseUserModelResponse>();
			var validationResult = _changeRoleValidator.Validate(model);
			if ( !validationResult.IsValid )
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - " , error);
				return result;
			}
			var userExist = await _userManager.FindByIdAsync(model.Id);
			if ( userExist != null )
			{	
				//check old role if have in list role of user then remove it
				var oldRoles = await _userManager.GetRolesAsync(userExist);
				if ( oldRoles.Contains(model.OldRole) )
				{
					// Remove old roles
					var removeResult = await _userManager.RemoveFromRoleAsync(userExist , model.OldRole);
					if ( !removeResult.Succeeded )
					{
						result.StatusCode = 500;
						result.Message = "Failed to remove old roles!";
						return result;
					}
				}
				//check role exist
				var roleExist = await _roleManager.RoleExistsAsync(model.NewRole);
				if ( roleExist == false )
				{
					result.StatusCode = 404;
					result.Message = "Role doesn't exist!";
					return result;
				}
				// Add new role
				var addResult = await _userManager.AddToRoleAsync(userExist , model.NewRole);
				if ( !addResult.Succeeded )
				{
					result.StatusCode = 500;
					result.Message = "Failed to add new role!";
					return result;
				}

				result.StatusCode = 200;
				result.Message = "Changed role for user (" + userExist.UserName + ") from (" + string.Join("," , oldRoles) + ") to (" + model.NewRole + ") successfully.";
				return result;
			}
			else
			{
				result.StatusCode = 404;
				result.Message = "User not Exist!";
				return result;
			}
		}
		public async Task<ResponseObject<BaseUserModelResponse>> AddRoleAsync(AddRoleUserModelRequest model)
		{
			var result = new ResponseObject<BaseUserModelResponse>();
			var validationResult = _addRoleValidator.Validate(model);
			if ( !validationResult.IsValid )
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - " , error);
				return result;
			}
			var userExist = await _userManager.FindByIdAsync(model.Id);
			if ( userExist != null )
			{
				//check role exist
				var roleExist = await _roleManager.RoleExistsAsync(model.Role);
				if ( roleExist == false )
				{
					result.StatusCode = 404;
					result.Message = "Role doesn't exist!";
					return result;
				}
				var addResult = await _userManager.AddToRoleAsync(userExist , model.Role);
				if ( !addResult.Succeeded )
				{
					result.StatusCode = 500;
					result.Message = "Failed to add new role!";
					return result;
				}
				result.StatusCode = 200;
				result.Message = "Add role (" + model.Role + ") for user (" + userExist.UserName + ") successfully.";
				return result;
			}
			else
			{
				result.StatusCode = 404;
				result.Message = "User not exist!";
				return result;
			}
		}
		public async Task<ResponseObject<BaseUserModelResponse>> ChangeStatusAsync(IdUserModelRequest model)
		{
			var result = new ResponseObject<BaseUserModelResponse>();
			//check validate
			var validationResult = _idUserValidator.Validate(model);
			if ( !validationResult.IsValid )
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - " , error);
				return result;
			}
			//check User exist
			var userExist = await _userManager.FindByIdAsync(model.Id);
			if ( userExist == null )
			{
				result.StatusCode = 404;
				result.Message = "User not found!";
				return result;
			}
			// Set LockAccount
			if ( userExist.LockoutEnd == null )
			{
				await _userManager.SetLockoutEndDateAsync(userExist , DateTimeOffset.MaxValue);
				result.StatusCode = 200;
				result.Message = "Changed User (" + userExist.UserName + ") with status (UnActive) successfully.";
				return result;
			}
			else
			{
				await _userManager.SetLockoutEndDateAsync(userExist , null);
				result.StatusCode = 200;
				result.Message = "Changed User (" + userExist.UserName + ") with status (Active) successfully.";
				return result;
			}
		}
		public async Task<ResponseObject<BaseUserModelResponse>> ChangeEmailConfirmAsync(IdUserModelRequest model)
		{
			var result = new ResponseObject<BaseUserModelResponse>();
			//check validate
			var validationResult = _idUserValidator.Validate(model);
			if ( !validationResult.IsValid )
			{
				var error = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				result.StatusCode = 400;
				result.Message = string.Join(" - " , error);
				return result;
			}
			//check User exist
			var userExist = await _userManager.FindByIdAsync(model.Id);
			if ( userExist == null )
			{
				result.StatusCode = 404;
				result.Message = "User not Exist!";
				return result;
			}
			if ( userExist.EmailConfirmed == false )
			{
				userExist.EmailConfirmed = true;
				var updateResult = await _userManager.UpdateAsync(userExist);
				if ( updateResult.Succeeded)
				{
					result.StatusCode = 200;
					result.Message = "Changed User (" + userExist.UserName + ") with confirm email successfully.";
					return result;
				}
				else
				{
					result.StatusCode = 500;
					result.Message = "Update email confirm user unsuccess!";
					return result;
				}
			}
			else
			{
				userExist.EmailConfirmed = false;
				var updateResult = await _userManager.UpdateAsync(userExist);
				if ( updateResult.Succeeded )
				{
					result.StatusCode = 200;
					result.Message = "Changed User (" + userExist.UserName + ") with not confirm email successfully.";
					return result;
				}
				else
				{
					result.StatusCode = 500;
					result.Message = "Update not email confirm user unsuccess!";
					return result;
				}
			}
		}
	}
}
