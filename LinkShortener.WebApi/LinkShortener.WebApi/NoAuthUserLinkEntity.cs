﻿using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace LinkShortener.WebApi
{
    public class NoAuthUserLinkEntity
    {
        [BsonId]
        public Guid Id { get; set; }
        public List<string> Links { get; set; }
    }
}