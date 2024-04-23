using AccountsTransactions_BusinessObjects.ResponseObjects;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.Services.Interface
{
	public interface IAuthService
	{
		Task<ResponseObject<UserModelResponse>> LoginEmailAsync(LoginModel model);
		Task<ResponseObject<UserModelResponse>> RegisterEmailAsync(RegisterModel model);
		Task<ResponseObject<BaseUserModelResponse>> ConfirmEmailAsync(ConfirmMailModel model);
		Task<ResponseObject<BaseUserModelResponse>> ResetPasswordAsync(ResetPasswordModelRequest model);
		Task<ResponseObject<BaseUserModelResponse>> ResetPasswordEmailAsync(ResetPasswordByEmailModelRequest model);
		Task<ResponseObject<BaseUserModelResponse>> SendCodeResetEmailAsync(SendCodeByEmailModelRequest model);



	}
}
