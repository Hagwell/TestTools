using NUnit.Framework;
using TestTools.Models;
using TestTools.Services;
using TestTools.Controllers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TestTools.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private HomeController _controller;
        private Mock<IWebHostEnvironment> _envMock;
        private Mock<IConfiguration> _configMock;

        [SetUp]
        public void Setup()
        {
            _envMock = new Mock<IWebHostEnvironment>();
            _envMock.Setup(e => e.WebRootPath).Returns(Directory.GetCurrentDirectory());
            _envMock.Setup(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var endpoints = new List<ApiEndpointModel> { new ApiEndpointModel { Name = "Test", Url = "/Home/MockApi", Active = true } };
            var sectionMock = new Mock<IConfigurationSection>();
            sectionMock.Setup(s => s.Value).Returns(""); // Not used, but required

            // Instead of mocking .Get<T>(), mock the section and use a helper in your test
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c.GetSection("ApiEndpoints")).Returns(sectionMock.Object);

            _controller = new HomeController(
                new NullLogger<HomeController>(),
                _envMock.Object,
                new DocumentConversionService(),
                new ImageConversionService(),
                new Base64Service(),
                new WebScraperService(new HttpClient()),
                _configMock.Object
            );
        }

        [Test]
        public void Index_ReturnsViewResult()
        {
            var result = _controller.Index();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Index_ReturnsViewResult_WithDocumentTabsViewModel()
        {
            var result = _controller.Index();
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.InstanceOf<DocumentTabsViewModel>());
        }

        [Test]
        public void GetFrameworksAndGuidesTab_ReturnsPartialView()
        {
            var result = _controller.GetFrameworksAndGuidesTab();
            Assert.That(result, Is.InstanceOf<PartialViewResult>());
            var partial = result as PartialViewResult;
            Assert.That(partial?.ViewName, Is.EqualTo("_FrameworksAndGuidesTab"));
            Assert.That(partial?.Model, Is.InstanceOf<DocumentsViewModel>());
        }

        [Test]
        public void GetTestingSupportTab_ReturnsPartialView()
        {
            var result = _controller.GetTestingSupportTab();
            Assert.That(result, Is.InstanceOf<PartialViewResult>());
            var partial = result as PartialViewResult;
            Assert.That(partial?.ViewName, Is.EqualTo("_TestingSupportTab"));
            Assert.That(partial?.Model, Is.InstanceOf<DocumentsViewModel>());
        }

        [Test]
        public void ServiceChecker_ReturnsViewResult_WithApiEndpointModelList()
        {
            var result = _controller.ServiceChecker();
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.InstanceOf<List<ApiEndpointModel>>());
        }

        [Test]
        public void PatientGenerator_ReturnsViewResult()
        {
            var result = _controller.PatientGenerator();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void FileConverter_ReturnsViewResult()
        {
            var result = _controller.FileConverter();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void XmlCompare_ReturnsViewResult_WithXmlCompareViewModel()
        {
            var result = _controller.XmlCompare();
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.InstanceOf<XmlCompareViewModel>());
        }

        [Test]
        public void ViewDocument_InvalidPath_ReturnsNotFound()
        {
            var result = _controller.ViewDocument(null!);
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public void GetDocumentsFromCategory_NonExistent_ReturnsEmptyList()
        {
            var docs = typeof(HomeController)
            .GetMethod("GetDocumentsFromCategory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_controller, new object[] { "NonExistent" }) as List<DocumentModel>;
            Assert.That(docs, Is.Not.Null);
            Assert.That(docs, Is.Empty);
        }

        [Test]
        public async Task CheckEndpoint_ReturnsJsonResult()
        {
            var result = await _controller.CheckEndpoint("/Home/MockApi");
            Assert.That(result, Is.InstanceOf<JsonResult>());
        }

        [Test]
        public async Task XmlCompare_ClearAction_ResetsModel()
        {
            var model = new XmlCompareViewModel();
            var result = await _controller.XmlCompare(model, "clear");
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewModel = ((ViewResult)result).Model as XmlCompareViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.ComparisonPerformed, Is.False);
            Assert.That(viewModel.MergePerformed, Is.False);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
            _controller = null;
        }
    }

    [TestFixture]
    public class ServiceTests
    {
        [Test]
        public void Base64Service_EncodeDecode_Works()
        {
            var service = new Base64Service();
            var original = "hello";
            var bytes = System.Text.Encoding.UTF8.GetBytes(original);
            var (dataUrl, contentType) = service.ToBase64(bytes, "text/plain");
            var (decodedBytes, decodedContentType) = service.FromBase64(dataUrl.Substring(dataUrl.IndexOf(",") + 1));
            var decoded = System.Text.Encoding.UTF8.GetString(decodedBytes);
            Assert.That(decoded, Is.EqualTo(original));
        }

        [Test]
        public void DocumentConversionService_ConvertPdfToText_HandlesNull()
        {
            var service = new DocumentConversionService();
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await service.ConvertAsync(null!, ".pdf", ".txt");
            });
        }
    }

    [TestFixture]
    public class ModelTests
    {
        [Test]
        public void DocumentModel_Properties_SetAndGet()
        {
            var doc = new DocumentModel
            {
                Name = "Test.pdf",
                Path = "/docs/Test.pdf",
                Category = "Education",
                Size = 1234
            };
            Assert.That(doc.Name, Is.EqualTo("Test.pdf"));
            Assert.That(doc.Path, Is.EqualTo("/docs/Test.pdf"));
            Assert.That(doc.Category, Is.EqualTo("Education"));
            Assert.That(doc.Size, Is.EqualTo(1234));
        }
    }
}