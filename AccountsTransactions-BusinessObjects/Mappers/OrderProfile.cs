using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels;
using AccountsTransactions_DataAccess.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_BusinessObjects.Mappers
{
	public class OrderProfile : Profile
	{
        public OrderProfile()
        {
            CreateMap<Order, OrderModelResponse>().ReverseMap();
            CreateMap<Order, CreateOrderModelRequest>().ReverseMap(); ;
		}
    }
}
