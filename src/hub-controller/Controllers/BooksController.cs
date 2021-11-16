using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;

using HubController.Entities;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using System.Security.Claims;
using System.Linq;

namespace HubController.Controllers
{
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IAmazonDynamoDB client;
        private readonly DynamoDBContext context;

        public BooksController(IAmazonDynamoDB client)
        {
            this.client = client;
            this.context = new DynamoDBContext(client);
        }

    // GET api/books
    [HttpGet]
        public async Task<IEnumerable<Book>> Get()
        {
            var result = new List<Book>();
            LambdaLogger.Log($"**************** User *************** {HttpContext.User}");
            foreach(var claim in HttpContext.User.Claims)
            {
                LambdaLogger.Log($"*** type: {claim.Type}, value: {claim.Value} ***");
            }
            ScanFilter filter = new ScanFilter();
            filter.AddCondition("Title", ScanOperator.IsNotNull);
            ScanOperationConfig scanConfig = new ScanOperationConfig
            {
                Limit = 10,
                Filter = filter
            };
            var queryResult = context.FromScanAsync<Book>(scanConfig);

            do
            {
                result.AddRange(await queryResult.GetNextSetAsync());
            }
            while (!queryResult.IsDone && result.Count < 10);

            var proxyRequest = Request.HttpContext.Items["LambdaRequestObject"] as APIGatewayProxyRequest;

            if (proxyRequest?.RequestContext?.Authorizer != null)
            {
                var claims = proxyRequest.RequestContext
                                         .Authorizer
                                         .Claims;
            LambdaLogger.Log($"**************** Claims *************** {claims}");
            }
            return result;
        }

        // GET api/books/5
        [HttpGet("{id}")]
        public async Task<Book> Get(Guid id)
        {
            LambdaLogger.Log($"Looking for book {id}");
            var search = context.QueryAsync<Book>(id);
            var books = await search.GetNextSetAsync();
            if (books.Count != 1)
            {
                throw new KeyNotFoundException($"Book {id} not found");
            }
            return books[0];
        }

        // POST api/books
        [HttpPost]
        public async Task Post([FromBody] Book book)
        {
            if (book == null)
            {
                throw new ArgumentException("Invalid input! Book not informed");
            }
            book.Id = Guid.NewGuid();

            await context.SaveAsync<Book>(book);
            LambdaLogger.Log($"Book {book.Id} is added");
        }

        // PUT api/books/5
        [HttpPut("{id}")]
        public async Task Put(Guid id, [FromBody] Book book)
        {
            // Retrieve the book.
            var search = context.QueryAsync<Book>(id);
            var books = await search.GetNextSetAsync();
            var bookRetrieved = books[0];

            if (bookRetrieved == null)
            {
                var errorMsg = $"Invalid input! No book found with id:{id}";
                LambdaLogger.Log(errorMsg);
                throw new KeyNotFoundException(errorMsg);
            }

            book.Id = bookRetrieved.Id;

            await context.SaveAsync<Book>(book);
            LambdaLogger.Log($"Book {book.Id} is updated");
        }

        // DELETE api/books/5
        [HttpDelete("{id}")]
        public async Task Delete(Guid id)
        {
            // Retrieve book
            var search = context.QueryAsync<Book>(id);
            var books = await search.GetNextSetAsync();
            if (books.Count != 1)
            {
                return;
            }
            var bookRetrieved = books[0];
            // Delete the book.
            await context.DeleteAsync<Book>(bookRetrieved.Id, bookRetrieved.TimeStamp);

            // Try to retrieve deleted book. It should return null.
            var operationConfig = new DynamoDBOperationConfig();
            operationConfig.ConsistentRead = true;
            Book deletedBook = await context.LoadAsync<Book>(id, bookRetrieved.TimeStamp, operationConfig);

            if (deletedBook == null)
                LambdaLogger.Log($"Book {id} is deleted");
        }
    }
}
