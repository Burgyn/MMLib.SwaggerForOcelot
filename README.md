# MMLib.SwaggerForOcelot
This repo will be contains swagger extension for ocelot.

- [x] návrh
- [x] dokončiť demo príklad
- [x] Do komba select specifi pridávať jednotlivé služby
- [x] Spraviť middleware, ktorý bude pridávať endpointy pre jednotlivé služby
- [x] Nahradiť cestu v endpointoch
- [ ] Umožniť konfigurovať SwaggerUI metódu. Ale asi prida+t aj nie4o ako EndpointPrefix napr `/swagger/v1/{projects}`
- [ ] Filtrovať len tie endpointy, ktoré naozaj sú ocelot konfigurácií
- [ ] Unit testy, upratať kód
- [ ] Dokumentácia
- [ ] Zverejniť
- [ ] Vlastné HTML + CSS

```
{
    "DownstreamPathTemplate": "/api/posts/{postId}",
    "DownstreamScheme": "https",
    "DownstreamHostAndPorts": [
            {
                "Host": "localhost",
                "Port": 80,
            }
        ],
    "UpstreamPathTemplate": "/posts/{postId}",
    "UpstreamHttpMethod": [ "Put", "Delete" ],
    "SwaggerKey": "projects"
}

"SwaggerEndPoints": [
    {
      "Key": "projects",
      "Name": "Projects API",
      "Url": "/test2.json"
    },
    {
      "Key": "contacts",
      "Name": "Contacts API",
      "Url": "/test.json"
    }
  ]
```

```
app.UseSwaggerForOcelot();
```