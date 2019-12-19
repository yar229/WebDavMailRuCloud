using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models
{
    class KnownYadModelConverter : JsonConverter<List<YadResponceModel>>
    {
        private readonly List<object> _createdModels;

        public KnownYadModelConverter(List<object> createdModels)
        {
            _createdModels = createdModels;
        }

        public override List<YadResponceModel> ReadJson(JsonReader reader, Type objectType, List<YadResponceModel> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            var children = token.Children().ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var chToken = children[i];
                var resItem = _createdModels[i];
                serializer.Populate(chToken.CreateReader(), resItem);
            }

            return null;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, List<YadResponceModel> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}