﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.Integrations;
using GmlCore.Interfaces.News;

namespace Gml.Core.Integrations;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
public abstract class BaseNewsProvider : INewsProvider
{
    public abstract Task<IReadOnlyCollection<INewsData>> GetNews(int count = 20);
    public NewsListenerType Type { get; set; }
    public string Url { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is BaseNewsProvider provider)
        {
            return provider.Type == Type;
        }

        return false;
    }

    protected bool Equals(BaseNewsProvider other)
    {
        return Type == other.Type;
    }

    public override int GetHashCode()
    {
        return (int)Type;
    }
}
