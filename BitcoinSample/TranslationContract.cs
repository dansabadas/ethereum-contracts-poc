using System.Linq;
using Newtonsoft.Json.Linq;

namespace BitcoinSample
{
    public class TranslationContract
    {
        /// <summary>
        /// Represents the email address of the translation/contractor
        /// </summary>
        public string E { get; set; }

        /// <summary>
        /// Represents the number of words translated for a given project
        /// </summary>
        public int N { get; set; }

        /// <summary>
        /// Represents the Source Language (to be translated)
        /// </summary>
        public string S { get; set; }

        /// <summary>
        /// Represents the Destination Languages (that were translated)
        /// </summary>
        public string[] D { get; set; }

        public static explicit operator string(TranslationContract c)
        {
            var that = (JObject)JToken.FromObject(c);
            return that.ToString().Replace("\r\n", string.Empty).Replace(" ", string.Empty);
        }

        public static implicit operator TranslationContract(string serializedJson)
        {
            string curatedJsonString = serializedJson.Replace(@"&#34;", "\"");
            JObject jObj = JObject.Parse(curatedJsonString);
            JArray destinationLanguagesArr = (JArray)jObj["D"];
            var contract = new TranslationContract
            {
                E = (string)jObj["E"],
                N = (int)jObj["N"],
                S = (string)jObj["S"],
                D = destinationLanguagesArr.Select(destLang => (string)destLang).ToArray()
            };

            return contract;
        }
    }
}
