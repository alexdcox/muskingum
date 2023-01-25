using System;
using Newtonsoft.Json;
using HexGame;
using Newtonsoft.Json.Linq;

public class UnitCollectionJsonConverter : JsonConverter {
  public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
    // JToken t = JToken.FromObject(value);
    // if (t.Type != JTokenType.Object) {
    //   t.WriteTo(writer);
    // } else {
    //   JObject o = (JObject)t;
    //   IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();
    //   o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));
    //   o.WriteTo(writer);
    // }

    // JArray array = new JArray();
    // array.Add("Manual text");
    // array.Add(new DateTime(2000, 5, 23));

    // writer.WriteStartArray();
    // writer.WriteValue(1);
    // writer.WriteValue(2);
    // writer.WriteEndArray();

    // List<int> unitIds = new();
    // UnitCollection unitCollection = (UnitCollection)value;
    // foreach(Unit unit in unitCollection) {
    //   unitIds.Add(unit.Id());
    // }
    // string json = "[" + unitIds.Join(",") + "]";
    // Debug.Log("Converted to json: " + json);
    // writer.WriteJson(json);

    // writer.WriteStartArray();
    // UnitCollection unitCollection = (UnitCollection)value;
    // foreach(Unit unit in unitCollection) {
    //   writer.WriteValue(unit.Id());
    // }
    // writer.WriteEndArray();
  }

  public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
    // JArray blogPostArray = JArray.Parse(json);
    // IList<BlogPost> blogPosts = blogPostArray.Select(p => new BlogPost
    // {
    //     Title = (string)p["Title"],
    //     AuthorName = (string)p["Author"]["Name"],
    //     AuthorTwitter = (string)p["Author"]["Twitter"],
    //     PostedDate = (DateTime)p["Date"],
    //     Body = HttpUtility.HtmlDecode((string)p["BodyHtml"])
    // }).ToList();

    // JArray categories = (JArray)rss["channel"]["item"][0]["category"];
    // Console.WriteLine(categories);
    // // [
    // //   "Json.NET",
    // //   "CodePlex"
    // // ]
    // string[] categoriesText = categories.Select(c => (string)c).ToArray();
    // Console.WriteLine(string.Join(", ", categoriesText));

    // string json = @"['Starcraft','Halo','Legend of Zelda']";
    // List<string> videogames = JsonConvert.DeserializeObject<List<string>>(json);
    // Console.WriteLine(string.Join(", ", videogames.ToArray()));

    // JArray unitIdsJson = JArray.ReadFrom(reader);
    // Unit[] unitIds = unitIdsJson.Select(c => (int)c).ToArray();
    // UnitCollection c = (UnitCollection)existingValue;

    return null;
  }

  public override bool CanConvert(Type objectType) {
    return objectType == typeof(UnitCollection);
    // return _types.Any(t => t == objectType);
  }
}