﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Gml.Models.News;
using GmlCore.Interfaces.Integrations;
using GmlCore.Interfaces.News;
using Newtonsoft.Json;

namespace Gml.Core.Integrations;

public class CustomNewsProvider : INewsProvider
{
    private readonly string _url;

    public CustomNewsProvider(string url)
    {
        _url = url;
    }

    public async Task<IReadOnlyCollection<INews>> GetNews(int count = 20)
    {
        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync(_url);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            var decoded = WebUtility.HtmlDecode(content);

            var data = JsonConvert.DeserializeObject<CustomNewsResponse[]>(decoded);

            if (data is null)
                return Array.Empty<INews>();

            return data.Select(x => new News
            {
                Title = x.Title ?? "Нет заголовка",
                Content = x.Description ?? "Нет описания",
                Date = x.CreatedAt
            }).ToList();
        }

        return Array.Empty<INews>();
    }
}
