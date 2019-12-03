# Html Request

When perform web http request you can choose different system components.
[HttpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=netframework-4.8), 
[WebClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.webclient?view=netframework-4.8) and
[HttpWebRequest](https://docs.microsoft.com/en-us/dotnet/api/system.net.httpwebrequest?view=netframework-4.8) are
come from dotNet library.
But you also can choose a common web request component from the community to accomplish the same task in easy way.
[RestSharp](),
[Flurl](https://www.nuget.org/packages/Flurl.Http/), 
[ServiceStack http utilities](https://github.com/ServiceStack/ServiceStack.Text) and others.

There are many articals discuss how to choose which component to use for web request and API.

https://www.infoworld.com/article/3198673/my-two-cents-on-webclient-vs-httpclient-vs-httpwebrequest.html

https://www.diogonunes.com/blog/webclient-vs-httpclient-vs-httpwebrequest/

https://code-maze.com/different-ways-consume-restful-api-csharp/

In this library it provides a WebApiHelper using HttpClient for common Web Api tasks:

# WebApiHelper

A WebApiHelper is an instance used to perfrom different http request.

    var httpHelper = new WebRequestApi("https://sheets.googleapis.com/v4/spreadsheets");


### New GET Http Request

The path parameter is a placeholder enclosed by "{" and "}".

The query parameter is parameter pass in query string.

Example: Read a single range 
    
GET https://sheets.googleapis.com/v4/spreadsheets/spreadsheetId/values/Sheet1!A1:D5

```c
    var request = httpHelper.NewGetRequest("/{spreadsheetId}/values/{range}");
    var response = request
        .AddPathParam("spreadsheetId", 12345)
        .AddPathParam("range", "Sheet1!A1:D5")
        .Send();
```
Example: Read a single range grouped by column

GET https://sheets.googleapis.com/v4/spreadsheets/spreadsheetId/values/Sheet1!A1:D3?majorDimension=COLUMNS

```c
    var request = httpHelper.NewGetRequest("/{spreadsheetId}/values/{range}");
    var response = request
        .AddPathParam("spreadsheetId", 12345)
        .AddPathParam("range", "Sheet1!A1:D3")
        .AddQueryParam("majorDimension", "COLUMNS")
        .Send();
```
Example: Read multiple ranges

GET https://sheets.googleapis.com/v4/spreadsheets/spreadsheetId/values:batchGet?ranges=Sheet1!B:B&ranges=Sheet1!D:D&valueRenderOption=UNFORMATTED_VALUES?majorDimension=COLUMNS

```c
    var request = httpHelper.NewGetRequest("/{spreadsheetId}/values:batchGet");
    var response = request
        .AddPathParam("spreadsheetId", 12345)
        .AddQueryParam("ranges", "Sheet1!B:B")
        .AddQueryParam("ranges", "Sheet1!D:D")
        .AddQueryParam("valueRenderOption", "UNFORMATTED_VALUES")
        .AddQueryParam("majorDimension", "COLUMNS")
        .Send();
```

### New PUT Http Request

Send JSON object (default data format is json)

Example: Write a single range

PUT https://sheets.googleapis.com/v4/spreadsheets/spreadsheetId/values/Sheet1!A1:D5?valueInputOption=USER_ENTERED

```c
    var request = httpHelper.NewPutRequest("/{spreadsheetId}/values/{range}");
    var response = request
        .AddPathParam("spreadsheetId", 12345)
        .AddPathParam("range", "Sheet1!A1:D5")
        .AddQueryParam("valueInputOption", "USER_ENTERED")
        .BodyRaw(
@"{
  "range": "Sheet1!A1:D5",
  "majorDimension": "ROWS",
  "values": [
    ["Item", "Cost", "Stocked", "Ship Date"],
    ["Wheel", "$20.50", "4", "3/1/2016"],
    ["Door", "$15", "2", "3/15/2016"],
    ["Engine", "$100", "1", "3/20/2016"],
    ["Totals", "=SUM(B2:B4)", "=SUM(C2:C4)", "=MAX(D2:D4)"]
  ],
}"
        )
        .Send();
```

or 

```c
    var bodyData = new SpreedsheetRange
        {
            range ="Sheet1!A1:D5",
            majorDimension = "ROWS",
            values = new Cells {
                {"Item", "Cost", "Stocked", "Ship Date"},
                {"Wheel", "$20.50", "4", "3/1/2016"},
                {"Door", "$15", "2", "3/15/2016"},
                {"Engine", "$100", "1", "3/20/2016"},
                {"Totals", "=SUM(B2:B4)", "=SUM(C2:C4)", "=MAX(D2:D4)"}
            },        
        };
    var response = request
        .AddPathParam("spreadsheetId", 12345)
        .AddPathParam("range", "Sheet1!A1:D5")
        .AddQueryParam("valueInputOption", "USER_ENTERED")
        .BodyRaw(bodyData)
        .Send();
```

### POST HTTP Request

Default post form items

```c
    var request = httpHelper.NewPostRequest("/Save");
    var response = request
        .AddBodyProperty("Id", 12345)
        .AddBodyProperty("FirstName", "John")
        .AddBodyProperty("LastName", "Kanj")
        .Send();
```

### Post an object in JSON format

```c
    var person = new Person{
        Id=1,
        Name ="John C"
    };
    var request = httpHelper.NewPostRequest("/Save");
    var response = request
        .JsonBody<Person>(person)
        .Send();
```

### Post a single file

```c
    var request = httpHelper.NewPostRequest("/UploadFile");
    var response = request
        .AddFile("c:/library/picture/pic1.jpg")
        .Send();
```

### Post simple multipart items

```c
    var request = httpHelper.NewPostRequest("/Save");
    var response = request
        .BodyDataType(CommonContentType.MultipartFormData)
        .AddBodyProperty("Id", 12345)
        .AddBodyProperty("FirstName", "John")
        .AddBodyProperty("LastName", "Kanj")
        .AddBodyProperty("Comments", Encoding.Unicode.GetBytes("unicode text..."))
        .Send();
```

### Post many files with additional properties

```c
    //Create a new Post request
    var request = httpHelper.NewPostRequest("/SaveFiles");
    
    var response = request
        .BodyDataType(CommonContentType.MultipartMix)
        .AddBodyProperty("PictureLocation", "/folder1")
        .AddFile("c:/library/picture/pic1.jpg")
        .AddFile("c:/library/picture/pic2.jpeg")
        .AddBodyProperty("DocumentLocation", "/folder2")
        .AddFile("c:/library/document/sample1.pdf")
        .AddFile("c:/library/document/sample2.pdf", "template.pdf")
        .Send();
```

### Post an object in XML format

```c
    var person = new Person{
        Id=1,
        Name ="John C"
    };
    var request = httpHelper.NewPostRequest("/Save");
    var response = request
        .XmlBody<Person>(person,Encoding.UTF8)
        .Send();
```

### Post raw html body data

```c
    var request = httpHelper.NewPostRequest("/Message");
    var response = request
        .BodyDataType(CommonContentType.Binary)
        .BodyRaw(binaryData)
        .Send();
```

## DELETE HTTP Request

```c
    var request = httpHelper.NewDeleteRequest("/Delete");
    var response = request
        .AddQueryParam("Id",12345)
        .Send();
```
