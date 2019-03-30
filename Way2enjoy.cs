/**
 * @file
 * The Way2enjoy API allows you to compress and optimize JPEG, PNG & GIF images.You can also compress PDF, Any Music or Video files
 * 
 * @author
 * way2enjoy2 <support@way2enjoy.com>
 * 
 * @documentation
 * https://way2enjoy.com/developers
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

/// <summary>
/// The Way2enjoy API allows you to compress and optimize JPEG, PNG & GIF images. YOu can also optimize PDF, Music & Video files
/// </summary>
public class Way2enjoy {
	/// <summary>
	/// Base 64 encoded API key.
	/// </summary>
	private string base64ApiKey { get; set; }

	/// <summary>
	/// HTTP status code from the last request.
	/// </summary>
	public int LastHttpStatusCode { get; set; }

	/// <summary>
	/// HTTP status description from the last request.
	/// </summary>
	public string LastHttpStatusDescription { get; set; }

	/// <summary>
	/// The Way2enjoy API allows you to compress and optimize JPEG, PNG & GIF images.You can also compress PDF, Music & Video files
	/// </summary>
	/// <param name="apiKey"></param>
	public Way2enjoy(string apiKey) {
		this.base64ApiKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey));
	}

	/// <summary>
	/// Scales the image proportionally and crops it if necessary so that the result has exactly the given dimensions. You must provide both a width and a height.
	/// </summary>
	/// <param name="response">Response object from a upload/shrink operation.</param>
	/// <param name="width">Width to scale within.</param>
	/// <param name="height">Height to scale within.</param>
	/// <param name="outputFile">Filename to save to.</param>
	/// <param name="store">Amazon S3 credentials and information.</param>
	/// <returns>Stream</returns>
	public Stream Cover(Way2enjoyResponse response, int width, int height, string outputFile = null, Way2enjoyOptionsStore store = null) {
		return executeOption(response, width, height, "cover", outputFile, store);
	}

	/// <summary>
	/// Scales the image down proportionally so that it fits within the given dimensions. You must provide both a width and a height.
	/// </summary>
	/// <param name="response">Response object from a upload/shrink operation.</param>
	/// <param name="width">Width to scale within.</param>
	/// <param name="height">Height to scale within.</param>
	/// <param name="outputFile">Filename to save to.</param>
	/// <param name="store">Amazon S3 credentials and information.</param>
	/// <returns>Stream</returns>
	public Stream Fit(Way2enjoyResponse response, int width, int height, string outputFile = null, Way2enjoyOptionsStore store = null) {
		return executeOption(response, width, height, "fit", outputFile, store);
	}

	/// <summary>
	/// Scales the image down proportionally. You must provide either a target width or a target height, but not both.
	/// </summary>
	/// <param name="response">Response object from a upload/shrink operation.</param>
	/// <param name="width">Width to scale within.</param>
	/// <param name="height">Height to scale within.</param>
	/// <param name="outputFile">Filename to save to.</param>
	/// <param name="store">Amazon S3 credentials and information.</param>
	/// <returns>Stream</returns>
	public Stream Scale(Way2enjoyResponse response, int width, int height, string outputFile = null, Way2enjoyOptionsStore store = null) {
		return executeOption(response, width, height, "scale", outputFile, store);
	}
	
	/// <summary>
	/// Upload image to Way2enjoy and attempt to shrink it, lossless.
	/// </summary>
	/// <param name="inputFile">File to upload.</param>
	/// <param name="outputFile">Filename to save to.</param>
	/// <param name="store">Amazon S3 credentials and information.</param>
	/// <returns>JSON with info about the upload/shrink.</returns>
	public Way2enjoyResponse Shrink(string inputFile, string outputFile = null, Way2enjoyOptionsStore store = null) {
		var response = new JavaScriptSerializer()
			.Deserialize<Way2enjoyResponse>(
				getResponse(request(binaryFile: inputFile)));

		if (outputFile == null ||
			response.output == null ||
			string.IsNullOrEmpty(response.output.url))
			return response;

		if (store != null)
			executeOption(
				response,
				null,
				null,
				null,
				null,
				store);

		var webClient = new WebClient();

		webClient.DownloadFile(
			response.output.url,
			outputFile);

		return response;
	}

	/// <summary>
	/// Execute a set of options against a previous uploaded/shrinked file.
	/// </summary>
	/// <param name="response">Response object from a upload/shrink operation.</param>
	/// <param name="width">Width to scale within.</param>
	/// <param name="height">Height to scale within.</param>
	/// <param name="method">Scale method to apply.</param>
	/// <param name="outputFile">Filename to save to.</param>
	/// <param name="store">Amazon S3 credentials and information.</param>
	/// <returns>Stream</returns>
	private Stream executeOption(Way2enjoyResponse response, int? width, int? height, string method = null, string outputFile = null, Way2enjoyOptionsStore store = null) {
		var options = new Way2enjoyOptions();

		if (method != null &&
		    (width.HasValue ||
		     height.HasValue)) {
			options.resize = new Way2enjoyOptionsResize {
				method = method
			};

			if (width.HasValue)
				options.resize.width = width.Value;

			if (height.HasValue)
				options.resize.height = height.Value;
		}

		if (store != null)
			options.store = store;

		var stream = request(
			"POST",
			response.output.url,
			null,
			options);

		writeStreamToDisk(
			stream,
			outputFile);

		return stream;
	}

	/// <summary>
	/// Get the response as a string.
	/// </summary>
	/// <param name="responseStream">Stream to read from.</param>
	/// <returns>String</returns>
	private string getResponse(Stream responseStream) {
		if (responseStream == null)
			return null;

		var output = new List<byte>();
		var buffer = new byte[1024];
		int byteCount;

		do {
			byteCount = responseStream.Read(buffer, 0, buffer.Length);

			for (var i = 0; i < byteCount; i++)
				output.Add(buffer[i]);

		} while (byteCount > 0);

		return Encoding.UTF8.GetString(output.ToArray());
	}

	/// <summary>
	/// Performs the actual communication towards the Way2enjoy API.
	/// </summary>
	/// <param name="method">HTTP method to perform.</param>
	/// <param name="url">URL to request.</param>
	/// <param name="binaryFile">File to upload.</param>
	/// <param name="options">Options to pass along to the API.</param>
	/// <returns>Stream</returns>
	private Stream request(string method = "POST", string url = null, string binaryFile = null, Way2enjoyOptions options = null) {
		if (url == null)
			url = "https://way2enjoy.com/modules/compress-png/way2enjoy-cli2.php";

		var request = WebRequest.Create(url) as HttpWebRequest;

		if (request == null)
			throw new WebException("Could not create webrequest.");

		request.Method = method;
		request.Headers.Add(
			"Authorization",
			"Basic " + this.base64ApiKey);

		if (!string.IsNullOrEmpty(binaryFile)) {
			var requestStream = request.GetRequestStream();
			var bytes = File.ReadAllBytes(binaryFile);
			requestStream.Write(bytes, 0, bytes.Length);
		}

		if (options != null) {
			request.ContentType = "application/json";

			var requestStream = request.GetRequestStream();
			var json = new JavaScriptSerializer().Serialize(options);
			var bytes = Encoding.UTF8.GetBytes(json);

			requestStream.Write(bytes, 0, bytes.Length);
		}

		this.LastHttpStatusCode = 0;
		this.LastHttpStatusDescription = null;

		HttpWebResponse response = null;

		try {
			response = request.GetResponse() as HttpWebResponse;

			if (response == null)
				throw new Exception("Request returned NULL response.");

			this.LastHttpStatusCode = (int) response.StatusCode;
			this.LastHttpStatusDescription = response.StatusDescription;
		}
		catch (WebException ex) {
			var erres = ex.Response as HttpWebResponse;

			if (erres == null)
				throw new Exception("Request returned NULL response.");

			this.LastHttpStatusCode = (int) erres.StatusCode;
			this.LastHttpStatusDescription = erres.StatusDescription;
		}

		return response != null
			? response.GetResponseStream()
			: null;
	}

	/// <summary>
	/// Write a stream to disk.
	/// </summary>
	/// <param name="stream">Stream to write.</param>
	/// <param name="outputFile">File to write too.</param>
	private void writeStreamToDisk(Stream stream, string outputFile) {
		if (outputFile == null)
			return;

		if (stream == null)
			return;

		var output = new List<byte>();
		var buffer = new byte[1024];
		int byteCount;

		do {
			byteCount = stream.Read(buffer, 0, buffer.Length);

			for (var i = 0; i < byteCount; i++)
				output.Add(buffer[i]);

		} while (byteCount > 0);

		using (var fileStream = File.Create(outputFile))
			fileStream.Write(output.ToArray(), 0, output.Count);
	}
}

public class Way2enjoyResponse {
	public Way2enjoyResponseInput input { get; set; }
	public Way2enjoyResponseOutput output { get; set; }
	public string error { get; set; }
	public string message { get; set; }
}

public class Way2enjoyResponseInput {
	public int size { get; set; }
	public string type { get; set; }
}

public class Way2enjoyResponseOutput {
	public int size { get; set; }
	public string type { get; set; }
	public int width { get; set; }
	public int height { get; set; }
	public decimal ratio { get; set; }
	public string url { get; set; }
}

public class Way2enjoyOptions {
	public Way2enjoyOptionsResize resize { get; set; }
	public Way2enjoyOptionsStore store { get; set; }
}

public class Way2enjoyOptionsResize {
	public string method { get; set; }
	public int width { get; set; }
	public int height { get; set; }
}

public class Way2enjoyOptionsStore {
	public string service = "s3";
	public string aws_access_key_id { get; set; }
	public string aws_secret_access_key { get; set; }
	public string region { get; set; }
	public string path { get; set; }
}
