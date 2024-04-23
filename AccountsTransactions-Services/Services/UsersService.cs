using AccountAndTransaction_Service;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using AccountsTransactions_BusinessObjects.Services.Interface;
using AccountsTransactions_DataAccess.Enums;
using Grpc.Core;

namespace AccountsTransactions_Services.Services
{ 
    public class UsersService : user.userBase
    {
		private readonly IUserService _userService;
		public UsersService(IUserService userService)
        {
            _userService = userService;
        }
		public override async Task<AllUserResponse> AllUser(AllUserRequest request, ServerCallContext context)
		{
			var result = await _userService.GetAllUsers();
			var users = new List<Users>();
			if (result.List != null)
				foreach (var user in result.List)
				{
					var u = new Users
					{
						FirstName = user.FirstName ?? "",
						LastName = user.LastName ?? "",
						DateOfBirth = user.DateOfBirth.ToString() ?? "",
						Phone = user.PhoneNumber ?? "",
						Email = user.Email ?? "",
						UserName = user.UserName ?? "",
						Gender = user.Gender.ToString(),
						Address = user.Address ?? "",
						Role = user.Role ?? "",
						Status = user.LockoutEnd != null ? "UnActive" : "Active",
						Id = user.ID.ToString(),
						AccessFailedCount = user.AccessFailedCount,
						EmailComfirm = user.EmailConfirmed.ToString(),
					};
					users.Add(u);
				}
			return await Task.FromResult(new AllUserResponse
			{
				StatusCode = result.StatusCode,
				Message = result.Message,
				Users = { users }
			});
		}
		public override async Task<GetUserResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
		{
			Guid userId;
			if (Guid.TryParse(request.Id, out userId))
			{
				var result = await _userService.GetUserByIdAsync(userId.ToString());
				if(result.Data != null)
				{
					var u = new User
					{
						Id = result.Data.ID.ToString(),
						Email = result.Data.Email ?? "",
						UserName = result.Data.UserName ?? "",
						Gender = result.Data.Gender.ToString(),
						Address = result.Data.Address ?? "",
						DateOfBirth = result.Data.DateOfBirth.ToString(),
						FirstName = result.Data.FirstName ?? "",
						LastName = result.Data.LastName ?? "",
						Phone = result.Data.PhoneNumber ?? "",
						Role = result.Data.Role ?? ""
					};
					return await Task.FromResult(new GetUserResponse
					{
						StatusCode = result.StatusCode,
						Message = result.Message,
						User = u
					});
				}
				else
				{
					return await Task.FromResult(new GetUserResponse
					{
						StatusCode = result.StatusCode,
						Message = result.Message
					});
				}
			}
			else
			{
				return new GetUserResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
		public override async Task<GetUserResponse> GetUserProfileByToken(GetUserProfileByTokenRequest request, ServerCallContext context)
		{
			var result = await _userService.GetProfileByTokenAsync(request.Token);
			if (result != null)
			{
				if (result.Data != null)
				{
					var userProfile = new User
					{
						Id = result.Data.ID.ToString(),
						FirstName = result.Data.FirstName.ToString(),
						LastName = result.Data.LastName.ToString(),
						Email = result.Data.Email.ToString(),
						Phone = result.Data.PhoneNumber.ToString(),
						Address = result.Data.Address.ToString(),
						DateOfBirth = result.Data.DateOfBirth.ToString(),
						Role = result.Data.Role.ToString(),
						Gender = result.Data.Gender.ToString(),
						UserName = result.Data.UserName.ToString(),
					};
					return await Task.FromResult(new GetUserResponse
					{
						StatusCode = result.StatusCode,
						Message = result.Message,
						User = userProfile
					});
				}
				else
				{
					return new GetUserResponse
					{
						StatusCode = 400,
						Message = "Data is null!"
					};
				}
			}
			return new GetUserResponse
			{
				StatusCode = 500,
				Message = "Error call GetProfileByTokenAsync!"
			};
		}
		public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
		{
			var result = await _userService.GetUserAsync(request.EmailOrUserName);
			if (result.Data != null)
			{
				var u = new User
				{
					Id = result.Data.ID.ToString(),
					Email = result.Data.Email ?? "",
					UserName = result.Data.UserName ?? "",
					Gender = result.Data.Gender.ToString(),
					Address = result.Data.Address ?? "",
					DateOfBirth = result.Data.DateOfBirth.ToString(),
					FirstName = result.Data.FirstName ?? "",
					LastName = result.Data.LastName ?? "",
					Phone = result.Data.PhoneNumber ?? "",
					Role = result.Data.Role ?? ""
				};
				return await Task.FromResult(new GetUserResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message,
					User = u
				});
			}
			else
			{
				return await Task.FromResult(new GetUserResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				});
			}
		}
		public override async Task<BaseUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
		{
			var newUser = new CreateUserModelRequest
			{
				Email = request.Email ?? "",
				FirstName = request.FirstName ?? "",
				LastName = request.LastName ?? "",
				Phone = request.Phone ?? "",
				Gender = UserGenderHelper.FromInt(request.Gender),
				Role = request.Role.ToUpper() ?? "",
				Address = request.Address ?? ""

			};
			if (!string.IsNullOrEmpty(request.DateOfBirth))
			{
				if (DateTime.TryParse(request.DateOfBirth, out DateTime dateOfBirth))
				{
					newUser.DateOfBirth = dateOfBirth;
				}
				else
				{
					newUser.DateOfBirth = null;
				}
			}
			var result = await _userService.CreateUserAsync(newUser);
			return await Task.FromResult(new BaseUserResponse
			{
				StatusCode = result.StatusCode,
				Message = result.Message
			});
		}
		public override async Task<BaseUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
		{
			Guid userId;
			if (Guid.TryParse(request.Id, out userId))
			{
				var updateModel = new UpdateUserModelRequest
				{
					Id = userId.ToString(),
					FirstName = request.FirstName ?? "",
					LastName = request.LastName ?? "",
					Email = request.Email ?? "",
					UserName = request.UserName ?? "",
					Gender = UserGenderHelper.FromInt(request.Gender),
					Address = request.Address ?? "",
					Phone = request.Phone ?? ""
				};
				if (!string.IsNullOrEmpty(request.DateOfBirth))
				{
					if (DateTime.TryParse(request.DateOfBirth, out DateTime dateOfBirth))
					{
						updateModel.DateOfBirth = dateOfBirth;
					}
					else
					{
						updateModel.DateOfBirth = null;
					}
				}
				var result = await _userService.UpdateUserAsync(updateModel);
				return new BaseUserResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				};
			}
			else
			{
				return new BaseUserResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
		public override async Task<BaseUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
		{
			Guid userId;
			if (Guid.TryParse(request.Id, out userId))
			{
				var deleteModel = new IdUserModelRequest
				{
					Id = userId.ToString(),
				};
				var result = await _userService.DeleteUserAsync(deleteModel);
				return new BaseUserResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				};
			}
			else
			{
				return new BaseUserResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
		public override async Task<BaseUserResponse> ChangeRoleUser(ChangeRoleRequest request, ServerCallContext context)
		{
			Guid userId;
			if (Guid.TryParse(request.Id, out userId))
			{
				var model = new ChangeRoleUserModelRequest
				{
					Id = userId.ToString(),
					OldRole = request.OldRole,
					NewRole = request.NewRole,
				};
				var result = await _userService.ChangeRoleAsync(model);
				return await Task.FromResult(new BaseUserResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				});
			}
			else
			{
				return new BaseUserResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
		public override async Task<BaseUserResponse> AddRoleUser(AddRoleRequest request, ServerCallContext context)
		{
			Guid userId;
			if (Guid.TryParse(request.Id, out userId))
			{
				var model = new AddRoleUserModelRequest
				{
					Id = userId.ToString(),
					Role = request.Role
				};
				var result = await _userService.AddRoleAsync(model);
				return await Task.FromResult(new BaseUserResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				});
			}
			else
			{
				return new BaseUserResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
		public override async Task<BaseUserResponse> ChangeStatusUser(IdRequest request, ServerCallContext context)
		{
			Guid userId;
			if (Guid.TryParse(request.Id, out userId))
			{
				var model = new IdUserModelRequest
				{
					Id = userId.ToString()
				};
				var result = await _userService.ChangeStatusAsync(model);
				return await Task.FromResult(new BaseUserResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				});
			}
			else
			{
				return new BaseUserResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
		public override async Task<BaseUserResponse> ChangeEmailConfirmUser(IdRequest request, ServerCallContext context)
		{
			Guid userId;
			if (Guid.TryParse(request.Id, out userId))
			{
				var model = new IdUserModelRequest
				{
					Id = userId.ToString()
				};
				var result = await _userService.ChangeEmailConfirmAsync(model);
				return await Task.FromResult(new BaseUserResponse
				{
					StatusCode = result.StatusCode,
					Message = result.Message
				});
			}
			else
			{
				return new BaseUserResponse
				{
					StatusCode = 400,
					Message = "Invalid Id format!"
				};
			}
		}
	}
}
