using AccountsTransactions_DataAccess.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;

namespace AccountsTransactions_BusinessObjects.Mappers
{
	public class UserProfile : Profile
	{
		public UserProfile()
		{
			CreateMap<User, AllUserResponseModel>().ReverseMap();
			CreateMap<User, UserModelResponse>().ReverseMap();
			CreateMap<User, CreateUserModelRequest>().ReverseMap();
		}
	}
}
