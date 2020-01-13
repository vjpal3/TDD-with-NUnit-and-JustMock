using System;
using NUnit.Framework;
using TddStore.Core;
using Telerik.JustMock;
using TddStore.Core.Exceptions;

namespace TDDStore.UnitTests
{
    [TestFixture]
    public class OrderServiceTests
    {
        private OrderService orderService;
        private IOrderDataService orderDataService;
        private ICustomerService customerService;

        [SetUp]
        public void Setup()
        {
            orderDataService = Mock.Create<IOrderDataService>();
            customerService = Mock.Create<ICustomerService>();
            orderService = new OrderService(orderDataService, customerService);
        }

        [Test]
        public void WhenUserPlacesACorrectOrderThenAnOrderNumberShouldBeReturned()
        {
            //Arrange
            var shoppingCart = new ShoppingCart();
            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = Guid.NewGuid(), Quantity = 1 });
            var customerId = Guid.NewGuid();
            var expectedOrderId = Guid.NewGuid();


            //Mock.Arrange command makes this orderDataService mock object into a stub.
            // Arg.IsAny<Order>()) is a matcher, not with any specific functionality, 
            //orther than being an object of Order.
            Mock.Arrange(() => orderDataService.Save(Arg.IsAny<Order>()))
                .Returns(expectedOrderId)
                .OccursOnce();

            
            //Act
            var result = orderService.PlaceOrder(customerId, shoppingCart);

            //Assert
            Assert.AreEqual(expectedOrderId, result);

            Mock.Assert(orderDataService);
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

    }
}
