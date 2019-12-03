using System;
using qshine.Utility.Http;

namespace TestHttpHelper
{
    class Program
    {
        static void Main(string[] args)
        {

            //https://pokeapi.co/api/v2/pokemon/ditto/
            //https://pokeapi.co/api/v2/evolution-chain/?limit=20&offset=20

            var webapiHelper = new WebApiHelper("https://pokeapi.co/api/v2/");
            var response = webapiHelper.NewGetRequest("evolution-chain/")
                .AddQueryParam("limit","20")
                .AddQueryParam("offset","0")
                .Send();

            var text = response.GetText();

            var result = response.GetJsonData<SampleSearchResult>();


            Console.WriteLine("Hello World!");
        }
    }
}
