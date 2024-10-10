using myshop.DataAccess.Data;
using myshop.Entities.Models;
using myshop.Entities.Repositories.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.DataAccess.ImplementationRepos
{
	public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
	{
		private readonly ApplicationDbContext _context;
		public OrderDetailRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public void Update(OrderDetail orderDetail)
		{
			_context.OrderDetails.Update(orderDetail);
		}
	}
}
