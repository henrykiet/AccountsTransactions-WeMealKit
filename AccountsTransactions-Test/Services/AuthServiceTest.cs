using AccountsTransactions_DataAccess.Models;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using AccountsTransactions_BusinessObjects.Services.Implement;
using AccountsTransactions_BusinessObjects.Services.Interface;
using AccountsTransactions_BusinessObjects.Helpers;
using AccountsTransactions_DataAccess.Enums;
using Hangfire;
using System.Linq.Expressions;
using AccountsTransactions_Services.Services;


namespace AccountsTransactions_Test.Services
{
	public class AuthServiceTest
	{
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly SignInManager<User> _signInManager;
		private readonly IConfiguration _configuration;
		public AuthServiceTest()
		{
			//reference
			_userManager = A.Fake<UserManager<User>>();
			_signInManager = A.Fake<SignInManager<User>>();
			_roleManager = A.Fake<RoleManager<IdentityRole>>();
			_configuration = A.Fake<IConfiguration>();

			//SUT

		}

		#region Login
		[Fact]
		public async void AuthService_LoginEmailAsync_ReturnSuccessToken()
		{
			//Arange
			var user = A.Fake<User>();
			user.Email = "test@gmail.com";
			user.EmailConfirmed = true;
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);

			var loginModel = new LoginModel
			{
				Email = user.Email ,
				Password = "User123!"
			};

			//Mock
			A.CallTo(() => _userManager.GetRolesAsync(user)).Returns(new List<string> { "Customer" });
			A.CallTo(() => _userManager.FindByEmailAsync(loginModel.Email)).Returns(user);
			A.CallTo(() => _signInManager.CheckPasswordSignInAsync(user , loginModel.Password , false)).Returns(SignInResult.Success);
			//A.CallTo(() => service.GenerateToken(user)).Returns("mocked_token");
			
			//Act
			var result = await service.LoginEmailAsync(loginModel);

			//Assert
			result.StatusCode.Should().Be(200);
			//result.Message.Should().NotBeNullOrEmpty();
		}

		[Fact]
		public async void AuthService_LoginEmailAsync_ReturnLockAccount()
		{
			//Arange
			var user = A.Fake<User>();
			user.Email = "test@gmail.com";
			user.EmailConfirmed = true;
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);

			var loginModel = new LoginModel
			{
				Email = user.Email ,
				Password = "User123!"
			};

			//Mock
			A.CallTo(() => _userManager.FindByEmailAsync(loginModel.Email)).Returns(user);
			A.CallTo(() => _signInManager.CheckPasswordSignInAsync(user , loginModel.Password , false)).Returns(SignInResult.LockedOut);

			//Act
			var result = await service.LoginEmailAsync(loginModel);

			//Assert
			result.StatusCode.Should().Be(400);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Be("Account locked! Please contact administrator!");
		}
		[Fact]
		public async void AuthService_LoginEmailAsync_ReturnEmailNotConfirm()
		{
			//Arange
			var user = A.Fake<User>();
			user.Email = "test@gmail.com";
			user.EmailConfirmed = false;
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);

			var loginModel = new LoginModel
			{
				Email = user.Email ,
				Password = "User123!"
			};
			var token = "fake_token";

			//Mock
			A.CallTo(() => _userManager.FindByEmailAsync(loginModel.Email)).Returns(user);
			A.CallTo(() => _signInManager.CheckPasswordSignInAsync(user , loginModel.Password , false)).Returns(SignInResult.Success);
			A.CallTo(() => _userManager.GenerateEmailConfirmationTokenAsync(user)).Returns(token);

			//Act
			var result = await service.LoginEmailAsync(loginModel);

			//Assert
			result.StatusCode.Should().Be(405);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Be(Base64UrlHelper.EncodeTokenToBase64(token));
		}

		[Fact]
		public async void AuthService_LoginEmailAsync_ReturnFailLogin()
		{
			//Arange
			var user = A.Fake<User>();
			user.Email = "test@gmail.com";
			user.EmailConfirmed = true;
			user.AccessFailedCount = 3;
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);

			var loginModel = new LoginModel
			{
				Email = user.Email ,
				Password = "IncorectPassword123!"
			};

			//Mock
			A.CallTo(() => _userManager.FindByEmailAsync(loginModel.Email)).Returns(user);
			A.CallTo(() => _signInManager.CheckPasswordSignInAsync(user , loginModel.Password , false)).Returns(SignInResult.Failed);

			//Act
			var result = await service.LoginEmailAsync(loginModel);

			//Assert
			result.StatusCode.Should().Be(400);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Be("Wrong password!");

			A.CallTo(() => _userManager.UpdateAsync(user)).MustHaveHappenedOnceExactly();
		}

		[Fact]
		public async void AuthService_LoginEmailAsync_ReturnFailLogin5time()
		{
			//Arange
			var user = A.Fake<User>();
			user.Email = "test@gmail.com";
			user.EmailConfirmed = true;
			user.AccessFailedCount = 5;
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);

			var loginModel = new LoginModel
			{
				Email = user.Email ,
				Password = "IncorectPassword123!"
			};

			//Mock
			A.CallTo(() => _userManager.FindByEmailAsync(loginModel.Email)).Returns(user);
			A.CallTo(() => _signInManager.CheckPasswordSignInAsync(user , loginModel.Password , false)).Returns(SignInResult.Failed);

			//Act
			var result = await service.LoginEmailAsync(loginModel);

			//Assert
			result.StatusCode.Should().Be(400);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Be("Try agian after 10 minute!");

			A.CallTo(() => _userManager.SetLockoutEndDateAsync(user , user.LockoutEnd)).MustHaveHappenedOnceExactly();
		}

		[Fact]
		public async void AuthService_LoginEmailAsync_ReturnUserNotFound()
		{
			//Arange
			User userByEmail = null;
			User userByUsername = null;
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);

			var loginModel = new LoginModel
			{
				Email = "notExistEmail@gmail.com" ,
				Password = "IncorectPassword123!"
			};

			//Mock
			A.CallTo(() => _userManager.FindByEmailAsync(loginModel.Email)).Returns(userByEmail);
			A.CallTo(() => _userManager.FindByNameAsync(loginModel.Email)).Returns(userByUsername);

			//Act
			var result = await service.LoginEmailAsync(loginModel);

			//Assert
			result.StatusCode.Should().Be(404);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Be("User not exist!");

		}
		#endregion
		#region Register
		[Fact]
		public async void AuthService_RegisterEmailAsync_ReturnSuccess()
		{
			//Arange
			User user = null;
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);

			var registerModel = new RegisterModel
			{
				Email = "newuser@gmail.com" ,
				Password = "User123!" ,
				ConfirmPassword = "User123!" ,
				FirstName = "John" ,
				LastName = "Doe" ,
				PhoneNumber = "123456789" ,
				Gender = UserGenderHelper.FromInt(2) ,
				Dob = DateTime.Now.AddYears(-25)
			};

			//Mock
			A.CallTo(() => _userManager.FindByEmailAsync(registerModel.Email)).Returns(user);
			A.CallTo(() => _userManager.CreateAsync(A<User>.Ignored)).Returns(Task.FromResult(IdentityResult.Success));

			//Act
			var result = await service.RegisterEmailAsync(registerModel);

			//Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Be("Register successfully.");


			A.CallTo(() => _userManager.AddToRoleAsync(A<User>.Ignored , "customer")).MustHaveHappenedOnceExactly();
			A.CallTo(() => _userManager.AddPasswordAsync(A<User>.Ignored , registerModel.Password)).MustHaveHappenedOnceExactly();
		}
		[Fact]
		public async void AuthService_RegisterEmailAsync_ReturnPasswordConfirmNotMatch()
		{
			//Arange
			var user = A.Fake<User>();
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);

			var registerModel = new RegisterModel
			{
				Email = "newuser@gmail.com" ,
				Password = "User123!" ,
				ConfirmPassword = "User12345!" ,
				FirstName = "John" ,
				LastName = "Doe" ,
				PhoneNumber = "123456789" ,
				Gender = UserGenderHelper.FromInt(2) ,
				Dob = DateTime.Now.AddYears(-25)
			};

			//Mock

			//Act
			var result = await service.RegisterEmailAsync(registerModel);

			//Assert
			result.StatusCode.Should().Be(400);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Be("Password and confirm password does not match!");
		}

		[Fact]
		public async void AuthService_RegisterEmailAsync_ReturnEmailHaveExist()
		{
			//Arange
			var user = A.Fake<User>();
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);

			var registerModel = new RegisterModel
			{
				Email = "newuser@gmail.com" ,
				Password = "User123!" ,
				ConfirmPassword = "User123!" ,
				FirstName = "John" ,
				LastName = "Doe" ,
				PhoneNumber = "123456789" ,
				Gender = UserGenderHelper.FromInt(2) ,
				Dob = DateTime.Now.AddYears(-25)
			};

			//Mock
			A.CallTo(() => _userManager.FindByEmailAsync(registerModel.Email)).Returns(user);

			//Act
			var result = await service.RegisterEmailAsync(registerModel);

			//Assert
			result.StatusCode.Should().Be(403);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Be("Email have exist!");
		}

		[Fact]
		public async void AuthService_RegisterEmailAsync_ReturnFailRegister()
		{
			//Arange
			User user = null;
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);

			var registerModel = new RegisterModel
			{
				Email = "newuser@gmail.com" ,
				Password = "User123!" ,
				ConfirmPassword = "User123!" ,
				FirstName = "John" ,
				LastName = "Doe" ,
				PhoneNumber = "123456789" ,
				Gender = UserGenderHelper.FromInt(2) ,
				Dob = DateTime.Now.AddYears(-25)
			};

			//Mock
			A.CallTo(() => _userManager.FindByEmailAsync(registerModel.Email)).Returns(user);
			A.CallTo(() => _userManager.CreateAsync(A<User>.Ignored))
				.Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Failed to create user!" })));

			//Act
			var result = await service.RegisterEmailAsync(registerModel);

			//Assert
			result.StatusCode.Should().Be(500);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Be("Failed to create user!");
		}

		#endregion
		#region Reset password
		[Fact]
		public async void AuthService_ResetPasswordAsync_ReturnSuccess()
		{
			//Arange
			var user = A.Fake<User>();
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);

			var model = new ResetPasswordModelRequest
			{
				Id = user.Id ,
				OldPassword = "OldPass123!" ,
				NewPassword = "TestResetPass123!" ,
				ConfirmPassword = "TestResetPass123!" ,
			};

			//Mock
			A.CallTo(() => _userManager.FindByIdAsync(user.Id)).Returns(user);
			A.CallTo(() => _userManager.ChangePasswordAsync(user , model.OldPassword , model.NewPassword)).Returns(Task.FromResult(IdentityResult.Success));

			//Act
			var result = await service.ResetPasswordAsync(model);

			//Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Be("Password reset successfully.");
		}

		[Fact]
		public async void AuthService_ResetPasswordAsync_ReturnFailed()
		{
			//Arange
			var user = A.Fake<User>();
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);

			var model = new ResetPasswordModelRequest
			{
				Id = user.Id ,
				OldPassword = "OldPass123!" ,
				NewPassword = "TestResetPass123!" ,
				ConfirmPassword = "TestResetPass123!" ,
			};

			//Mock
			A.CallTo(() => _userManager.FindByIdAsync(user.Id)).Returns(user);
			A.CallTo(() => _userManager.ChangePasswordAsync(user , model.OldPassword , model.NewPassword)).Returns(Task.FromResult(IdentityResult.Failed()));

			//Act
			var result = await service.ResetPasswordAsync(model);

			//Assert
			result.StatusCode.Should().Be(500);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Contain("Failed to");
		}

		[Fact]
		public async void AuthService_ResetPasswordAsync_ReturnPasswordNotMatch()
		{
			//Arange
			var user = A.Fake<User>();
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);

			var model = new ResetPasswordModelRequest
			{
				Id = user.Id ,
				OldPassword = "OldPass123!" ,
				NewPassword = "TestResetPass123!" ,
				ConfirmPassword = "WrongConFirmPass123!" ,
			};

			//Mock
			A.CallTo(() => _userManager.FindByIdAsync(user.Id)).Returns(user);

			//Act
			var result = await service.ResetPasswordAsync(model);

			//Assert
			result.StatusCode.Should().Be(403);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Be("NewPassword and confirm password does not match!");
		}
		#endregion
		#region Reset Password By Email
		//[Fact]
		//public async void AuthService_ResetPasswordEmailAsync_ReturnSuccess()
		//{
		//	//Arange
		//	var user = A.Fake<User>();
		//	user.Email = "TestEmail@gmail.com";
		//	user.EmailConfirmed = true;
		//	user.Code = "Code123";
		//	var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager , _tokenGenerator);
		//	var model = new ResetPasswordByEmailModelRequest
		//	{
		//		Email = user.Email ,
		//		NewPassword = "NewPass123!" ,
		//		ConfirmPassword = "NewPass123!" ,
		//		CodeReset = "Code123"
		//	};
		//	var token = "token";

		//	//Mock
		//	A.CallTo(() => _userManager.FindByEmailAsync(user.Email)).Returns(user);
		//	A.CallTo(() => _userManager.GeneratePasswordResetTokenAsync(user)).Returns(token);
		//	A.CallTo(() => _userManager.ResetPasswordAsync(user , token , model.NewPassword)).Returns(IdentityResult.Success);

		//	//Act
		//	var result = await service.ResetPasswordEmailAsync(model);

		//	//Assert
		//	result.StatusCode.Should().Be(200);
		//	result.Message.Should().NotBeNullOrEmpty();
		//	result.Message.Should().Be("Password reset successfully.");
		//}

		[Fact]
		public async void AuthService_ResetPasswordEmailAsync_ReturnFailed()
		{
			//Arange
			var user = A.Fake<User>();
			user.Email = "TestEmail@gmail.com";
			user.EmailConfirmed = true;
			user.Code = "Code123";
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);
			var model = new ResetPasswordByEmailModelRequest
			{
				Email = user.Email ,
				NewPassword = "NewPass123!" ,
				ConfirmPassword = "NewPass123!" ,
				CodeReset = "Code123"
			};
			var token = "token";

			//Mock
			A.CallTo(() => _userManager.FindByEmailAsync(user.Email)).Returns(user);
			A.CallTo(() => _userManager.GeneratePasswordResetTokenAsync(user)).Returns(token);
			A.CallTo(() => _userManager.ResetPasswordAsync(user , token , model.NewPassword)).Returns(IdentityResult.Failed());

			//Act
			var result = await service.ResetPasswordEmailAsync(model);

			//Assert
			result.StatusCode.Should().Be(500);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Contain("Failed to reset");
		}

		[Fact]
		public async void AuthService_ResetPasswordEmailAsync_ReturnNotEmailConfirm()
		{
			//Arange
			var user = A.Fake<User>();
			user.Email = "TestEmail@gmail.com";
			user.EmailConfirmed = false;
			user.Code = "Code123";
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);
			var model = new ResetPasswordByEmailModelRequest
			{
				Email = user.Email ,
				NewPassword = "NewPass123!" ,
				ConfirmPassword = "NewPass123!" ,
				CodeReset = "Code123"
			};

			//Mock
			A.CallTo(() => _userManager.FindByEmailAsync(user.Email)).Returns(user);

			//Act
			var result = await service.ResetPasswordEmailAsync(model);

			//Assert
			result.StatusCode.Should().Be(405);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Be("Email not confirm!");
		}
		[Fact]
		public async void AuthService_ResetPasswordEmailAsync_ReturnConfirmPassNotMatch()
		{
			//Arange
			var user = A.Fake<User>();
			user.Email = "TestEmail@gmail.com";
			user.EmailConfirmed = true;
			user.Code = "Code123";
			var service = new AuthService(_userManager , _roleManager , _configuration , _signInManager);
			var model = new ResetPasswordByEmailModelRequest
			{
				Email = user.Email ,
				NewPassword = "NewPass123!" ,
				ConfirmPassword = "Pass123!" ,
				CodeReset = "Code123"
			};

			//Mock
			A.CallTo(() => _userManager.FindByEmailAsync(user.Email)).Returns(user);

			//Act
			var result = await service.ResetPasswordEmailAsync(model);

			//Assert
			result.StatusCode.Should().Be(400);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Contain("NewPassword and confirm password does not match!");
		}
		#endregion
	}
}
