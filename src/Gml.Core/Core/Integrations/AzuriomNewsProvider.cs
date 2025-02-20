﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Gml.Models.News;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.News;
using Newtonsoft.Json;

namespace Gml.Core.Integrations;

public class AzuriomNewsProvider : BaseNewsProvider
{
    public NewsListenerType Type { get; }
    private readonly string _url;

    public AzuriomNewsProvider(string url, NewsListenerType type)
    {
        Type = type;
        _url = url;
    }

    public override async Task<IReadOnlyCollection<INewsData>> GetNews(int count = 20)
    {
        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync(_url);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<AzuriomNewsResponse[]>(content);

            if (data is null)
                return Array.Empty<INewsData>();

            return data.Select(x => new NewsData
            {
                Title = x.Title ?? "Нет заголовка",
                Content = x.Description ?? "Нет описания",
                Date = x.PublishedAt
            }).ToList();
        }

        return Array.Empty<INewsData>();
    }
}
