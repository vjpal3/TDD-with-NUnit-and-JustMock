using System;
using System.Collections.Generic;
using TddStore.Core.Exceptions;

namespace TddStore.Core
{
    public class OrderService
    {
        private IOrderDataService _orderDataService;
        private ICustomerService _customerService;
        private IOrderFulfillmentService _orderFulfillmentService;
        private const string USERNAME = "Bob";
        private const string PASSWORD = "Foo";

        public OrderService(IOrderDataService orderDataService, ICustomerService customerService, IOrderFulfillmentService orderFulfillmentService)
        {
            _orderDataService = orderDataService;
            _customerService = customerService;
            _orderFulfillmentService = orderFulfillmentService;

        }

        public Guid PlaceOrder(Guid customerId, ShoppingCart shoppingCart)
        {
            foreach (var item in shoppingCart.Items)
            {
                if (item.Quantity == 0)
                {
                    throw new InvalidOrderException();
                }
            }

            var customer = _customerService.GetCustomer(customerId);

            PlaceOrderWithFulfillmentService(shoppingCart, customer);

            var order = new Order();
            return _orderDataService.Save(order);

        }

        private void PlaceOrderWithFulfillmentService(ShoppingCart shoppingCart, Customer customer)
        {
            //Open session
            Guid orderFulfillmentSessionId = OpenOrderFuilfillmentSession();

            PlaceOrderWithFulfillmentService(orderFulfillmentSessionId, shoppingCart, customer);

            // Close session
            CloseOrderFulfillmentService(orderFulfillmentSessionId);
        }

        private void PlaceOrderWithFulfillmentService(Guid orderFulfillmentSessionId, ShoppingCart shoppingCart, Customer customer)
        {
            Dictionary<Guid, int> orderForFulfillmentService = CheckInventoryLevels(orderFulfillmentSessionId, shoppingCart);

            // Place order
            var orderPlaced = _orderFulfillmentService
                .PlaceOrder(orderFulfillmentSessionId, orderForFulfillmentService, customer.ShippingAddress.ToString());
        }

        private Dictionary<Guid, int> CheckInventoryLevels(Guid orderFulfillmentSessionId, ShoppingCart shoppingCart)
        {
            var firstItemId = shoppingCart.Items[0].ItemId;
            var firstItemQuantity = shoppingCart.Items[0].Quantity;


            // check inventory
            var itemIsInInventory = _orderFulfillmentService.IsInInventory(orderFulfillmentSessionId, firstItemId, firstItemQuantity);

            var orderForFulfillmentService = new Dictionary<Guid, int>();
            orderForFulfillmentService.Add(firstItemId, firstItemQuantity);
            return orderForFulfillmentService;
        }

        private void CloseOrderFulfillmentService(Guid orderFulfillmentSessionId)
        {
            _orderFulfillmentService.CloseSession(orderFulfillmentSessionId);
        }

        private Guid OpenOrderFuilfillmentSession()
        {
            return _orderFulfillmentService.OpenSession(USERNAME, PASSWORD);
        }
    }  
} 