using AccountsTransactions_BusinessObjects.ResponseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;

namespace AccountsTransactions_BusinessObjects.Services.Interface
{
	public interface IUserService
	{
		Task<ResponseObject<AllUserResponseModel>> GetAllUsers();
		Task<ResponseObject<UserModelResponse?>> GetUserByIdAsync(string id);
		Task<ResponseObject<UserModelResponse?>> GetUserAsync(string email);
		Task<ResponseObject<UserModelResponse>> GetProfileByTokenAsync(string token);
		Task<ResponseObject<BaseUserModelResponse>> CreateUserAsync(CreateUserModelRequest model);
		Task<ResponseObject<BaseUserModelResponse>> UpdateUserAsync(UpdateUserModelRequest model);
		Task<ResponseObject<BaseUserModelResponse>> DeleteUserAsync(IdUserModelRequest model);
		Task<ResponseObject<BaseUserModelResponse>> AddRoleAsync(AddRoleUserModelRequest model);
		Task<ResponseObject<BaseUserModelResponse>> ChangeRoleAsync(ChangeRoleUserModelRequest model);
		Task<ResponseObject<BaseUserModelResponse>> ChangeStatusAsync(IdUserModelRequest model);
		Task<ResponseObject<BaseUserModelResponse>> ChangeEmailConfirmAsync(IdUserModelRequest model);

	}
}
