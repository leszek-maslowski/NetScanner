using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetScanner.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ScannerHostController : ControllerBase
    {
        IpScanner scanner;

        public ScannerHostController()
        {
            scanner = new IpScanner();
        }

        private readonly ILogger<ScannerHostController> _logger;

        public ScannerHostController(ILogger<ScannerHostController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<ScannerHost> Get()
        {
            return new ScannerHost[] { new ScannerHost()
            {
                Address = "test",
                ComputerName = "bla bla"
            } };
            //return scanner.Scan();
        }
    }
}
