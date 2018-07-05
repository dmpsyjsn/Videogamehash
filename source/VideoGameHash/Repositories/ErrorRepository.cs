using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VideoGameHash.Models;

namespace VideoGameHash.Repositories
{
    public interface IErrorRepository
    {
        void AddError(string errorMessage);
        Task<IEnumerable<string>> GetErrorMessages();
        Task DeleteAllErrors();
    }

    public class ErrorRepository : IErrorRepository
    {
        private readonly VGHDatabaseContainer _db;

        public ErrorRepository(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public void AddError(string errorMessage)
        {
            var error = new Errors
            {
                ErrorMessage = errorMessage
            };

            _db.Errors.Add(error);
            _db.SaveChanges();
        }

        public async Task<IEnumerable<string>> GetErrorMessages()
        {
            return await _db.Errors.Select(x => x.ErrorMessage).ToListAsync();
        }

        public async Task DeleteAllErrors()
        {
            if (!_db.Errors.Any()) return;

            foreach (var error in _db.Errors)
                _db.Errors.Remove(error);

            await _db.SaveChangesAsync();
        }
    }
}