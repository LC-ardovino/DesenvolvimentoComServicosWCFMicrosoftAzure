using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using App.BLL.Models;

namespace App.DAL.Repositories
{
    public class FriendRepository : IFriendRepository
    {
        private readonly DbContext _context;

        public FriendRepository(DbContext context)
        {
            _context = context;
        }

        public bool EntityExists(int id)
        {
            return _context.Set<Friend>().Any(p => p.Id == id);
        }

        public async Task<Friend> GetById(int id)
        {
            return await _context.Set<Friend>().FindAsync(id);
        }
        
        public  async Task<IEnumerable<Friend>> GetAll()
        {

            return await _context.Set<Friend>()
                .AsNoTracking()
                .OrderBy(p => p.FirstName)
                .ToListAsync();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void Create(Friend item)
        {
            _context.Set<Friend>().Add(item);
            _context.SaveChanges();

        }

        public void Update(Friend item)
        {
            _context.Set<Friend>().Update(item);
            _context.SaveChanges();

        }

        public void Delete(Friend item)
        {
            _context.Set<Friend>().Remove(item);
            _context.SaveChanges();

        }
    }
}
