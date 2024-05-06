using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels;
using AccountsTransactions_BusinessObjects.Services.Implement;
using AccountsTransactions_DataAccess.Enums;
using AccountsTransactions_DataAccess.Models;
using AccountsTransactions_DataAccess.Repository.Interface;
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_Test.Services
{

	public class UserServiceTest
	{
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IValidator<CreateUserModelRequest> _createValidator;
		private readonly IValidator<UpdateUserModelRequest> _updateValidator;
		private readonly IValidator<IdUserModelRequest> _idUserValidator;
		private readonly IValidator<AddRoleUserModelRequest> _addRoleValidator;
		private readonly IValidator<ChangeRoleUserModelRequest> _changeRoleValidator;
		public UserServiceTest()
		{
			_userManager = A.Fake<UserManager<User>>();
			_roleManager = A.Fake<RoleManager<IdentityRole>>();
			_unitOfWork = A.Fake<IUnitOfWork>();
			_mapper = A.Fake<IMapper>();
			// Fake validators
			_createValidator = A.Fake<IValidator<CreateUserModelRequest>>();
			_updateValidator = A.Fake<IValidator<UpdateUserModelRequest>>();
			_idUserValidator = A.Fake<IValidator<IdUserModelRequest>>();
			_addRoleValidator = A.Fake<IValidator<AddRoleUserModelRequest>>();
			_changeRoleValidator = A.Fake<IValidator<ChangeRoleUserModelRequest>>();
		}
		#region GetByIdAsync
		[Fact]
		public async Task UserService_GetUserByIdAsync_ReturnUserFound()
		{
			// Arrange
			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);
			var userId = Guid.NewGuid();

			var user = new User
			{
				Id = userId.ToString() ,
				Email = "example@example.com" ,
				UserName = "example" ,
				FirstName = "John" ,
				LastName = "Doe" ,
				DateOfBirth = new DateTime(1990 , 1 , 1) ,
				Gender = UserGender.Male ,
				PhoneNumber = "123456789" ,
				Address = "123 Main St" ,
				LockoutEnabled = true
			};

			var userModel = new UserModelResponse
			{
				ID = userId ,
				Email = user.Email ,
				UserName = user.UserName ,
				FirstName = user.FirstName ,
				LastName = user.LastName ,
				DateOfBirth = user.DateOfBirth ,
				Gender = user.Gender ,
				PhoneNumber = user.PhoneNumber ,
				Address = user.Address ,
				Role = ""
			};

			// Thiết lập hành vi cho UserManager giả
			A.CallTo(() => _userManager.FindByIdAsync(userId.ToString())).Returns(user);

			// Thiết lập hành vi cho IMapper giả
			A.CallTo(() => _mapper.Map<UserModelResponse>(user))
				.Returns(userModel);

			// Act
			var result = await userService.GetUserByIdAsync(userId.ToString());

			// Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().Be("User");
			result.Data.Should().BeEquivalentTo(userModel);
		}
		[Fact]
		public async Task UserService_GetUserByIdAsync_ReturnUserFoundAndUnActive()
		{
			// Arrange
			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);
			var userId = Guid.NewGuid().ToString();
			var user = new User
			{
				Id = userId ,
				LockoutEnd = DateTimeOffset.MaxValue
			};
			// Thiết lập hành vi cho UserManager giả
			A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(user);

			// Act
			var result = await userService.GetUserByIdAsync(userId);

			// Assert
			result.StatusCode.Should().Be(300);
			result.Message.Should().Be("User UnActive!");
			result.Data.Should().BeNull();
		}
		[Fact]
		public async Task UserService_GetUserByIdAsync_ReturnUserNotFound()
		{
			// Arrange
			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);
			var userId = Guid.NewGuid().ToString();
			User? user = null;
			A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(user);

			// Act
			var result = await userService.GetUserByIdAsync(userId);

			// Assert
			result.StatusCode.Should().Be(404);
			result.Message.Should().Be("User not found!");
			result.Data.Should().BeNull();
		}
		#endregion
		#region GetUserAsync
		[Fact]
		public async Task UserService_GetUserAsync_ReturnUsernameExist()
		{
			// Arrange
			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);
			var username = "test123";
			var user = new User
			{
				Id = Guid.NewGuid().ToString() ,
				UserName = username ,
			};
			var userModel = new UserModelResponse
			{
				ID = Guid.Parse(user.Id) ,
				Email = user.Email ?? "" ,
				UserName = user.UserName ,
				FirstName = user.FirstName ,
				LastName = user.LastName ,
				DateOfBirth = user.DateOfBirth ,
				Gender = user.Gender ,
				PhoneNumber = user.PhoneNumber ?? "" ,
				Address = user.Address ?? "" ,
				Role = ""
			};
			User? userNull = null;
			A.CallTo(() => _userManager.FindByNameAsync(username)).Returns(user);
			A.CallTo(() => _userManager.FindByEmailAsync(username)).Returns(userNull);

			A.CallTo(() => _mapper.Map<UserModelResponse>(user)).Returns(userModel);

			// Act
			var result = await userService.GetUserAsync(username);

			// Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().Be("User");
			result.Data.Should().BeEquivalentTo(userModel);
		}

		[Fact]
		public async Task UserService_GetUserAsync_ReturnEmailExist()
		{
			// Arrange
			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);
			var email = "example@example.com";
			var user = new User
			{
				Id = Guid.NewGuid().ToString() ,
				UserName = "example" ,
				Email = email ,
				LockoutEnabled = true
			};
			var userModel = new UserModelResponse
			{
				ID = Guid.Parse(user.Id) ,
				Email = user.Email ,
				UserName = user.UserName ,
				FirstName = user.FirstName ,
				LastName = user.LastName ,
				DateOfBirth = user.DateOfBirth ,
				Gender = user.Gender ,
				PhoneNumber = user.PhoneNumber ?? "" ,
				Address = user.Address ?? "" ,
				Role = ""
			};
			User? userNull = null;

			A.CallTo(() => _userManager.FindByNameAsync(email)).Returns(userNull);
			A.CallTo(() => _userManager.FindByEmailAsync(email)).Returns(user);

			A.CallTo(() => _mapper.Map<UserModelResponse>(user)).Returns(userModel);

			// Act
			var result = await userService.GetUserAsync(email);

			// Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().Be("User");
			result.Data.Should().BeEquivalentTo(userModel);
		}

		[Fact]
		public async Task UserService_GetUserAsync_ReturnUserNotFound()
		{
			// Arrange
			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);
			var email = "example@example.com";
			User? userNull = null;

			A.CallTo(() => _userManager.FindByNameAsync(email)).Returns(userNull);
			A.CallTo(() => _userManager.FindByEmailAsync(email)).Returns(userNull);

			// Act
			var result = await userService.GetUserAsync(email);

			// Assert
			result.StatusCode.Should().Be(404);
			result.Message.Should().Be("User not found!");
			result.Data.Should().BeNull();
		}

		[Fact]
		public async Task UserService_GetUserAsync_ReturnUserUnactive()
		{
			// Arrange
			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);
			var email = "example@example.com";
			var user = new User
			{
				Id = Guid.NewGuid().ToString() ,
				UserName = "example" ,
				Email = email ,
				LockoutEnd = DateTimeOffset.MaxValue
			};
			User? userNull = null;

			A.CallTo(() => _userManager.FindByNameAsync(email)).Returns(userNull);
			A.CallTo(() => _userManager.FindByEmailAsync(email)).Returns(user);

			// Act
			var result = await userService.GetUserAsync(email);

			// Assert
			result.StatusCode.Should().Be(300);
			result.Message.Should().Be("User UnActive!");
			result.Data.Should().BeNull();
		}
		#endregion
		#region GetProfileByTokenAsync
		private string TokenGen(string userId)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, userId),
				new Claim(ClaimTypes.Role, "Admin"),
				new Claim(ClaimTypes.Role, "User"),
			};
			var identity = new ClaimsIdentity(claims);
			var claimsPrincipal = new ClaimsPrincipal(identity);
			var tokenString = new JwtSecurityToken("issuer" , "audience" , claims);
			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenSerialized = tokenHandler.WriteToken(tokenString);
			return tokenSerialized;
		}
		[Fact]
		public async Task UserService_GetProfileByTokenAsync_ReturnSuccess()
		{
			// Arrange
			var userId = Guid.NewGuid().ToString();
			var user = new User
			{
				Id = userId ,
				Email = "example@example.com" ,
				UserName = "example" ,
				FirstName = "John" ,
				LastName = "Doe" ,
				DateOfBirth = new DateTime(1990 , 1 , 1) ,
				Gender = UserGender.Male ,
				PhoneNumber = "123456789" ,
				Address = "123 Main St" ,
			};
			var token = TokenGen(userId);

			// Mock UserManager
			A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(user);

			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);

			// Act
			var result = await userService.GetProfileByTokenAsync(token);

			// Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().Be("User Profile");
			result.Data.Should().NotBeNull();
		}

		[Fact]
		public async Task UserService_GetProfileByTokenAsync_ReturnUserNotFound()
		{
			// Arrange
			var userId = Guid.NewGuid().ToString();
			var token = TokenGen(userId);
			User? userNull = null;
			A.CallTo(() => _userManager.FindByIdAsync(userId)).Returns(userNull);

			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);

			// Act
			var result = await userService.GetProfileByTokenAsync(token);

			// Assert
			result.StatusCode.Should().Be(404);
			result.Message.Should().Be("User not found!");
			result.Data.Should().BeNull();
		}

		[Fact]
		public async Task UserService_GetProfileByTokenAsync_ReturnTokenNotContainUserId()
		{
			// Arrange
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Role, "Admin"),
				new Claim(ClaimTypes.Role, "User"),
			};
			var identity = new ClaimsIdentity(claims);
			var claimsPrincipal = new ClaimsPrincipal(identity);
			var tokenString = new JwtSecurityToken("issuer" , "audience" , claims);
			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenSerialized = tokenHandler.WriteToken(tokenString);

			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);

			// Act
			var result = await userService.GetProfileByTokenAsync(tokenSerialized);

			// Assert
			result.StatusCode.Should().Be(401);
			result.Message.Should().Be("Token not have userId!");
			result.Data.Should().BeNull();
		}

		//[Fact]
		//public async Task UserService_GetProfileByTokenAsync_ReturnInvalidToken_WhenTokenIsInvalid()
		//{
		//	// Arrange
		//	var token = "";
		//	var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);

		//	// Act
		//	var result = await userService.GetProfileByTokenAsync(token);

		//	// Assert
		//	result.StatusCode.Should().Be(404);
		//	result.Message.Should().Be("Token invalid!");
		//	result.Data.Should().BeNull();
		//}
		[Fact]
		public async Task UserService_GetProfileByTokenAsync_ReturnServerError()
		{
			// Arrange
			var token = "valid_token";
			var userId = Guid.NewGuid().ToString();
			A.CallTo(() => _userManager.FindByIdAsync(A<string>._)).Throws<Exception>();

			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);

			// Act
			var result = await userService.GetProfileByTokenAsync(token);

			// Assert
			result.StatusCode.Should().Be(500);
			result.Message.Should().Contain("Error when processing:");
			result.Data.Should().BeNull();
		}
		#endregion
		#region CreateUserAsync
		[Fact]
		public async Task UserService_CreateUserAsync_ReturnSuccess()
		{
			// Arrange
			var model = new CreateUserModelRequest
			{
				Email = "test@example.com" ,
				FirstName = "John" ,
				LastName = "Doe" ,
				Gender = UserGender.Male ,
				DateOfBirth = new DateTime(1990 , 1 , 1) ,
				Phone = "123456789" ,
				Address = "123 Main St" ,
				Role = "User"
			};

			// Fake the validator
			A.CallTo(() => _createValidator.Validate(model)).Returns(new ValidationResult());

			// Fake the UserManager to return null, indicating that the email doesn't exist
			User? userNull = null;
			A.CallTo(() => _userManager.FindByEmailAsync(model.Email)).Returns(userNull);

			// Fake the RoleManager to return true, indicating that the role exists
			A.CallTo(() => _roleManager.RoleExistsAsync(model.Role)).Returns(true);

			// Fake the result of creating a new user
			var newUser = new User { Email = model.Email };
			A.CallTo(() => _userManager.AddToRoleAsync(newUser, model.Role)).Returns(IdentityResult.Success);
			var identityResult = IdentityResult.Success;
			A.CallTo(() => _userManager.CreateAsync(A<User>.That.Matches(u => u.Email == model.Email))).Returns(identityResult);

			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);

			// Act
			var result = await userService.CreateUserAsync(model);

			// Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().StartWith("Created user");
		}

		[Fact]
		public async Task UserService_CreateUserAsync_EmailExists_ReturnsBadRequest()
		{
			// Arrange
			var model = new CreateUserModelRequest
			{
				Email = "test@example.com" ,
				Role = "UserRole"
				// Add other properties as needed
			};

			// Fake the validator
			A.CallTo(() => _createValidator.Validate(model)).Returns(new ValidationResult());

			// Fake the UserManager to return a user, indicating that the email exists
			A.CallTo(() => _userManager.FindByEmailAsync(model.Email)).Returns(new User());

			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);

			// Act
			var result = await userService.CreateUserAsync(model);

			// Assert
			result.StatusCode.Should().Be(400);
			result.Message.Should().Be("Email have exist!");
		}

		[Fact]
		public async Task UserService_CreateUserAsync_RoleDoesNotExist_ReturnsBadRequest()
		{
			// Arrange
			var model = new CreateUserModelRequest
			{
				Email = "test@example.com" ,
				Role = "NonExistentRole"
				// Add other properties as needed
			};

			// Fake the validator
			A.CallTo(() => _createValidator.Validate(model)).Returns(new ValidationResult());

			// Fake the UserManager to return null, indicating that the email doesn't exist
			User? userNull = null;
			A.CallTo(() => _userManager.FindByEmailAsync(model.Email)).Returns(userNull);

			// Fake the RoleManager to return false, indicating that the role doesn't exist
			A.CallTo(() => _roleManager.RoleExistsAsync(model.Role)).Returns(false);

			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);

			// Act
			var result = await userService.CreateUserAsync(model);

			// Assert
			result.StatusCode.Should().Be(400);
			result.Message.Should().Be("Role doesn't exist!");
		}

		[Fact]
		public async Task UserService_CreateUserAsync_CreateUserFailed_ReturnsInternalServerError()
		{
			// Arrange
			var model = new CreateUserModelRequest
			{
				Email = "test@example.com" ,
				Role = "UserRole"
				// Add other properties as needed
			};

			// Fake the validator
			A.CallTo(() => _createValidator.Validate(model)).Returns(new ValidationResult());

			// Fake the UserManager to return null, indicating that the email doesn't exist
			User? userNull = null;
			A.CallTo(() => _userManager.FindByEmailAsync(model.Email)).Returns(userNull);

			// Fake the RoleManager to return true, indicating that the role exists
			A.CallTo(() => _roleManager.RoleExistsAsync(model.Role)).Returns(true);

			// Fake the result of creating a new user to indicate failure
			var identityResult = IdentityResult.Failed(new IdentityError { Description = "Failed to create user" });
			A.CallTo(() => _userManager.CreateAsync(A<User>._)).Returns(identityResult);

			var userService = new UserService(_unitOfWork , _mapper , _userManager , _roleManager);

			// Act
			var result = await userService.CreateUserAsync(model);

			// Assert
			result.StatusCode.Should().Be(500);
			result.Message.Should().Be("Failed to create new User!");
		}

		#endregion
	}
}
