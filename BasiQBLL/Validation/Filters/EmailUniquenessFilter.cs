using BasiQBLL.DTOs.UserDTOs;
using BasiQBLL.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace BasiQBLL.Validation.Filters
{
    public class EmailUniquenessFilter : IAsyncActionFilter
    {
        private readonly IUserService _userService;
        private readonly ILogger<EmailUniquenessFilter> _logger;

        public EmailUniquenessFilter(
            IUserService userService,
            ILogger<EmailUniquenessFilter> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get the DTO from the action arguments
            CreateUserDTO? createUserDto = null;

            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is CreateUserDTO dto)
                {
                    createUserDto = dto;
                    break;
                }
            }

            if (createUserDto == null)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Success = false,
                    Message = "Invalid request data."
                });
                return;
            }

            // Check if email is already registered
            if (!string.IsNullOrEmpty(createUserDto.Email))
            {
                var existingUser = await _userService.GetUserByEmailAsync(createUserDto.Email);

                if (existingUser.Success && existingUser.Data != null)
                {
                    _logger.LogWarning("Registration attempt with existing email: {Email}", createUserDto.Email);

                    context.Result = new ConflictObjectResult(new
                    {
                        Success = false,
                        Message = "This email address is already registered. Please use a different email or try logging in."
                    });
                    return;
                }

                // We might want to allow re-registration of deleted users or not
                // If we want to check deleted users as well, we'd need to modify GetUserByEmailAsync
                // to include deleted users, or create a separate method.
            }

            // If email is unique, continue to the action
            await next();
        }
    }
}
