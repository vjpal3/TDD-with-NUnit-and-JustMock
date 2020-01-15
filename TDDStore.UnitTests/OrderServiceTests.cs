using System;
using NUnit.Framework;
using TddStore.Core;
using Telerik.JustMock;
using TddStore.Core.Exceptions;
using System.Collections.Generic;

namespace TDDStore.UnitTests
{
    [TestFixture]
    public class OrderServiceTests
    {
        private OrderService orderService;
        private IOrderDataService orderDataService;
        private ICustomerService customerService;
        private IOrderFulfillmentService orderFulfillmentService;

        [SetUp]
        public void Setup()
        {
            orderDataService = Mock.Create<IOrderDataService>();
            customerService = Mock.Create<ICustomerService>();
            
            orderFulfillmentService = Mock.Create<IOrderFulfillmentService>();
            orderService = new OrderService(orderDataService, customerService, orderFulfillmentService);
        }

        [Test]
        public void WhenUserPlacesACorrectOrderThenAnOrderNumberShouldBeReturned()
        {
            //Arrange
            var shoppingCart = new ShoppingCart();
            var itemId = Guid.NewGuid();
            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = itemId, Quantity = 1 });

            var customerId = Guid.NewGuid();
            var expectedOrderId = Guid.NewGuid();
            var orderFulfillmentSessionId = Guid.NewGuid();
            
            var customer = new Customer { Id = customerId };


            //Mock.Arrange command makes this orderDataService mock object into a stub.
            // Arg.IsAny<Order>()) is a matcher, not with any specific functionality, 
            //orther than being an object of Order.
            Mock.Arrange(() => orderDataService.Save(Arg.IsAny<Order>()))
                .Returns(expectedOrderId)
                .OccursOnce();

            Mock.Arrange(() => customerService
                .GetCustomer(customerId)).Returns(customer).OccursOnce();

            Mock.Arrange(() => orderFulfillmentService
                .OpenSession(Arg.IsAny<string>(), Arg.IsAny<string>()))
                .Returns(orderFulfillmentSessionId);

            //  IsInInventory method is called with the expected item id (the same itemId in the shopping cart). If two itemsIds are different, the test will fail.
            Mock.Arrange(() => orderFulfillmentService
                .IsInInventory(orderFulfillmentSessionId, itemId, 1))
                .Returns(true)
                .OccursOnce();

            Mock.Arrange(() => orderFulfillmentService
                .PlaceOrder(orderFulfillmentSessionId, Arg.IsAny<IDictionary<Guid, int>>(), Arg.IsAny<string>()))
                .Returns(true);

            Mock.Arrange(() => orderFulfillmentService
                .CloseSession(orderFulfillmentSessionId));

            //Act
            var result = orderService.PlaceOrder(customerId, shoppingCart);

            //Assert
            Assert.That(expectedOrderId, Is.EqualTo(result));

            Mock.Assert(orderDataService);
            Mock.Assert(orderFulfillmentService);
        }

        [Test]
        public void WhenUserAttemptsToOrderAnItemWithAQuantityOfZeroThrowInValidOrderException()
        {
            //Arrange
            var shoppingCart = new ShoppingCart();
            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = Guid.NewGuid(), Quantity = 0 });
            var customerId = Guid.NewGuid();
            var expectedOrderId = Guid.NewGuid();

            Mock.Arrange(() => orderDataService.Save(Arg.IsAny<Order>()))
                .Returns(expectedOrderId)
                .OccursNever();


            //Act & Assert
            Assert.Throws<InvalidOrderException>(() => orderService.PlaceOrder(customerId, shoppingCart));

            Mock.Assert(orderDataService);
        }

        [Test]
        public void WhenAValidCustomerPlacesAValidOrderAnOrderShouldBePlaced()
        {
            //Arrange
            var shoppingCart = new ShoppingCart();
            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = Guid.NewGuid(), Quantity = 1 });

            var customerId = Guid.NewGuid();
            var customerToReturn = new Customer { Id = customerId, FirstName = "Sarah", LastName = "Flinstone" };

            Mock.Arrange(() => customerService.GetCustomer(customerId))
                .Returns(customerToReturn)
                .OccursOnce();

            //Act
            orderService.PlaceOrder(customerId, shoppingCart);

            //Assert
            Mock.Assert(customerService);

        }

        [Test]
        public void WhenUserPlacesOrderWithItemThatIsInInventoryOrderFulfillmentWorkflowShouldComplete()
        {
            //Arrange
            var shoppingCart = new ShoppingCart();
            var itemId = Guid.NewGuid();
            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = itemId, Quantity = 1 });
            
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId };

            var orderFulfillmentSessionId = Guid.NewGuid();  

            Mock.Arrange(() => customerService.GetCustomer(customerId))
                .Returns(customer)
                .OccursOnce();

            Mock.Arrange(() => orderFulfillmentService
                .OpenSession(Arg.IsAny<string>(), Arg.IsAny<string>()))
                .Returns(orderFulfillmentSessionId)
                .InOrder();

            Mock.Arrange(() => orderFulfillmentService
                .IsInInventory(orderFulfillmentSessionId, itemId, 1))
                .Returns(true)
                .InOrder(); 
            
            Mock.Arrange(() => orderFulfillmentService
                .PlaceOrder(orderFulfillmentSessionId, Arg.IsAny<IDictionary<Guid, int>>(), Arg.IsAny<string>()))
                .Returns(true)
                .InOrder();

            Mock.Arrange(() => orderFulfillmentService
                .CloseSession(orderFulfillmentSessionId))
                .InOrder();


            // Act
            orderService.PlaceOrder(customerId, shoppingCart);

            // Assert
            Mock.Assert(orderFulfillmentService);

        }

        [Test]
        public void WhenUserPlacesACorrectOrderWithMoreThenOneItemThenAnOrderNumberShouldBeReturned()
        {
            //Arrange
            var shoppingCart = new ShoppingCart();
            var itemOneId = Guid.NewGuid();
            var itemTwoId = Guid.NewGuid();
            int itemOneQuantity = 1;
            int itemTwoQuantity = 4;

            shoppingCart.Items
                .Add(new ShoppingCartItem { ItemId = itemOneId, Quantity = itemOneQuantity });
            shoppingCart
                .Items.Add(new ShoppingCartItem { ItemId = itemTwoId, Quantity = itemTwoQuantity });

            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId };

            var expectedOrderId = Guid.NewGuid();
            var orderFulfillmentSessionId = Guid.NewGuid();

            Mock.Arrange(() => orderDataService.Save(Arg.IsAny<Order>()))
                .Returns(expectedOrderId)
                .OccursOnce();

            Mock.Arrange(() => customerService.GetCustomer(customerId))
                .Returns(customer)
                .OccursOnce();

            Mock.Arrange(() => orderFulfillmentService
                .OpenSession(Arg.IsAny<string>(), Arg.IsAny<string>()))
                .Returns(orderFulfillmentSessionId);


            Mock.Arrange(() => orderFulfillmentService
                .IsInInventory(orderFulfillmentSessionId, itemOneId, itemOneQuantity))
                .Returns(true)
                .OccursOnce();
                
            
            Mock.Arrange(() => orderFulfillmentService
                .IsInInventory(orderFulfillmentSessionId, itemTwoId, itemTwoQuantity))
                .Returns(true)
                .OccursOnce();


            Mock.Arrange(() => orderFulfillmentService
                .PlaceOrder(orderFulfillmentSessionId, Arg.IsAny<IDictionary<Guid, int>>(), Arg.IsAny<string>()))
                .Returns(true);

            // Act
            var result = orderService.PlaceOrder(customerId, shoppingCart);

            //Assert
            Assert.That(expectedOrderId, Is.EqualTo(result));
            Mock.Assert(orderDataService);
            Mock.Assert(orderFulfillmentService); 

        }
    }
}
