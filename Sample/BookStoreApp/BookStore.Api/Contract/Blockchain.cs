using System;
using Neo.Lux.Cryptography;
using Neo.Lux.Core;
using Neo.Lux.Utils;

namespace BookStore.Api.Contract
{
    public static class Blockchain
    {
        //The private key of the wallet used to deploy the smart contract (In hex format)
        private static string privateKey = "";

        //Get this when deploying your contract to the blockchain (In hex format)
        private static string contractScriptHash = ""; 

        public static bool InvokeScript(string method, object[] values)
        {
            try
            {
                var key = new KeyPair(privateKey.HexToBytes());
                var scriptHash = new UInt160(contractScriptHash.HexToBytes());

                var api = NeoRPC.ForTestNet();
                var response = api.InvokeScript(scriptHash, method, values);
                if (response != null && response.result != null)
                {
                    return response.result.GetBoolean();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Null response received on InvokeScript");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public static bool CallContract(string method, object[] values)
        {
            try
            {
                var key = new KeyPair(privateKey.HexToBytes());
                var scriptHash = new UInt160(contractScriptHash.HexToBytes());

                var api = NeoRPC.ForTestNet();
                var response = api.CallContract(key, scriptHash, method, values);
                if (response != null)
                {
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Null response received on CallContract");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}