using System.Linq;

namespace OpenRCT2.Content
{
    public static class Validation
    {
        public static void ValidateName(ValidatedValue<string> name)
        {
            if ((name.Value ?? "").Length < 3)
            {
                name.Message = "Invalid user name.";
                name.IsValid = false;
            }
            else
            {
                name.IsValid = true;
            }
        }

        public static void ValidateEmail(ValidatedValue<string> email)
        {
            if ((email.Value ?? "").Count(c => c == '@') != 1)
            {
                email.Message = "Invalid email address.";
                email.IsValid = false;
            }
            else
            {
                email.IsValid = true;
            }
        }

        public static void ValidatePassword(ValidatedValue<string> password, ValidatedValue<string> confirmPassword)
        {
            password.IsValid = null;
            confirmPassword.IsValid = null;

            if ((password.Value ?? "").Length < 6)
            {
                password.Message = "Password must be at least 6 characters.";
                password.IsValid = false;
            }
            else if (password.Value != confirmPassword.Value)
            {
                confirmPassword.Message = "Did not match password.";
                confirmPassword.IsValid = false;
            }
            else
            {
                password.IsValid = true;
                confirmPassword.IsValid = true;
            }
        }
    }
}
