# MMLib.SwaggerForOcelot
This repo will be contains swagger extension for ocelot.

- [ ] návrh
- [ ] dokončiť demo príklad
- [ ] Do komba select specifi pridávať jednotlivé služby
- [ ] Spraviť middleware, ktorý bude pridávať endpointy pre jednotlivé služby
- [ ] Nahradiť cestu v endpointoch
- [ ] Filtrovať len tie endpointy, ktoré naozaj sú ocelot konfigurácií
- [ ] Unit testy
- [ ] Dokumentácia
- [ ] Zverejniť

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
    "Swagger": {
        "ServiceName":"Service1",
        "SwaggerEndPoint":"/swagger/v1/swagger.json"
    }
}
```

```
app.UseSwaggerForOcelot();
```