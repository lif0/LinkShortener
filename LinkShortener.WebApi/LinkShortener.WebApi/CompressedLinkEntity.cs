﻿using MongoDB.Bson.Serialization.Attributes;

namespace LinkShortener.WebApi
{
    public class CompressedLinkEntity
    {
        [BsonId]
        public string Id { get; set; }

        public string Link { get; set; }
        public int OpenedCount { get; set; }
    }
}