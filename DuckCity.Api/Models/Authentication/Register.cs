﻿namespace DuckCity.Api.Models.Authentication
{
    public class Register
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? PasswordConfirmation { get; set; }
    }
}
