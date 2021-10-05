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
        private static IpScanner scanner = new IpScanner();

        

        private readonly ILogger<ScannerHostController> _logger;

        public ScannerHostController(ILogger<ScannerHostController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<ScannerHost> Get()
        {
            return scanner.Scan();
        }
    }
}
