using System.Collections.Generic;

namespace VideoGameHash.Models
{
    public class ErrorModel
    {
        public ErrorModel()
        {
            ErrorMessages = new List<string>();
        }

        public IEnumerable<string> ErrorMessages { get; set; }
    }
}