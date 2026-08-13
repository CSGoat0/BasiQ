using BasiQBLL.Validation.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BasiQBLL.Validation.Attributes
{
    public class EmailUniquenessAttribute : ServiceFilterAttribute
    {
        public EmailUniquenessAttribute()
            : base(typeof(EmailUniquenessFilter))
        {
        }
    }
}
