# Way2enjoy

C# wrapper for the Way2enjoy API.

The Way2enjoy API allows you to compress and optimize JPEG, PNG & GIF images. It also allows you to optimize all pdf, Music & Video files

**Usage**

```csharp
// Init a new instance of the class.
var way2enjoy = new Way2enjoy("--your-api-key-from-way2enjoy--");

// Upload and shrink an image.
var resp = way2enjoy.Shrink(Server.MapPath("~/wallhaven-88501.jpg"));
```

If you specify a second file in the `Shrink` function, it will automatically download the shrinked file.
You can also pass along credentials and info to tell way2enjoy to upload the shrinked file directly to Amazon S3.

The response object you get back from the wrapper can be used to scale, fit, or adjust the image before downloading it again.

```csharp
// Create a cover with the uploaded image.
way2enjoy.Cover(resp, 200, 200, Server.MapPath("~/wallhaven-88501-tinify-cover-200x200.jpg"));

// Create a thumbnail with the uploaded image.
way2enjoy.Fit(resp, 200, 200, Server.MapPath("~/wallhaven-88501-tinify-fit-200x200.jpg"));

// Scale the uploaded image.
way2enjoy.Scale(resp, 0, 200, Server.MapPath("~/wallhaven-88501-tinify-scale-200x200.jpg"));
```

The HTTP status code and description from the last request can be found in the `way2enjoy.LastHttpStatusCode` and `way2enjoy.LastHttpStatusDescription` respectivly.
