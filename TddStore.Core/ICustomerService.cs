using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TddStore.Core
{
    public interface ICustomerService
    {
        Customer GetCustomer(Guid customerId);
    }
}
