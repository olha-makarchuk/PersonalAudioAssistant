using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalAudioAssistant.Application.Services
{
    public class BlobStorageConfig
    {
        private readonly IConfiguration _configuration;
        public BlobStorageConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ConnectionString => _configuration.GetConnectionString("BlobConnectionString");
    }
}
