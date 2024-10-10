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
	public class ApplicationUserRepository : GenericRepository<ApplicationUser>, IApplicationUserRepository
	{
		private readonly ApplicationDbContext _context;
		public ApplicationUserRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
	}
}
