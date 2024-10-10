using myshop.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Entities.Repositories.Contract
{
    public interface IShoppingCartRepository : IGenericRepository<ShoppingCart>
    {
        int IncreaseCart(ShoppingCart shoppingCart, int count);
        int DecreaseCart(ShoppingCart shoppingCart, int count);

    }
}
