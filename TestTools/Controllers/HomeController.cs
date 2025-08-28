using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using TestTools.Models;
using TestTools.Services; // Use new services

namespace TestTools.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string[] _documentCategories = { "Education", "Framework", "Support", "Instructions" };
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _rootPath;
        private readonly DocumentConversionService _documentConversionService;
        private readonly ImageConversionService _imageConversionService;
        private readonly Base64Service _base64Service;
        private readonly IWebScraperService _webScraperService;
        private readonly IConfiguration _configuration;
        private readonly string _apiEndpointsPath;

        private readonly XmlCompareService _xmlCompareService;

        public HomeController(
            ILogger<HomeController> logger,
            IWebHostEnvironment webHostEnvironment,
            DocumentConversionService documentConversionService,
            ImageConversionService imageConversionService,
            Base64Service base64Service,
            IWebScraperService webScraperService,
            IConfiguration configuration
            )
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _rootPath = Path.Combine(_webHostEnvironment.WebRootPath, "exampleFiles");
            _documentConversionService = documentConversionService;
            _imageConversionService = imageConversionService;
            _base64Service = base64Service;
            _xmlCompareService = new XmlCompareService();
            _webScraperService = webScraperService;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var frameworkGuides = new FrameworkGuidesViewModel
            {
                EducationDocuments = GetDocumentsFromCategory("Education"),
                FrameworkDocuments = GetDocumentsFromCategory("Framework")
            };
            var supportGuides = new SupportGuidesViewModel
            {
                SupportDocuments = GetDocumentsFromCategory("Support"),
                InstructionsDocuments = GetDocumentsFromCategory("Instructions")
            };
            var model = new DocumentTabsViewModel
            {
                FrameworkGuides = frameworkGuides,
                SupportGuides = supportGuides
            };
            return View(model);
        }

        #region Document Management

        // Document management methods
        private List<DocumentModel> GetDocumentsFromCategory(string category)
        {
            var folderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Documents", category);
            if (!Directory.Exists(folderPath))
            {
                return new List<DocumentModel>();
            }

            return Directory.GetFiles(folderPath)
                .Select(filePath => new DocumentModel
                {
                    Name = Path.GetFileName(filePath),
                    Path = $"/Home/ViewDocument?path={Uri.EscapeDataString(Path.Combine("Documents", category, Path.GetFileName(filePath)))}",
                    Category = category,
                    Size = new FileInfo(filePath).Length
                })
                .ToList();
        }

        public IActionResult ViewDocument(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, path);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out string? contentType))
            {
                contentType = "application/octet-stream";
            }

            return File(System.IO.File.OpenRead(filePath), contentType);
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument(DocumentUploadModel model)
        {
            // Only one check for null file and model state
            if (!ModelState.IsValid || model.File == null)
            {
                TempData["UploadError"] = "No file selected.";
                return RedirectToAction(nameof(Index));
            }
            if (!ModelState.IsValid)
            {
                TempData["UploadError"] = "Invalid model state.";
                return RedirectToAction(nameof(Index));
            }

            if (!_documentCategories.Contains(model.Category))
            {
                TempData["UploadError"] = "Invalid category.";
                return RedirectToAction(nameof(Index));
            }

            // Validate PDF extension and MIME type
            var allowedExtension = ".pdf";
            var allowedMime = "application/pdf";
            var fileExtension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
            if (fileExtension != allowedExtension || model.File.ContentType != allowedMime)
            {
                TempData["UploadError"] = "Only PDF files are allowed.";
                return RedirectToAction(nameof(Index));
            }

            var folderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Documents", model.Category);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, model.File.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            return RedirectToAction(nameof(Index));
        }

        // Partial view methods for tabs
        [HttpGet]
        public IActionResult GetFrameworksAndGuidesTab()
        {
            var model = new DocumentsViewModel
            {
                EducationDocuments = GetDocumentsFromCategory("Education"),
                FrameworkDocuments = GetDocumentsFromCategory("Framework")
            };

            return PartialView("_FrameworksAndGuidesTab", model);
        }

        [HttpGet]
        public IActionResult GetTestingSupportTab()
        {
            var model = new DocumentsViewModel
            {
                SupportDocuments = GetDocumentsFromCategory("Support"),
                InstructionsDocuments = GetDocumentsFromCategory("Instructions")
            };

            return PartialView("_TestingSupportTab", model);
        }

        #endregion

        #region Contact Form

        public class ContactFormHandlerModel : PageModel
        {
            [BindProperty]
            public ContactFormModel.ContactFormInput Input { get; set; }

            public string SuccessMessage { get; set; }
            public string ErrorMessage { get; set; }

            public void OnGet()
            {
            }

            public async Task<IActionResult> OnPostAsync(ContactFormModel.ContactFormInput model)
            {
                if (!ModelState.IsValid)
                    return Page();

                try
                {
                    var message = new MailMessage();
                    message.To.Add("tom.connor@wales.nhs.uk");
                    message.Subject = Input.Subject;
                    message.Body = $"Name: {Input.Name}\nEmail: {Input.Email}\n\n{Input.Message}";
                    message.From = new MailAddress(model.Email);

                    SmtpClient smtpClient = new SmtpClient("smtp.office365.com");
                    smtpClient.Port = 587;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential("SingleRecord.Test3@wales.nhs.uk", "SmallOrangeTiger6"); // Replace with your email/nadex credentials as required - te252115
                    smtpClient.EnableSsl = true;

                    // Send email
                    smtpClient.Send(message);

                    SuccessMessage = "Thank you for contacting us! We have received your message.";
                    ModelState.Clear();
                    Input = new ContactFormModel.ContactFormInput();
                }
                catch
                {
                    ErrorMessage = "There was an error sending your message. Please try again later.";
                }

                return Page();
            }

        }

        #endregion

        #region Service Checker

        public IActionResult ServiceChecker()
        {
            var endpoints = GetApiEndpoints();
            return View(endpoints);
        }

        [HttpGet]
        public async Task<JsonResult> CheckEndpoint(string url)
        {
            var result = await CheckApiAsync(url);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> CheckAllEndpoints()
        {
            // NOTE: ASP.NET MVC does not support true response streaming via yield return in JsonResult.
            // For now, we return the results as a list after checking each endpoint one at a time.
            // Future: Consider SignalR or chunked responses for live streaming to the client.
            var endpoints = GetApiEndpoints();
            var results = new List<ApiEndpointModel>();
            foreach (var api in endpoints)
            {
                var result = await CheckApiAsync(api.Url);
                result.Name = api.Name;
                result.Url = api.Url;
                results.Add(result);
                // Optionally: log or hook here for partial progress
            }
            return Json(results);
        }

        [HttpPost]
        public IActionResult AddApiEndpoint([FromBody] ApiEndpointModel newEndpoint)
        {
            if (string.IsNullOrWhiteSpace(newEndpoint.Name) || string.IsNullOrWhiteSpace(newEndpoint.Url))
            {
                return BadRequest("Name and URL are required.");
            }
            // Prototype: Add directly to appsettings.json
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "appsettings.json");
            try
            {
                string json = System.IO.File.ReadAllText(filePath);
                var jObj = JObject.Parse(json);
                var endpoints = jObj["ApiEndpoints"] as JArray ?? new JArray();
                var newJObj = new JObject
                {
                    ["Name"] = newEndpoint.Name,
                    ["Url"] = newEndpoint.Url
                };
                endpoints.Add(newJObj);
                jObj["ApiEndpoints"] = endpoints;
                System.IO.File.WriteAllText(filePath, jObj.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add API endpoint to appsettings.json");
                return StatusCode(500, "Failed to add endpoint: " + ex.Message);
            }
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetEndpointMetrics()
        {
            // Only include active endpoints
            var endpoints = GetApiEndpoints().Where(x => x.Active).ToList();
            var results = new List<ApiEndpointModel>();
            foreach (var api in endpoints)
            {
                var result = await CheckApiAsync(api.Url);
                result.Name = api.Name;
                result.Url = api.Url;
                results.Add(result);
                // Optionally: log or hook here for partial progress
            }
            return Json(results);
        }

        private List<ApiEndpointModel> GetApiEndpoints()
        {
            var endpoints = _configuration.GetSection("ApiEndpoints").Get<List<ApiEndpointModel>>() ?? new List<ApiEndpointModel>();
            return endpoints;
        }

        // Stub/mock API endpoint for testing
        // You can change status/response time by query: /Home/MockApi?status=200&delay=100
        [HttpGet]
        public IActionResult MockApi(int status = 200, int delay = 100)
        {
            System.Threading.Thread.Sleep(delay); // Simulate delay
            if (status == 200)
                return Json(new { status = 200, message = "OK" });
            else
                return StatusCode(500, new { status = 500, message = "Error" });
        }

        private async Task<ApiEndpointModel> CheckApiAsync(string url)
        {
            var model = new ApiEndpointModel { Url = url };
            var client = new HttpClient();
            var sw = Stopwatch.StartNew();
            try
            {
                var baseUrl = Request.Scheme + "://" + Request.Host;
                var fullUrl = url.StartsWith("http") ? url : baseUrl + url;
                var response = await client.GetAsync(fullUrl);
                sw.Stop();
                var responseTime = (int)sw.ElapsedMilliseconds;
                string responseBody = await response.Content.ReadAsStringAsync();
                int jsonStatus = 0;
                try
                {
                    using var doc = JsonDocument.Parse(responseBody);
                    if (doc.RootElement.TryGetProperty("status", out var statusProp))
                        jsonStatus = statusProp.GetInt32();
                }
                catch { }

                int httpStatus = (int)response.StatusCode;
                model.ResponseTimeMs = responseTime;

                // Set status based on HTTP response code and response time
                if (httpStatus == 200)
                {
                    if (responseTime < 500)
                    {
                        model.Status = "Active";
                    }
                    else
                    {
                        model.Status = "Slow";
                    }
                }
                else if (httpStatus == 503)
                {
                    model.Status = "Unavailable";
                }
                else
                {
                    // Any other HTTP response should be considered unavailable
                    model.Status = "Unavailable";
                }

                // Format the status detail according to requirements
                model.StatusDetail = $"HTTP Response = {httpStatus} / Response Time = {responseTime}ms";
            }
            catch (Exception ex)
            {
                sw.Stop();
                model.Status = "Unavailable";
                model.StatusDetail = ex.Message;
                model.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            }
            return model;
        }

        #endregion

        #region PatientGenerator

        public IActionResult PatientGenerator()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePatients(int count)
        {
            try
            {
                if (count <= 0 || count > 1000)
                {
                    return BadRequest("Count must be between 1 and 1000.");
                }

                var patients = PatientService.GeneratePatients(count);
                return PartialView("_PatientTable", patients);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating patients: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateMasterPatient()
        {
            try
            {
                var patient = Patient.GenerateRandomPatient();
                var hl7Message = GenerateHL7Message(patient);
                return Ok(hl7Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating HL7 message: {ex.Message}");
            }
        }

        private string GenerateHL7Message(Patient patient)
        {
            var timestamp = DOBGenerator.AltDob.Generate();
            var wpasRef = WpasRef.GetWDSWpas;
            var schemaNumber = "129";
            var address = "Ty Glan-yr-Afon^21 Cowbridge Road East^^Cardiff^CF11 9AD^^H";
            // Use patient.Sex for M/F in HL7 message
            var hl7 = $"MSH|^~\\&|{schemaNumber}|{schemaNumber}|100|100|{timestamp}||ADT^A28^ADT_A05|{timestamp}|P|2.5|||||||EN\r\n";
            hl7 += $"EVN||{timestamp}||||\r\n";
            hl7 += $"PID|||{patient.GetNhsNumber.Replace(" ", "")}^^^NHS^NH~{wpasRef}^^^{schemaNumber}^PI||{patient.GetSurname}^{patient.GetName}^^^{patient.GetTitle}^^||{DOBGenerator.AltDob.Generate()}|{patient.Sex}|||{address}\r\n";
            hl7 += $"PD1|||^^w92616|G8512071\r\n";
            hl7 += $"PV1||N\r\n";
            return hl7;
        }

        [HttpGet]
        public async Task<IActionResult> GenerateAllWalesPatients()
        {
            try
            {
                var patient = Patient.GenerateRandomPatient();
                var hl7Message = Generatehl7Message(patient);
                return Ok(hl7Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating All Wales HL7 messages: {ex.Message}");
            }
        }

        // Overload for schema number
        private string Generatehl7Message(Patient patient)
        {
            var wpasRef = WpasRef.GetWDSWpas;
            var hddRef = WpasRef.GetHddWpas;
            var cttRef = WpasRef.GetCttWpas;
            var sbuRef = WpasRef.GetSbuWpas;
            var schemaNumber1 = "129"; // All Wales (MPI)
            var schemaNumber2 = "149"; // Hywel Dda (HDD)
            var schemaNumber3 = "126"; // Cwm Taff (CTT)
            var schemaNumber4 = "108"; // Swansea Bay (SBU)
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var Dob = DOBGenerator.AltDob.Generate();
            var address = "Ty Glan-yr-Afon^21 Cowbridge Road East^^Cardiff^CF11 9AD^^H";
            var hl7 = "===== All Wales (MPI) =====\r\n";
            hl7 += $"MSH|^~\\&|{schemaNumber1}|{schemaNumber1}|100|100|{timestamp}||ADT^A28^ADT_A05|{timestamp}|P|2.5|||||||EN\r\n";
            hl7 += $"EVN||{timestamp}||||\r\n";
            hl7 += $"PID|||{patient.GetNhsNumber.Replace(" ", "")}^^^NHS^NH~{wpasRef}^^^{schemaNumber1}^PI||{patient.GetSurname}^{patient.GetName}^^^{patient.GetTitle}^^||{Dob}|{patient.Sex}|||{address}\r\n";
            hl7 += $"PD1|||^^w92616|G8512071\r\n";
            hl7 += $"PV1||N\r\n";
            hl7 += "\r\n\n";
            hl7 += "===== Hywel Dda (HDD) =====\r\n";
            hl7 += $"MSH|^~\\&|{schemaNumber2}|{schemaNumber2}|100|100|{timestamp}||ADT^A28^ADT_A05|{timestamp}|P|2.5|||||||EN\r\n";
            hl7 += $"EVN||{timestamp}||||\r\n";
            hl7 += $"PID|||{patient.GetNhsNumber.Replace(" ", "")}^^^NHS^NH~{wpasRef}^^^{schemaNumber2}^PI||{patient.GetSurname}^{patient.GetName}^^^{patient.GetTitle}^^||{Dob}|{patient.Sex}|||{address}\r\n";
            hl7 += $"PD1|||^^w92616|G8512071\r\n";
            hl7 += $"PV1||N\r\n";
            hl7 += "\r\n\n";
            hl7 += "===== Cwm Taff (CTT) =====\r\n";
            hl7 += $"MSH|^~\\&|{schemaNumber3}|{schemaNumber3}|100|100|{timestamp}||ADT^A28^ADT_A05|{timestamp}|P|2.5|||||||EN\r\n";
            hl7 += $"EVN||{timestamp}||||\r\n";
            hl7 += $"PID|||{patient.GetNhsNumber.Replace(" ", "")}^^^NHS^NH~{wpasRef}^^^{schemaNumber3}^PI||{patient.GetSurname}^{patient.GetName}^^^{patient.GetTitle}^^||{Dob}|{patient.Sex}|||{address}\r\n";
            hl7 += $"PD1|||^^w92616|G8512071\r\n";
            hl7 += $"PV1||N\r\n";
            hl7 += "\r\n\n";
            hl7 += "===== Swansea Bay (SBU) =====\r\n";
            hl7 += $"MSH|^~\\&|{schemaNumber4}|{schemaNumber4}|100|100|{timestamp}||ADT^A28^ADT_A05|{timestamp}|P|2.5|||||||EN\r\n";
            hl7 += $"EVN||{timestamp}||||\r\n";
            hl7 += $"PID|||{patient.GetNhsNumber.Replace(" ", "")}^^^NHS^NH~{wpasRef}^^^{schemaNumber4}^PI||{patient.GetSurname}^{patient.GetName}^^^{patient.GetTitle}^^||{Dob}|{patient.Sex}|||{address}\r\n";
            hl7 += $"PD1|||^^w92616|G8512071\r\n";
            hl7 += $"PV1||N\r\n";
            return hl7;
        }

        #endregion

        #region FileConverter

        [HttpGet]
        public IActionResult FileConverter()
        {
            return View();
        }

        #endregion

        #region Xml Coomparison

        [HttpGet]
        public IActionResult XmlCompare()
        {
            return View(new XmlCompareViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XmlCompare(XmlCompareViewModel model, string action)
        {
            if (action == "clear")
            {
                // Reset the model
                ModelState.Clear();
                return View(new XmlCompareViewModel());
            }

            // Read uploaded files if present, otherwise use hidden fields
            if (model.FirstXmlFile != null && model.SecondXmlFile != null)
            {
                using (var firstStream = new System.IO.StreamReader(model.FirstXmlFile.OpenReadStream()))
                using (var secondStream = new System.IO.StreamReader(model.SecondXmlFile.OpenReadStream()))
                {
                    model.FirstXmlContent = await firstStream.ReadToEndAsync();
                    model.SecondXmlContent = await secondStream.ReadToEndAsync();
                }
            }
            else
            {
                // Use hidden fields if files are not uploaded (e.g., after Compare)
                if (!string.IsNullOrEmpty(model.FirstXmlContent))
                    model.FirstXmlContent = Uri.UnescapeDataString(model.FirstXmlContent);
                if (!string.IsNullOrEmpty(model.SecondXmlContent))
                    model.SecondXmlContent = Uri.UnescapeDataString(model.SecondXmlContent);
            }

            if (!string.IsNullOrWhiteSpace(model.FirstXmlContent) && !string.IsNullOrWhiteSpace(model.SecondXmlContent))
            {
                if (action == "compare")
                {
                    model.Differences = _xmlCompareService.CompareXml(model.FirstXmlContent, model.SecondXmlContent);
                    model.ComparisonPerformed = true;
                    model.MergePerformed = false;
                }
                else if (action == "merge")
                {
                    model.MergedXmlContent = _xmlCompareService.MergeXmlDocuments(model.FirstXmlContent, model.SecondXmlContent);
                    model.MergePerformed = true;
                    model.ComparisonPerformed = false;
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(XmlCompareViewModel model, string action)
        {
            if (action == "clear")
            {
                return RedirectToAction(nameof(Index));
            }

            if (TempData.ContainsKey("FirstXmlContent") && TempData.ContainsKey("SecondXmlContent"))
            {
                model.FirstXmlContent = TempData["FirstXmlContent"] as string;
                model.SecondXmlContent = TempData["SecondXmlContent"] as string;
            }

            if (model.FirstXmlFile != null && model.SecondXmlFile != null)
            {
                using (var firstStream = new System.IO.StreamReader(model.FirstXmlFile.OpenReadStream()))
                using (var secondStream = new System.IO.StreamReader(model.SecondXmlFile.OpenReadStream()))
                {
                    model.FirstXmlContent = await firstStream.ReadToEndAsync();
                    model.SecondXmlContent = await secondStream.ReadToEndAsync();
                }
            }

            if (!string.IsNullOrEmpty(model.FirstXmlContent) && !string.IsNullOrEmpty(model.SecondXmlContent))
            {
                TempData["FirstXmlContent"] = model.FirstXmlContent;
                TempData["SecondXmlContent"] = model.SecondXmlContent;

                if (action == "compare" || action == null)
                {
                    model.Differences = _xmlCompareService.CompareXml(model.FirstXmlContent, model.SecondXmlContent);
                    model.ComparisonPerformed = true;
                    model.MergePerformed = false;
                }
                else if (action == "merge")
                {
                    model.MergedXmlContent = _xmlCompareService.MergeXmlDocuments(model.FirstXmlContent, model.SecondXmlContent);
                    model.MergePerformed = true;
                    model.ComparisonPerformed = false;
                    TempData["MergedXmlContent"] = model.MergedXmlContent;
                }
            }

            return View("~/Views/Home/XmlCompare.cshtml", model);
        }

        [HttpGet]
        public IActionResult DownloadMergedXml()
        {
            if (TempData.TryGetValue("MergedXmlContent", out object xmlContentObj) && xmlContentObj is string xmlContent)
            {
                TempData.Keep("MergedXmlContent");
                byte[] bytes = Encoding.UTF8.GetBytes(xmlContent);
                return File(bytes, "text/xml", "merged_document.xml");
            }
            return NotFound("Merged XML content not found. Please merge documents first.");
        }

        [HttpGet]
        [HttpPost]
        public IActionResult GetHighlightedXml(string xmlContent, string differencesJson, bool isFirstFile)
        {
            if (string.IsNullOrEmpty(xmlContent) || string.IsNullOrEmpty(differencesJson))
            {
                return Content("<p>No XML content available</p>", "text/html");
            }
            try
            {
                _logger.LogInformation("Processing XML content of length: {Length}", xmlContent.Length);
                var differences = System.Text.Json.JsonSerializer.Deserialize<List<XmlDifference>>(differencesJson) ?? new List<XmlDifference>();
                var highlightedXml = _xmlCompareService.HighlightDifferences(xmlContent, differences, isFirstFile);
                var htmlContent = $@"<!DOCTYPE html><html><head><style>body {{ font-family: monospace; white-space: pre-wrap; padding: 10px; }} .diff-value {{ background-color: #ffdddd; }} .diff-attr {{ background-color: #ffffcc; }} .diff-missing {{ background-color: #ffaaaa; text-decoration: line-through; }} .diff-extra {{ background-color: #aaffaa; }}</style></head><body>{highlightedXml}</body></html>";
                return Content(htmlContent, "text/html");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error highlighting XML");
                return Content($"<div class='text-danger'>Error highlighting XML: {ex.Message}</div>", "text/html");
            }
        }

        #endregion

        #region WebScraper

        [HttpGet]
        public IActionResult WebScraper()
        {
            return View("~/Views/Home/WebScraper.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Scrape([FromBody] ScrapeRequest request)
        {
            if (request?.Urls == null || !request.Urls.Any())
            {
                return BadRequest("No URLs provided");
            }
            var result = await _webScraperService.ScrapeUrlsAsync(request.Urls);
            return Json(result);
        }

        [HttpPost]
        public IActionResult GeneratePOM([FromBody] List<WebElement> elements)
        {
            if (elements == null || !elements.Any())
            {
                return BadRequest("No elements provided");
            }
            var sb = new StringBuilder();
            sb.AppendLine("// Generated Page Object Model");
            sb.AppendLine("// Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine();
            foreach (var element in elements)
            {
                if (element.Id != "N/A")
                {
                    sb.AppendLine($"private IWebElement {element.Name}Id => _driver.FindById(\"{element.Id}\");");
                }
                if (element.RelativeXPath != "N/A")
                {
                    sb.AppendLine($"private IWebElement {element.Name}RelXpath => _driver.FindByXpath(\"{element.RelativeXPath}\");");
                }
                sb.AppendLine($"private IWebElement {element.Name}FullXpath => _driver.FindByXpath(\"{element.FullXPath}\");");
                sb.AppendLine();
            }
            var pomContent = sb.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(pomContent);
            return File(bytes, "text/plain", "PageObjectModel.txt");
        }
        #endregion

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
