using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.ComponentModel;
using System.Numerics;

namespace BookStore.SmartContracts
{
    public class BookStoreContract : SmartContract
    {
        //Token Settings
        public static string Name() => "BookStore";
        public static string Symbol() => "BST";
        public static byte Decimals() => 8;
        private const ulong factor = 100000000; //decided by Decimals()

        //ICO Settings
        private static readonly byte[] neo_asset_id = { 155, 124, 255, 218, 166, 116, 190, 174, 15, 147, 14, 190, 96, 133, 175, 144, 147, 229, 254, 86, 179, 74, 92, 34, 12, 205, 207, 110, 252, 51, 111, 197 };
        private const ulong total_amount = 100000000 * factor; // total token amount
        private const ulong pre_ico_cap = 30000000 * factor; // pre ico token amount
        
        /// <summary>
        /// Main method of a contract.
        /// </summary>
        /// <param name="operation">Method to invoke.</param>
        /// <param name="args">Method parameters.</param>
        /// <returns>Method's return value or false if operation is invalid.</returns>
        public static object Main(string operation, params object[] args)
        {
            if (operation == "name")
                return Name();
            if (operation == "symbol")
                return Symbol();
            if (operation == "decimals")
                return Decimals();
            if (operation == "totalsupply")
                return TotalSupply();
            if (operation == "owner")
                return Owner();

            if (args.Length > 0)
            {
                if (operation == "deploy")
                    return Deploy((byte[])args[0]);

                if (operation == "balanceOf")
                    return BalanceOf((byte[])args[0]);

                if (operation == "transfer")
                    return Transfer((byte[])args[0], (byte[])args[1], (BigInteger)args[2]);

                if (operation == "addBook")
                    return AddBook((byte[])args[0], (string)args[1], (string)args[2], (string)args[3], (BigInteger)args[4]);

                if (operation == "updateBook")
                    return UpdateBook((byte[])args[0], (string)args[1], (string)args[2], (string)args[3], (BigInteger)args[4]);

                if (operation == "deleteBook")
                    return DeleteBook((byte[])args[0], (string)args[1]);
                
                if (operation == "purchaseBook")
                    return PurchaseBook((byte[])args[0], (string)args[1], (string)args[2]);
            }

            return false;
        }
        
        #region Default Methods

        // Storage Key Prefix are used for storing different categories of data on the blockchain
        // by prefixing a unique character to the storage key.
        private static string PREFIX_ACCOUNT = "A";

        public static bool Deploy(byte[] account)
        {
            byte[] supplyCheck = Storage.Get(Storage.CurrentContext, "totalsupply");

            if (supplyCheck == null)
            {
                Storage.Put(Storage.CurrentContext, "owner", account);
                byte[] owner = Storage.Get(Storage.CurrentContext, "owner");
                Storage.Put(Storage.CurrentContext, Key(PREFIX_ACCOUNT, owner), pre_ico_cap);
                Storage.Put(Storage.CurrentContext, "totalsupply", pre_ico_cap);
                //Transferred(null, owner, pre_ico_cap);
                return true;
            }
            return false;
        }

        private static BigInteger TotalSupply()
        {
            return Storage.Get(Storage.CurrentContext, "totalsupply").AsBigInteger();
        }

        private static string Owner()
        {
            return Storage.Get(Storage.CurrentContext, "owner").AsString();
        }

        private static BigInteger BalanceOf(byte[] account)
        {
            byte[] balance = Storage.Get(Storage.CurrentContext, Key(PREFIX_ACCOUNT, account));

            if (balance == null)
                return 0;

            return balance.AsBigInteger();
        }
        
        private static bool Transfer(byte[] from, byte[] to, BigInteger amount)
        {
            if (amount >= 0)
            {
                if (from == to)
                    return true;

                BigInteger senderBalance = Storage.Get(Storage.CurrentContext, Key(PREFIX_ACCOUNT, from)).AsBigInteger();
                BigInteger recipientBalance = Storage.Get(Storage.CurrentContext, Key(PREFIX_ACCOUNT, to)).AsBigInteger();

                if (senderBalance >= amount)
                {
                    BigInteger newSenderBalance = senderBalance - amount;
                    BigInteger newRecipientBalance = recipientBalance + amount;

                    Storage.Put(Storage.CurrentContext, Key(PREFIX_ACCOUNT, from), newSenderBalance);
                    Storage.Put(Storage.CurrentContext, Key(PREFIX_ACCOUNT, to), newRecipientBalance);
                    
                    return true;
                }
                else
                {
                    Runtime.Log("Transfer: Sender has insufficient balance.");
                    return false;
                }
            }

            return false;
        }

        #endregion

        #region Dapp Methods

        private static bool AddBook(byte[] ownerAddress, string bookId, string title, string author, BigInteger price)
        {
            //Validate input
            if (ownerAddress == null || title == null || author == null || price == null)
            {
                Runtime.Log("AddBook: One or more required parameter is not specified.");
                return false;
            }

            //Put data in storage
            Storage.Put(Storage.CurrentContext, Key("Book_OwnerAddress", bookId), ownerAddress);
            Storage.Put(Storage.CurrentContext, Key("Book_Title", bookId), title);
            Storage.Put(Storage.CurrentContext, Key("Book_Author", bookId), author);
            Storage.Put(Storage.CurrentContext, Key("Book_Price", bookId), price);

            Runtime.Log("AddBook: Successfully added book.");

            return true;
        }

        private static bool UpdateBook(byte[] ownerAddress, string bookId, string title, string author, BigInteger price)
        {
            //Validate input
            if (ownerAddress == null || title == null || author == null || price == null)
            {
                Runtime.Log("UpdateBook: One or more required parameter is not specified.");
                return false;
            }

            //Validate book existence and owner address
            byte[] bookOwnerAddress = Storage.Get(Storage.CurrentContext, Key("Book_OwnerAddress", bookId));
            if (bookOwnerAddress != null)
            {
                Runtime.Log("UpdateBook: Book not found.");
                return false;
            }
            if (bookOwnerAddress.AsString() != ownerAddress.AsString())
            {
                Runtime.Log("UpdateBook: Book is owned by a different owner.");
                return false;
            }

            //Update data in storage
            Storage.Put(Storage.CurrentContext, Key("Book_OwnerAddress", bookId), ownerAddress);
            Storage.Put(Storage.CurrentContext, Key("Book_Title", bookId), title);
            Storage.Put(Storage.CurrentContext, Key("Book_Author", bookId), author);
            Storage.Put(Storage.CurrentContext, Key("Book_Price", bookId), price);

            Runtime.Log("UpdateBook: Successfully updated book.");

            return true;
        }

        private static bool DeleteBook(byte[] ownerAddress, string bookId)
        {
            //Validate input
            if (ownerAddress == null || bookId == null)
            {
                Runtime.Log("DeleteBook: One or more required parameter is not specified.");
                return false;
            }

            //Validate book existence and owner address
            byte[] bookOwnerAddress = Storage.Get(Storage.CurrentContext, Key("Book_OwnerAddress", bookId));
            if (bookOwnerAddress != null)
            {
                Runtime.Log("UpdateBook: Book not found.");
                return false;
            }
            if (bookOwnerAddress.AsString() != ownerAddress.AsString())
            {
                Runtime.Log("UpdateBook: Book is owned by a different owner.");
                return false;
            }

            //Delete data in storage
            Storage.Delete(Storage.CurrentContext, Key("Book_OwnerAddress", bookId));
            Storage.Delete(Storage.CurrentContext, Key("Book_Title", bookId));
            Storage.Delete(Storage.CurrentContext, Key("Book_Author", bookId));
            Storage.Delete(Storage.CurrentContext, Key("Book_Price", bookId));

            Runtime.Log("DeleteBook: Successfully deleted book.");

            return true;
        }

        private static bool PurchaseBook(byte[] buyerAddress, string orderId, string bookId)
        {
            //Validate input
            if (buyerAddress == null || orderId == null || bookId == null)
            {
                Runtime.Log("PurchaseBook: One or more required parameter is not specified.");
                return false;
            }

            //Get book owner
            byte[] bookOwnerAddress = Storage.Get(Storage.CurrentContext, Key("Book_OwnerAddress", bookId));
            if (bookOwnerAddress == null)
            {
                Runtime.Log("PurchaseBook: Book not found.");
                return false;
            }

            //Check customer balance
            BigInteger bookPrice = Storage.Get(Storage.CurrentContext, Key("Book_Price", bookId)).AsBigInteger();
            if (BalanceOf(buyerAddress) < bookPrice)
            {
                Runtime.Log("PurchaseBook: Buyer has insufficient funds.");
                return false;
            }

            //Transfer funds from customer account to event account
            Transfer(buyerAddress, bookOwnerAddress, bookPrice);

            //Update data in storage
            Storage.Put(Storage.CurrentContext, Key("Purchase_BuyerAddress", orderId), buyerAddress);
            Storage.Put(Storage.CurrentContext, Key("Purchase_BookId", orderId), bookId);

            Runtime.Log("PurchaseBook: Successfully purchased book.");

            return true;
        }

        #endregion

        #region Helpers

        private static string Key(string prefix, byte[] id)
        {
            return string.Concat(prefix, id.AsString());
        }
        private static string Key(string prefix, string id)
        {
            return string.Concat(prefix, id);
        }

        #endregion
        
    }
}
