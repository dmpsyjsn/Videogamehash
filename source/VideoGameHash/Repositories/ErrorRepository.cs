﻿using System.Collections.Generic;
using System.Linq;
using VideoGameHash.Models;

namespace VideoGameHash.Repositories
{
    public class ErrorRepository
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

            _db.Errors.AddObject(error);
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
                _db.DeleteObject(error);

            _db.SaveChanges();
        }
    }
}