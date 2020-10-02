﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyLeasing.Web.Data.Entities;

namespace MyLeasing.Web.Models
{
    public class ContractViewModel : Contract
    {
        //ownerid es para ligarla al contrato
        public int OwnerId { get; set; }

        public int PropertyId { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Lessee")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a lessee.")]
        //cuando se selecciona el arrendatario
        public int LesseeId { get; set; }

        public IEnumerable<SelectListItem> Lessees { get; set; }
    }
}
