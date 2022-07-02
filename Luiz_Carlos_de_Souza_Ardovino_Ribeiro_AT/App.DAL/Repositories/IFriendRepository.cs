using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.BLL.Models;

namespace App.DAL.Repositories
{
    public interface IFriendRepository
    {
        Task<IEnumerable<Friend>> GetAll();
        Task<Friend> GetById(int id);
        bool EntityExists(int id);
        void Create(Friend item);
        void Update(Friend item);
        void Delete(Friend item);
        void SaveChanges();
    }
}
