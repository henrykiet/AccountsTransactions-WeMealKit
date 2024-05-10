using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels;
using AccountsTransactions_BusinessObjects.ServiceModels.ResponseModels;
using AccountsTransactions_BusinessObjects.ServiceModels.ValidateModels;
using AccountsTransactions_BusinessObjects.Services.Implement;
using AccountsTransactions_DataAccess.Enums;
using AccountsTransactions_DataAccess.Models;
using AccountsTransactions_DataAccess.Repository.Interface;
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountsTransactions_Test.Services
{
	public class OrderServiceTest
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IValidator<CreateOrderModelValidator> _createOrderValidator;
		private readonly IValidator<UpdateOrderModelValidator> _updateOrderValidator;
		private readonly IValidator<DeleteOrderModelValidator> _deleteOrderValidator;
		private readonly IValidator<ChangeOderStatusModelValidator> _changeStatusOrderValidator;
		private readonly IValidator<ChangeOderStatusDeliveredModelValidator> _changeStatusOrderDeliveredValidator;

		public OrderServiceTest()
		{
			_mapper = A.Fake<IMapper>();
			_unitOfWork = A.Fake<IUnitOfWork>();
			//Validate
			_createOrderValidator = A.Fake<IValidator<CreateOrderModelValidator>>();
			_updateOrderValidator = A.Fake<IValidator<UpdateOrderModelValidator>>();
			_deleteOrderValidator = A.Fake<IValidator<DeleteOrderModelValidator>>();
			_changeStatusOrderValidator = A.Fake<IValidator<ChangeOderStatusModelValidator>>();
			_changeStatusOrderDeliveredValidator = A.Fake<IValidator<ChangeOderStatusDeliveredModelValidator>>();
		}

		#region GetOrderByIdAsync
		[Fact]
		public async Task OrderService_GetOrderByIdAsync_ReturnSuccess()
		{
			//Arrange
			var order = A.Fake<Order>();
			var id = Guid.NewGuid().ToString();
			var orderMap = new OrderModelResponse();
			var service = new OrderService(_unitOfWork , _mapper);

			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(id)).Returns(order);
			A.CallTo(() => _mapper.Map<OrderModelResponse>(order)).Returns(orderMap);

			//Act
			var result = await service.GetOrderByIdAsync(id);

			//Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().Be("Order:");
			result.Data.Should().Be(orderMap);
		}
		[Fact]
		public async Task OrderService_GetOrderByIdAsync_ReturnNotFound()
		{
			//Arrange
			Order? orderNull = null;
			var id = Guid.NewGuid().ToString();
			var service = new OrderService(_unitOfWork , _mapper);

			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(id)).Returns(orderNull);

			//Act
			var result = await service.GetOrderByIdAsync(id);

			//Assert
			result.StatusCode.Should().Be(404);
			result.Message.Should().Be("Order not found!");
			result.Data.Should().Be(orderNull);
		}


		#endregion
		#region GetAllOrders
		[Fact]
		public async Task OrderService_GetAllOrders_ReturnSuccess()
		{
			//Arrange
			var orders = new List<Order>()
			{
				new Order
				{
					Id = Guid.NewGuid(),
					UserId = "user123",
					WeekPlanId = Guid.NewGuid(),
					Note = "Please deliver before 5 PM.",
					ShipDate = DateTime.UtcNow.AddDays(2),
					OrderDate = DateTime.UtcNow,
					TotalPrice = 99.99,
					Status = OrderStatus.Processing,
					User = new User { Id = "user123", }
				},
				new Order
				{
					Id = Guid.NewGuid(),
					UserId = "user123",
					WeekPlanId = Guid.NewGuid(),
					Note = "Please deliver before 5 PM.",
					ShipDate = DateTime.UtcNow.AddDays(2),
					OrderDate = DateTime.UtcNow,
					TotalPrice = 99.99,
					Status = OrderStatus.Processing,
					User = new User { Id = "user123", }
				}
			};
			var orderMap = new List<OrderModelResponse>();
			var service = new OrderService(_unitOfWork , _mapper);

			A.CallTo(() => _unitOfWork.OrderRepository.GetAllAsync()).Returns(orders);
			A.CallTo(() => _mapper.Map<List<OrderModelResponse>>(orders)).Returns(orderMap);

			//Act
			var result = await service.GetAllOrders();

			//Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().Be("Order List");
			result.List.Should().NotBeNull();
		}
		[Fact]
		public async Task OrderService_GetAllOrders_ReturnDontHaveOrder()
		{
			//Arrange
			List<Order> ordersNull = new List<Order>(); 
			var service = new OrderService(_unitOfWork , _mapper);

			A.CallTo(() => _unitOfWork.OrderRepository.GetAllAsync()).Returns(ordersNull);

			//Act
			var result = await service.GetAllOrders();

			//Assert
			result.StatusCode.Should().Be(404);
			result.Message.Should().Be("Don't have Order!");
			result.List.Should().BeNullOrEmpty();
		}
		#endregion
		#region CreateOrderAsync
		[Fact]
		public async Task OrderService_CreateOrderAsync_ReturnSuccess()
		{
			//Arrange
			var userId = Guid.NewGuid().ToString();
			var user = A.Fake<User>();
			user.Orders = new List<Order>();
			A.CallTo(() => user.Id).Returns(userId);//fake user with userId
			var model = new CreateOrderModelRequest
			{
				UserId = userId ,
				WeekPlanId = Guid.NewGuid() ,
				Note = "Urgent delivery requested." ,
				ShipDate = DateTime.UtcNow.AddDays(1) ,
				TotalPrice = 129.99
			};

			var modelMap = new Order();
			var service = new OrderService(_unitOfWork , _mapper);

			//Mock
			A.CallTo(() => _unitOfWork.UserRepository.GetByIdAsync(userId)).Returns(user);
			A.CallTo(() => _mapper.Map<Order>(model)).Returns(modelMap);
			A.CallTo(() => _unitOfWork.OrderRepository.CreateAsync(modelMap)).Returns(true);

			//Act
			var result = await service.CreateOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().StartWith("Created order of user");
			result.Message.Should().Contain("successfully");
			user.Orders.Should().NotBeNull();
			user.Orders.Should().Contain(modelMap);
			A.CallTo(() => _unitOfWork.CompleteAsync()).MustHaveHappenedOnceExactly();
		}
		[Fact]
		public async Task OrderService_CreateOrderAsync_ReturnFailValidator()
		{
			//Arrange
			var userId = Guid.NewGuid().ToString();
			var model = new CreateOrderModelRequest
			{
				UserId = userId ,
				WeekPlanId = Guid.NewGuid() ,
				Note = "Urgent delivery requested." ,
				ShipDate = DateTime.UtcNow.AddDays(1) ,
				TotalPrice = 0 //error
			};

			var service = new OrderService(_unitOfWork , _mapper);

			//Mock

			//Act
			var result = await service.CreateOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(400);
			result.Message.Should().NotBeNullOrEmpty();
			result.Message.Should().Contain("TotalPrice");
		}
		[Fact]
		public async Task OrderService_CreateOrderAsync_ReturnServerError()
		{
			//Arrange
			var userId = Guid.NewGuid().ToString();
			var user = A.Fake<User>();
			user.Orders = new List<Order>();
			A.CallTo(() => user.Id).Returns(userId);//fake user with userId
			var model = new CreateOrderModelRequest
			{
				UserId = userId ,
				WeekPlanId = Guid.NewGuid() ,
				Note = "Urgent delivery requested." ,
				ShipDate = DateTime.UtcNow.AddDays(1) ,
				TotalPrice = 129.99
			};

			var modelMap = new Order();
			var service = new OrderService(_unitOfWork , _mapper);

			//Mock
			A.CallTo(() => _unitOfWork.UserRepository.GetByIdAsync(userId)).Returns(user);
			A.CallTo(() => _mapper.Map<Order>(model)).Returns(modelMap);
			A.CallTo(() => _unitOfWork.OrderRepository.CreateAsync(modelMap)).Returns(false);

			//Act
			var result = await service.CreateOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(500);
			result.Message.Should().Be("Create order unsuccessfully!");
		}
		[Fact]
		public async Task OrderService_CreateOrderAsync_ReturnUserNotFound()
		{
			//Arrange
			var userId = Guid.NewGuid().ToString();
			User? userNull = null;
			var model = new CreateOrderModelRequest
			{
				UserId = userId ,
				WeekPlanId = Guid.NewGuid() ,
				Note = "Urgent delivery requested." ,
				ShipDate = DateTime.UtcNow.AddDays(1) ,
				TotalPrice = 129.99
			};

			var service = new OrderService(_unitOfWork , _mapper);

			//Mock
			A.CallTo(() => _unitOfWork.UserRepository.GetByIdAsync(userId)).Returns(userNull);

			//Act
			var result = await service.CreateOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(404);
			result.Message.Should().Be("User not found!");
		}
		#endregion
		#region UpdateOrderAsync
		[Fact]
		public async Task OrderService_UpdateOrderAsync_ReturnSuccess()
		{
			//Arrange
			var order = A.Fake<Order>();
			var user = A.Fake<User>();
			order.User = user;
			var model = new UpdateOrderModelRequest
			{
				Id = Guid.NewGuid() ,
				Note = "Updated note for the order" , 
				ShipDate = DateTime.UtcNow.AddDays(3) ,
				TotalPrice = 149.99 
			};

			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);
			A.CallTo(() => _unitOfWork.OrderRepository.UpdateAsync(order)).Returns(true);

			//Act
			var result = await service.UpdateOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().StartWith("Updated order of user");
			result.Message.Should().Contain("successfully");
			A.CallTo(() => _unitOfWork.CompleteAsync()).MustHaveHappenedOnceExactly();
		}
		[Fact]
		public async Task OrderService_UpdateOrderAsync_ReturnServerError()
		{
			//Arrange
			var order = A.Fake<Order>();
			var user = A.Fake<User>();
			order.User = user;
			var model = new UpdateOrderModelRequest
			{
				Id = Guid.NewGuid() ,
				Note = "Updated note for the order" , 
				ShipDate = DateTime.UtcNow.AddDays(3) ,
				TotalPrice = 149.99 
			};

			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);
			A.CallTo(() => _unitOfWork.OrderRepository.UpdateAsync(order)).Returns(false);

			//Act
			var result = await service.UpdateOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(500);
			result.Message.Should().Be("Update order unsuccessfully!");
		}
		[Fact]
		public async Task OrderService_UpdateOrderAsync_ReturnFailValidator()
		{
			//Arrange
			var order = A.Fake<Order>();
			var model = new UpdateOrderModelRequest
			{
				Id = Guid.NewGuid() ,
				Note = "Updated note for the order" , 
				ShipDate = DateTime.UtcNow.AddDays(3) ,
				TotalPrice = 0 // error
			};

			var service = new OrderService(_unitOfWork , _mapper);

			//Act
			var result = await service.UpdateOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(400);
			result.Message.Should().Contain("TotalPrice");
		}
		[Fact]
		public async Task OrderService_UpdateOrderAsync_ReturnOrderNotFound()
		{
			//Arrange
			Order? order = null;
			var model = new UpdateOrderModelRequest
			{
				Id = Guid.NewGuid() ,
				Note = "Updated note for the order" , 
				ShipDate = DateTime.UtcNow.AddDays(3) ,
				TotalPrice = 149.99 
			};

			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);

			//Act
			var result = await service.UpdateOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(404);
			result.Message.Should().Be("Order not found!");
		}
		#endregion
		#region DeleteOrderAsync
		[Fact]
		public async Task OrderService_DeleteOrderAsync_ReturnSuccessDelete()
		{
			//Arrange
			var model = new DeleteOrderModelRequest
			{
				Id = Guid.NewGuid(),
			};
			var order = A.Fake<Order>();
			var user = A.Fake<User>();
			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);
			A.CallTo(() => _unitOfWork.UserRepository.GetOrderExistInUserAsync(order.UserId)).Returns(false);
			A.CallTo(() => _unitOfWork.OrderRepository.DeleteAsync(order.Id.ToString())).Returns(true);

			//Act
			var result = await service.DeleteOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().Be("Delete order successfully.");
			A.CallTo(() => _unitOfWork.CompleteAsync()).MustHaveHappenedOnceExactly();
		}
		[Fact]
		public async Task OrderService_DeleteOrderAsync_ReturnSuccessChangeStatus()
		{
			//Arrange
			var model = new DeleteOrderModelRequest
			{
				Id = Guid.NewGuid(),
			};
			var order = A.Fake<Order>();
			var user = A.Fake<User>();
			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);
			A.CallTo(() => _unitOfWork.UserRepository.GetOrderExistInUserAsync(order.UserId)).Returns(true);
			A.CallTo(() => _unitOfWork.OrderRepository.UpdateAsync(order)).Returns(true);

			//Act
			var result = await service.DeleteOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().Be("Trust change order status into 'canceled' successfully.");
			A.CallTo(() => _unitOfWork.CompleteAsync()).MustHaveHappenedOnceExactly();
		}
		[Fact]
		public async Task OrderService_DeleteOrderAsync_ReturnFailDelete()
		{
			//Arrange
			var model = new DeleteOrderModelRequest
			{
				Id = Guid.NewGuid(),
			};
			var order = A.Fake<Order>();
			var user = A.Fake<User>();
			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);
			A.CallTo(() => _unitOfWork.UserRepository.GetOrderExistInUserAsync(order.UserId)).Returns(false);
			A.CallTo(() => _unitOfWork.OrderRepository.DeleteAsync(order.Id.ToString())).Returns(false);

			//Act
			var result = await service.DeleteOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(500);
			result.Message.Should().Be("Delete order unsuccessfully!");
		}
		[Fact]
		public async Task OrderService_DeleteOrderAsync_ReturnFailChangeStatus()
		{
			//Arrange
			var model = new DeleteOrderModelRequest
			{
				Id = Guid.NewGuid(),
			};
			var order = A.Fake<Order>();
			var user = A.Fake<User>();
			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);
			A.CallTo(() => _unitOfWork.UserRepository.GetOrderExistInUserAsync(order.UserId)).Returns(true);
			A.CallTo(() => _unitOfWork.OrderRepository.UpdateAsync(order)).Returns(false);

			//Act
			var result = await service.DeleteOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(500);
			result.Message.Should().Be("Change order status unsuccessfully!");
		}
		[Fact]
		public async Task OrderService_DeleteOrderAsync_ReturnOrderNotFound()
		{
			//Arrange
			var model = new DeleteOrderModelRequest
			{
				Id = Guid.NewGuid(),
			};
			Order? order = null;
			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);

			//Act
			var result = await service.DeleteOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(404);
			result.Message.Should().Be("Order not found!");
		}
		[Fact]
		public async Task OrderService_DeleteOrderAsync_ReturnFailVaildator()
		{
			//Arrange
			var model = new DeleteOrderModelRequest
			{
				Id = Guid.Empty,
			};
			var service = new OrderService(_unitOfWork , _mapper);

			//Act
			var result = await service.DeleteOrderAsync(model);

			//Assert
			result.StatusCode.Should().Be(400);
			result.Message.Should().Be("ID is required!");
		}
		#endregion
		#region ChangeOrderStatusAsync
		[Fact]
		public async Task OrderService_ChangeOrderStatusAsync_ReturnSuccess()
		{
			//Arrange
			var model = new ChangeOderStatusModelRequest
			{
				Id = Guid.NewGuid(),
				Status = OrderStatus.Shipping
			};
			var order = A.Fake<Order>();
			var service = new OrderService(_unitOfWork , _mapper);

			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);
			A.CallTo(() => _unitOfWork.OrderRepository.ChangeStatusAsync(order.Id, model.Status)).Returns(true);

			//Act
			var result = await service.ChangeOrderStatusAsync(model);

			//Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().StartWith("Changed order status");
			result.Message.Should().Contain("successfully");
			A.CallTo(() => _unitOfWork.CompleteAsync()).MustHaveHappenedOnceExactly();
		}
		[Fact]
		public async Task OrderService_ChangeOrderStatusAsync_ReturnServerError()
		{
			//Arrange
			var model = new ChangeOderStatusModelRequest
			{
				Id = Guid.NewGuid(),
				Status = OrderStatus.Shipping
			};
			var order = A.Fake<Order>();
			var service = new OrderService(_unitOfWork , _mapper);

			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);
			A.CallTo(() => _unitOfWork.OrderRepository.ChangeStatusAsync(order.Id, model.Status)).Returns(false);

			//Act
			var result = await service.ChangeOrderStatusAsync(model);

			//Assert
			result.StatusCode.Should().Be(500);
			result.Message.Should().Be("Change order status unsuccessfully!");
		}
		[Fact]
		public async Task OrderService_ChangeOrderStatusAsync_ReturnOrderNotFound()
		{
			//Arrange
			var model = new ChangeOderStatusModelRequest
			{
				Id = Guid.NewGuid(),
				Status = OrderStatus.Shipping
			};
			Order? order = null;
			var service = new OrderService(_unitOfWork , _mapper);

			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);

			//Act
			var result = await service.ChangeOrderStatusAsync(model);

			//Assert
			result.StatusCode.Should().Be(404);
			result.Message.Should().Be("Order not found!");
		}
		[Fact]
		public async Task OrderService_ChangeOrderStatusAsync_ReturnOrderIsDelivered()
		{
			//Arrange
			var model = new ChangeOderStatusModelRequest
			{
				Id = Guid.NewGuid(),
				Status = OrderStatus.Shipping
			};
			Order order = A.Fake<Order>();
			order.Status = OrderStatus.Delivered;
			var service = new OrderService(_unitOfWork , _mapper);

			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);

			//Act
			var result = await service.ChangeOrderStatusAsync(model);

			//Assert
			result.StatusCode.Should().Be(402);
			result.Message.Should().Be("Order has been received! Can't change order status");
		}
		[Fact]
		public async Task OrderService_ChangeOrderStatusAsync_ReturnFailValidator()
		{
			//Arrange
			var model = new ChangeOderStatusModelRequest
			{
				Id = Guid.Empty, //Error
				Status = OrderStatus.Shipping
			};
			Order order = A.Fake<Order>();
			order.Status = OrderStatus.Delivered;
			var service = new OrderService(_unitOfWork , _mapper);

			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);

			//Act
			var result = await service.ChangeOrderStatusAsync(model);

			//Assert
			result.StatusCode.Should().Be(400);
			result.Message.Should().Contain("ID");
		}
		#endregion
		#region ChangeOrderStatusDeliveredAsync
		[Fact]
		public async Task OrderService_ChangeOrderStatusDeliveredAsync_ReturnSuccess()
		{
			//Arrange
			var userId = Guid.NewGuid().ToString();
			var model = new ChangeOrderStatusDeliveredByUser
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				TransactionId = "123456"
			};

			var order = A.Fake<Order>();
			order.UserId = userId;
			order.Status = OrderStatus.Shipped;
			var transaction = new Transaction
			{
				Id = "123456",
				Status = TransactionStatus.PAID
			};
			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);
			A.CallTo(() => _unitOfWork.TransactionRepository.GetTransactionIdStringAsync(model.TransactionId)).Returns(transaction);
			A.CallTo(() => _unitOfWork.OrderRepository.UpdateAsync(order)).Returns(true);

			//Act
			var result = await service.ChangeOrderStatusDeliveredAsync(model);

			//Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().Be("Order is delivered.");
			A.CallTo(() => _unitOfWork.CompleteAsync()).MustHaveHappenedOnceExactly();
		}
		[Fact]
		public async Task OrderService_ChangeOrderStatusDeliveredAsync_ReturnTransactionNotPaid()
		{
			//Arrange
			var userId = Guid.NewGuid().ToString();
			var model = new ChangeOrderStatusDeliveredByUser
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				TransactionId = "123456"
			};

			var order = A.Fake<Order>();
			order.UserId = userId;
			order.Status = OrderStatus.Shipped;
			var transaction = new Transaction
			{
				Id = "123456",
				Status = TransactionStatus.UNPAID
			};
			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);
			A.CallTo(() => _unitOfWork.TransactionRepository.GetTransactionIdStringAsync(model.TransactionId)).Returns(transaction);

			//Act
			var result = await service.ChangeOrderStatusDeliveredAsync(model);

			//Assert
			result.StatusCode.Should().Be(400);
			result.Message.Should().Be("Unpaid orders!");
		}
		[Fact]
		public async Task OrderService_ChangeOrderStatusDeliveredAsync_ReturnOrderExistNotShipped()
		{
			//Arrange
			var userId = Guid.NewGuid().ToString();
			var model = new ChangeOrderStatusDeliveredByUser
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				TransactionId = "123456"
			};

			var order = A.Fake<Order>();
			order.UserId = userId;
			order.Status = OrderStatus.Shipping;
			var transaction = new Transaction
			{
				Id = "123456",
				Status = TransactionStatus.PAID
			};
			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);
			A.CallTo(() => _unitOfWork.TransactionRepository.GetTransactionIdStringAsync(model.TransactionId)).Returns(transaction);

			//Act
			var result = await service.ChangeOrderStatusDeliveredAsync(model);

			//Assert
			result.StatusCode.Should().Be(402);
			result.Message.Should().Be("Order not shipped! Can't check Delivered");
		}
		[Fact]
		public async Task OrderService_ChangeOrderStatusDeliveredAsync_ReturnServerError()
		{
			//Arrange
			var userId = Guid.NewGuid().ToString();
			var model = new ChangeOrderStatusDeliveredByUser
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				TransactionId = "123456"
			};

			var order = A.Fake<Order>();
			order.UserId = userId;
			order.Status = OrderStatus.Shipped;
			var transaction = new Transaction
			{
				Id = "123456",
				Status = TransactionStatus.PAID
			};
			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);
			A.CallTo(() => _unitOfWork.TransactionRepository.GetTransactionIdStringAsync(model.TransactionId)).Returns(transaction);
			A.CallTo(() => _unitOfWork.OrderRepository.UpdateAsync(order)).Returns(false);

			//Act
			var result = await service.ChangeOrderStatusDeliveredAsync(model);

			//Assert
			result.StatusCode.Should().Be(500);
			result.Message.Should().Be("Failed to update order status!");
		}
		[Fact]
		public async Task OrderService_ChangeOrderStatusDeliveredAsync_ReturnUnAuthorizedUser()
		{
			//Arrange
			var userId = Guid.NewGuid().ToString();
			var model = new ChangeOrderStatusDeliveredByUser
			{
				Id = Guid.NewGuid(),
				UserId = Guid.NewGuid().ToString(),
				TransactionId = "123456"
			};

			var order = A.Fake<Order>();
			order.UserId = userId;
			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);

			//Act
			var result = await service.ChangeOrderStatusDeliveredAsync(model);

			//Assert
			result.StatusCode.Should().Be(403);
			result.Message.Should().Be("Unauthorized to update order status!");
		}
		[Fact]
		public async Task OrderService_ChangeOrderStatusDeliveredAsync_ReturnOrderNotFound()
		{
			//Arrange
			var userId = Guid.NewGuid().ToString();
			var model = new ChangeOrderStatusDeliveredByUser
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				TransactionId = "123456"
			};

			Order? order = null;
			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetByIdAsync(model.Id.ToString())).Returns(order);

			//Act
			var result = await service.ChangeOrderStatusDeliveredAsync(model);

			//Assert
			result.StatusCode.Should().Be(404);
			result.Message.Should().Be("Order not found!");
		}
		#endregion
		#region OrderHistory
		[Fact]
		public async Task OrderService_OrderHistory_ReturnSuccess()
		{
			//Arrange
			string userId = Guid.NewGuid().ToString();
			var orders = new List<Order>()
			{
				new Order
				{
					Id = Guid.NewGuid(),
					UserId = "user123",
					WeekPlanId = Guid.NewGuid(),
					Note = "Order for weekly groceries",
					ShipDate = DateTime.UtcNow.AddDays(1),
					OrderDate = DateTime.UtcNow,
					TotalPrice = 99.99,
					Status = OrderStatus.Processing
				},
				new Order
				{
					Id = Guid.NewGuid(),
					UserId = "user456",
					WeekPlanId = Guid.NewGuid(),
					Note = "Urgent delivery requested",
					ShipDate = DateTime.UtcNow.AddDays(2),
					OrderDate = DateTime.UtcNow,
					TotalPrice = 149.99,
					Status = OrderStatus.Shipping
				}
			};
			var orderMap = new List<OrderModelResponse>();

			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetOrderHistoryAsync(userId)).Returns(orders);
			A.CallTo(() => _mapper.Map<List<OrderModelResponse>>(orders)).Returns(orderMap);

			//Act
			var result = await service.OrderHistory(userId);

			//Assert
			result.StatusCode.Should().Be(200);
			result.Message.Should().Be("Order history retrieved successfully");
			result.List.Should().NotBeNull();
		}
		[Fact]
		public async Task OrderService_OrderHistory_ReturnNotHaveHistory()
		{
			//Arrange
			string userId = Guid.NewGuid().ToString();
			List<Order>? orders = null;
			var orderMap = new List<OrderModelResponse>();

			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.OrderRepository.GetOrderHistoryAsync(userId)).Returns(orders);

			//Act
			var result = await service.OrderHistory(userId);

			//Assert
			result.StatusCode.Should().Be(404);
			result.Message.Should().Be("User does not have any order history");
			result.List.Should().BeNull();
		}
		[Fact]
		public async Task OrderService_OrderHistory_ReturnUserNotFound()
		{
			//Arrange
			string userId = Guid.NewGuid().ToString();
			User? user = null;	
			var orderMap = new List<OrderModelResponse>();

			var service = new OrderService(_unitOfWork , _mapper);
			A.CallTo(() => _unitOfWork.UserRepository.GetByIdAsync(userId)).Returns(user);

			//Act
			var result = await service.OrderHistory(userId);

			//Assert
			result.StatusCode.Should().Be(404);
			result.Message.Should().StartWith("User with id");
			result.Message.Should().Contain("not found!");
			result.List.Should().BeNull();
		}

		#endregion
	}
}
