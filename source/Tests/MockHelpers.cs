using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Moq;

namespace Tests
{
    public static class MockHelpers
    {
        public static Mock<DbSet<T>> CreateMockSet<T>() where T : class
        {
            var mockSet = new Mock<DbSet<T>>(); 

            var sourceList = new List<T>();
            var data = sourceList.AsQueryable();

            mockSet.As<IDbAsyncEnumerable<T>>() 
                .Setup(m => m.GetAsyncEnumerator()) 
                .Returns(new TestDbAsyncEnumerator<T>(data.GetEnumerator())); 
 
            mockSet.As<IQueryable<T>>() 
                .Setup(m => m.Provider) 
                .Returns(new TestDbAsyncQueryProvider<T>(data.Provider)); 
 
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression); 
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType); 
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(s => sourceList.Add(s));
            mockSet.Setup(d => d.Remove(It.IsAny<T>())).Callback<T>(s => sourceList.Remove(s));
            return mockSet;
        }
    }
}
