using AccountAndTransaction_Service;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using AccountsTransactions_BusinessObjects.Services.Interface;
using AccountsTransactions_DataAccess.Enums;
using Google.Rpc;
using Grpc.Core;

namespace AccountsTransactions_Services.Services
{
	public class AuthsService : auth.authBase
	{
        private readonly IAuthService _authService;
        private readonly ISendMailService _sendMailService;
		public AuthsService(IAuthService authService, ISendMailService sendMailService)
        {
            _authService = authService;
			_sendMailService = sendMailService;
        }

		public override async Task<BaseAuthResponse> Login(LoginRequest request, ServerCallContext context)
		{
			var model = new LoginModel
			{
				Email = request.Email,
				Password = request.Password
			};
			var result = await _authService.LoginEmailAsync(model);
			if (result != null)
			{
				if (result.StatusCode == 405)
				{
					var callbackUrl = $"https://localhost:7135/v1/auths/confirm-mail?token={result.Message}&email={model.Email}";
					_sendMailService.SendMail(model.Email, "Confirm Mail", "Please click to confirm email at WeMealKit: <a href=\"" + callbackUrl + "\">here</a>");
					return new BaseAuthResponse
					{
						StatusCode = result.StatusCode,
						Message = "Please check mail to confirm!"
					};
				}
				return await Task.FromResult(new BaseAuthResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				});
			}
			else
			{
				return new BaseAuthResponse
				{
					StatusCode = 500,
					Message = "Internal server error!"
				};
			}
		}
		public override async Task<BaseAuthResponse> ConfirmMail(ConfirmMailRequest request, ServerCallContext context)
		{
			var model = new ConfirmMailModel
			{
				Email = request.Email,
				Token = request.Token
			};
			var result = await _authService.ConfirmEmailAsync(model);
			return new BaseAuthResponse
			{
				StatusCode = result.StatusCode,
				Message = result.Message
			};
		}
		public override async Task<BaseAuthResponse> Register(RegisterRequest request, ServerCallContext context)
		{
			var newUser = new RegisterModel
			{
				Email = request.Email,
				Password = request.Password,
				ConfirmPassword = request.ConfirmPassword,
				FirstName = request.FirstName,
				LastName = request.LastName,
				Gender = UserGenderHelper.FromInt(request.Gender),
				PhoneNumber = request.Phone
			};
			if (!string.IsNullOrEmpty(request.Dob))
			{
				if (DateTime.TryParse(request.Dob, out DateTime dateOfBirth))
				{
					newUser.Dob = dateOfBirth;
				}
				else
				{
					newUser.Dob = null;
					return await Task.FromResult(new BaseAuthResponse
					{
						StatusCode = 400,
						Message = "Wrong datetime format!"
					});
				}
			}
			var result = await _authService.RegisterEmailAsync(newUser);
			return await Task.FromResult(new BaseAuthResponse
			{
				StatusCode = result.StatusCode,
				Message = result.Message
			});
		}
		public override async Task<BaseAuthResponse> ResetPassword(ResetPasswordRequest request, ServerCallContext context)
		{
			Guid userId;
			if (Guid.TryParse(request.Id, out userId))
			{
				var model = new ResetPasswordModelRequest
				{
					Id = request.Id,
					OldPassword = request.OldPassword,
					NewPassword = request.NewPassword,
					ConfirmPassword = request.ConfirmPassword
				};
				var result = await _authService.ResetPasswordAsync(model);
				return await Task.FromResult(new BaseAuthResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				});
			}
			else
			{
				return new BaseAuthResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
		public override async Task<BaseAuthResponse> ResetPasswordByEmail(ResetPasswordByEmailRequest request, ServerCallContext context)
		{
			var model = new ResetPasswordByEmailModelRequest
			{
				Email = request.Email,
				CodeReset = request.CodeReset,
				NewPassword = request.NewPassword,
				ConfirmPassword = request.ConfirmPassword
			};
			var result = await _authService.ResetPasswordEmailAsync(model);
			return await Task.FromResult(new BaseAuthResponse
			{
				StatusCode = result.StatusCode,
				Message = result.Message
			});
		}
		public override async Task<BaseAuthResponse> SendCodeResetPassword(SendCodeResetPasswordRequest request, ServerCallContext context)
		{
			var model = new SendCodeByEmailModelRequest
			{
				Email = request.Email
			};
			var result = await _authService.SendCodeResetEmailAsync(model);
			if (result != null)
			{
				_sendMailService.SendMail(model.Email, "Reset Code", "This code to reset password your account WeMealKit: " + result.Message + " (Code valid after 2 minutes)");
				return await Task.FromResult(new BaseAuthResponse
				{
					StatusCode = result.StatusCode,
					Message = "Please check mail to take code to reset password"
				});
			}
			return new BaseAuthResponse
			{
				StatusCode = 400,
				Message = "Send Failed!"
			};
		}
	}
}
