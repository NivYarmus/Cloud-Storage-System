﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NivDrive.Protocol.Types
{
    internal class DictionaryStringObjectJsonConverter : JsonConverter<Dictionary<string, object>>
    {
        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, object>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"JsonTokenType was of type {reader.TokenType}, only objects are supported");

            var dictionary = new Dictionary<string, object>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return dictionary;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("JsonTokenType was not PropertyName");

                var propertyName = reader.GetString();
                if (string.IsNullOrEmpty(propertyName))
                    throw new JsonException("Failed to get property name");

                reader.Read();
                dictionary.Add(propertyName, ExtractValue(ref reader, options));
            }
            return dictionary;
        }

        public string Write(object value, Type inputType, JsonSerializerOptions options)
        {
            return JsonSerializer.Serialize(value, inputType, options);
        }

        private object? ExtractValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.Number:
                    return reader.GetInt64();
                case JsonTokenType.StartObject:
                    return Read(ref reader, null, options);
                case JsonTokenType.StartArray:
                    var list = new List<object>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        list.Add(ExtractValue(ref reader, options));
                    return list;
                default:
                    throw new JsonException($"'{reader.TokenType}' is not supported");
            }
        }
    }
}
