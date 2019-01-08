using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitcoinSample
{
    /// <summary>
    /// Extremely
    /// </summary>
    public class TranslationContract
    {
        /// <summary>
        /// Represents the email address of the translation/contractor
        /// </summary>
        [JsonProperty(PropertyName = "e")]
        public string Email { get; set; }

        /// <summary>
        /// Represents the number of words translated for a given project
        /// </summary>
        [JsonProperty(PropertyName = "n")]
        public int NumberOfWords { get; set; }

        /// <summary>
        /// Represents the Source Language (to be translated)
        /// </summary>
        [JsonProperty(PropertyName = "s")]
        public string Source { get; set; }

        /// <summary>
        /// Represents the Destination Languages (that were translated)
        /// </summary>
        [JsonProperty(PropertyName = "d")]
        public string[] Destinations { get; set; }

        public static explicit operator string(TranslationContract c)
        {
            var that = (JObject)JToken.FromObject(c);
            return that.ToString().Replace("\r\n", string.Empty).Replace(" ", string.Empty);
        }

        public static implicit operator TranslationContract(string serializedJson)
        {
            string curatedJsonString = serializedJson.Replace(@"&#34;", "\"");
            JObject jObj = JObject.Parse(curatedJsonString);
            JArray destinationLanguagesArr = (JArray)jObj["d"];
            var contract = new TranslationContract
            {
                Email = (string)jObj["e"],
                NumberOfWords = (int)jObj["n"],
                Source = (string)jObj["s"],
                Destinations = destinationLanguagesArr.Select(destLang => (string)destLang).ToArray()
            };

            return contract;
        }
    }
}
