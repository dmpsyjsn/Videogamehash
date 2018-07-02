using System.Collections.Generic;
using System.Linq;
using VideoGameHash.Models;

namespace VideoGameHash.Repositories
{
    public interface IErrorRepository
    {
        void AddError(string errorMessage);
        IEnumerable<string> GetErrorMessages();
        void DeleteAllErrors();
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

        public IEnumerable<string> GetErrorMessages()
        {
            return _db.Errors.AsEnumerable().Select(x => x.ErrorMessage);
        }

        public void DeleteAllErrors()
        {
            if (!_db.Errors.Any()) return;

            foreach (var error in _db.Errors)
                _db.Errors.Remove(error);

            _db.SaveChanges();
        }
    }
}