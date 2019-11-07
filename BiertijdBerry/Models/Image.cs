using System;
using System.Collections.Generic;
using System.Text;

namespace BiertijdBerry.Models
{
    public class Image
    {
        public string lon { get; set; }
        public string lat { get; set; }
        public string temp { get; set; }
        public string blobName { get; set; }
        public string blobContainerReference { get; set; }

        public Image(string lon, string lat, string temp, string blobName, string blobContainerReference)
        {
            this.lon = lon;
            this.lat = lat;
            this.temp = temp;
            this.blobName = blobName;
            this.blobContainerReference = blobContainerReference;
        }
    }
}
