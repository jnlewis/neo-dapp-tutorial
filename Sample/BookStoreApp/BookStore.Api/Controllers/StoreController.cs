using BookStore.Api.Contract;
using LevelDB;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;

namespace BookStore.Api.Controllers
{
    public class StoreController : ApiController
    {
        #region Struct
        public struct BookRequest
        {
            public string OwnerAddress { get; set; }
            public Book Book { get; set; }
        }
        public struct OrderRequest
        {
            public string BuyerAddress { get; set; }
            public string OrderId { get; set; }
            public string BookId { get; set; }
        }
        public struct Book
        {
            public string BookId { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public long Price { get; set; }
        }
        #endregion

        #region DB Configuration
        //dbFolder: The folder where the database file will reside.
        private string dbFolder = null;
        //CreateIfMissing: This option will create the data folder if not already exists. 
        //BloomFilterPolicy: This option is meant to optimize disk reads.
        Options dbOptions = new Options() { CreateIfMissing = true, FilterPolicy = new BloomFilterPolicy(10) };
        #endregion
        
        public StoreController()
        {
            dbFolder = System.Web.Hosting.HostingEnvironment.MapPath("~/Data");
            if(dbFolder == null)    //Fallback when running Unit Test
                dbFolder = System.Environment.CurrentDirectory + @"/Data";
        }

        [HttpGet]
        [Route("books/{bookId}")]
        public HttpResponseMessage GetBookInfo(string bookId)
        {
            Book result;
            
            //Retrieve the book info from off-chain database and return the results to the front-end of our Dapp
            using (var database = DB.Open(dbFolder, dbOptions))
            {
                Slice dbValue;
                result = new Book()
                {
                    BookId = bookId,
                    Title = database.TryGet(ReadOptions.Default, Key("Book_Title", bookId), out dbValue) ? dbValue.ToString() : null,
                    Author = database.TryGet(ReadOptions.Default, Key("Book_Author", bookId), out dbValue) ? dbValue.ToString() : null,
                    Price = database.TryGet(ReadOptions.Default, Key("Book_Price", bookId), out dbValue) ? dbValue.ToInt64() : 0
                };
            }
            
            //return Request.CreateResponse(HttpStatusCode.OK, result);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ObjectContent<Book>(result, new JsonMediaTypeFormatter(), "application/json");
            return response;

        }

        [HttpPost]
        [Route("books/add")]
        public HttpResponseMessage AddBook(BookRequest value)
        {
            //Add the given book into the off-chain database.
            using (var database = DB.Open(dbFolder, dbOptions))
            {
                database.Put(WriteOptions.Default, Key("Book_OwnerAddress", value.Book.BookId), value.OwnerAddress);
                database.Put(WriteOptions.Default, Key("Book_Title", value.Book.BookId), value.Book.Title);
                database.Put(WriteOptions.Default, Key("Book_Author", value.Book.BookId), value.Book.Author);
                database.Put(WriteOptions.Default, Key("Book_Price", value.Book.BookId), value.Book.Price);
            }

            //Commit to blockchain
            Blockchain.InvokeScript("addBook",
                new object[] {
                    value.OwnerAddress,
                    value.Book.Title,
                    value.Book.Author,
                    value.Book.Price
                });

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("books/update")]
        public HttpResponseMessage UpdateBook(BookRequest value)
        {
            //Update the given book on the off-chain database and on the blockchain.
            using (var database = DB.Open(dbFolder, dbOptions))
            {
                database.Put(WriteOptions.Default, Key("Book_OwnerAddress", value.Book.BookId), value.OwnerAddress);
                database.Put(WriteOptions.Default, Key("Book_Title", value.Book.BookId), value.Book.Title);
                database.Put(WriteOptions.Default, Key("Book_Author", value.Book.BookId), value.Book.Author);
                database.Put(WriteOptions.Default, Key("Book_Price", value.Book.BookId), value.Book.Price);
            }

            //Commit to blockchain
            Blockchain.InvokeScript("updateBook",
                new object[] {
                    value.OwnerAddress,
                    value.Book.Title,
                    value.Book.Author,
                    value.Book.Price
                });

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("books/delete")]
        public HttpResponseMessage DeleteBook(BookRequest value)
        {
            //Delete the given book from the off-chain database.
            using (var database = DB.Open(dbFolder, dbOptions))
            {
                database.Delete(WriteOptions.Default, Key("Book_OwnerAddress", value.Book.BookId));
                database.Delete(WriteOptions.Default, Key("Book_Title", value.Book.BookId));
                database.Delete(WriteOptions.Default, Key("Book_Author", value.Book.BookId));
                database.Delete(WriteOptions.Default, Key("Book_Price", value.Book.BookId));
            }

            //Commit to blockchain
            Blockchain.InvokeScript("deleteBook",
                new object[] {
                    value.OwnerAddress,
                    value.Book.BookId
                });

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("books/purchase")]
        public HttpResponseMessage PurchaseBook(OrderRequest value)
        {
            //Add a purchase order for the given book into the off-chain database.
            using (var database = DB.Open(dbFolder, dbOptions))
            {
                database.Put(WriteOptions.Default, Key("Purchase_BuyerAddress", value.OrderId), value.BuyerAddress);
                database.Put(WriteOptions.Default, Key("Purchase_BookId", value.OrderId), value.BookId);
            }

            //Commit to blockchain
            Blockchain.InvokeScript("purchaseBook",
                new object[] {
                    value.BuyerAddress,
                    value.OrderId,
                    value.BookId
                });

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private string Key(string prefix, string id)
        {
            return prefix + "_" + id;
        }
        
    }
}