using System;
using BookStore.Api.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static BookStore.Api.Controllers.StoreController;

namespace BookStore.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        private string sellerAddress = "a8e2b5436cab6ff74be2d5c91b8a67053494ab5b454ac2851f872fb0fd30ba5e";
        private string customerAddress = "b8e2b5436cab6ff74be2d5c91b8a67053494ab5b454ac2851f872fb0fd30ba5c";
        //Note: When running tests in sequence, set this variable as a static string
        private string bookId = Guid.NewGuid().ToString();

        [TestMethod]
        public void TestMethod_AddBook()
        {
            Book book = new Book
            {
                BookId = bookId,
                Author = "Khaled Hosseini",
                Title = "The Kite Runner",
                Price = 225000
            };
            BookRequest bookRequest = new BookRequest
            {
                OwnerAddress = sellerAddress,
                Book = book
            };
            
            StoreController controller = new StoreController();
            var response = controller.AddBook(bookRequest);
            bool successful = response.IsSuccessStatusCode;

            Assert.AreEqual(true, successful);
        }

        [TestMethod]
        public void TestMethod_UpdateBook()
        {
            Book book = new Book
            {
                BookId = bookId,
                Author = "Khaled Hosseini",
                Title = "The Kite Runner",
                Price = 187000
            };
            BookRequest bookRequest = new BookRequest
            {
                OwnerAddress = sellerAddress,
                Book = book
            };

            StoreController controller = new StoreController();
            var response = controller.UpdateBook(bookRequest);
            bool successful = response.IsSuccessStatusCode;

            Assert.AreEqual(true, successful);
        }

        [TestMethod]
        public void TestMethod_PurchaseBook()
        {
            OrderRequest order = new OrderRequest
            {
                BuyerAddress = customerAddress,
                OrderId = Guid.NewGuid().ToString(),
                BookId = bookId
            };

            StoreController controller = new StoreController();
            var response = controller.PurchaseBook(order);
            bool successful = response.IsSuccessStatusCode;

            Assert.AreEqual(true, successful);
        }
        
        [TestMethod]
        public void TestMethod_DeleteBook()
        {
            Book book = new Book
            {
                BookId = bookId
            };
            BookRequest bookRequest = new BookRequest
            {
                OwnerAddress = sellerAddress,
                Book = book
            };

            StoreController controller = new StoreController();
            var response = controller.DeleteBook(bookRequest);
            bool successful = response.IsSuccessStatusCode;

            Assert.AreEqual(true, successful);
        }
        
    }
}
