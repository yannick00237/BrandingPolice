using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BrandingPolice.Models
{
    public class PowerpointFile
    {

       // public string FileTitle { get; set; }

        //[Required(ErrorMessage = "Upload PowerPoint file")]
        //[Display(Name = "choose File")]
        //[DataType(DataType.Upload)]
        //[FileExtensions(Extensions = "pptx,pptm,potx,potm,ppam,ppsx,ppsm,sldx,ppt,pot,pps,sldm")]
        public IFormFile MyFile { get; set; }

    }
}
