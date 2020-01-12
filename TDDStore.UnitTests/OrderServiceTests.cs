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

        [SetUp]
        public void Setup()
        {
            orderDataService = Mock.Create<IOrderDataService>();
            orderService = new OrderService(orderDataService);
        }
        
        [Test]
        public void WhenUserPlacesACorrectOrderThenAnOrderNumberShouldBeReturned()
        {
            //Arrange
            var shoppingCart = new ShoppingCart();
            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = Guid.NewGuid(), Quantity = 1 });
            var customerId = Guid.NewGuid();
            var expectedOrderId = Guid.NewGuid();

            //var orderDataService = Mock.Create<IOrderDataService>();

            //Mock.Arrange command makes this orderDataService mock object into a stub.
            // Arg.IsAny<Order>()) is a matcher, not with any specific functionality, 
            //orther than being an object of Order.
            Mock.Arrange(() => orderDataService.Save(Arg.IsAny<Order>()))
                .Returns(expectedOrderId)
                .OccursOnce();

            //var orderService = new OrderService(orderDataService);

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
    }
}
