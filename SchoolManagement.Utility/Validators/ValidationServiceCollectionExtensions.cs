using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Utility.Validators.StudentValidators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Utility.Validators
{
    public static class ValidationServiceCollectionExtensions
    {
        public static IServiceCollection ApplicationEntityValidation(this IServiceCollection services)
        {
            services.AddScoped<IStudentValidatorService, StudentValidatorService>();

            return services;
        }
    }
}