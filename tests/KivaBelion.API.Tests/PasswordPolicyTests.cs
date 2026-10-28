using KivaBelion.API.Security;

namespace KivaBelion.API.Tests;

public class PasswordPolicyTests
{
    [Fact] public void Strong_password_is_accepted() => Assert.Empty(PasswordPolicy.Validate("Correct-Horse-4-Battery", "owner@example.com"));
    [Fact] public void Short_password_is_rejected() => Assert.NotEmpty(PasswordPolicy.Validate("Ab1"));
    [Fact] public void Empty_or_null_password_is_rejected()
    {
        Assert.NotEmpty(PasswordPolicy.Validate(null));
        Assert.NotEmpty(PasswordPolicy.Validate(""));
    }
    [Fact] public void Password_without_digit_is_rejected() => Assert.NotEmpty(PasswordPolicy.Validate("OnlyLettersHereAbc"));
    [Fact] public void Password_without_letter_is_rejected() => Assert.NotEmpty(PasswordPolicy.Validate("123456789012345"));
    [Fact] public void Repetitive_password_is_rejected() => Assert.NotEmpty(PasswordPolicy.Validate("aaaaaaaaaaaa1"));
    [Fact] public void Password_containing_email_name_is_rejected() =>
        Assert.NotEmpty(PasswordPolicy.Validate("ownername-2026-xyz", "ownername@example.com"));
}
