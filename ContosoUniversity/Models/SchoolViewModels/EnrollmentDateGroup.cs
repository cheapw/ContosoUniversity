using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ContosoUniversity.Models.SchoolViewModels
{
    public class EnrollmentDateGroup
    {
        [DataType(DataType.Date),Display(Name ="Enrollment Date")]
        public DateTime? EnrollmentDate { get; set; }
        [Display(Name ="Students")]
        public int StudentCount { get; set; }
    }
}
